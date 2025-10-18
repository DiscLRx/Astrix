namespace PocketExplorer.Web.Utils;

public class TaskDispatch
{
    private readonly int _parallelLimit;
    private readonly List<Task> _parallelTasks = [];
    private readonly List<Task> _tasksToRun;
    private bool _canAddTask = true;

    public TaskDispatch(int parallelLimit, IEnumerable<Task>? tasks = null)
    {
        if (parallelLimit <= 0)
        {
            throw new ArgumentException("parallelLimit should be greater than 0");
        }

        _parallelLimit = parallelLimit;
        _tasksToRun = tasks is null ? [] : [.. tasks];
    }

    public void AddTask(Task t)
    {
        if (!_canAddTask)
        {
            throw new InvalidOperationException("Tasks has already begun");
        }
        _tasksToRun.Add(t);
    }

    public void WaitAll()
    {
        _canAddTask = false;
        foreach (var t in _tasksToRun)
        {
            if (_parallelTasks.Count == _parallelLimit)
            {
                var finishedTaskIndex = Task.WaitAny([.. _parallelTasks]);
                _parallelTasks.RemoveAt(finishedTaskIndex);
            }

            t.Start();
            _parallelTasks.Add(t);
        }

        Task.WaitAll([.. _parallelTasks]);
    }
}