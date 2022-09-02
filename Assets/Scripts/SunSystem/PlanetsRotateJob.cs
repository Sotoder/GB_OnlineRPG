using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct PlanetsRotateJob : IJobParallelForTransform
{
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public NativeArray<float> AxisSpeeds;
    [ReadOnly] public NativeArray<float> StarSpeeds;

    public NativeArray<float> AxisAngles;

    public void Execute(int index, TransformAccess transform)
    {
        transform.localRotation = Quaternion.AngleAxis(AxisAngles[index], Vector3.up);
        AxisAngles[index] = AxisAngles[index] == 180 ? 0 : AxisAngles[index] + (AxisSpeeds[index] * DeltaTime);

        Quaternion rotation = Quaternion.Euler(0, StarSpeeds[index] * DeltaTime, 0);
        transform.position = rotation * transform.position;
    }

}
