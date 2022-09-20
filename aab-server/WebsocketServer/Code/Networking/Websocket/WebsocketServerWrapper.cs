using MasterNetworking.EventHandling;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp.Server;

namespace MasterNetworking.WebSocket
{
    public delegate void WebsocketServerEventHandler(string message, JsonRpcService service = null);

    /// <summary>
    /// A Wrapper for the overall websocket server implementation. 
    /// This will handle all the necessary connection and registration steps.
    /// </summary>
    public class WebSocketServerWrapper
    {
        #region Singleton
        private static WebSocketServerWrapper _instance;
        public static WebSocketServerWrapper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new WebSocketServerWrapper();
                return _instance;
            }
        }

        public static bool ExistsInstance => _instance != null;

        private WebSocketServerWrapper()
        {

        }
        #endregion

        public event WebsocketServerEventHandler Started;
        public event WebsocketServerEventHandler OnError;
        public event WebsocketServerEventHandler Stopped;
        public event WebsocketServerEventHandler MessageReceived;
        public event WebsocketServerEventHandler MessageSend;
        public event WebsocketServerEventHandler ConnectionOpened;
        public event WebsocketServerEventHandler ConnectionClosed;
        public event WebsocketServerEventHandler DeviceRegistrated;

        private WebSocketServer _server;
        private List<JsonRpcService> _connections = new List<JsonRpcService>();
        private Queue<Action> _messageQueue;

        public bool ProvideMessageQueue => _messageQueue != null;

        public List<string> ConnectionNames => _connections.Where(s => s.Name != null && s.ClientConnectionType != ClientConnectionType.NotDefined).Select(s => s.Name).ToList();
        public List<ClientConnectionType> ClientConnectionTypes => _connections.Where(s => s.Name != null && s.ClientConnectionType != ClientConnectionType.NotDefined).Select(s => s.ClientConnectionType).Distinct().ToList();
        public List<JsonRpcService> UnregisteredServies => _connections.Where(s => s.Name == null && s.ClientConnectionType == ClientConnectionType.NotDefined).ToList();

        public int Port { get; private set; } = -1;

        #region Lifecycle Methods
        /// <summary>
        /// Starts the websocket server.
        /// </summary>
        /// <param name="port">The port of the server to be opened on.</param>
        public void StartServer(int port, Queue<Action> messageQueue = null)
        {
            if (_server != null)
                return;

            _messageQueue = new Queue<Action>();

            Port = port;

            _server = new WebSocketServer(port);
            _server.AddWebSocketService<JsonRpcService>("/JsonRpcService");

            try
            {
                _server.Start();
            }
            catch (InvalidOperationException ex)
            {
                OnError?.Invoke(null);
                return;
            }

            Started?.Invoke(null);
        }

        /// <summary>
        /// Stops the websocket.
        /// </summary>
        public void StopServer() 
        {
            _server.Stop();
            _server = null;

            _messageQueue = null;

            Port = -1;

            Stopped?.Invoke(null);
        }

        /// <summary>
        /// Saves the newly opened connection.
        /// </summary>
        /// <param name="service">The service the connection was opened on.</param>
        public void OpenConnection(JsonRpcService service)
        {
            if (_connections.Contains(service))
                return;

            _connections.Add(service);
            ConnectionOpened?.Invoke(service.Context.UserEndPoint.ToString(), service);
        }

        /// <summary>
        /// Deletes the closed connection.
        /// </summary>
        /// <param name="service">The service that was closed.</param>
        public void CloseConnection(JsonRpcService service)
        {
            if (!_connections.Contains(service))
                return;

            _connections.Remove(service);
            ConnectionClosed?.Invoke(service.Context.UserEndPoint.ToString(), service);
        }
        #endregion

        #region Handling of Messages
        /// <summary>
        /// Handles the message received over the connection of the provided service.
        /// This will invoke all connected events via the JsonRpcSerializer.
        /// </summary>
        /// <param name="service">The service that got this message.</param>
        /// <param name="message">The message that was send over the connection.</param>
        public void HandleMessage(JsonRpcService service, string message)
        {
            if (ProvideMessageQueue)
                MessageReceived?.Invoke(message, service);
            else
                _messageQueue.Enqueue(() => MessageReceived?.Invoke(message, service));
        }

        /// <summary>
        /// Handles only one specific message type: "DeviceRegistration".
        /// </summary>
        /// <param name="service">The service that got this message.</param>
        /// <param name="message">The message that was send over the connection.</param>
        public void HandleRegistration(JsonRpcMessage message, JsonRpcService service)
        {
            // Check if the service exists and is not already registered.
            if (!UnregisteredServies.Contains(service))
                return;

            // Check if the registration is not not defined.
            ClientConnectionType? ccType = ClientConnectionTypeUtils.FromString(message.Data["clientConnectionType"].ToString());
            if (!ccType.HasValue || ccType.Value == ClientConnectionType.NotDefined)
                return;

            // If a connection with this name is already present, don't register the new one.
            string connectionName = message.Data["name"].ToString();
            if (ConnectionNames.Contains(connectionName))
                return;

            // Updated the service according to the registration information.
            bool successfulRegistration = service.Register(connectionName, ccType.Value);
            if (!successfulRegistration)
                return;

            // Send the answer, that the registration was successful.
            Send(
                message: message.CreateAnswerMessage(server: true),
                connectionName: service.Name
            );

            DeviceRegistrated?.Invoke(service.Name, service);
        }
        #endregion

        #region Send Methods
        /// <summary>
        /// Sends a message to the specified connection.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="ccType">The conenction type to send to. If this is "NotDefined" (default) then it will send to every open connection.</param>
        /// <param name="connectionName">The name of the open connection to send to. If this is null (default) then it will send to every open connection.</param>
        public void Send(JsonRpcMessage message, ClientConnectionType? ccType = null, string connectionName = null)
        {
            JObject jObject = message.ToJObject();
            string stringMessage = jObject.ToString();

            MessageSend?.Invoke(stringMessage);

            foreach (JsonRpcService connection in _connections.ToList())
            {
                // Don't send this message to the connection this message came from.
                // This is only used/needed for the server.
                if (message.Communication.Direction == "from" && message.Communication.Name == connection.Name)
                    continue;

                if (ccType.HasValue && ccType.Value != connection.ClientConnectionType)
                    continue;
                if (connectionName != null && connectionName != "" && connection.Name != connectionName)
                    continue;

                connection.SendMessage(stringMessage);
            }
        }
        #endregion
    }
}
