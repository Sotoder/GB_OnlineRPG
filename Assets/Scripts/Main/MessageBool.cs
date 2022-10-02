using UnityEngine.Networking;

namespace Main
{
    public class MessageBool : MessageBase
    {
        public bool Flag;

        public override void Deserialize(NetworkReader reader)
        {
            Flag = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Flag);
        }
    }
}
