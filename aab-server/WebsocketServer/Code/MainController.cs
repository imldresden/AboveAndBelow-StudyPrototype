using MasterNetworking.EventHandling;
using MasterNetworking.NatNet;
using MasterNetworking.Osc;
using MasterNetworking.Utils;
using MasterNetworking.WebServer;
using MasterNetworking.WebSocket;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;

namespace MasterNetworking
{
    public struct ServerSetupValues
    {
        public bool Input_NatNet;
        public string NatNet_LocalIp;
        public string NatNet_MotiveIp;
        public int NatNet_MotiveType;

        public bool Output_Osc;
        public string OscOutput_Ip;
        public int OscOutput_Port;

        public bool Input_Osc;
        public int OscInput_Port;

        public bool Output_Websocket;
        public int Websocket_Port;

        public bool Servive_Website;
        public int Website_Port;
    }

    public struct LoggingSetupValues
    {
        public enum LoggingDetails {
            Everything,
            LoggingMessages,
            None
        }

        public string Prefix;
        public LoggingDetails WebLogLog;
    }

    public class MainController
    {
        private Dispatcher _dispatcher;

        public Logger GeneralLogger { get; private set; }
        public Logger WebsocketRawLogger { get; private set; }
        public Logger WebsocketLogMessageLogger { get; private set; }
        public Dictionary<string, Logger> OtherLoggers { get; private set; }
        public bool Started { get; private set; }
        public ServerSetupValues ServerSetupValues { get; private set; }
        public LoggingSetupValues LoggingSetupValues;

        public MainController(Dispatcher dispatcher)
        {
            if (dispatcher == null)
                return;

            _dispatcher = dispatcher;

            LoggingSetupValues = new LoggingSetupValues()
            {
                Prefix = "",
                WebLogLog = LoggingSetupValues.LoggingDetails.None
            };

            GeneralLogger = new Logger("out", "GeneralLog.csv", new string[] { "info" });
            GeneralLogger.Start();

            OtherLoggers = new Dictionary<string, Logger>();

            OscClient.Instance.Started += _oscClient_Started;
            OscClient.Instance.Stopped += _oscClient_Stopped;

            OscServer.Instance.Started += _oscServer_Started;
            OscServer.Instance.Stopped += _oscServer_Stopped;

            WebSocketServerWrapper.Instance.Started += _websocket_Started;
            WebSocketServerWrapper.Instance.Stopped += _websocket_Stopped;
            WebSocketServerWrapper.Instance.OnError += _websocket_OnError;
            WebSocketServerWrapper.Instance.ConnectionOpened += _websocket_ConnectionOpened;
            WebSocketServerWrapper.Instance.ConnectionClosed += _websocket_ConnectionClosed;
            WebSocketServerWrapper.Instance.DeviceRegistrated += _websocket_DeviceRegistrated;
            JsonRpcHandler.Instance.MessageReceived += _websocket_MessageReceived;
            JsonRpcHandler.Instance.MessageSend += _websocket_MessageSend;

            NatNetClient.Instance.Started += _natNet_Started;
            NatNetClient.Instance.Stopped += _natNet_Stopped;
            NatNetClient.Instance.ConnectionOpened += _natNet_ConnectionOpened;
            NatNetClient.Instance.ConnectionClosed += _natNet_ConnectionClosed;
            NatNetClient.Instance.ConnectionError += _natNet_ConnectionError;

            WebServerWrapper.Instance.Started += _webserver_Started;
            WebServerWrapper.Instance.Stopped += _webserver_Stopped;

            _serverSpecificStart();

            Started = false;
        }

        #region Lifecycle Methods
        /// <summary>
        /// Starts this controller.
        /// It will start all servers/clients accordingly to the given values in the WPF form.
        /// </summary>
        /// <param name="serverSetupValues">The input values from the form.</param>
        public void Start(ServerSetupValues serverSetupValues)
        {
            ServerSetupValues = serverSetupValues;

            _startLogger();

            if (serverSetupValues.Input_NatNet)
            {
                NatNetClient.Instance.RigidBodyReceived += _natNet_RigidBodyReceived;
                NatNetClient.Instance.StartClient(serverSetupValues.NatNet_MotiveType);
                NatNetClient.Instance.ConnectClient(serverSetupValues.NatNet_LocalIp, serverSetupValues.NatNet_MotiveIp);
            } 
            else if (serverSetupValues.Input_Osc)
            {
                OscClient.Instance.Messaged += _oscServer_Messaged;
                OscClient.Instance.Start(serverSetupValues.OscInput_Port);
            }

            if (serverSetupValues.Output_Osc)
                OscServer.Instance.Start(serverSetupValues.OscOutput_Ip, serverSetupValues.OscOutput_Port);

            if (serverSetupValues.Output_Websocket)
            {
                WebSocketServerWrapper.Instance.StartServer(serverSetupValues.Websocket_Port);
                JsonRpcHandler.Instance.ConnectToServer();

                JsonRpcHandler.Instance.AddRequestDelegate(JsonRpcEvent.EmulatedOptiTrackMessage, _onRequest_EmulatedOptiTrackMessage);
            }

            if (serverSetupValues.Servive_Website)
            {
                WebServerWrapper.Instance.StartServer(serverSetupValues.Website_Port);
                WebServerWrapper.Instance.SetApiInformation(
                    wsUrl: Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Last().ToString(), 
                    wsPort: serverSetupValues.Websocket_Port
                );
            }

            Started = true;
        }

