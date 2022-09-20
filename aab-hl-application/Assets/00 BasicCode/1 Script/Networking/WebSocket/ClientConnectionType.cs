namespace MasterNetworking.WebSocket
{
    public enum ClientConnectionType
    {
        NotDefined,
        Server,
        EmulationDevice,

        Wearable,
        HoloLens,
        MobileDevice,
        Desktop
    }

    public static class ClientConnectionTypeUtils
    {
        public static string ToString(ClientConnectionType ccType)
        {
            switch (ccType)
            {
                case ClientConnectionType.Server:            return "Server";
                case ClientConnectionType.EmulationDevice:   return "EmulationDevice";
                case ClientConnectionType.Wearable:          return "Wearable";
                case ClientConnectionType.HoloLens:          return "HoloLens";
                case ClientConnectionType.MobileDevice:      return "MobileDevice";
                case ClientConnectionType.Desktop:           return "Desktop";
            }

            return null;
        }

        public static ClientConnectionType? FromString(string ccType)
        {
            switch (ccType)
            {
                case "Server":          return ClientConnectionType.Server;
                case "EmulationDevice": return ClientConnectionType.EmulationDevice;
                case "Wearable":        return ClientConnectionType.Wearable;
                case "HoloLens":        return ClientConnectionType.HoloLens;
                case "MobileDevice":    return ClientConnectionType.MobileDevice;
                case "Desktop":         return ClientConnectionType.Desktop;
            }

            return null;
        }
    }
}
