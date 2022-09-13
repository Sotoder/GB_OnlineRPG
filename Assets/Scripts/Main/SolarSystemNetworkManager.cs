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
        [SerializeField] private int _cristallsCount;
        [SerializeField] private int _spawnRadius;
        [SerializeField] private float _rotationSpeed;

        private NetworkManager _manager;
        private int _playerServerID;
        private int _collectCristallCount;
        private int _currentCristalCount;
        private Dictionary<int, ShipController> _shipMatchings = new Dictionary<int, ShipController>();
        private Dictionary<int, GameObject> _cristalls = new Dictionary<int,GameObject>();

        private Vector4 _tmpCristallRotation;

        private void Awake()
        {
            _manager = GetComponent<NetworkManager>();
            _objectMover.SetNetworkmanager(this);
            _currentCristalCount = _cristallsCount;

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

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            client.RegisterHandler(101, CreateCristallOnClient);
            client.RegisterHandler(102, ReciveRotateCristallOnClient);
            client.RegisterHandler(103, ReciveIDCristallOnClient);
            client.RegisterHandler(104, RecivePlayerID);
            client.RegisterHandler(105, ReciveCristallIDForDelete);

            var login = new LoginMessege();
            login.messege = _playerNameField.text == "" ? "Player" + conn.connectionId.ToString() : _playerNameField.text;

            conn.Send(100, login);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            var spawnTransform = GetStartPosition();

            var player = Instantiate(playerPrefab, spawnTransform.position, spawnTransform.rotation);
            var ship = player.GetComponent<ShipController>();
            ship.PlayerID = conn.connectionId;
            ship.OnCristallCollision += CristallCollision;

            var idMessage = new MessageInt
            {
                Number = conn.connectionId
            };
            NetworkServer.SendToClient(conn.connectionId, 104, idMessage);


            _shipMatchings.Add(conn.connectionId, ship);

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

            for(int i = 0; i < _cristalls.Count; i++)
            {
                var cristallPositionMsg = new MessageVector()
                {
                    Vector3 = _cristalls[i].transform.position
                };

                NetworkServer.SendToClient(conn.connectionId, 101, cristallPositionMsg);
            }
        }

        #region Send
        public void SendCristallRotation(Vector4 vector4)
        {
            var cristallRotationMsg = new MessageVector4()
            {
                Vector4 = vector4
            };

            NetworkServer.SendToAll(102, cristallRotationMsg);
        }

        public void SendCristalId(int id)
        {
            var cristallIdMsg = new MessageInt()
            {
                Number = id
            };

            NetworkServer.SendToAll(103, cristallIdMsg);
        }
        #endregion

        #region ClientRecive
        private void RecivePlayerID(NetworkMessage netMsg)
        {
            var id = netMsg.reader.ReadInt16();
            _playerServerID = id;
        }

        private void ReciveRotateCristallOnClient(NetworkMessage netMsg)
        {
            var rotation = netMsg.reader.ReadVector4();

            _tmpCristallRotation = rotation;
        }

        private void ReciveIDCristallOnClient(NetworkMessage netMsg)
        {
            var id = netMsg.reader.ReadInt16();

            var cristall = _cristalls[id];

            cristall.transform.rotation = new Quaternion(_tmpCristallRotation.x,
                                                         _tmpCristallRotation.y,
                                                         _tmpCristallRotation.z,
                                                         _tmpCristallRotation.w);
        }

        private void CreateCristallOnClient(NetworkMessage netMsg)
        {
            var cristallPosition = netMsg.reader.ReadVector3();

            var cristall = Instantiate(_cristallPref, _cristallsHolder.transform);

            cristall.transform.position = cristallPosition;

            _cristalls.Add(_cristalls.Count, cristall);

        }

        private void ReciveCristallIDForDelete(NetworkMessage netMsg)
        {
            var cristallID = netMsg.reader.ReadInt16();

            _cristalls[cristallID].SetActive(false);
            _collectCristallCount++;
        }
        #endregion

        #region ServerRecive
        public void ReciveLoginMessege(NetworkMessage message)
        {
            var loginName = message.reader.ReadString();
            _shipMatchings[message.conn.connectionId].PlayerName = loginName;
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
            foreach (var cristall in _cristalls)
            {
                if (cristall.Value.gameObject == cristallObject)
                {
                    var cristallKeyMsg = new MessageInt
                    {
                        Number = cristall.Key
                    };

                    NetworkServer.SendToAll(105, cristallKeyMsg);
                    _currentCristalCount--;

                    if (_currentCristalCount <= 0)
                    {
                        ShowLeaderTab();
                    }
                }
            }
        }

        private void ShowLeaderTab()
        {
        }

        private void CreateCristalls()
        {
            
            for (int i = 0; i < _cristallsCount; i++)
            {
                var cristall = Instantiate(_cristallPref, _cristallsHolder.transform);
                cristall.transform.position = Random.insideUnitSphere * _spawnRadius;
                _cristalls.Add(i, cristall);
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
