using MasterNetworking.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterNetworking.EventHandling
{
    public delegate void JsonRpcMessageEventHandler(JsonRpcMessage message);

    public class JsonRpcHandler
    {
        #region Singleton
        private static JsonRpcHandler _instance;
        public static JsonRpcHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new JsonRpcHandler();
                return _instance;
            }
        }
        #endregion

        public string ConnectionType { get; private set; } = null;

        private int _currentId = 0;
        public int NextId => _currentId++;
        
        private Dictionary<string, List<JsonRpcMessageEventHandler>> _notificationDict;
        private Dictionary<string, List<JsonRpcMessageEventHandler>> _requestDict;
        private Dictionary<string, List<JsonRpcMessageEventHandler>> _answerDict;

        private Dictionary<string, List<JsonRpcMessageEventHandler>> _oneTimeNotification;
        private Dictionary<string, List<JsonRpcMessageEventHandler>> _oneTimeRequests;
        private Dictionary<string, List<JsonRpcMessageEventHandler>> _oneTimeAnswers;

        /// <summary>
        /// All registered RPC events. Either as Request or Answer.
        /// </summary>
        public List<string> RegisteredRpcEvents => _notificationDict.Keys.Concat(_requestDict.Keys.Concat(_answerDict.Keys)).Distinct().ToList();
        /// <summary>
        /// All registered request RPC events.
        /// </summary>
        public List<string> RegisteredNotificationRpcEvents => _notificationDict.Keys.Distinct().ToList();
        /// <summary>
        /// All registered request RPC events.
        /// </summary>
        public List<string> RegisteredRequestRpcEvents => _requestDict.Keys.Distinct().ToList();
        /// <summary>
        /// All registered answer RPC events.
        /// </summary>
        public List<string> RegisteredAnswerRpcEvents => _answerDict.Keys.Distinct().ToList();

        private JsonRpcHandler()
        {
            _notificationDict = new Dictionary<string, List<JsonRpcMessageEventHandler>>();
            _requestDict = new Dictionary<string, List<JsonRpcMessageEventHandler>>();
            _answerDict = new Dictionary<string, List<JsonRpcMessageEventHandler>>();

            _oneTimeNotification = new Dictionary<string, List<JsonRpcMessageEventHandler>>();
            _oneTimeRequests = new Dictionary<string, List<JsonRpcMessageEventHandler>>();
            _oneTimeAnswers = new Dictionary<string, List<JsonRpcMessageEventHandler>>();

            // Fill all the dicts with an empty list for all possible rps events.
            foreach (JsonRpcEvent rpcEvent in Enum.GetValues(typeof(JsonRpcEvent)))
            {
                string rpcEventString = JsonRpcEventUtils.ToString(rpcEvent);

                _notificationDict.Add(rpcEventString, new List<JsonRpcMessageEventHandler>());
                _requestDict.Add(rpcEventString, new List<JsonRpcMessageEventHandler>());
                _answerDict.Add(rpcEventString, new List<JsonRpcMessageEventHandler>());
                _oneTimeNotification.Add(rpcEventString, new List<JsonRpcMessageEventHandler>());
                _oneTimeRequests.Add(rpcEventString, new List<JsonRpcMessageEventHandler>());
                _oneTimeAnswers.Add(rpcEventString, new List<JsonRpcMessageEventHandler>());
            }
        }

        #region Websocket Connections
        public void ConnectToServer()
        {
            if (ConnectionType != null)
                return;

            ConnectionType = "server";
            WebSocketServerWrapper.Instance.MessageReceived += _server_MessageReceived;
        }

        public void DisconnectFromServer()
        {
            if (ConnectionType == "server")
                return;

            ConnectionType = null;
            WebSocketServerWrapper.Instance.MessageReceived -= _server_MessageReceived;
        }

        public void ConnectToClient()
        {
            if (ConnectionType != null)
                return;

            ConnectionType = "client";
            WebSocketClientWrapper.Instance.MessageReceived += _client_MessageReceived;
        }

        public void DisconnectFromClient()
        {
            if (ConnectionType == "client")
                return;

            ConnectionType = null;
            WebSocketClientWrapper.Instance.MessageReceived -= _client_MessageReceived;
        }
        #endregion

        #region Handle Messages
        /// <summary>
        /// Called when the websocket server received a message.
        /// </summary>
        /// <param name="message">The message that the server received.</param>
        /// <param name="service">The service/open connection that received this message.</param>
        private void _server_MessageReceived(string message, JsonRpcService service = null)
        {
            JsonRpcMessage jsonMessage = JsonRpcMessage.DeserializeMessage(message);
            if (jsonMessage == null)
                return;

            if (jsonMessage.Communication.ClientConnectionType.HasValue && jsonMessage.Communication.ClientConnectionType.Value == ClientConnectionType.Server)
                _server_handleInternalMessage(jsonMessage, service);
            else
                _server_handleTunnelMessage(jsonMessage, service);
        }

        private void _server_handleInternalMessage(JsonRpcMessage jsonMessage, JsonRpcService service)
        {
            // If the client is not yet registered, try to do this.
            if (WebSocketServerWrapper.Instance.UnregisteredServies.Contains(service))
            {
                if (jsonMessage.MessageType != JsonRpcMessageType.Request)
                    return;
                if (jsonMessage.Method != JsonRpcEventUtils.ToString(JsonRpcEvent.DeviceRegistration))
                    return;

                WebSocketServerWrapper.Instance.HandleRegistration(jsonMessage, service);
                // If the registration was unsucessful, the state hasn't changed.
                if (WebSocketServerWrapper.Instance.UnregisteredServies.Contains(service))
                    return;
            }

            switch (jsonMessage.MessageType)
            {
                case JsonRpcMessageType.Notification:   _handleNotification(jsonMessage);   break;
                case JsonRpcMessageType.Request:        _handleRequest(jsonMessage);        break;
                case JsonRpcMessageType.Answer:         _handleAnswer(jsonMessage);         break;
            }
        }

        private void _server_handleTunnelMessage(JsonRpcMessage jsonMessage, JsonRpcService service)
        {
            // If the client is not yet registered, don't handle it's messages.
            if (WebSocketServerWrapper.Instance.UnregisteredServies.Contains(service))
                return;

            if (jsonMessage.Communication.Direction != "to")
                return;

            // #Check Is the message correctly created?
            WebSocketServerWrapper.Instance.Send(
                message: jsonMessage.CreateTunnelMessage(service),
                ccType: jsonMessage.Communication.ClientConnectionType,
                connectionName: jsonMessage.Communication.Name
            );
        }

        /// <summary>
        /// Called when the websocket client received a message.
        /// </summary>
        /// <param name="message">The message that the client received.</param>
        private void _client_MessageReceived(string message)
        {
            JsonRpcMessage jsonMessage = JsonRpcMessage.DeserializeMessage(message);
            if (jsonMessage == null)
                return;

            // If the client is not yet registered, try to do this.
            if (WebSocketClientWrapper.Instance.Status != WsConnectionStatus.Registered)
            {
                if (jsonMessage.MessageType != JsonRpcMessageType.Request)
                    return;
                if (jsonMessage.Method != JsonRpcEventUtils.ToString(JsonRpcEvent.DeviceRegistration))
                    return;

                WebSocketClientWrapper.Instance.HandleRegistration(jsonMessage);
                // If the registration was unsucessful, the state hasn't changed.
                if (WebSocketClientWrapper.Instance.Status != WsConnectionStatus.Registered)
                    return;
            }

            switch (jsonMessage.MessageType)
            {
                case JsonRpcMessageType.Notification: _handleNotification(jsonMessage); break;
                case JsonRpcMessageType.Request: _handleRequest(jsonMessage); break;
                case JsonRpcMessageType.Answer: _handleAnswer(jsonMessage); break;
            }
        }

        /// <summary>
        /// Handles a newly gotten notification.
        /// </summary>
        /// <param name="message">The notification to handle.</param>

        private void _handleNotification(JsonRpcMessage message)
        {
            if (!_notificationDict.ContainsKey(message.Method))
                return;

            foreach (JsonRpcMessageEventHandler notificationCall in _notificationDict[message.Method])
            {
                notificationCall.Invoke(message);

                // If this was a one time request, remove it after invoking it.
                if (_oneTimeNotification.ContainsKey(message.Method) && _oneTimeNotification[message.Method].Contains(notificationCall))
                {
                    _notificationDict[message.Method].Remove(notificationCall);
                    _oneTimeNotification[message.Method].Remove(notificationCall);
                }
            }
        }

        /// <summary>
        /// Handles a newly gotten request.
        /// </summary>
        /// <param name="message">The request to handle.</param>
        private void _handleRequest(JsonRpcMessage message)
        {
            if (!_requestDict.ContainsKey(message.Method))
                return;

            foreach (JsonRpcMessageEventHandler requestCall in _requestDict[message.Method])
            {
                requestCall.Invoke(message);

                // If this was a one time request, remove it after invoking it.
                if (_oneTimeRequests.ContainsKey(message.Method) && _oneTimeRequests[message.Method].Contains(requestCall))
                {
                    _requestDict[message.Method].Remove(requestCall);
                    _oneTimeRequests[message.Method].Remove(requestCall);
                }
            }
        }

        /// <summary>
        /// Handles a newly gotten answer.
        /// </summary>
        /// <param name="message">The answer to handle.</param>
        private void _handleAnswer(JsonRpcMessage message)
        {
            if (!_answerDict.ContainsKey(message.Method))
                return;

            foreach (JsonRpcMessageEventHandler answerCall in _answerDict[message.Method])
            {
                answerCall.Invoke(message);

                // If this was a one time request, remove it after invoking it.
                if (_oneTimeAnswers.ContainsKey(message.Method) && _oneTimeAnswers[message.Method].Contains(answerCall))
                {
                    _answerDict[message.Method].Remove(answerCall);
                    _oneTimeAnswers[message.Method].Remove(answerCall);
                }
            }
        }
        #endregion

        #region Add Delegate Handling
        /// <summary>
        /// Adds a new RPC Notification delegate to the given method names.
        /// If the exact same method was already added, it will do nothing.
        /// </summary>
        /// <param name="rpcEvent">The name of the notification method.</param>
        /// <param name="notification">The method/delegate for the given method name.</param>
        /// <param name="oneTime">Should this event only be called one time with the next event?</param>
        public void AddNotificationDelegate(JsonRpcEvent rpcEvent, JsonRpcMessageEventHandler notification, bool oneTime = false)
            => AddNotificationDelegate(JsonRpcEventUtils.ToString(rpcEvent), notification, oneTime);

        /// <summary>
        /// Adds a new RPC Notification delegate to the given method names.
        /// If the exact same method was already added, it will do nothing.
        /// </summary>
        /// <param name="rpcEvent">The name of the notification method.</param>
        /// <param name="notification">The method/delegate for the given method name.</param>
        /// <param name="oneTime">Should this event only be called one time with the next event?</param>
        public void AddNotificationDelegate(string rpcEvent, JsonRpcMessageEventHandler notification, bool oneTime = false)
            => _addDelegate(_notificationDict, _oneTimeNotification, rpcEvent, notification, oneTime);

        /// <summary>
        /// Adds a new RPC Request delegate to the given method names.
        /// If the exact same method was already added, it will do nothing.
        /// </summary>
        /// <param name="rpcEvent">The name of the request method.</param>
        /// <param name="request">The method/delegate for the given method name.</param>
        /// <param name="oneTime">Should this event only be called one time with the next event?</param>
        public void AddRequestDelegate(JsonRpcEvent rpcEvent, JsonRpcMessageEventHandler request, bool oneTime = false)
            => AddRequestDelegate(JsonRpcEventUtils.ToString(rpcEvent), request, oneTime);

        /// <summary>
        /// Adds a new RPC Request delegate to the given method names.
        /// If the exact same method was already added, it will do nothing.
        /// </summary>
        /// <param name="rpcEvent">The name of the request method.</param>
        /// <param name="request">The method/delegate for the given method name.</param>
        /// <param name="oneTime">Should this event only be called one time with the next event?</param>
        public void AddRequestDelegate(string rpcEvent, JsonRpcMessageEventHandler request, bool oneTime = false)
            => _addDelegate(_requestDict, _oneTimeRequests, rpcEvent, request, oneTime);

        /// <summary>
        /// Adds a new RPC Answer delegate to the given method names.
        /// If the exact same method was already added, it will do nothing.
        /// </summary>
        /// <param name="rpcEvent">The name of the answer method.</param>
        /// <param name="answer">The method/delegate for the given method name.</param>
        /// <param name="oneTime">Should this event only be called one time with the next event?</param>
        public void AddAnswerDelegate(JsonRpcEvent rpcEvent, JsonRpcMessageEventHandler answer, bool oneTime = false)
            => AddAnswerDelegate(JsonRpcEventUtils.ToString(rpcEvent), answer, oneTime);

        /// <summary>
        /// Adds a new RPC Answer delegate to the given method names.
        /// If the exact same method was already added, it will do nothing.
        /// </summary>
        /// <param name="rpcEvent">The name of the answer method.</param>
        /// <param name="answer">The method/delegate for the given method name.</param>
        /// <param name="oneTime">Should this event only be called one time with the next event?</param>
        public void AddAnswerDelegate(string rpcEvent, JsonRpcMessageEventHandler answer, bool oneTime = false)
            => _addDelegate(_answerDict, _oneTimeAnswers, rpcEvent, answer, oneTime);

        /// <summary>
        /// Adds the given method delegate to the given dicts.
        /// </summary>
        /// <param name="methodDict">The dict where all registered delegates are stored.</param>
        /// <param name="oneTimeDict">The dict that defines if a delegate should only be fired one time.</param>
        /// <param name="rpcEvent">The name of the method.</param>
        /// <param name="method">The method/delegate for the given method name.</param>
        /// <param name="oneTime">Should this event only be called one time with the next event?</param>
        private void _addDelegate(Dictionary<string, List<JsonRpcMessageEventHandler>> methodDict, Dictionary<string, List<JsonRpcMessageEventHandler>> oneTimeDict, string rpcEvent, JsonRpcMessageEventHandler method, bool oneTime = false)
        {
            if (!methodDict.ContainsKey(rpcEvent))
            {
                methodDict.Add(rpcEvent, new List<JsonRpcMessageEventHandler>());
                oneTimeDict.Add(rpcEvent, new List<JsonRpcMessageEventHandler>());
            }

            if (methodDict[rpcEvent].Contains(method))
                return;

            if (oneTime)
                oneTimeDict[rpcEvent].Add(method);
            methodDict[rpcEvent].Add(method);
        }
        #endregion

        #region Remove Delegate Handling
        /// <summary>
        /// Removes the given notification method from the given event.
        /// </summary>
        /// <param name="rpcEvent">The event the method should be removed from.</param>
        /// <param name="notification">The method to remove.</param>
        public void RemoveNotificationDelegate(JsonRpcEvent rpcEvent, JsonRpcMessageEventHandler notification)
            => RemoveNotificationDelegate(JsonRpcEventUtils.ToString(rpcEvent), notification);

        /// <summary>
        /// Removes the given notification method from the given event.
        /// </summary>
        /// <param name="rpcEvent">The event the method should be removed from.</param>
        /// <param name="notification">The method to remove.</param>
        public void RemoveNotificationDelegate(string rpcEvent, JsonRpcMessageEventHandler notification)
            => _removeDelegate(_notificationDict, _oneTimeNotification, rpcEvent, notification);

        /// <summary>
        /// Removes the given request method from the given event.
        /// </summary>
        /// <param name="rpcEvent">The event the method should be removed from.</param>
        /// <param name="request">The method to remove.</param>
        public void RemoveRequestDelegate(JsonRpcEvent rpcEvent, JsonRpcMessageEventHandler request)
            => RemoveRequestDelegate(JsonRpcEventUtils.ToString(rpcEvent), request);

        /// <summary>
        /// Removes the given request method from the given event.
        /// </summary>
        /// <param name="rpcEvent">The event the method should be removed from.</param>
        /// <param name="request">The method to remove.</param>
        public void RemoveRequestDelegate(string rpcEvent, JsonRpcMessageEventHandler request)
            => _removeDelegate(_requestDict, _oneTimeRequests, rpcEvent, request);

        /// <summary>
        /// Removes the given answer method from the given event.
        /// </summary>
        /// <param name="rpcEvent">The event the method should be removed from.</param>
        /// <param name="answer">The method to remove.</param>
        public void RemoveAnswerDelegate(JsonRpcEvent rpcEvent, JsonRpcMessageEventHandler answer)
            => RemoveAnswerDelegate(JsonRpcEventUtils.ToString(rpcEvent), answer);

        /// <summary>
        /// Removes the given answer method from the given event.
        /// </summary>
        /// <param name="rpcEvent">The event the method should be removed from.</param>
        /// <param name="answer">The method to remove.</param>
        public void RemoveAnswerDelegate(string rpcEvent, JsonRpcMessageEventHandler answer)
            => _removeDelegate(_answerDict, _oneTimeAnswers, rpcEvent, answer);

        /// <summary>
        /// Removes the given method delegate from the given dicts.
        /// </summary>
        /// <param name="methodDict">The dict where all registered delegates are stored.</param>
        /// <param name="oneTimeDict">The dict that defines if a delegate should only be fired one time.</param>
        /// <param name="rpcEvent">The event the method should be removed from.</param>
        /// <param name="method">The method to remove.</param>
        private void _removeDelegate(Dictionary<string, List<JsonRpcMessageEventHandler>> methodDict, Dictionary<string, List<JsonRpcMessageEventHandler>> oneTimeDict, string rpcEvent, JsonRpcMessageEventHandler method)
        {
            if (!methodDict.ContainsKey(rpcEvent))
                return;

            if (!methodDict[rpcEvent].Contains(method))
                return;

            if (oneTimeDict[rpcEvent].Contains(method))
                oneTimeDict[rpcEvent].Remove(method);
            methodDict[rpcEvent].Remove(method);
        }
        #endregion

        #region Send Methods
        public void SendNotification(JsonRpcMessage message, ClientConnectionType? ccType = null, string connectionName = null)
        {
            if (ConnectionType == "server")
                _sendServerNotification(message, ccType, connectionName);
            else if (ConnectionType == "client")
                _sendClientNotification(message, ccType, connectionName);
        }

        /// <summary>
        /// Sends a new request message over the websocket.
        /// </summary>
        /// <param name="message">The json rpc message to send.</param>
        /// <param name="ccType">The connection type to send this message to.</param>
        /// <param name="connectionName">The connection name to send this message to.</param>
        public void SendRequest(JsonRpcMessage message, ClientConnectionType? ccType = null, string connectionName = null)
        {
            if (ConnectionType == "server")
                _sendServerRequest(message, ccType, connectionName);
            else if (ConnectionType == "client")
                _sendClientRequest(message, ccType, connectionName);
        }

        /// <summary>
        /// Sends a new answer message over the websocket.
        /// </summary>
        /// <param name="message">The json rpc message to send.</param>
        /// <param name="ccType">The connection type to send this message to. Will not be used if the ConnectionType is "client".</param>
        /// <param name="connectionName">The connection name to send this message to. Will not be used if the ConnectionType is "client".</param>
        public void SendAnswer(JsonRpcMessage message, ClientConnectionType? ccType = null, string connectionName = null)
        {
            if (ConnectionType == "server")
                _sendServerAnswer(message, ccType, connectionName);
            else if (ConnectionType == "client")
                _sendClientAnswer(message);
        }

        /// <summary>
        /// Sends a new notification message over the websocket server.
        /// </summary>
        /// <param name="message">The json rpc message to send.</param>
        /// <param name="ccType">The connection type to send this message to.</param>
        /// <param name="connectionName">The connection name to send this message to.</param>
        private void _sendServerNotification(JsonRpcMessage message, ClientConnectionType? ccType = null, string connectionName = null)
        {
            if (message == null)
                return;

            JsonRpcCommunication communication = JsonRpcCommunication.CommunicationFrom(
                name: null,
                ccType: ClientConnectionType.Server
            );
            message.SetCommunication(communication);

            WebSocketServerWrapper.Instance.Send(message, ccType, connectionName);
        }

        /// <summary>
        /// Sends a new request message over the websocket server.
        /// </summary>
        /// <param name="message">The json rpc message to send.</param>
        /// <param name="ccType">The connection type to send this message to.</param>
        /// <param name="connectionName">The connection name to send this message to.</param>
        private void _sendServerRequest(JsonRpcMessage message, ClientConnectionType? ccType = null, string connectionName = null)
        {
            if (message == null)
                return;

            JsonRpcCommunication communication = JsonRpcCommunication.CommunicationFrom(
                name: null,
                ccType: ClientConnectionType.Server
            );
            message.SetCommunication(communication);
            message.SetId(NextId);

            WebSocketServerWrapper.Instance.Send(message, ccType, connectionName);
        }

        /// <summary>
        /// Sends a new answer message over the websocket server.
        /// </summary>
        /// <param name="message">The json rpc message to send.</param>
        /// <param name="ccType">The connection type to send this message to.</param>
        /// <param name="connectionName">The connection name to send this message to.</param>
        private void _sendServerAnswer(JsonRpcMessage message, ClientConnectionType? ccType = null, string connectionName = null)
        {
            if (message == null)
                return;

            JsonRpcMessage newMessage = message.CreateAnswerMessage(server: true);
            WebSocketServerWrapper.Instance.Send(newMessage, ccType, connectionName);
        }

        /// <summary>
        /// Sends a new notification message over the websocket client.
        /// </summary>
        /// <param name="message">The json rpc message to send.</param>
        /// <param name="ccType">The connection type to send this message to.</param>
        /// <param name="connectionName">The connection name to send this message to.</param>
        private void _sendClientNotification(JsonRpcMessage message, ClientConnectionType? ccType = null, string connectionName = null)
        {
            if (message == null)
                return;

            JsonRpcCommunication communication = JsonRpcCommunication.CommunicationTo(
                name: connectionName,
                ccType: ccType
            );
            message.SetCommunication(communication);

            WebSocketClientWrapper.Instance.Send(message);
        }

        /// <summary>
        /// Sends a new request message over the websocket client.
        /// </summary>
        /// <param name="message">The json rpc message to send.</param>
        /// <param name="ccType">The connection type to send this message to.</param>
        /// <param name="connectionName">The connection name to send this message to.</param>
        private void _sendClientRequest(JsonRpcMessage message, ClientConnectionType? ccType = null, string connectionName = null)
        {
            if (message == null)
                return;

            JsonRpcCommunication communication = JsonRpcCommunication.CommunicationTo(
                name: connectionName,
                ccType: ccType
            );
            message.SetCommunication(communication);
            message.SetId(NextId);

            WebSocketClientWrapper.Instance.Send(message);
        }

        /// <summary>
        /// Sends a new answer message over the websocket client.
        /// </summary>
        /// <param name="message">The json rpc message to send.</param>
        private void _sendClientAnswer(JsonRpcMessage message)
        {
            if (message == null)
                return;

            JsonRpcMessage newMessage = message.CreateAnswerMessage(server: false);
            WebSocketClientWrapper.Instance.Send(newMessage);
        }
        #endregion
    }
}