        /// <summary>
        /// Stops this controller and all connected servers/clients.
        /// </summary>
        public void Stop()
        {
            if (ServerSetupValues.Input_NatNet)
            {
                NatNetClient.Instance.RigidBodyReceived -= _natNet_RigidBodyReceived;
                NatNetClient.Instance.StopClient();
            }
            else if (ServerSetupValues.Input_Osc)
            {
                OscClient.Instance.Messaged -= _oscServer_Messaged;
                OscClient.Instance.Stop();
            }

            if (ServerSetupValues.Output_Osc)
                OscServer.Instance.Stop();

            if (ServerSetupValues.Output_Websocket)
            {
                WebSocketServerWrapper.Instance.StopServer();
                JsonRpcHandler.Instance.DisconnectFromServer();

                JsonRpcHandler.Instance.RemoveRequestDelegate(JsonRpcEvent.EmulatedOptiTrackMessage, _onRequest_EmulatedOptiTrackMessage);
            }

            if (ServerSetupValues.Servive_Website)
                WebServerWrapper.Instance.StopServer();

            _stopLogger();

            Started = false;
        }

        /// <summary>
        /// Starts the logger.
        /// </summary>
        private void _startLogger()
        {
            string prefix = LoggingSetupValues.Prefix == "" ? "" : $"{LoggingSetupValues.Prefix}-";

            WebsocketRawLogger = new Logger("out", $"{prefix}WebsocketRawLog.csv", new string[] { "info" });
            WebsocketRawLogger.Start();

            WebsocketLogMessageLogger = new Logger("out", $"{prefix}WebsocketLogMessageLog.csv", new string[] { "from", "data" });
            WebsocketLogMessageLogger.Start();
        }

        /// <summary>
        /// Stops the logger.
        /// </summary>
        private void _stopLogger()
        {
            WebsocketRawLogger.Stop();
            WebsocketRawLogger = null;

            WebsocketLogMessageLogger.Stop();
            WebsocketLogMessageLogger = null;
        }
        #endregion

        #region NatNet Events
        private void _natNet_Started(Rigidbody rigidbody) => GeneralLogger.Write("Started: NatNet");
        private void _natNet_Stopped(Rigidbody rigidbody) => GeneralLogger.Write("Stopped: NatNet");
        private void _natNet_ConnectionOpened(Rigidbody rigidbody) => GeneralLogger.Write("Connection Opened: NatNet");
        private void _natNet_ConnectionError(Rigidbody rigidbody) => GeneralLogger.Write("Connection Error : NatNet");
        private void _natNet_ConnectionClosed(Rigidbody rigidbody) => GeneralLogger.Write("Connection Closed: NatNet");

        /// <summary>
        /// Called when the natnet server received a message.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that was send over this message.</param>
        private void _natNet_RigidBodyReceived(Rigidbody rigidbody)
        {
            // This will send the rigidbody right through the websocket server.
            if (ServerSetupValues.Output_Websocket)
            {
                JsonRpcMessage message = new JsonRpcMessage(
                    method: JsonRpcEventUtils.ToString(JsonRpcEvent.OptiTrackMessage),
                    data: new JObject(
                        new JProperty("rigidbody", rigidbody.ToJObject())
                    )
                );

                // #TODO Decide to which connection to send this message?
                JsonRpcHandler.Instance.SendNotification(
                    message: message
                );
            }

            // If the osc server is also active, this will tunnel those information right through the server as well.
            if (ServerSetupValues.Output_Osc)
            {
                OscServer.Instance.Send(rigidbody);
            }
        }
        #endregion

        #region Osc Server Events
        private void _oscServer_Started(Rigidbody rigidbody) => GeneralLogger.Write("Started: OSC Server");
        private void _oscServer_Stopped(Rigidbody rigidbody) => GeneralLogger.Write("Stopped: OSC Server");
        #endregion

        #region Osc Client Events
        private void _oscClient_Started(Rigidbody rigidbody) => GeneralLogger.Write("Started: OSC Client");
        private void _oscClient_Stopped(Rigidbody rigidbody) => GeneralLogger.Write("Stopped: OSC Client");

