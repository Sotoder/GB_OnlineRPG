using UnityEngine.Networking;

namespace Main
{
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
