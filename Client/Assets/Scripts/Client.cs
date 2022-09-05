using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using System;

public class Client : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _nameField;

    public delegate void OnMessageReceive(object message);
    public event OnMessageReceive onMessageReceive;
    private const int MAX_CONNECTION = 10;
    private int port = 0;
    private int serverPort = 5805;
    private int hostID;
    private int reliableChannel;

    private int connectionID;
    private bool isConnected = false;
    private byte error;
    public void Connect()
    {
        if (isConnected) return;

        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);
        HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
        hostID = NetworkTransport.AddHost(topology, port);
        connectionID = NetworkTransport.Connect(hostID, "192.168.1.33", serverPort, 0, out error);
        if ((NetworkError)error == NetworkError.Ok)
        {
            isConnected = true;
            _nameField.onSubmit.AddListener(OnNameFieldWasChange);
        }
        else
            Debug.Log((NetworkError)error);
    }

    public void Disconnect()
    {
        if (!isConnected) return;
        NetworkTransport.Disconnect(hostID, connectionID, out error);
        isConnected = false;
        _nameField.onSubmit.RemoveAllListeners();
    }
    void Update()
    {
        if (!isConnected) return;
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
                    onMessageReceive?.Invoke($"You have been connected to server.");
                    SendMessage(_nameField.text);
                    Debug.Log($"You have been connected to server.");
                    break;
                case NetworkEventType.DataEvent:
                    string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ParseMessege(message);
                    Debug.Log(message);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    onMessageReceive?.Invoke($"You have been disconnected from server.");
                    Debug.Log($"You have been disconnected from server.");
                    break;
                case NetworkEventType.BroadcastEvent:
                    break;
            }
            recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer,
            bufferSize, out dataSize, out error);
        }
    }

    private void OnNameFieldWasChange(string name)
    {
        if (!_nameField.wasCanceled)
        {
            SendMessage($"-setname {name}");
        }
    }

    private void ParseMessege(string message)
    {
        if (string.Equals("-", message[0].ToString()))
        {
            string commandString = "";
            int commandEndIndex = 0;

            for (int i = 0; i < message.Length; i++)
            {
                if (message[i].ToString() != " ")
                {
                    commandString += message[i];
                    commandEndIndex = i;
                }
                else
                {
                    break;
                }
            }

            if (string.Equals("-setnamecomplite", commandString))
            {
                _nameField.text = message.Substring(commandEndIndex + 2);
            }
        }
        else
        {
            onMessageReceive?.Invoke(message);
        }
    }

    public void SendMessage(string message)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostID, connectionID, reliableChannel, buffer, message.Length *
        sizeof(char), out error);
        if ((NetworkError)error != NetworkError.Ok) Debug.Log((NetworkError)error);
    }
}