using Main;
using Mechanics;
using Network;
using System;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace Characters
{
    public class ShipController : NetworkMovableObject
    {
        public string PlayerName
        {
            get => playerName;
            set
            {
                playerName = value;
                gameObject.name = value;
            }
        }

        public int PlayerID;
        public Action<int> OnCristallCollision;

        protected override float speed => shipSpeed;
        [SerializeField] private Transform cameraAttach;
        private CameraOrbit cameraOrbit;
        private PlayerLabel playerLabel;
        private float shipSpeed;
        private Rigidbody rb;
        [SyncVar] private string playerName;
        private void OnGUI()
        {
            if (cameraOrbit == null)
            {
                return;
            }
            cameraOrbit.ShowPlayerLabels(playerLabel);
        }
        public override void OnStartAuthority()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                return;
            }
            serverPosition = transform.position;
            gameObject.name = playerName;
            cameraOrbit = FindObjectOfType<CameraOrbit>();
            cameraOrbit.Initiate(cameraAttach == null ? transform : cameraAttach);
            playerLabel = GetComponentInChildren<PlayerLabel>();
            base.OnStartAuthority();
        }
        protected override void HasAuthorityMovement()
        {
            var spaceShipSettings = SettingsContainer.Instance?.SpaceShipSettings;
            if (spaceShipSettings == null)
            {
                return;
            }
            var isFaster = Input.GetKey(KeyCode.LeftShift);
            var speed = spaceShipSettings.ShipSpeed;
            var faster = isFaster ? spaceShipSettings.Faster : 1.0f;

            serverPosition = transform.position;
            shipSpeed = Mathf.Lerp(shipSpeed, speed * faster,
                SettingsContainer.Instance.SpaceShipSettings.Acceleration);

            var currentFov = isFaster
                ? SettingsContainer.Instance.SpaceShipSettings.FasterFov
                : SettingsContainer.Instance.SpaceShipSettings.NormalFov;
            cameraOrbit.SetFov(currentFov, SettingsContainer.Instance.SpaceShipSettings.ChangeFovSpeed);
            var velocity = cameraOrbit.transform.TransformDirection(Vector3.forward) * shipSpeed;
            rb.velocity = velocity * Time.deltaTime;
            if (!Input.GetKey(KeyCode.C))
            {
                var targetRotation = Quaternion.LookRotation(
                    Quaternion.AngleAxis(cameraOrbit.LookAngle, -transform.right) *
                    velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
            }
        }

        protected override void FromServerUpdate()
        {
            transform.position = serverPosition;
        }
        protected override void SendToServer() { }

        [ClientCallback]
        private void LateUpdate()
        {
            cameraOrbit?.CameraMovement();
            gameObject.name = playerName;
        }

        [ServerCallback]
        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Cristall")
            {
                OnCristallCollision?.Invoke(PlayerID);
                Destroy(other.gameObject);
            }
            else
            {

                RpcChangePosition(
                    new Vector3(
                        Random.Range(50, 150),
                        Random.Range(50, 150),
                        Random.Range(50, 150)));
            }
        }

        [ClientRpc]
        public void RpcChangePosition(Vector3 position)
        {
            gameObject.SetActive(false);
            transform.position = position;
            gameObject.SetActive(true);
        }
    }
}