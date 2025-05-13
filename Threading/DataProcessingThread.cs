using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Services;

namespace SimpleSyncPlugin.Threading
{
    public class DataProcessingThread
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly ConcurrentQueue<ChangeDto> _changes = new ConcurrentQueue<ChangeDto>();
        private readonly object _notifyingLock = new object();
        private readonly object _modificationLock = new object();
        private readonly DataProcessingService _service;

        public DataProcessingThread(DataProcessingService service)
        {
            _service = service;
        }

        public bool ShouldShutdown { get; set; }

        public void Start()
        {
            Logger.Info("Starting DataProcessingThread...");
            Task.Run(() =>
            {
                while (!ShouldShutdown)
                {
                    try
                    {
                        if (_changes.TryDequeue(out var dto))
                        {
                            if (ShouldShutdown)
                            {
                                return;
                            }

                            try
                            {
                                LockForProcessing();
                                var processTask = _service.ProcessChange(dto);
                                processTask.Wait();
                            }
                            finally
                            {
                                UnlockForProcessing();
                            }
                        }
                        else
                        {
                            lock (_notifyingLock)
                            {
                                Monitor.Wait(_notifyingLock);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Error in DataProcessingThread");
                    }
                }
            });
        }

        public void LockForProcessing()
        {
            Logger.Trace("Acquiring lock for processing...");
            Monitor.Enter(_modificationLock);
            Logger.Trace("Acquired lock for processing!");
        }

        public void UnlockForProcessing()
        {
            Logger.Trace("Releasing lock for processing...");
            Monitor.Exit(_modificationLock);
            Logger.Trace("Released lock for processing!");
        }

        public void SubmitChange(ChangeDto change)
        {
            _changes.Enqueue(change);
            lock (_notifyingLock)
            {
                Monitor.PulseAll(_notifyingLock);
            }
        }

        public void Shutdown()
        {
            ShouldShutdown = true;
            lock (_notifyingLock)
            {
                Monitor.PulseAll(_notifyingLock);
            }
        }
    }
}