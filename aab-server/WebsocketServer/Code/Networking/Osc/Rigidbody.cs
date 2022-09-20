using NatNetML;
using Newtonsoft.Json.Linq;
using SharpOSC;

namespace MasterNetworking.Osc
{
    public class Rigidbody
    {
        public int Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public float Quat0 { get; set; }
        public float Quat1 { get; set; }
        public float Quat2 { get; set; }
        public float Quat3 { get; set; }
        public string Name { get; set; }

        public Rigidbody(OscMessage message)
        {
            Id = (int)message.Arguments[0];
            X = (float)message.Arguments[1];
            Y = (float)message.Arguments[2];
            Z = (float)message.Arguments[3];
            RotX = (float)message.Arguments[4];
            RotY = (float)message.Arguments[5];
            RotZ = (float)message.Arguments[6];
            Quat0 = (float)message.Arguments[7];
            Quat1 = (float)message.Arguments[8];
            Quat2 = (float)message.Arguments[9];
            Quat3 = (float)message.Arguments[10];
            Name = (string)message.Arguments[11];
        }

        public Rigidbody (RigidBody rigidBody, RigidBodyData rigidBodyData, float[] eulerAngles)
        {
            Id = rigidBody.ID;
            X = rigidBodyData.x;
            Y = rigidBodyData.y;
            Z = rigidBodyData.z;
            RotX = eulerAngles[0];
            RotY = eulerAngles[1];
            RotZ = eulerAngles[2];
            Quat0 = rigidBodyData.qx;
            Quat1 = rigidBodyData.qy;
            Quat2 = rigidBodyData.qz;
            Quat3 = rigidBodyData.qw;
            Name = rigidBody.Name;
        }

        public override string ToString() => $"{Name}: Pos = ({X}, {Y}, {Z}), Rot = ({RotX}, {RotY}, {RotZ})";

        public JObject ToJObject()
        {
            return new JObject(
                new JProperty("Id", Id),
                new JProperty("X", X),
                new JProperty("Y", Y),
                new JProperty("Z", Z),
                new JProperty("RotX", RotX),
                new JProperty("RotY", RotY),
                new JProperty("RotZ", RotZ),
                new JProperty("Quat0", Quat0),
                new JProperty("Quat1", Quat2),
                new JProperty("Quat2", Quat2),
                new JProperty("Quat3", Quat3),
                new JProperty("Name", Name)
            );
        }

        public object[] ToOscMessageObject()
        {
            return new object[]
            {
                Id,
                X, Y, Z,
                RotX, RotY, RotZ,
                Quat0, Quat1, Quat2, Quat3,
                Name
            };
        }
    }
}
