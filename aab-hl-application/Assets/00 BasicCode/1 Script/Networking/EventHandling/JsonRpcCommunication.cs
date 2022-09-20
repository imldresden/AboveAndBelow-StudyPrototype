using MasterNetworking.WebSocket;
using Newtonsoft.Json.Linq;
using System;

namespace MasterNetworking.EventHandling
{
    /// <summary>
    /// Represnts the json rpcs communication information.
    /// </summary>
    public class JsonRpcCommunication
    {
        /// <summary>
        /// The direction this message is meant for.
        /// Either "to" or "from".
        /// </summary>
        public string Direction { get; private set; }
        /// <summary>
        /// The name the message should be send "to" is comming "from".
        /// </summary>
        public string Name { get; private set; } = null;
        /// <summary>
        /// The device type this message should be send "to" is comming "from".
        /// </summary>
        public ClientConnectionType? ClientConnectionType { get; private set; } = null;

        /// <summary>
        /// Creates a json rpc communication object based on a given jObject.
        /// </summary>
        /// <param name="jObject">The jObject representation of the data needed for such an object.</param>
        /// <returns>The deserialized object, or null.</returns>
        public static JsonRpcCommunication FromJObject(JObject jObject)
        {
            try
            {
                JsonRpcCommunication communication = new JsonRpcCommunication();
                communication.Direction = jObject["direction"].ToString();

                // Checks if the direction is correctly specefied.
                if (communication.Direction != "from" && communication.Direction != "to")
                    return null;

                if (jObject["name"] != null)
                    communication.Name = jObject["name"].ToString();
                if (jObject["clientConnectionType"] != null)
                    communication.ClientConnectionType = ClientConnectionTypeUtils.FromString(jObject["clientConnectionType"].ToString()).Value;

                return communication;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// Creates a communication object that has as the direction "to".
        /// </summary>
        /// <param name="name">The name of the connection to communicate to.</param>
        /// <param name="ccType">The type of the connection to communicate to.</param>
        /// <returns>A new communication.</returns>
        public static JsonRpcCommunication CommunicationTo(string name, ClientConnectionType? ccType)
        {
            return new JsonRpcCommunication()
            {
                Direction = "to",
                Name = name,
                ClientConnectionType = ccType
            };
        }

        /// <summary>
        /// Creates a communication object that has as the direction "from".
        /// </summary>
        /// <param name="name">The name of the connection this communication comes from.</param>
        /// <param name="ccType">The type of the connection this communication comes from.</param>
        /// <returns>A new communication.</returns>
        public static JsonRpcCommunication CommunicationFrom(string name, ClientConnectionType? ccType)
        {
            return new JsonRpcCommunication()
            {
                Direction = "from",
                Name = name,
                ClientConnectionType = ccType
            };
        }

        /// <summary>
        /// Transforms this commmunication object into an JObjects.
        /// </summary>
        /// <returns>The JObject representation.</returns>
        public JObject ToJObject()
        {
            string ccType = null;
            if (ClientConnectionType.HasValue)
                ccType = ClientConnectionTypeUtils.ToString(ClientConnectionType.Value);

            return new JObject(
                new JProperty("direction", Direction),
                new JProperty("name", Name),
                new JProperty("clientConnectionType", ccType)
            );
        }
    }
}
