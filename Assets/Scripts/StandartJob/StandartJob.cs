using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class StandartJob : MonoBehaviour
{
    private NativeArray<int> _numbers;

    public void Generate()
    {
        if (_numbers.IsCreated)
        {
            _numbers.Dispose();
        }

        _numbers = new NativeArray<int>(10, Allocator.Persistent);
        for (int i = 0; i < _numbers.Length; i++)
        {
            _numbers[i] = Random.Range(5, 20);
        }

        Debug.Log("Old numbers: " + GetNumbersString());
    }

    public void ExecuteJob ()
    {
        if (!_numbers.IsCreated) return;

        NumbersJob job = new NumbersJob
        {
            Numbers = _numbers
        };

        JobHandle jobHandle = job.Schedule();

        jobHandle.Complete();
        Debug.Log("New numbers: " + GetNumbersString());
        _numbers.Dispose();
    }

    private string GetNumbersString()
    {
        var numbers = "";

        for (int i = 0; i < _numbers.Length; i++)
        {
            numbers = i != _numbers.Length - 1 ? numbers + _numbers[i] + ", " : numbers + _numbers[i];
        }
        return numbers;
    }

    private void OnDestroy()
    {
        if(_numbers.IsCreated)
        {
            _numbers.Dispose();
        }
    }
}

