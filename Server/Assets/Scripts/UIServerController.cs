using UnityEngine;
using UnityEngine.UI;

public class UIServerController : MonoBehaviour
{
    [SerializeField]
    private Button buttonStartServer;
    [SerializeField]
    private Button buttonShutDownServer;
    [SerializeField]
    private Server server;

    private void Start()
    {
        buttonStartServer.onClick.AddListener(() => StartServer());
        buttonShutDownServer.onClick.AddListener(() => ShutDownServer());
    }
    private void StartServer()
    {
        server.StartServer();
    }
    private void ShutDownServer()
    {
        server.ShutDownServer();
    }
}