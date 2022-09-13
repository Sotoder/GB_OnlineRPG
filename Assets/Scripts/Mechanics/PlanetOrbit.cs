using Network;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Mechanics
{
    public class PlanetOrbit : NetworkMovableObject
    {
        protected override float speed => smoothTime;
        public float OffsetSin => offsetSin;
        public float OffsetCos => offsetCos;
        public float RotationSpeed => rotationSpeed;
        public float CircleInSecond => circleInSecond;

        [SerializeField] private Transform aroundPoint;
        [SerializeField] private float smoothTime = .3f;
        [SerializeField] private float circleInSecond = 1f;

        [SerializeField] private float offsetSin = 1;
        [SerializeField] private float offsetCos = 1;
        [SerializeField] private float rotationSpeed;

        private Vector3 currentPositionSmoothVelocity;

        private void Awake()
        {
            Initiate(UpdatePhase.FixedUpdate);
        }

        protected override void HasAuthorityMovement()
        {
            if (isServer)
            {
                SendToServer();
            }
        }

        protected override void SendToServer()
        {
            serverPosition = transform.position;
            serverEuler = transform.eulerAngles;
        }

        protected override void FromServerUpdate()
        {
            if (!isClient)
            {
                return;
            }
            transform.position = Vector3.SmoothDamp(transform.position,
                serverPosition, ref currentPositionSmoothVelocity, speed);
            transform.rotation = Quaternion.Euler(serverEuler);
        }
    }
}
