using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace Mechanics
{
    public class ObjectMover: NetworkBehaviour
    {
        [SerializeField] private Transform _solarTransform;
        [SerializeField] private List<PlanetOrbit> _planetOrbits;

        private TransformAccessArray _planetsTransform;
        private NativeArray<float> _offsetsSin;
        private NativeArray<float> _offsetsCos;
        private NativeArray<float> _distanses;
        private NativeArray<float> _currentAngles;
        private NativeArray<float> _rotationSpeeds;
        private NativeArray<float> _circlesInSecond;
        private NativeArray<float> _currentRotationAngles;

        private TransformAccessArray _cristallsTransform;
        private NativeArray<float> _cristallAngles;
        private float _rotationSpeed;

        private SolarSystemNetworkManager _manager;

        private void Start()
        {
            if (isServer)
            {
                Init(_planetOrbits);
            }
        }

        private void Init(List<PlanetOrbit> planets)
        {
            _planetsTransform = new TransformAccessArray(planets.Count);
            _offsetsSin = new NativeArray<float>(planets.Count, Allocator.Persistent);
            _offsetsCos = new NativeArray<float>(planets.Count, Allocator.Persistent);
            _distanses = new NativeArray<float>(planets.Count, Allocator.Persistent);
            _currentAngles = new NativeArray<float>(planets.Count, Allocator.Persistent);
            _rotationSpeeds = new NativeArray<float>(planets.Count, Allocator.Persistent);
            _circlesInSecond = new NativeArray<float>(planets.Count, Allocator.Persistent);
            _currentRotationAngles = new NativeArray<float>(planets.Count, Allocator.Persistent);

            for (int i = 0; i < planets.Count; i++)
            {
                _planetsTransform.Add(planets[i].transform);
                _offsetsSin[i] = planets[i].OffsetSin;
                _offsetsCos[i] = planets[i].OffsetCos;
                _distanses[i] = (planets[i].transform.position - _solarTransform.position).magnitude;
                _rotationSpeeds[i] = planets[i].RotationSpeed;

                var rndAngle = Random.Range(0, 360);
                _currentAngles[i] = rndAngle;

                _circlesInSecond[i] = planets[i].CircleInSecond;
                _currentRotationAngles[i] = 0;
            }
        }

        public void SetNetworkmanager(SolarSystemNetworkManager solarSystemNetworkManager)
        {
            _manager = solarSystemNetworkManager;
        }

        public void SetCrisstalsConfig(List<GameObject> cristallsList, float rotationSpeed)
        {
            _cristallsTransform = new TransformAccessArray(cristallsList.Count);
            _cristallAngles = new NativeArray<float>(cristallsList.Count, Allocator.Persistent);
            _rotationSpeed = rotationSpeed;

            for(int i = 0; i < cristallsList.Count; i++)
            {
                _cristallsTransform.Add(cristallsList[i].transform);
                _cristallAngles[i] = Random.Range(0f, 361f);
            }
        }

        public void FixedUpdate()
        {
            if (!isServer) return;

            var planetMoveJob = new PlanetsMoveJob
            {
                DeltaTime = Time.deltaTime,
                AroundPointPos = _solarTransform.position,
                OffsetsCos = _offsetsCos,
                OffsetsSin = _offsetsSin,
                Distanses = _distanses,
                CurrentAngles = _currentAngles,
                CurrentRotationAngles = _currentRotationAngles,
                RotationSpeeds = _rotationSpeeds,
                CirclesInSecond = _circlesInSecond
            };

            var planetMoveJobHandler = planetMoveJob.Schedule(_planetsTransform);
            planetMoveJobHandler.Complete();

            if(_cristallsTransform.isCreated)
            {
                var cristallRotationJob = new CristallsRotateJob
                {
                    DeltaTime = Time.deltaTime,
                    AxisAngles = _cristallAngles,
                    RotationSpeed = _rotationSpeed
                };

                var cristallRotationJobHandler = cristallRotationJob.Schedule(_cristallsTransform);
                cristallRotationJobHandler.Complete();

                //for(int i = 0; i < _cristallsTransform.length; i++)
                //{
                //    _manager.SendVector4(new Vector4(_cristallsTransform[i].rotation.x,
                //                                              _cristallsTransform[i].rotation.y,
                //                                              _cristallsTransform[i].rotation.z,
                //                                              _cristallsTransform[i].rotation.w), 102);
                //    _manager.SendInt(i, 103);
                //}
            }
        }

        public void OnDestroy()
        {
            if(_planetsTransform.isCreated)
            {
                _planetsTransform.Dispose();
                _offsetsSin.Dispose();
                _offsetsCos.Dispose();
                _distanses.Dispose();
                _currentAngles.Dispose();
                _rotationSpeeds.Dispose();
                _circlesInSecond.Dispose();
                _currentRotationAngles.Dispose();
             }

            if(_cristallsTransform.isCreated)
            {
                _cristallsTransform.Dispose();
                _cristallAngles.Dispose();
            }
        }
    }
}