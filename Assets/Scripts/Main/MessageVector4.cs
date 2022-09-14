using UnityEngine;
using UnityEngine.Networking;

namespace Main
{
    public class MessageVector4 : MessageBase
    {
        public Vector4 Vector4;

        public override void Deserialize(NetworkReader reader)
        {
            Vector4 = reader.ReadVector4();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Vector4);
        }
    }
}
