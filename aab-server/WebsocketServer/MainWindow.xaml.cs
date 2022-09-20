using MasterNetworking.NatNet;
using MasterNetworking.Osc;
using MasterNetworking.WebServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace MasterNetworking
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainController _mainController;
        public Dictionary<string, Rigidbody> Rigidbodies = new Dictionary<string, Rigidbody>();

        public MainWindow()
        {
            InitializeComponent();
            _initializeComponents();

            _mainController = new MainController(Dispatcher);
            _mainController.GeneralLogger.LogWritten += (s, m) => Dispatcher.Invoke(() => _generalLogger_LogWritten(s, m));
            NatNetClient.Instance.RigidBodyReceived += (rb) => Dispatcher.Invoke(() => _natNet_RigidBodyReceived(rb));
        }

        /// <summary>
        /// Closes all other active threads.
        /// Taken from: #Link https://stackoverflow.com/questions/26613956/system-threading-tasks-taskcanceledexception-occurred-in-windowsbase-dll-when
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e) => Environment.Exit(0);

        #region Lifecycle Methods
        private void _initializeComponents()
        {
            //RigidBody_DG.ItemsSource = RigidbodyList;

            // Set the Ip Addresses for the motive.
            string ipString = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).First().ToString();
            NatNetLocalIP_TB.Text = ipString;
            NatNetMotiveIP_TB.Text = ipString;
        }

        private void _startMainController()
        {
            Server_Btn.Content = "Stop Server";
            _toggleUiEnables();

            // Set the logging values.
            _mainController.LoggingSetupValues.Prefix = LoggingWebPrefix_TB.Text;
            if (LoggingWebLogEverything_RB.IsChecked.Value)
                _mainController.LoggingSetupValues.WebLogLog = LoggingSetupValues.LoggingDetails.Everything;
            else if (LoggingWebLogOnlyLog_RB.IsChecked.Value)
                _mainController.LoggingSetupValues.WebLogLog = LoggingSetupValues.LoggingDetails.LoggingMessages;
            else
                _mainController.LoggingSetupValues.WebLogLog = LoggingSetupValues.LoggingDetails.None;

            _mainController.Start(
                serverSetupValues: new ServerSetupValues()
                {
                    Input_NatNet = InputNatNet_RB.IsChecked.Value,
                    NatNet_LocalIp = NatNetLocalIP_TB.Text,
                    NatNet_MotiveIp = NatNetMotiveIP_TB.Text,
                    NatNet_MotiveType = NatNetMotiveUnicast_RB.IsChecked.Value ? 1 : 0,
                    Output_Osc = OutputOsc_CB.IsChecked.Value,
                    OscOutput_Ip = OscDestinationIP_TB.Text,
                    OscOutput_Port = int.Parse(OscOutputPort_TB.Text),
                    Input_Osc = InputOsc_RB.IsChecked.Value,
                    OscInput_Port = int.Parse(OscInputPort_TB.Text),
                    Output_Websocket = OutputWebsocket_CB.IsChecked.Value,
                    Websocket_Port = int.Parse(WebSocketPort_TB.Text),
                    Servive_Website = Webserver_GB.IsEnabled && Webserver_CB.IsChecked.Value,
                    Website_Port = int.Parse(WebserverPort_TB.Text)
                }
            );

            Websocket_RTB.Document.Blocks.Clear();
            // Connect to the logger.
            _mainController.WebsocketRawLogger.LogWritten += (s, m) => Dispatcher.Invoke(() => _websocketRawLogger_LogWritten(s, m));
        }

        private void _stopMainController()
        {
            Server_Btn.Content = "Start Server";
            _toggleUiEnables();

            Rigidbodies.Clear();

            _mainController.Stop();
        }
        #endregion

        #region Logging Methods
        private void _generalLogger_LogWritten(object sender, string message)
        {
            message = message.Replace("\n", "").Replace("\r", "");

            GeneralLog_RTB.AppendText($"{message}\r");
            GeneralLog_RTB.ScrollToEnd();
        }

        private void _websocketRawLogger_LogWritten(object sender, string message)
        {
            message = message.Replace("\n", "").Replace("\r", "");

            Websocket_RTB.AppendText($"{message}\r");
            Websocket_RTB.ScrollToEnd();
        } 

        private void _natNet_RigidBodyReceived(Rigidbody rigidbody)
        {
            if (rigidbody == null)
                return;

            if (!Rigidbodies.ContainsKey(rigidbody.Name))
                Rigidbodies.Add(rigidbody.Name, rigidbody);
            else
                Rigidbodies[rigidbody.Name] = rigidbody;
        }
        #endregion

        #region UI Events
        private void _Server_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!_mainController.Started)
                _startMainController();
            else
                _stopMainController();
        }

        private void _OpenWebsite_Btn_Click(object sender, RoutedEventArgs e) => WebServerWrapper.Instance.OpenBrowser();

        private void _InputNatNet_RB_Unchecked(object sender, RoutedEventArgs e) => OutputOsc_CB.IsChecked = false;

        private void _OutputWebsocket_CB_CheckStateChanged(object sender, RoutedEventArgs e)
        {
            if (Webserver_GB == null)
                return;

            Webserver_GB.IsEnabled = OutputWebsocket_CB.IsChecked.Value;
        }

        private void LoggingWebLogEverything_RB_Checked(object sender, RoutedEventArgs e)
        {
            if (_mainController == null)
                return;

            _mainController.LoggingSetupValues.WebLogLog = LoggingSetupValues.LoggingDetails.Everything;
        }

        private void LoggingWebLogOnlyLog_RB_Checked(object sender, RoutedEventArgs e)
        {
            if (_mainController == null)
                return;

            _mainController.LoggingSetupValues.WebLogLog = LoggingSetupValues.LoggingDetails.LoggingMessages;
        }

        private void LoggingWebLogNothing_RB_Checked(object sender, RoutedEventArgs e)
        {
            if (_mainController == null)
                return;

            _mainController.LoggingSetupValues.WebLogLog = LoggingSetupValues.LoggingDetails.None;
        }

        private void LoggingWebLogEmptyUiRTB_Btn_Click(object sender, RoutedEventArgs e) => Websocket_RTB.Document.Blocks.Clear();
        #endregion

        private void _toggleUiEnables()
        {
            InputNatNet_GB.IsEnabled = !InputNatNet_GB.IsEnabled;
            OutputOsc_GB.IsEnabled = !OutputOsc_GB.IsEnabled;
            InputOsc_GB.IsEnabled = !InputOsc_GB.IsEnabled;
            Websocket_GB.IsEnabled = !Websocket_GB.IsEnabled;

            WebserverPort_TB.IsEnabled = !WebserverPort_TB.IsEnabled;
            Webserver_CB.IsEnabled = !Webserver_CB.IsEnabled;
            OpenWebsite_Btn.IsEnabled = !OpenWebsite_Btn.IsEnabled;

            LoggingWebPrefix_TB.IsEnabled = !LoggingWebPrefix_TB.IsEnabled;
        }
    }
}
