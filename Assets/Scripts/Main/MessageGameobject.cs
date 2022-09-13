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

    public class MessageInt : MessageBase
    {
        public int Number;

        public override void Deserialize(NetworkReader reader)
        {
            Number = reader.ReadInt16();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Number);
        }
    }
}
