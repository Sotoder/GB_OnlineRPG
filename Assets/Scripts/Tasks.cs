using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Tasks : MonoBehaviour
{
    public bool _isTasksStarted;
    CancellationTokenSource _cancelTokenSource;
    CancellationToken _cancelToken;

    public void StartOperation()
    {
        if (_isTasksStarted) return;
        _isTasksStarted = true;
        _cancelTokenSource = new CancellationTokenSource();
        _cancelToken = _cancelTokenSource.Token;

        RunTasks(_cancelToken);
    }

    private async void RunTasks(CancellationToken cancelToken)
    {
        var task1 = TimeLog(cancelToken);
        var task2 = UpdateLog(cancelToken);
        var resultTask = await Task.WhenAny(task1, task2);

        Debug.Log("Result: " + resultTask.Result.ToString());
        _isTasksStarted = false;
        _cancelTokenSource.Cancel(); // stop other tasks
        _cancelTokenSource.Dispose();
    }

    public void Cancel()
    {
        _cancelTokenSource.Cancel();
    }

    private async Task<bool> TimeLog(CancellationToken cancelToken)
    {
        var time = 0f;

        while (1 > time)
        {
            if(cancelToken.IsCancellationRequested)
            {
                Debug.Log("Time: " + time.ToString());
                return false;
            }

            await Task.Yield();
            time += Time.deltaTime;
        }

        return true;
    }

    private async Task<bool> UpdateLog(CancellationToken cancelToken)
    {
        var updates = 0;
        while (updates < 310) // примерное число апдейтов на моем ПК за 1 секунду
        {
            if (cancelToken.IsCancellationRequested)
            {
                Debug.Log("Updates: " + updates.ToString());
                return false;
            }

            await Task.Yield();
            updates++;
        }
        
        return false;
    }

    private void OnDestroy()
    {
        _cancelTokenSource.Dispose();
    }
}
