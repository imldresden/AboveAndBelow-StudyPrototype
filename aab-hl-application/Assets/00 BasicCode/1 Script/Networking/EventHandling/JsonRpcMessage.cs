using MasterNetworking.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace MasterNetworking.EventHandling
{
    /// <summary>
    /// Enum that defines the json rpcs message type.
    /// </summary>
    public enum JsonRpcMessageType
    {
        Notification,
        Request,
        Answer
    }

    /// <summary>
    /// Represents a json rpc message.
    /// </summary>
    public class JsonRpcMessage
    {
        /// <summary>
        /// The message type for this message.
        /// </summary>
        public JsonRpcMessageType MessageType { get; private set; }

        /// <summary>
        /// The version number of the message.
        /// </summary>
        public string Version { get; private set; } = "2.0";
        /// <summary>
        /// The id of this message. If this is null, this will only handled as a notification.
        /// </summary>
        public int? Id { get; private set; } = null;
        /// <summary>
        /// The data of this message. The structure depends on the message type.
        /// </summary>
        public JObject Data { get; private set; }
        /// <summary>
        /// The method this message should be calling.
        /// </summary>
        public string Method { get; private set; }
        /// <summary>
        /// The communiction argument.
        /// If this is null, the message should be sed to all clients connected to a server
        /// </summary>
        public JsonRpcCommunication Communication { get; private set; } = null;
        /// <summary>
        /// An extra flag that can be set to signalize that the data of this message should be logged.
        /// </summary>
        public string LoggingMethod { get; private set; }

        /// <summary>
        /// Creates new JsonRpcMessage.
        /// </summary>
        /// <param name="data">The data this message should transport.</param>
        /// <param name="method">The method this message should call.</param>
        /// <param name="logging">Should this message be logged on the server?</param>
        public JsonRpcMessage(JObject data, JsonRpcEvent method, string loggingMethod = null)
            : this(data, JsonRpcEventUtils.ToString(method), loggingMethod) { }

        /// <summary>
        /// Creates new JsonRpcMessage.
        /// </summary>
        /// <param name="data">The data this message should transport.</param>
        /// <param name="method">The method this message should call.</param>
        public JsonRpcMessage(JObject data, string method, string loggingMethod = null)
        {
            Data = data;
            Method = method;
            LoggingMethod = loggingMethod;

            MessageType = JsonRpcMessageType.Notification;
        }

        #region Setter
        /// <summary>
        /// Initialize this id of this message.
        /// </summary>
        /// <param name="id"></param>
        public void SetId(int id)
        {
            if (Id != null)
                return;

            Id = id;
            if (Id != null)
                MessageType = JsonRpcMessageType.Request;
        }

        /// <summary>
        /// Initializes the communication value of this message.
        /// </summary>
        /// <param name="communication"></param>
        public void SetCommunication(JsonRpcCommunication communication)
        {
            if (Communication != null)
                return;

            Communication = communication;
        }
        #endregion

        #region Deserialization
        /// <summary>
        /// Creates a message object.
        /// </summary>
        /// <param name="message">The string message in json format.</param>
        /// <returns>The created message. Null if no message could be extracted.</returns>
        public static JsonRpcMessage DeserializeMessage(string message)
        {
            try
            {
                return DeserializeMessage(JObject.Parse(message));
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"!!! Could not parse string to JObject: {message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a message object.
        /// </summary>
        /// <param name="message">The jObject represntation of the message.</param>
        /// <returns>The created message. Null if no message could be extracted.</ret
        public static JsonRpcMessage DeserializeMessage(JObject message)
        {
            if (message["jsonrpc"].ToString() != "2.0")
                return null;

            try
            {
                JObject data = null;
                string method = null;
                JsonRpcCommunication communication = null;
                string loggingMethod = null;

                if (message["params"] != null && message["params"]?.Type != JTokenType.Null)
                {
                    data = message["params"]["data"] as JObject;
                    method = message["method"].ToString();
                    communication = JsonRpcCommunication.FromJObject(message["params"]["communication"] as JObject);
                    if (message["params"]["loggingMethod"] != null && message["params"]["loggingMethod"]?.Type != JTokenType.Null)
                        loggingMethod = message["params"]["loggingMethod"].ToString();
                }
                else if (message["result"] != null && message["result"]?.Type != JTokenType.Null)
                {
                    data = message["result"]["data"] as JObject;
                    method = message["result"]["method"].ToString();
                    communication = JsonRpcCommunication.FromJObject(message["result"]["communication"] as JObject);
                    if (message["result"]["loggingMethod"] != null && message["result"]["loggingMethod"]?.Type != JTokenType.Null)
                        loggingMethod = message["result"]["loggingMethod"].ToString();
                }
                if (data == null || method == null || communication == null)
                    return null;

                JsonRpcMessage jsonMessage = new JsonRpcMessage(
                    data: data,
                    method: method,
                    loggingMethod: loggingMethod
                );
                jsonMessage.SetCommunication(communication);
                if (message["id"] != null && message["id"]?.Type != JTokenType.Null)
                    jsonMessage.SetId(message["id"].ToObject<int>());

                return jsonMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! Could nor correctly parse the given JObject to a json rpc message: {message}");
                return null;
            }
        }
        #endregion

        #region Message Creation Methods
        /// <summary>
        /// Creates a new message based on this message.
        /// It will use the information from the service this message was coming from.
        /// </summary>
        /// <param name="service">The service the original message was coming from.</param>
        /// <returns>The new message.</returns>
        public JsonRpcMessage CreateTunnelMessage(JsonRpcService service)
        {
            JsonRpcMessage newMessage = new JsonRpcMessage(
                data: Data,
                method: Method
            );
            JsonRpcCommunication communication = JsonRpcCommunication.CommunicationFrom(
                name: service.Name,
                ccType: service.ClientConnectionType
            );
            newMessage.SetCommunication(communication);
            if (Id.HasValue)
                newMessage.SetId(Id.Value);

            return newMessage;
        }

        /// <summary>
        /// Creates a new answer based on this message.
        /// </summary>
        /// <param name="data">The data for the answer.</param>
        /// <param name="server">Decides if the communication will be generated based on this message. If true, the communaction will be replaced by infos for the server.</param>
        /// <returns>The new message.</returns>
        public JsonRpcMessage CreateAnswerMessage(JObject data = null, bool server = false)
        {
            // If no id is given, no answer should be generated.
            if (!Id.HasValue)
                return null;

            // Checks if the answer should come from the server or from an client.
            JsonRpcCommunication communication;
            if (server)
                communication = JsonRpcCommunication.CommunicationFrom(
                    name: null,
                    ccType: ClientConnectionType.Server
                );
            // If the message should come from a client, this will prepare it to be tunneled through a server.
            else
                communication = JsonRpcCommunication.CommunicationTo(
                    name: Communication.Name,
                    ccType: Communication.ClientConnectionType
                );

            JsonRpcMessage newMessage = new JsonRpcMessage(
                data: Data,
                method: Method
            );
            newMessage.SetCommunication(communication);
            newMessage.MessageType = JsonRpcMessageType.Answer;
            newMessage.SetId(Id.Value);

            return newMessage;
        }
        #endregion

        #region JObject casting
        /// <summary>
        /// Transforms this message object into an JObjects.
        /// </summary>
        /// <returns>The JObject representation.</returns>
        public JObject ToJObject()
        {
            switch (MessageType)
            {
                case JsonRpcMessageType.Notification:   return _notificationJObject();
                case JsonRpcMessageType.Request:        return _requestJObject();
                case JsonRpcMessageType.Answer:         return _answerJObject();
                default:                                return null;
            }
        }

        /// <summary>
        /// Transforms this message into a notification message JObject.
        /// </summary>
        /// <returns>The JObject representation.</returns>
        private JObject _notificationJObject()
        {
            JObject jObject = new JObject(
                new JProperty("jsonrpc", Version),
                new JProperty("method", Method),
                new JProperty("params", new JObject(
                    new JProperty("data", Data),
                    new JProperty("communication", Communication.ToJObject()),
                    new JProperty("loggingMethod", LoggingMethod)
                ))
            );

            return jObject;
        }

        /// <summary>
        /// Transforms this message into a request message JObject.
        /// </summary>
        /// <returns>The JObject representation.</returns>
        private JObject _requestJObject()
        {
            JObject jObject = new JObject(
                new JProperty("jsonrpc", Version),
                new JProperty("id", Id),
                new JProperty("method", Method),
                new JProperty("params", new JObject(
                    new JProperty("data", Data),
                    new JProperty("communication", Communication.ToJObject()),
                    new JProperty("loggingMethod", LoggingMethod)
                ))
            );

            return jObject;
        }

        /// <summary>
        /// Transforms this message into a answer message JObject.
        /// </summary>
        /// <returns>The JObject representation.</returns>
        private JObject _answerJObject()
        {
            JObject jObject = new JObject(
                new JProperty("jsonrpc", Version),
                new JProperty("id", Id),
                new JProperty("result", new JObject(
                    new JProperty("method", Method),
                    new JProperty("data", Data),
                    new JProperty("communication", Communication.ToJObject()),
                    new JProperty("loggingMethod", LoggingMethod)
                ))
            );

            return jObject;
        } 
        #endregion
    }
}
