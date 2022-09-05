using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
public class Server : MonoBehaviour
{
    private const int MAX_CONNECTION = 10;
    private int port = 5805;
    private int hostID;
    private int reliableChannel;
    private int unreliableChannel;
    private bool isStarted = false;
    private byte error;
    //List<int> connectionIDs = new List<int>();
    private Dictionary<int, User> _usersMatchings;

    public void StartServer()
    {
        _usersMatchings = new Dictionary<int, User>();
        
        NetworkTransport.Init();//инициаализа€
        ConnectionConfig cc = new ConnectionConfig();
      //cc.ConnectTimeout = 500; //
      //Timeout in ms which library will wait before it will send another connection request.
      // cc.MaxConnectionAttempt = 2;
      //Defines the maximum number of times Unity Multiplayer will attempt
      //to send a connection request without receiving a response before
      //it reports that it cannot establish a connection. Default value = 10.
        reliableChannel = cc.AddChannel(QosType.Reliable);//гарантироованнна€ доставка 
        HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
        hostID = NetworkTransport.AddHost(topology, port);
        isStarted = true;
    }
    public void ShutDownServer()
    {
        if (!isStarted) return;
        NetworkTransport.RemoveHost(hostID);
        NetworkTransport.Shutdown();
        isStarted = false;
    }
    void Update()
    {
        if (!isStarted) return;
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out
        channelId, recBuffer, bufferSize, out dataSize, out error);
        while (recData != NetworkEventType.Nothing)
        {
            switch (recData)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    _usersMatchings.Add(connectionId, new User());
                    break;
                case NetworkEventType.DataEvent:
                    string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);

                    if (!_usersMatchings[connectionId].IsNameSet)
                    {
                        _usersMatchings[connectionId].Name = message;
                        SendMessageToAll($"All say hello to {message}");
                    }
                    else
                    {
                        SendMessageToAll($"{_usersMatchings[connectionId].Name}: {message}");
                    }

                    Debug.Log($"{_usersMatchings[connectionId].Name}: {message}");
                    break;
                case NetworkEventType.DisconnectEvent:
                    SendMessageToAll($"{_usersMatchings[connectionId].Name} has disconnected.");
                    Debug.Log($"{_usersMatchings[connectionId].Name} has disconnected.");
                    _usersMatchings.Remove(connectionId);
                    break;
                case NetworkEventType.BroadcastEvent:
                    break;
            }
            recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer,
            bufferSize, out dataSize, out error);
        }
    }
    public void SendMessageToAll(string message)
    {
        foreach(var user in _usersMatchings)
        {
            SendMessage(message, user.Key);
        }
    }
    public void SendMessage(string message, int connectionID)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostID, connectionID, reliableChannel, buffer, message.Length *
        sizeof(char), out error);
        if ((NetworkError)error != NetworkError.Ok) Debug.Log((NetworkError)error);
    }
}

public class User
{
    private string _name;
    private bool _isNameSet;

    public bool IsNameSet => _isNameSet;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            _isNameSet = true;
        }
    }
}
