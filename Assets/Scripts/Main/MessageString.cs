using UnityEngine.Networking;

public class MessageString: MessageBase
{
    public string messege;

    public override void Deserialize(NetworkReader reader)
    {
        messege = reader.ReadString();
    }

    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(messege);
    }
}
