using SharpOSC;

namespace MasterNetworking.Osc
{
    public delegate void OscEventHandler(Rigidbody rigidbody);

    public class OscClient
    {
        #region Singleton
        private static OscClient _instance;
        public static OscClient Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OscClient();
                return _instance;
            }
        }

        private OscClient()
        {

        } 
        #endregion

        private UDPListener _oscListener;

        public event OscEventHandler Started;
        public event OscEventHandler Stopped;
        public event OscEventHandler Messaged;

        #region Lifecycle Methods
        /// <summary>
        /// Starts the osc client.
        /// </summary>
        /// <param name="port">The port to connect this client to.</param>
        public void Start(int port)
        {
            if (_oscListener != null)
                return;

            _oscListener = new UDPListener(port, _onOscMessage);

            Started?.Invoke(null);
        }

        /// <summary>
        /// Stops this osc client.
        /// </summary>
        public void Stop()
        {
            if (_oscListener == null)
                return;

            _oscListener.Close();
            _oscListener = null;

            Stopped?.Invoke(null);
        } 
        #endregion

        /// <summary>
        /// Called when a new osc message was received.
        /// </summary>
        /// <param name="packet"></param>
        private void _onOscMessage(OscPacket packet)
        {
            foreach (OscMessage message in (packet as OscBundle).Messages)
            {
                Rigidbody rigidbody = new Rigidbody(message);

                Messaged?.Invoke(rigidbody);
            }
        }
    }
}
