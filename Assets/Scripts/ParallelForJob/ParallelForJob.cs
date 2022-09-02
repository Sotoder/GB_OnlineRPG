using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ParallelForJob : MonoBehaviour
{
    private NativeArray<Vector3> _positions;
    private NativeArray<Vector3> _velocities;
    private NativeArray<Vector3> _finalPositions;
    private ParallelJob _parallelJob;

    private const int ARRAY_COUNTS = 10;

    public void Generate()
    {
        if(_positions.IsCreated)
        {
            ClearArrays();
        }

        _positions = new NativeArray<Vector3>(ARRAY_COUNTS, Allocator.Persistent);
        _velocities = new NativeArray<Vector3>(ARRAY_COUNTS, Allocator.Persistent);
        _finalPositions = new NativeArray<Vector3>(ARRAY_COUNTS, Allocator.Persistent);

        for (int i = 0; i < ARRAY_COUNTS; i++)
        {
            _positions[i] = new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10));
            _velocities[i] = new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10));
        }
    }

    public void Execute()
    {
        if (!_finalPositions.IsCreated) return;

        ParallelJob job = new ParallelJob
        {
            Positions = _positions,
            Velosities = _velocities,
            FinalPositions = _finalPositions
        };

        JobHandle jobHandle = job.Schedule(ARRAY_COUNTS, 0);

        jobHandle.Complete();

        for (int i = 0; i < ARRAY_COUNTS; i++)
        {
            Debug.Log("OldPosition: " + _positions[i].ToString() + " Velosities: " + _velocities[i].ToString() + " Result: " + _finalPositions[i].ToString());
        }
        ClearArrays();
    }

    private void ClearArrays()
    {
        _positions.Dispose();
        _velocities.Dispose();
        _finalPositions.Dispose();
    }

    private void OnDestroy()
    {
        if (_positions.IsCreated)
        {
            ClearArrays();
        }
    }
}
