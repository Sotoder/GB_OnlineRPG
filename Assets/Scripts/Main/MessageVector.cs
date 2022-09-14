using UnityEngine;
using UnityEngine.Networking;

namespace Main
{
    public class MessageVector : MessageBase
    {
        public Vector3 Vector3;

        public override void Deserialize(NetworkReader reader)
        {
            Vector3 = reader.ReadVector3();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Vector3);
        }
    }
}
