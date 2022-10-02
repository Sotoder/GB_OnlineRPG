using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Mechanics
{
    [BurstCompile]
    public struct CristallsRotateJob : IJobParallelForTransform
    {
        public float DeltaTime;
        public float RotationSpeed;
        public NativeArray<float> AxisAngles;

        public void Execute(int index, TransformAccess transform)
        {
            transform.localRotation = Quaternion.AngleAxis(AxisAngles[index], Vector3.right);
            AxisAngles[index] = AxisAngles[index] == 180 ? 0 : AxisAngles[index] + (RotationSpeed * DeltaTime);
        }
    }
}