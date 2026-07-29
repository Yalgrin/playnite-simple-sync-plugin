using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using SimpleSyncPlugin.Exceptions;
using SimpleSyncPlugin.Extensions;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Services;
using SimpleSyncPlugin.Settings;
using static SimpleSyncPlugin.Commons.MessageConstants;

namespace SimpleSyncPlugin.Threading
{
    public class ServerConnectionThread
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly DataProcessingThread _dataProcessingThread;
        private readonly SyncBackendService _syncBackendService;
        private readonly SimpleSyncPluginSettingsViewModel _settings;
        private readonly IPlayniteAPI _api;
        private readonly object _streamLock = new object();

        private CancellationTokenSource _shutdownCts;
        private CancellationTokenSource _interruptCts;
        private Task _workerTask;
        private Stream _currentStream;

        public ServerConnectionThread(DataProcessingThread dataProcessingThread,
            SyncBackendService syncBackendService,
            SimpleSyncPluginSettingsViewModel settings, IPlayniteAPI playniteApi)
        {
            _dataProcessingThread = dataProcessingThread;
            _syncBackendService = syncBackendService;
            _settings = settings;
            _api = playniteApi;

            _settings.PropertyChanged += (sender, args) => { TriggerInterrupt(); };
        }

        public void Start()
        {
            if (_workerTask != null && !_workerTask.IsCompleted)
            {
                Logger.Trace("Server connection thread is already running.");
                return;
            }

            Logger.Info("Starting server connection thread...");
            _shutdownCts = new CancellationTokenSource();
            var cancellationToken = _shutdownCts.Token;

            _workerTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await TryToConnect(cancellationToken);
                }
            }, cancellationToken);
        }

        private async Task TryToConnect(CancellationToken cancellationToken)
        {
            CancellationToken? clientToken = null;
            CancellationTokenSource interruptTokenSource = null;
            try
            {
                Logger.Trace("Initializing the connection...");

                cancellationToken.ThrowIfCancellationRequested();

                interruptTokenSource = GenerateNewInterruptSource();

                var syncBackendClient = _syncBackendService.SyncBackendClient;
                if (syncBackendClient == null || !_settings.Settings.SynchronizationEnabled)
                {
                    clientToken = MergeTokens(syncBackendClient?.ShutdownToken, interruptTokenSource.Token);
                    Logger.Trace("Synchronization is disabled, waiting...");
                    await Task.Delay(60000, MergeTokens(cancellationToken, clientToken));
                    return;
                }

                clientToken = MergeTokens(syncBackendClient.ShutdownToken, interruptTokenSource.Token);

                await ConnectToStreamAndFetchMessages(syncBackendClient, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Logger.Trace("Server connection thread was cancelled.");
            }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested ||
                                                  (clientToken != null && clientToken.Value.IsCancellationRequested))
            {
                Logger.Trace("Stream was disposed during shutdown.");
            }
            catch (IOException) when (cancellationToken.IsCancellationRequested ||
                                      (clientToken != null && clientToken.Value.IsCancellationRequested))
            {
                Logger.Trace("Connection was closed.");
            }
            catch (Exception e)
            {
                Logger.Error(e, "ServerConnectionThread failed");
                if (clientToken == null || !clientToken.Value.IsCancellationRequested)
                {
                    CancellationToken token;
                    if (clientToken != null)
                    {
                        token = MergeTokens(cancellationToken, clientToken.Value);
                    }
                    else
                    {
                        token = cancellationToken;
                    }

                    token = MergeTokens(token, interruptTokenSource?.Token ?? CancellationToken.None);

                    await Task.Delay(5000, token);
                }
            }
            finally
            {
                SessionManager.CurrentSession = null;
            }
        }

        private async Task ConnectToStreamAndFetchMessages(SyncBackendClient syncBackendClient,
            CancellationToken cancellationToken)
        {
            Logger.Info("Connecting to sync server...");
            var stream = await DoConnect(syncBackendClient, cancellationToken);
            if (stream == null)
            {
                Logger.Trace("Failed to acquire stream, waiting...");
                await Task.Delay(1000, cancellationToken);
                return;
            }

            lock (_streamLock)
            {
                _currentStream = stream;
            }

            try
            {
                using (stream)
                using (var streamReader = new StreamReader(stream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var message = await streamReader.ReadLineAsync();

                        cancellationToken.ThrowIfCancellationRequested();

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

                        var jObject = JObject.Parse(truncMsg);
                        var messageType = (string)jObject["messageType"];

                        switch (messageType)
                        {
                            case "INITIALIZATION":
                            {
                                var obj = JsonConvert.DeserializeObject<InitializationMessage>(truncMsg);
                                Logger.Debug("Received initialization message. Settings the session.");
                                SessionManager.CurrentSession = new SessionInfo
                                {
                                    SessionId = obj.SessionId
                                };
                                break;
                            }
                            case "CHANGE":
                            {
                                var obj = JsonConvert.DeserializeObject<ChangeMessage>(truncMsg);
                                _dataProcessingThread.SubmitChange(obj);
                                break;
                            }
                        }
                    }
                }

                SessionManager.CurrentSession = null;
                Logger.Trace("Stream has been terminated...");
            }
            finally
            {
                lock (_streamLock)
                {
                    if (ReferenceEquals(_currentStream, stream))
                    {
                        _currentStream = null;
                    }
                }
            }
        }

        private async Task<Stream> DoConnect(SyncBackendClient syncBackendClient, CancellationToken cancellationToken)
        {
            try
            {
                return await syncBackendClient.Connect(_settings.Settings.LastProcessedId, cancellationToken);
            }
            catch (AuthException ex)
            {
                Logger.Error(ex, "Exception while checking the connection!");
                await _api.MainView.UIDispatcher.InvokeAsync(() =>
                {
                    _api.Dialogs.ShowErrorMessage(
                        ex.Message == "AuthException.CLIENT_ALREADY_REGISTERED"
                            ? "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_ClientAlreadyConnected"
                            : "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_AuthError",
                        "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_PassiveAuthCaption");
                });
                _settings.MarkAsDisabled();
                throw;
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to connect!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to connect!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to connect!");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        private string GetLocalizedString(string key)
        {
            return _api.GetLocalizedString(key);
        }

        public void Shutdown()
        {
            Logger.Trace("Requesting shutdown...");

            var cts = _shutdownCts;
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }

            lock (_streamLock)
            {
                _currentStream?.Dispose();
                _currentStream = null;
            }
        }

        private void TriggerInterrupt()
        {
            try
            {
                _interruptCts?.Cancel();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to cancel token source!");
            }
        }

        private CancellationTokenSource GenerateNewInterruptSource()
        {
            try
            {
                _interruptCts?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to dispose token source!");
            }

            _interruptCts = new CancellationTokenSource();
            return _interruptCts;
        }

        private static CancellationToken MergeTokens(CancellationToken? firstToken, CancellationToken? otherToken)
        {
            return CancellationTokenSource
                .CreateLinkedTokenSource(firstToken ?? CancellationToken.None, otherToken ?? CancellationToken.None)
                .Token;
        }
    }
}