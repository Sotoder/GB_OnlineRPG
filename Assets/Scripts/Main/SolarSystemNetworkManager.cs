using Characters;
using Mechanics;
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
        private int _playerServerID;
        private int _collectCristallsCount;
        private int _currentCristalCount;
        private List<GameObject> _cristalls = new List<GameObject>();
        private Dictionary<int, ShipController> _shipMatchings = new Dictionary<int, ShipController>();
        private Dictionary<int, int> _leaderTab = new Dictionary<int, int>();

        private Vector4 _tmpCristallRotation;

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
            CreateCristalls();
            _objectMover.SetCrisstalsConfig(_cristalls, _rotationSpeed);
            NetworkServer.RegisterHandler(100, ReciveLoginMessege);
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

            client.RegisterHandler(101, CreateCristallOnClient);
            client.RegisterHandler(102, ReciveRotationCristallOnClient);
            client.RegisterHandler(103, ReciveCristallIdForRotate);
            client.RegisterHandler(104, RecivePlayerIDOnServer);
            client.RegisterHandler(105, ReciveCristallIDForDelete);
            client.RegisterHandler(106, ReciveCurrentCristallCount);
            client.RegisterHandler(107, ReciveCollectedCristallCount);
            client.RegisterHandler(108, ReciveStopGame);
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

            SendInt(conn.connectionId, 104, conn.connectionId);

            _shipMatchings.Add(conn.connectionId, ship);
            _leaderTab.Add(conn.connectionId, 0);

            SendInt(_currentCristalCount, 106, conn.connectionId);

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

            for(int i = 0; i < _cristalls.Count; i++)
            {
                SendVector3(_cristalls[i].transform.position, 101, conn.connectionId);
            }
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

        public void SendVector4(Vector4 vector4, short messageID)
        {
            var message = new MessageVector4()
            {
                Vector4 = vector4
            };

            NetworkServer.SendToAll(messageID, message);
        }

        public void SendVector3(Vector3 vector3, short messageID, int clientID)
        {
            var message = new MessageVector()
            {
                Vector3 = vector3
            };

            NetworkServer.SendToClient(clientID, messageID, message);
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
        private void RecivePlayerIDOnServer(NetworkMessage netMsg)
        {
            var id = netMsg.reader.ReadInt16();
            _playerServerID = id;
        }

        private void ReciveStopGame(NetworkMessage netMsg)
        {
            var timeScale = netMsg.reader.ReadInt16();
            Time.timeScale = timeScale;
        }

        private void ReciveRotationCristallOnClient(NetworkMessage netMsg)
        {
            var rotation = netMsg.reader.ReadVector4();

            _tmpCristallRotation = rotation;
        }

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


        private void ReciveCristallIdForRotate(NetworkMessage netMsg)
        {
            if (_cristalls.Count == 0) return; // ������� �� ��������� ������ �������� �� �������� ���������� �� �������, ��������� ��� ����� ����� ����������

            var id = netMsg.reader.ReadInt16();

            var cristall = _cristalls[id];

            cristall.transform.rotation = new Quaternion(_tmpCristallRotation.x,
                                                         _tmpCristallRotation.y,
                                                         _tmpCristallRotation.z,
                                                         _tmpCristallRotation.w);
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
        }

        private void CreateCristallOnClient(NetworkMessage netMsg)
        {
            var cristallPosition = netMsg.reader.ReadVector3();

            var cristall = Instantiate(_cristallPref, _cristallsHolder.transform);

            cristall.transform.position = cristallPosition;

            _cristalls.Add(cristall);

        }

        private void ReciveCristallIDForDelete(NetworkMessage netMsg)
        {
            var cristallID = netMsg.reader.ReadInt16();

            _cristalls[cristallID].SetActive(false);
        }
        #endregion

        #region ServerRecive
        public void ReciveLoginMessege(NetworkMessage message)
        {
            var loginName = message.reader.ReadString();

            _shipMatchings[message.conn.connectionId].PlayerName = loginName == "" ? "Player" + message.conn.connectionId.ToString() : loginName;
        }
        #endregion

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

        private void CristallCollision(int clientID, GameObject cristallObject)
        {
            for(int i = 0; i < _cristalls.Count; i++)
            {
                if (_cristalls[i] == cristallObject)
                {
                    SendInt(i, 105);

                    _currentCristalCount--;
                    SendInt(_currentCristalCount, 106);

                    _leaderTab[clientID]++;
                    SendInt(1, 107, clientID);

                    if (_currentCristalCount <= 0)
                    {
                        ShowLeaderTab();
                    }
                }
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

        private void CreateCristalls()
        {
            
            for (int i = 0; i < _maxCristallsCount; i++)
            {
                var cristall = Instantiate(_cristallPref, _cristallsHolder.transform);
                cristall.transform.position = Random.insideUnitSphere * _spawnRadius;
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
    }
}