        /// <summary>
        /// Called when the osc server received a message.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that was send over this message.</param>
        private void _oscServer_Messaged(Rigidbody rigidbody)
        {
            // This will send the rigidbody right through the websocket server.
            if (!ServerSetupValues.Output_Websocket)
                return;

            JsonRpcMessage message = new JsonRpcMessage(
                method: JsonRpcEvent.OptiTrackMessage,
                data: new JObject(
                    new JProperty("rigidbody", rigidbody.ToJObject())
                )
            );

            // #TODO Decide to which connection to send this message?
            JsonRpcHandler.Instance.SendNotification(
                message: message
            );
        }
        #endregion

        #region Websocket Events
        private void _websocket_Started(string message, JsonRpcService service = null) => GeneralLogger.Write("Started: WebSocket");
        private void _websocket_Stopped(string message, JsonRpcService service = null) => GeneralLogger.Write("Stopped: WebSocket");
        private void _websocket_OnError(string message, JsonRpcService service = null) => GeneralLogger.Write("Error  : WebSocket");
        private void _websocket_ConnectionOpened(string message, JsonRpcService service = null) => WebsocketRawLogger.Write($"Connection Opened: {message}");
        private void _websocket_ConnectionClosed(string message, JsonRpcService service = null) => WebsocketRawLogger.Write($"Connection Closed: {message}");
        private void _websocket_DeviceRegistrated(string message, JsonRpcService service = null) => WebsocketRawLogger.Write($"Device Registrated: {service.Context.UserEndPoint} as {message}");
        private void _websocket_MessageReceived(JsonRpcMessage jsonMessage, string communicationPartner)
        {
            if (LoggingSetupValues.WebLogLog == LoggingSetupValues.LoggingDetails.None)
                return;
            if (jsonMessage.Method == JsonRpcEventUtils.ToString(JsonRpcEvent.OptiTrackMessage))
                return;

            if (LoggingSetupValues.WebLogLog == LoggingSetupValues.LoggingDetails.Everything)
                WebsocketRawLogger.Write($"WS <<< {jsonMessage.ToJObject().ToString().Replace(System.Environment.NewLine, "")}");

            string logger;
            if (jsonMessage.Method.Contains("Logging"))
                logger = jsonMessage.Method;
            else if (jsonMessage.LoggingMethod != null)
                logger = jsonMessage.LoggingMethod;
            else
                return;

            if (!OtherLoggers.ContainsKey(logger))
            {
                OtherLoggers.Add(
                    logger,
                    new Logger("out", $"{logger}.csv", new string[] { "data" })
                );
                OtherLoggers[logger].Start();
            }

            OtherLoggers[logger].Write(jsonMessage.Data.ToString().Replace(System.Environment.NewLine, ""));
        }

        private void _websocket_MessageSend(JsonRpcMessage jsonMessage, string communicationPartner)
        {
            if (LoggingSetupValues.WebLogLog != LoggingSetupValues.LoggingDetails.Everything)
                return;
            if (jsonMessage.Method == JsonRpcEventUtils.ToString(JsonRpcEvent.OptiTrackMessage))
                return;

            WebsocketRawLogger.Write($"WS >>> {jsonMessage.ToJObject().ToString().Replace(System.Environment.NewLine, "")}");
        }
        #endregion

        #region Webserver Events
        private void _webserver_Started() => GeneralLogger.Write("Started: Webserver");
        private void _webserver_Stopped() => GeneralLogger.Write("Stopped: Webserver"); 
        #endregion

        #region Emulated Events
        /// <summary>
        /// Called when an emulated opti track message comes i.
        /// </summary>
        /// <param name="message">The request that was received.</param>
        private void _onRequest_EmulatedOptiTrackMessage(JsonRpcMessage message)
        {
            JsonRpcMessage newMessage = new JsonRpcMessage(
                method: JsonRpcEvent.OptiTrackMessage,
                data: message.Data
            );

            // #TODO Decide to which connection to send this message?
            JsonRpcHandler.Instance.SendNotification(
                message: newMessage
            );
        }
        #endregion

        #region A&B Server
        private Logger _trialLogger;

        private void _serverSpecificStart()
        {
            JsonRpcHandler.Instance.AddNotificationDelegate("StartExperiment", _onNotification_StartExperiment);
            JsonRpcHandler.Instance.AddNotificationDelegate("LogTrialData", _onNotification_LogTrialData);
        }

        private void _onNotification_StartExperiment(JsonRpcMessage message)
        {
            if (_trialLogger != null)
                _trialLogger.Stop();

            _trialLogger = new Logger("out", $"trialLog_{message.Data["subjectId"].ToString()}.csv", new string[]
            {
                "part",
                "trial",
                "area",
                "content",
                "size",
                "distance",
                "tilt"
            });
            _trialLogger.Start();
        }

        private void _onNotification_LogTrialData(JsonRpcMessage message)
        {
            _trialLogger.Write(
                message.Data["part"],
                message.Data["trial"],
                message.Data["area"],
                message.Data["content"],
                message.Data["size"],
                message.Data["distance"],
                message.Data["tilt"]
            );
        }
        #endregion
    }
}
