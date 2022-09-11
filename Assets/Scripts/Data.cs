using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Data : NetworkManager
{
    [SerializeField] private Button _startHost;
    [SerializeField] private Button _stopHost;
    [SerializeField] private Button _startClient;
    [SerializeField] private Button _stopClient;

    private NetworkManager _manager;

    private void Awake()
    {
        _manager = GetComponent<NetworkManager>();
        _startHost.onClick.AddListener(ManualStartServer);
        _stopHost.onClick.AddListener(ManualStopServer);
        _startClient.onClick.AddListener(ManualStartClient);
        _stopClient.onClick.AddListener(ManualStopClient);
    }

    private void Update()
    {
        ChekInputKey();
    }

    private void ChekInputKey()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ManualStartServer();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ManualStartClient();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            ManualStopServer();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ManualStopClient();
        }
    }

    private void ManualStartClient()
    {
        _manager.StartClient();
    }

    private void ManualStartServer()
    {
        _manager.StartServer();
    }

    private void ManualStopClient()
    {
        _manager.StopClient();
    }

    private void ManualStopServer()
    {
        _manager.StopServer();
    }

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