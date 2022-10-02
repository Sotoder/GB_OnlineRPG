using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Mechanics
{
    [BurstCompile]
    public struct PlanetsMoveJob : IJobParallelForTransform
    {
        public float DeltaTime;
        public Vector3 AroundPointPos;
        public NativeArray<float> OffsetsSin;
        public NativeArray<float> OffsetsCos;
        public NativeArray<float> Distanses;
        public NativeArray<float> CurrentAngles;
        public NativeArray<float> CurrentRotationAngles;
        public NativeArray<float> RotationSpeeds;
        public NativeArray<float> CirclesInSecond;

        private const float circleRadians = Mathf.PI * 2;

        public void Execute(int index, TransformAccess transform)
        {
            Vector3 p = AroundPointPos;

            p.x += Mathf.Sin(CurrentAngles[index]) * Distanses[index] * OffsetsSin[index];
            p.z += Mathf.Cos(CurrentAngles[index]) * Distanses[index] * OffsetsCos[index];

            transform.position = p;

            CurrentRotationAngles[index] += DeltaTime * RotationSpeeds[index];
            CurrentRotationAngles[index] = Mathf.Clamp(CurrentRotationAngles[index], 0, 361);
            if (CurrentRotationAngles[index] >= 360)
            {
                CurrentRotationAngles[index] = 0;
            }

            transform.rotation = Quaternion.AngleAxis(CurrentRotationAngles[index], new Vector3(0,1,0));
            CurrentAngles[index] += circleRadians * CirclesInSecond[index] * DeltaTime;
        }
    }
}