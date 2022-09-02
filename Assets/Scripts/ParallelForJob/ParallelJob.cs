using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct ParallelJob : IJobParallelFor
{
    public NativeArray<Vector3> Positions;
    public NativeArray<Vector3> Velosities;
    public NativeArray<Vector3> FinalPositions;
    public void Execute(int i)
    {
        FinalPositions[i] = Positions[i] + Velosities[i];
    }
}
