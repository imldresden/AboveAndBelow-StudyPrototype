using MasterNetworking.EventHandling;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace MasterNetworking.WebSocket
{
    public enum WsConnectionStatus
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Registered = 3
    };

    public delegate void WebsocketClientEventHandler(string message);

    /// <summary>
    /// A Wrapper for the overall websocket client implementation.
    /// </summary>
    public class WebSocketClientWrapper
    {
        #region Singleton
        private static WebSocketClientWrapper _instance;
        public static WebSocketClientWrapper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new WebSocketClientWrapper();
                return _instance;
            }
        }

        public static bool ExistsInstance => _instance != null;

        private WebSocketClientWrapper()
        {

        }
        #endregion

        public event WebsocketClientEventHandler Started;
        public event WebsocketClientEventHandler Stopped;
        public event WebsocketClientEventHandler MessageReceived;
        public event WebsocketClientEventHandler MessageSend;
        public event WebsocketClientEventHandler ConnectionOpened;
        public event WebsocketClientEventHandler ConnectionClosed;
        public event WebsocketClientEventHandler Registered;

        private WebSocketSharp.WebSocket _client;
        private Queue<Action> _messageQueue;
        public bool ProvideMessageQueue => _messageQueue != null;

        public WsConnectionStatus Status { get; private set; } = WsConnectionStatus.Disconnected;
        public ClientConnectionType ClientConnectionType { get; private set; } = ClientConnectionType.NotDefined;
        public string ConnectionName { get; private set; } = null;
        public bool Usable => ((int)Status) >= 2 && ClientConnectionType != ClientConnectionType.NotDefined;

        public string Ip { get; private set; } = null;
        public int Port { get; private set; } = -1;
        public string Address { get; private set; } = null;
        public string CompleteWsAddress => $"ws://{Ip}:{Port}{Address}";

        #region Lifecycle Methods
        /// <summary>
        /// Starts the websocket client.
        /// </summary>
        /// <param name="ip">The ip to connect to.</param>
        /// <param name="port">The port to open to.</param>
        /// <param name="address">The address of the websocket connection.</param>
        public void StartClient(string ip, int port, string address = null, Queue<Action> messageQueue = null)
        {
            if (_client != null)
                return;

            _messageQueue = messageQueue;

            Ip = ip;
            Port = port;
            Address = address;

            _client = new WebSocketSharp.WebSocket(CompleteWsAddress);
            _client.OnOpen += _client_OnOpen;
            _client.OnClose += _client_OnClose;
            _client.OnMessage += _client_OnMessage;

            Started?.Invoke(null);
        }

        /// <summary>
        /// Stops the websocket client.
        /// </summary>
        public void StopClient()
        {
            if (Status == WsConnectionStatus.Connected)
                _client.CloseAsync();
            
            try
            {
                _client.OnOpen -= _client_OnOpen;
                _client.OnClose -= _client_OnClose;
                _client.OnMessage -= _client_OnMessage;
            }
            catch(Exception ex) { }
            _client = null;

            _messageQueue = null;

            Ip = null;
            Port = -1;
            Address = null;

            Stopped?.Invoke(null);
        }

        /// <summary>
        /// Opens a new connection to the server.
        /// </summary>
        public void ConnectClient()
        {
            if (Status != WsConnectionStatus.Disconnected)
                return;

            try
            {
                Status = WsConnectionStatus.Connecting;
                _client.ConnectAsync();
            }
            catch (Exception ex)
            {
                Status = WsConnectionStatus.Disconnected;
                Console.WriteLine("!!! Could not connect.");
            }
        }

        /// <summary>
        /// Closes the existing connection to the server.
        /// </summary>
        public void DisconnectClient()
        {
            if (Status != WsConnectionStatus.Connected)
                return;

            _client.CloseAsync();
        }
        #endregion

        #region Websocket Events
        /// <summary>
        /// Called when the websocket client has successfully opened a connection to the server.
        /// </summary>
        private void _client_OnOpen(object sender, EventArgs e)
        {
            Status = WsConnectionStatus.Connected;
            ConnectionOpened?.Invoke(null);
        }

        /// <summary>
        /// Called when the websocket client has successfully closed the open connection to the server.
        /// </summary>
        private void _client_OnClose(object sender, CloseEventArgs e)
        {
            Status = WsConnectionStatus.Disconnected;
            ClientConnectionType = ClientConnectionType.NotDefined;
            ConnectionClosed?.Invoke(null);
        }
        #endregion

        #region Handling of Messages
        /// <summary>
        /// Called when a new message was received from the server.
        /// </summary>
        private void _client_OnMessage(object sender, MessageEventArgs e)
        {
            if (!ProvideMessageQueue)
                MessageReceived?.Invoke(e.Data);
            else
                _messageQueue.Enqueue(() => MessageReceived?.Invoke(e.Data));
        }

        /// <summary>
        /// Handles only one specific message type: "DeviceRegistration".
        /// </summary>
        /// <param name="message">The message send over the connection.</param>
        public void HandleRegistration(JsonRpcMessage message)
        {
            // Check if the registration is not not defined.
            ClientConnectionType? deviceConnectionType = ClientConnectionTypeUtils.FromString(message.Data["clientConnectionType"].ToString());
            if (!deviceConnectionType.HasValue || deviceConnectionType.Value == ClientConnectionType.NotDefined)
                return;

            ClientConnectionType = deviceConnectionType.Value;
            ConnectionName = message.Data["name"].ToString();
            Status = WsConnectionStatus.Registered;

            Registered?.Invoke(null);
        }
        #endregion

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void Send(JsonRpcMessage message)
        {
            JObject jObject = message.ToJObject();
            string stringMessage = jObject.ToString();

            _sendAsync(stringMessage);
            MessageSend?.Invoke(stringMessage);
        }

        /// <summary>
        /// Wrapper method to send the message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        private async void _sendAsync(string message) => _client.SendAsync(message, null);
    }
}
