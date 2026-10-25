using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;

namespace SimpleSyncPlugin.Threading
{
    public class SimpleSyncWorker
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly object _gate = new object();
        private readonly Queue<Func<Task>> _queue = new Queue<Func<Task>>();

        private Task _processor = Task.CompletedTask;
        private bool _running;
        private bool _stopped;
        private TaskCompletionSource<bool> _resumeSignal;

        public bool IsPaused { get; private set; }

        public Task Enqueue(Action action, CancellationToken cancellationToken = default)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            return EnqueueCore(token =>
            {
                token.ThrowIfCancellationRequested();
                action();
                return Task.FromResult(true);
            }, cancellationToken);
        }

        public Task<T> Enqueue<T>(Func<T> function, CancellationToken cancellationToken = default)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            return EnqueueCore(token =>
            {
                token.ThrowIfCancellationRequested();
                return Task.FromResult(function());
            }, cancellationToken);
        }

        public Task Enqueue(Func<Task> function, CancellationToken cancellationToken = default)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            return Enqueue(async token =>
            {
                token.ThrowIfCancellationRequested();
                await function().ConfigureAwait(false);
            }, cancellationToken);
        }

        public Task<T> Enqueue<T>(Func<Task<T>> function, CancellationToken cancellationToken = default)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            return EnqueueCore(token => function(), cancellationToken);
        }

        public Task Enqueue(Func<CancellationToken, Task> function, CancellationToken cancellationToken = default)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            return EnqueueCore(async token =>
            {
                await function(token).ConfigureAwait(false);
                return true;
            }, cancellationToken);
        }

        public Task<T> Enqueue<T>(
            Func<CancellationToken, Task<T>> function,
            CancellationToken cancellationToken = default)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            return EnqueueCore(function, cancellationToken);
        }

        private Task<T> EnqueueCore<T>(
            Func<CancellationToken, Task<T>> function,
            CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var registration = cancellationToken.Register(() => completion.TrySetCanceled());

            try
            {
                Add(async () =>
                {
                    try
                    {
                        if (!completion.Task.IsCompleted)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var result = await function(cancellationToken).ConfigureAwait(false);
                            completion.TrySetResult(result);
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        completion.TrySetCanceled();
                    }
                    catch (Exception exception)
                    {
                        completion.TrySetException(exception);
                    }
                    finally
                    {
                        try
                        {
                            registration.Dispose();
                        }
                        catch
                        {
                            Logger.Warn("Error on disposal!");
                        }
                    }
                });
            }
            catch
            {
                try
                {
                    registration.Dispose();
                }
                catch
                {
                    Logger.Warn("Error on disposal!");
                }

                throw;
            }

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

                Logger.Trace("Pausing sync worker");

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

                Logger.Trace("Resuming sync worker");

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