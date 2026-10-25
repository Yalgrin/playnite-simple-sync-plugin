using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleSyncPlugin.Threading
{
    public class SimpleSyncWorker
    {
        private readonly object _gate = new object();
        private readonly Queue<Func<Task>> _queue = new Queue<Func<Task>>();

        private Task _processor = Task.CompletedTask;
        private bool _running;
        private bool _stopped;
        private TaskCompletionSource<bool> _resumeSignal;

        public bool IsPaused { get; private set; }

        public Task Enqueue(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var completion = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            Add(() =>
            {
                try
                {
                    action();
                    completion.SetResult(true);
                }
                catch (Exception exception)
                {
                    completion.SetException(exception);
                }

                return Task.CompletedTask;
            });

            return completion.Task;
        }

        public Task<T> Enqueue<T>(Func<T> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var completion = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            Add(() =>
            {
                try
                {
                    completion.SetResult(function());
                }
                catch (Exception exception)
                {
                    completion.SetException(exception);
                }

                return Task.CompletedTask;
            });

            return completion.Task;
        }

        public Task Enqueue(Func<Task> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var completion = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            Add(async () =>
            {
                try
                {
                    await function().ConfigureAwait(false);
                    completion.SetResult(true);
                }
                catch (Exception exception)
                {
                    completion.SetException(exception);
                }
            });

            return completion.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var completion = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            Add(async () =>
            {
                try
                {
                    var result = await function().ConfigureAwait(false);
                    completion.SetResult(result);
                }
                catch (Exception exception)
                {
                    completion.SetException(exception);
                }
            });

            return completion.Task;
        }

        private void Add(Func<Task> work)
        {
            lock (_gate)
            {
                if (_stopped)
                    throw new InvalidOperationException("The worker has been stopped.");

                _queue.Enqueue(work);

                if (!_running)
                {
                    _running = true;
                    _processor = Task.Run(ProcessQueueAsync);
                }
            }
        }

        private async Task ProcessQueueAsync()
        {
            while (true)
            {
                Func<Task> work = null;
                Task waitForResume = null;

                lock (_gate)
                {
                    if (_queue.Count == 0)
                    {
                        _running = false;
                        return;
                    }

                    if (IsPaused)
                    {
                        waitForResume = _resumeSignal.Task;
                    }
                    else
                    {
                        work = _queue.Dequeue();
                    }
                }

                if (waitForResume != null)
                {
                    await waitForResume.ConfigureAwait(false);
                    continue;
                }

                if (work != null)
                {
                    await work().ConfigureAwait(false);
                }
            }
        }

        public void Pause()
        {
            lock (_gate)
            {
                if (_stopped)
                    throw new InvalidOperationException("The worker has been stopped.");

                if (IsPaused)
                    return;

                IsPaused = true;
                _resumeSignal = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        public void Resume()
        {
            lock (_gate)
            {
                if (!IsPaused)
                    return;

                IsPaused = false;
                _resumeSignal.SetResult(true);
                _resumeSignal = null;
            }
        }

        public Task StopAsync()
        {
            lock (_gate)
            {
                _stopped = true;

                if (IsPaused)
                {
                    IsPaused = false;
                    _resumeSignal.SetResult(true);
                    _resumeSignal = null;
                }

                return _processor;
            }
        }
    }
}