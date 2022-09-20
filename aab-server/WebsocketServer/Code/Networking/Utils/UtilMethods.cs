using MasterNetworking.WebServer;
using System.IO;

namespace MasterNetworking.Utils
{
    public static class UtilMethods
    {
        public delegate string HtmlRootPathMethod();

        public static string GetHtmlRootPath()
        {
            string assemblyPath = Path.GetDirectoryName(typeof(WebServerWrapper).Assembly.Location);

#if DEBUG
            return Path.Combine(Directory.GetParent(assemblyPath).Parent.FullName, "../../aab-web-application");
#else
            return Path.Combine(assemblyPath, "WebserverContent");
#endif
        }
    }
}
