using WebSocketSharp;
using WebSocketSharp.Server;

namespace MasterNetworking.WebSocket
{
    /// <summary>
    /// A class that handles the connections for this application.
    /// Will be used as the server!
    /// </summary>
    public class JsonRpcService : WebSocketBehavior
    {
        public string Name { get; private set; } = null;
        public ClientConnectionType ClientConnectionType { get; private set; } = ClientConnectionType.NotDefined;

        protected override void OnOpen() => WebSocketServerWrapper.Instance.OpenConnection(this);
        protected override void OnClose(CloseEventArgs e) => WebSocketServerWrapper.Instance.CloseConnection(this);
        protected override void OnMessage(MessageEventArgs e) => WebSocketServerWrapper.Instance.HandleMessage(this, e.Data);
        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
        }

        // #TODO Check if we should use async calls here.
        public void SendMessage(string message)
        {
            if (this.ConnectionState != WebSocketState.Open)
                return;

            Send(message);
        }

        public bool Register(string name, ClientConnectionType ccType)
        {
            if (Name != null || ClientConnectionType != ClientConnectionType.NotDefined)
                return false;

            Name = name;
            ClientConnectionType = ccType;

            return true;
        }
    }
}
