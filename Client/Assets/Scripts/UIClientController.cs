using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIClientController : MonoBehaviour
{
    [SerializeField]
    private Button buttonConnectClient;
    [SerializeField]
    private Button buttonDisconnectClient;
    [SerializeField]
    private Button buttonSendMessage;
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private TextField textField;
    [SerializeField]
    private Client client;
    [SerializeField]
    private TMP_InputField nameField;

    private void Start()
    {
        buttonConnectClient.onClick.AddListener(() => Connect());
        buttonDisconnectClient.onClick.AddListener(() => Disconnect());
        buttonSendMessage.onClick.AddListener(() => SendMessage());
        client.onMessageReceive += ReceiveMessage;
    }

    private void Connect()
    {
        if(nameField.text.Length < 3)
        {
            textField.ReceiveMessage("Перед подключением необходимо указать имя. Минимальная длина 3 символа");
        }
        else
        {
            client.Connect();
        }
    }
    private void Disconnect()
    {
        client.Disconnect();
    }
    private void SendMessage()
    {
        client.SendMessage(inputField.text);
        inputField.text = "";
    }
    public void ReceiveMessage(object message)
    {
        textField.ReceiveMessage(message);
    }
}