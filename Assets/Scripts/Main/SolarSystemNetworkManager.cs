using Characters;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Main
{
    public class SolarSystemNetworkManager : NetworkManager
    {
        [SerializeField] private InputField _playerNameField;
        [SerializeField] private Button _startServerButton;
        [SerializeField] private Button _stopServerButton;
        [SerializeField] private Button _startClientButton;
        [SerializeField] private Button _stopClientButton;

        private Dictionary<int, ShipController> _shipMatchings = new Dictionary<int, ShipController>();
        private NetworkManager _manager;

        private void Awake()
        {
            _manager = GetComponent<NetworkManager>();

            _startServerButton.onClick.AddListener(ManualStartServer);
            _stopServerButton.onClick.AddListener(_manager.StopServer);
            _startClientButton.onClick.AddListener(ManualStartClient);
            _stopClientButton.onClick.AddListener(_manager.StopClient);
        }

        private void ManualStartServer()
        {
            _manager.StartServer();
        }

        private void ManualStartClient()
        {
            _manager.StartClient();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            NetworkServer.RegisterHandler(100, ReciveLoginMessege);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            var spawnTransform = GetStartPosition();

            var player = Instantiate(playerPrefab, spawnTransform.position, spawnTransform.rotation);
            var ship = player.GetComponent<ShipController>();
            _shipMatchings.Add(conn.connectionId, ship);

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            var login = new LoginMessege();
            login.messege = _playerNameField.text == "" ? "Player" + conn.connectionId.ToString() : _playerNameField.text;

            conn.Send(100, login);
        }

        public void ReciveLoginMessege(NetworkMessage message)
        {
            var loginName = message.reader.ReadString();
            _shipMatchings[message.conn.connectionId].PlayerName = loginName;
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
                _manager.StopServer();
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                _manager.StopClient();
            }
        }

        private void OnDestroy()
        {
            _startServerButton.onClick.RemoveListener(ManualStartServer);
            _stopServerButton.onClick.RemoveListener(StopServer);
            _startClientButton.onClick.RemoveListener(ManualStartClient);
            _stopClientButton.onClick.RemoveListener(StopClient);
        }
    }
}
