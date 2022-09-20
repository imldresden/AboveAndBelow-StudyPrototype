using UnityEngine;

namespace BasicCode.Utils
{
    /// <summary>
    /// This is a pseude singleton class which can provide the user with global references of important objects.
    /// </summary>
    public class NetworkingGlobal : MonoBehaviour
    {
        #region Pseudo-Singleton
        public static NetworkingGlobal Instance { get; private set; }

        public NetworkingGlobal()
        {
            Instance = this;
        }
        #endregion

        public bool WebSocketErrorHandling;

        [Space(5)]
        public WebSocketServerBehaviour WebSocketServerBehaviour;
        public WebSocketClientBehaviour WebSocketClientBehaviour;
    }
}
