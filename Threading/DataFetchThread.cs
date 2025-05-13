using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.SDK;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Services;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Threading
{
    public class DataFetchThread
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly DataProcessingThread _dataProcessingThread;
        private readonly SyncBackendService _syncBackendService;
        private readonly SimpleSyncPluginSettingsViewModel _settings;

        public DataFetchThread(DataProcessingThread dataProcessingThread, SyncBackendService syncBackendService,
            SimpleSyncPluginSettingsViewModel settings)
        {
            _dataProcessingThread = dataProcessingThread;
            _syncBackendService = syncBackendService;
            _settings = settings;
        }

        private bool ShouldShutdown { get; set; }

        public void Start()
        {
            Logger.Info("Starting DataFetchThread...");
            Task.Run(async () =>
            {
                while (!ShouldShutdown)
                {
                    try
                    {
                        Logger.Trace("Initializing the change stream...");
                        await ConnectToStreamAndFetchMessages();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "DataFetchThread failed");
                        await Task.Delay(5000);
                    }
                }
            });
        }

        private async Task ConnectToStreamAndFetchMessages()
        {
            var syncBackendClient = _syncBackendService.SyncBackendClient;
            if (syncBackendClient == null || !_settings.Settings.SynchronizationEnabled ||
                !_settings.Settings.FetchLiveChanges)
            {
                Logger.Trace("Live syncing is disabled, waiting...");
                await Task.Delay(10000);
                return;
            }

            Logger.Info("Connecting to sync server...");
            var stream = await syncBackendClient.GetStream(_settings.Settings.LastProcessedId);
            if (stream == null)
            {
                Logger.Trace("Failed to acquire stream, waiting...");
                await Task.Delay(1000);
                return;
            }

            using (var streamReader = new StreamReader(stream))
            {
                while (!streamReader.EndOfStream)
                {
                    var message = await streamReader.ReadLineAsync();

                    if (ShouldShutdown)
                    {
                        Logger.Trace("Application is being terminated...");
                        return;
                    }

                    if (syncBackendClient.ShouldShutdown)
                    {
                        Logger.Trace("Client has been terminated...");
                        return;
                    }

                    if (string.IsNullOrEmpty(message))
                    {
                        continue;
                    }

                    Logger.Trace($"Client {syncBackendClient.ServerAddress} - received message: {message}");
                    if (!message.StartsWith("data:"))
                    {
                        continue;
                    }

                    var truncMsg = message.Replace("data:", "");
                    var obj = JsonConvert.DeserializeObject<ChangeDto>(truncMsg);
                    _dataProcessingThread.SubmitChange(obj);
                }
            }

            Logger.Trace("Stream has been terminated...");
        }

        public void Shutdown()
        {
            Logger.Trace("Requesting shutdown...");
            ShouldShutdown = true;
        }
    }
}