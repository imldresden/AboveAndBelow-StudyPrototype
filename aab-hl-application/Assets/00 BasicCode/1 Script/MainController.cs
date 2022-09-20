using BasicCode.Utils;
using UnityEngine;

namespace BasicCode
{
    public class MainController : MonoBehaviour
    {
        public bool StartServer = false;
        public bool StartClient = false;

        #region Unity Methods
        protected void Start()
        {
            if (StartServer)
                NetworkingGlobal.Instance.WebSocketServerBehaviour.StartServer();
            if (StartClient)
                NetworkingGlobal.Instance.WebSocketClientBehaviour.StartClient();
        }
        #endregion
    }
}
