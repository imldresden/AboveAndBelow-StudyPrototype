using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace MasterNetworking.WebServer
{
    public sealed class ServerApi : WebApiController
    {
        [Route(HttpVerbs.Get, "/wsIp")]
        public string GetWebsocketUrl() => WebServerWrapper.Instance.WebsocketURL;

        [Route(HttpVerbs.Get, "/wsPort")]
        public int GetWeboscketPort() => WebServerWrapper.Instance.WebsocketPort;
    }
}
