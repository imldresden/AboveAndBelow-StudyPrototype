using MasterNetworking.EventHandling;
using MasterNetworking.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BasicCode.Utils
{
    public class WebSocketClientBehaviour : MonoBehaviour
    {
        private Queue<Action> _messageQueue = new Queue<Action>();
        [SerializeField]
        private bool _started = false;

        [Space(5)]
        public string Ip = "localhost";
        public int Port = 5000;
        public string Address;
        [Space(5)]
        public string ConnectionName;
        public ClientConnectionType ClientConnectionType;
        [Space(5)]
        public WsConnectionStatus Status;

        #region Unity Methods
        protected void Update()
        {
#if UNITY_EDITOR
            Status = WebSocketClientWrapper.Instance.Status;
#endif

            while (_messageQueue.Count > 0)
            {
                if (NetworkingGlobal.Instance.WebSocketErrorHandling)
                {
                    try
                    {
                        _messageQueue.Dequeue()?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex.Message);
                    }

                }
                else 
                    _messageQueue.Dequeue()?.Invoke();
            }
        }

        protected void OnDestroy()
        {
            if (!_started)
                return;

            WebSocketClientWrapper.Instance.StopClient();
            JsonRpcHandler.Instance.DisconnectFromClient();
            _started = false;
        } 
#endregion

        public void StartClient()
        {
            WebSocketClientWrapper.Instance.ConnectionOpened += _websocketClient_ConnectionOpened;

            WebSocketClientWrapper.Instance.StartClient(Ip, Port, Address, messageQueue: _messageQueue);
            JsonRpcHandler.Instance.ConnectToClient();
            WebSocketClientWrapper.Instance.ConnectClient();

            _started = true;
            Debug.Log("Server Started");
        }

        private void _websocketClient_ConnectionOpened(string message)
        {
            JsonRpcMessage jsonMessage = new JsonRpcMessage(
                method: JsonRpcEvent.DeviceRegistration,
                data: new JObject(
                    new JProperty("name", ConnectionName),
                    new JProperty("clientConnectionType", ClientConnectionTypeUtils.ToString(ClientConnectionType))
                )
            );

            JsonRpcHandler.Instance.SendRequest(
                message: jsonMessage,
                ccType: ClientConnectionType.Server
            );
        }
    }
}
