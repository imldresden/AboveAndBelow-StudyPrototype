using SharpOSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterNetworking.Osc
{
    public class OscServer
    {
        #region Singleton
        private static OscServer _instance;
        public static OscServer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OscServer();
                return _instance;
            }
        }

        private OscServer()
        {

        }
        #endregion

        private UDPSender _oscSender;

        public event OscEventHandler Started;
        public event OscEventHandler Stopped;

        #region Lifecycle Methods
        /// <summary>
        /// Starts the osc server.
        /// </summary>
        /// <param name="address">The address this server should be available from.</param>
        /// <param name="port">The port this server should be available on.</param>
        public void Start(string address, int port)
        {
            if (_oscSender != null)
                return;

            _oscSender = new UDPSender(address, port);

            Started?.Invoke(null);
        }

        /// <summary>
        /// Stops this osc server.
        /// </summary>
        public void Stop()
        {
            if (_oscSender == null)
                return;

            _oscSender.Close();
            _oscSender = null;

            Stopped?.Invoke(null);
        }
        #endregion

        /// <summary>
        /// Sends the given message in form of a rigidbody to the client.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to send.</param>
        public void Send(Rigidbody rigidbody)
        {
            OscMessage oscMessage = new OscMessage(
                address: "/tracking/optitrack/rigidbodies",
                args: rigidbody.ToOscMessageObject()
            );

            _oscSender.Send(oscMessage);
        }
    }
}
