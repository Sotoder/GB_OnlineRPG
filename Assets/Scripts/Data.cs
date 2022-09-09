using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class Data : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler(100, ReceiveInt);

    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        MyMessage myMessage = new MyMessage();
        myMessage.chislo = 2;
        conn.Send(100, myMessage);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

    }

    public void ReceiveInt(NetworkMessage networkMessage)
    { 
    //что-то
    }
}
public class MyMessage : MessageBase
{
    public int chislo;    
    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(chislo);
    }
    public override void Deserialize(NetworkReader reader)
    {
        chislo = reader.ReadInt32();
    }
}