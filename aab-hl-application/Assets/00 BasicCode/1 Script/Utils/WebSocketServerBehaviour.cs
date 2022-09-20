using MasterNetworking.EventHandling;
using MasterNetworking.WebSocket;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BasicCode.Utils
{
    public class WebSocketServerBehaviour : MonoBehaviour
    {
        private Queue<Action> _messageQueue = new Queue<Action>();
        [SerializeField]
        private bool _started = false;

        [Space(5)]
        public int Port = 5000;
        [Space(5)]
        public List<string> ConnectionNames;
        public List<ClientConnectionType> DeviceTypes;

        #region Unity Methods
        protected void Update()
        {
#if UNITY_EDITOR
            ConnectionNames = WebSocketServerWrapper.Instance.ConnectionNames;
            DeviceTypes = WebSocketServerWrapper.Instance.ClientConnectionTypes;
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

            WebSocketServerWrapper.Instance.StopServer();
            JsonRpcHandler.Instance.DisconnectFromServer();
            _started = false;
        }
        #endregion

        public void StartServer()
        {
            WebSocketServerWrapper.Instance.Started += _websocketServer_Started;
            WebSocketServerWrapper.Instance.StartServer(Port, messageQueue: _messageQueue);
            JsonRpcHandler.Instance.ConnectToServer();
        }

        private void _websocketServer_Started(string message, JsonRpcService service = null)
        {
            _started = true;
        }
    }
}
