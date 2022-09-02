using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct PlanetsGenerateJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<float> RndAngles;
    [ReadOnly] public NativeArray<float> RndPositions;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position = new Vector3(RndPositions[index], 0, 0);

        // правильная генерация угла отклонения (вроде бы)
        Quaternion rotation = Quaternion.Euler(0, RndAngles[index], 0);
        transform.position = rotation * transform.position;
    }
}
