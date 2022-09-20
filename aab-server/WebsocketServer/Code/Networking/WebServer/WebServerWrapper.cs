using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.WebApi;
using MasterNetworking.Utils;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MasterNetworking.WebServer
{
    public delegate void WebserverEventHandler();

    public class WebServerWrapper
    {
        #region Singleton
        private static WebServerWrapper _instance;
        public static WebServerWrapper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new WebServerWrapper();
                return _instance;
            }
        }

        public static bool ExistsInstance => _instance != null;

        private WebServerWrapper()
        {

        }
        #endregion

        public event WebserverEventHandler Started;
        public event WebserverEventHandler Stopped;

        private int? _port;
        private CancellationTokenSource _cancellationTokenSource;
        private EmbedIO.WebServer _webserver;

        public string MainURL => _port.HasValue ? $"http://localhost:{_port}/" : null;
        public string WebsocketURL { get; private set; }
        public int WebsocketPort { get; private set; }

        public string HtmlRootPath { get; private set; } = null;

        #region Lifecycle Methods
        /// <summary>
        /// Starts the webserver.
        /// </summary>
        /// <param name="port">The port to open the webserver on.</param>
        /// <param name="htmlRootPath">The html root path. If this is null, it will be automatically calculated.</param>
        public void StartServer(int port, string htmlRootPath = null)
        {
            HtmlRootPath = htmlRootPath != null ? htmlRootPath : UtilMethods.GetHtmlRootPath();

            _port = port;
            _cancellationTokenSource = new CancellationTokenSource();

            _webserver = new EmbedIO.WebServer(port)
                .WithLocalSessionManager()
                .WithWebApi("/api", m => m.WithController<ServerApi>())
                .WithStaticFolder("/", HtmlRootPath, true, m => m.WithContentCaching(false))
                .WithModule(new ActionModule("/", HttpVerbs.Any, _onRequestHandlerCallback));

            _webserver.StateChanged += _webserver_StateChanged;

            _webserver.RunAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Stops this webserver.
        /// </summary>
        public void StopServer() => _cancellationTokenSource.Cancel(); 

        /// <summary>
        /// Sets the websocket information.
        /// This will be used for the api and to allow the websockets on the html client side to connect to the correct websocket.
        /// </summary>
        /// <param name="wsUrl">The url/ip to the websocket.</param>
        /// <param name="wsPort">The port the websocket is opened onto.</param>
        public void SetApiInformation(string wsUrl, int wsPort)
        {
            WebsocketURL = wsUrl;
            WebsocketPort = wsPort;
        }
        #endregion


        /// <summary>
        /// This will open a browser with the main page.
        /// </summary>
        public void OpenBrowser()
        {
            if (_webserver.State != WebServerState.Listening)
                return;
            if (MainURL == null)
                return;

            Process browser = new Process()
            {
                StartInfo = new ProcessStartInfo(MainURL)
                {
                    UseShellExecute = true,
                }
            };
            browser.Start();
        }

        #region Server Events
        private Task _onRequestHandlerCallback(IHttpContext context) => context.SendDataAsync(new { Message = "Error" });

        private void _webserver_StateChanged(object sender, WebServerStateChangedEventArgs e)
        {
            if (e.NewState == WebServerState.Listening)
                Started?.Invoke();
            else if (e.NewState == WebServerState.Stopped)
            {
                _cancellationTokenSource = null;
                _webserver = null;
                _port = null;
                HtmlRootPath = null;

                Stopped?.Invoke();
            }
        } 
        #endregion
    }
}
