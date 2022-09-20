using MasterNetworking.Osc;
using NatNetML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterNetworking.NatNet
{
    public enum NnConnectionStatus
    {
        Idle,
        Disconnected,
        Connected,
    }

    public delegate void NatNetEventHandler(Rigidbody rigidbody);

    public class NatNetClient
    {
        #region Singleton
        private static NatNetClient _instance;
        public static NatNetClient Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NatNetClient();
                return _instance;
            }
        }

        public static bool ExistsInstance => _instance != null;

        private NatNetClient()
        {

        }
        #endregion

        public event NatNetEventHandler Started;
        public event NatNetEventHandler Stopped;
        public event NatNetEventHandler ConnectionOpened;
        public event NatNetEventHandler ConnectionError;
        public event NatNetEventHandler ConnectionClosed;
        public event NatNetEventHandler RigidBodyReceived;

        public NnConnectionStatus Status { get; private set; }

        private NatNetClientML _client;

        private ServerDescription _serverDescription;
        private List<RigidBody> _rigidBodies = new List<RigidBody>();

        #region Lifecycle Methods
        /// <summary>
        /// Starts the NatNat client.
        /// </summary>
        /// <param name="connectionType">Defines the connection type. 0 for multi cast, 1 for unicast.</param>
        public void StartClient(int connectionType)
        {
            if (_client != null)
                return;

            _client = new NatNetClientML(connectionType);
            _client.OnFrameReady += _client_OnFrameReady;
            //_client.OnFrameReady2 += _client_OnFrameReady2;

            _serverDescription = new ServerDescription();

            Status = NnConnectionStatus.Disconnected;
            Started?.Invoke(null);
        }

        /// <summary>
        /// Stops the client.
        /// </summary>
        public void StopClient()
        {
            if (_client == null)
                return;

            DisconnectClient();
            _client.OnFrameReady -= _client_OnFrameReady;
            //_client.OnFrameReady2 -= _client_OnFrameReady2;
            _client = null;

            _serverDescription = null;

            Stopped?.Invoke(null);
        }

        /// <summary>
        /// Connects the client with the server.
        /// </summary>
        /// <param name="localAddress">The address where this connections is opened from.</param>
        /// <param name="serverAddress">The address of the server.</param>
        public void ConnectClient(string localAddress, string serverAddress)
        {
            if (Status == NnConnectionStatus.Connected)
                return;

            // Initialize the client.
            int returnCode = _client.Initialize(localAddress, serverAddress);
            if (returnCode != 0)
                return;

            // Check if the connection is valid.
            returnCode = _client.GetServerDescription(_serverDescription);
            if (returnCode != 0)
            {
                Status = NnConnectionStatus.Disconnected;
                ConnectionError?.Invoke(null);
                return;
            }

            // Get the description of the rigidbodies that will be send.
            List<DataDescriptor> descriptors = new List<DataDescriptor>();
            bool success = _client.GetDataDescriptions(out descriptors);
            if (success)
                foreach (DataDescriptor desc in descriptors)
                    if (desc.type == (int)DataDescriptorType.eRigidbodyData)
                        _rigidBodies.Add(desc as RigidBody);

            Status = NnConnectionStatus.Connected;
            ConnectionOpened?.Invoke(null);
        }

        /// <summary>
        /// Disconnects this client from the server.
        /// </summary>
        public void DisconnectClient()
        {
            if (Status != NnConnectionStatus.Connected)
                return;

            _client.Uninitialize();
            _rigidBodies.Clear();

            Status = NnConnectionStatus.Disconnected;

            ConnectionClosed?.Invoke(null);
        } 
        #endregion

        /// <summary>
        /// Called when a new frame of information was send from the server to this client.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        private void _client_OnFrameReady(FrameOfMocapData data, NatNetClientML client)
        {
            if (_rigidBodies.Count == 0)
                return;

            for (int i = 0; i < data.nRigidBodies; i++)
            {
                RigidBodyData rigidBodyData = data.RigidBodies[i];
                RigidBody rigidBody = _rigidBodies.Where(v => v.ID == rigidBodyData.ID).First();

                float[] eulers = _client.QuatToEuler(new float[4] { rigidBodyData.qx, rigidBodyData.qy, rigidBodyData.qz, rigidBodyData.qw }, (int)NATEulerOrder.NAT_XYZr);
                Rigidbody internalRB = new Rigidbody(
                    rigidBody: rigidBody,
                    rigidBodyData: rigidBodyData,
                    eulerAngles: new float[3]
                    {
                            (float)(eulers[0] * (180.0f / Math.PI)),
                            (float)(eulers[1] * (180.0f / Math.PI)),
                            (float)(eulers[2] * (180.0f / Math.PI))
                    }
                );

                RigidBodyReceived?.Invoke(internalRB);
            }
        }

        /// <summary>
        /// Called when a new frame of information was send from the server to this client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _client_OnFrameReady2(object sender, NatNetEventArgs e) => _client_OnFrameReady(e.data, e.client);
    }
}
