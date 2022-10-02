using Characters;
using Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Main
{
    public class SolarSystemNetworkManager : NetworkManager
    {
        [SerializeField] private SpawnPointsMap _spawnPointsMap;
        [SerializeField] private InputField _playerNameField;
        [SerializeField] private Button _startServerButton;
        [SerializeField] private Button _stopServerButton;
        [SerializeField] private Button _startClientButton;
        [SerializeField] private Button _stopClientButton;
        [SerializeField] private ObjectMover _objectMover;
        [Header("Cristalls")]
        [SerializeField] private GameObject _cristallPref;
        [SerializeField] private GameObject _cristallsHolder;
        [SerializeField] private int _maxCristallsCount;
        [SerializeField] private int _spawnRadius;
        [SerializeField] private float _rotationSpeed;
        [Header("Leaders Tab")]
        [SerializeField] private GameObject _leaderPanel;
        [SerializeField] private Text _leadersTabText;
        [SerializeField] private Text _remainingCristallsText;
        [SerializeField] private Text _collectedCristallsText;
        [SerializeField] private Text _remainingCristallsCountText;
        [SerializeField] private Text _collectedCristallsCountText;

        private NetworkManager _manager;
        private int _collectCristallsCount;
        private int _currentCristalCount;
        private List<GameObject> _cristalls = new List<GameObject>();
        private Dictionary<int, ShipController> _shipMatchings = new Dictionary<int, ShipController>();
        private Dictionary<int, int> _leaderTab = new Dictionary<int, int>();

        private void Awake()
        {
            _manager = GetComponent<NetworkManager>();
            _objectMover.SetNetworkmanager(this);
            _currentCristalCount = _maxCristallsCount;

            _startServerButton.onClick.AddListener(ManualStartServer);
            _stopServerButton.onClick.AddListener(_manager.StopServer);
            _startClientButton.onClick.AddListener(ManualStartClient);
            _stopClientButton.onClick.AddListener(_manager.StopClient);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(SpawnCristalls());
            NetworkServer.RegisterHandler(100, ReciveLoginMessege);
            for (int i = 0; i < _spawnPointsMap.SpawnPoints.Count; i++)
            {
                _spawnPointsMap.SpawnPoints[i].HideMark();
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            for (int i = 0; i < _spawnPointsMap.SpawnPoints.Count; i++)
            {
                _spawnPointsMap.SpawnPoints[i].ShowMark();
            }
        }

        public override void OnStartClient(NetworkClient client)
        {
            base.OnStartClient(client);
            _collectedCristallsText.gameObject.SetActive(true);
            _collectedCristallsCountText.gameObject.SetActive(true);
            _collectedCristallsCountText.text = _collectCristallsCount.ToString();

            _remainingCristallsCountText.gameObject.SetActive(true);
            _remainingCristallsText.gameObject.SetActive(true);
            _remainingCristallsCountText.text = _currentCristalCount.ToString();
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            client.RegisterHandler(106, ReciveCurrentCristallCount);
            client.RegisterHandler(107, ReciveCollectedCristallCount);
            client.RegisterHandler(109, ReciveLeaderTabShow);
            client.RegisterHandler(110, ReciveLeaderTabFill);

            var login = new MessageString();
            login.messege = _playerNameField.text;

            conn.Send(100, login);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            var spawnTransform = GetStartPosition();

            var player = Instantiate(playerPrefab, spawnTransform.position, spawnTransform.rotation);
            var ship = player.GetComponent<ShipController>();
            ship.PlayerID = conn.connectionId;
            ship.OnCristallCollision += CristallCollision;

            _shipMatchings.Add(conn.connectionId, ship);
            _leaderTab.Add(conn.connectionId, 0);

            SendInt(_currentCristalCount, 106, conn.connectionId);

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
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

        private void CristallCollision(int clientID)
        {
            _currentCristalCount--;
            SendInt(_currentCristalCount, 106);

            _leaderTab[clientID]++;
            SendInt(1, 107, clientID);

            if (_currentCristalCount <= 0)
            {
                ShowLeaderTab();
            }
        }

        private void ShowLeaderTab()
        {

            var count = 1;
            foreach (var player in _leaderTab)
            {
                var messageString = $"{count}) {_shipMatchings[player.Key].PlayerName}: {player.Value}" + "\n";
                SendString(messageString, 110);
                count++;
            }

            SendBool(true, 109);
            SendInt(0, 108);
        }

        private IEnumerator SpawnCristalls()
        {
            yield return new WaitWhile(() => NetworkServer.active);
            CreateCristalls();
            _objectMover.SetCrisstalsConfig(_cristalls, _rotationSpeed);
        }

        private void CreateCristalls()
        {
            
            for (int i = 0; i < _maxCristallsCount; i++)
            {
                var cristall = Instantiate(_cristallPref, _cristallsHolder.transform);
                cristall.transform.position = Random.insideUnitSphere * _spawnRadius;
                NetworkServer.Spawn(cristall);
                _cristalls.Add(cristall);
            }
        }

        private void ManualStartServer()
        {
            _manager.StartServer();
        }

        private void ManualStartClient()
        {
            _manager.StartClient();
        }

        private void OnDestroy()
        {
            _startServerButton.onClick.RemoveListener(ManualStartServer);
            _stopServerButton.onClick.RemoveListener(StopServer);
            _startClientButton.onClick.RemoveListener(ManualStartClient);
            _stopClientButton.onClick.RemoveListener(StopClient);
        }

        #region Send

        public void SendInt(int messageInt, short messageId)
        {
            var message = new MessageInt
            {
                Number = messageInt
            };

            NetworkServer.SendToAll(messageId, message);
        }

        public void SendInt(int messageInt, short messageId, int clientID)
        {
            var message = new MessageInt
            {
                Number = messageInt
            };

            NetworkServer.SendToClient(clientID, messageId, message);
        }

        public void SendString(string messageStr, short messageId)
        {
            var message = new MessageString
            {
                messege = messageStr
            };

            NetworkServer.SendToAll(messageId, message);
        }

        public void SendBool(bool messageBool, short messageId)
        {
            var message = new MessageBool
            {
                Flag = messageBool
            };

            NetworkServer.SendToAll(messageId, message);
        }

        #endregion

        #region ClientRecive
        private void ReciveCurrentCristallCount(NetworkMessage netMsg)
        {
            var currentCristallCount = netMsg.reader.ReadInt16();

            _currentCristalCount = currentCristallCount;
            _remainingCristallsCountText.text = _currentCristalCount.ToString();
        }

        private void ReciveCollectedCristallCount(NetworkMessage netMsg)
        {
            _collectCristallsCount++;
            _collectedCristallsCountText.text = _collectCristallsCount.ToString();
        }

        private void ReciveLeaderTabFill(NetworkMessage netMsg)
        {
            var leaderStr = netMsg.reader.ReadString();

            _leadersTabText.text += leaderStr;
        }

        private void ReciveLeaderTabShow(NetworkMessage netMsg)
        {
            var flag = netMsg.reader.ReadBoolean();

            _leaderPanel.SetActive(flag);
            Time.timeScale = 0;
        }
        #endregion

        #region ServerRecive
        public void ReciveLoginMessege(NetworkMessage message)
        {
            var loginName = message.reader.ReadString();

            _shipMatchings[message.conn.connectionId].PlayerName = loginName == "" ? "Player" + message.conn.connectionId.ToString() : loginName;
        }
        #endregion
    }
}
