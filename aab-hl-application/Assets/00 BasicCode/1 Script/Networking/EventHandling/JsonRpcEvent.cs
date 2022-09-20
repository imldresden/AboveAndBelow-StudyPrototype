namespace MasterNetworking.EventHandling
{
    public enum JsonRpcEvent
    {
        EmulatedOptiTrackMessage,
        DeviceRegistration,
        OptiTrackMessageRequest,
        OptiTrackMessage,
        OpenConnections,
    }

    public static class JsonRpcEventUtils
    {
        public static string ToString(JsonRpcEvent eventName)
        {
            switch (eventName)
            {
                case JsonRpcEvent.EmulatedOptiTrackMessage: return "EmulatedOptiTrackMessage";
                case JsonRpcEvent.DeviceRegistration:       return "DeviceRegistration";
                case JsonRpcEvent.OptiTrackMessageRequest:  return "OptiTrackMessageRequest";
                case JsonRpcEvent.OptiTrackMessage:         return "OptiTrackMessage";
                case JsonRpcEvent.OpenConnections:          return "OpenConnections";
            }

            return null;
        }

        public static JsonRpcEvent? FromString(string eventName)
        {
            switch (eventName)
            {
                case "EmulatedOptiTrackMessage":    return JsonRpcEvent.EmulatedOptiTrackMessage;
                case "DeviceRegistration":          return JsonRpcEvent.DeviceRegistration;
                case "OptiTrackMessageRequest":     return JsonRpcEvent.OptiTrackMessageRequest;
                case "OptiTrackMessage":            return JsonRpcEvent.OptiTrackMessage;
                case "OpenConnections":             return JsonRpcEvent.OpenConnections;
            }

            return null;
        }
    }
}
