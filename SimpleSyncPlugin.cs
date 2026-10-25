using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using SimpleSyncPlugin.Exceptions;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Services;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin
{
    public class SimpleSyncPlugin : GenericPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private SimpleSyncPluginSettingsViewModel Settings { get; }
        private DataProcessingBroker DataProcessingBroker { get; set; }
        private ServerConnectionThread ServerConnectionThread { get; set; }
        private SyncBackendService SyncBackendService { get; }
        private DataProcessingService DataProcessingService { get; }
        private DataSynchronizationService DataSynchronizationService { get; }
        private SimpleSyncWorker Worker { get; }
        private bool GameAdditionAllowed { get; set; }

        public override Guid Id { get; } = Guid.Parse("9780d304-9b7e-4f83-bd8c-e1838dae2f2e");

        public SimpleSyncPlugin(IPlayniteAPI api) : base(api)
        {
            Settings = new SimpleSyncPluginSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            Logger.Info("Initializing SimpleSyncPlugin...");
            GameAdditionAllowed = true;
            Worker = new SimpleSyncWorker();
            RegisterMainViewListener(api);
            SyncBackendService = new SyncBackendService(api, Settings);
            DataSynchronizationService = new DataSynchronizationService(api, Settings, SyncBackendService);
            DataProcessingService =
                new DataProcessingService(api, Settings, SyncBackendService, DataSynchronizationService);
            DataProcessingBroker = new DataProcessingBroker(DataProcessingService, DataSynchronizationService, Worker);
            DataSynchronizationService.Initialize(DataProcessingBroker, DataProcessingService);
            DataSynchronizationService.RegisterListeners();
        }

        private void RegisterMainViewListener(IPlayniteAPI api)
        {
            try
            {
                var mainView = api.MainView;
                var mainModelField = mainView?.GetType().GetField(
                    "mainModel", BindingFlags.Instance | BindingFlags.NonPublic);
                var mainModel = mainModelField?.GetValue(mainView) as ObservableObject;
                var allowedProperty = mainModel?.GetType().GetProperty(
                    "GameAdditionAllowed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (allowedProperty != null && allowedProperty.PropertyType == typeof(bool))
                {
                    mainModel.PropertyChanged += (sender, args) =>
                    {
                        GameAdditionAllowed = (bool)allowedProperty.GetValue(mainModel);
                        Logger.Trace($"GameAdditionAllowed: {GameAdditionAllowed}");
                        if (GameAdditionAllowed)
                        {
                            Worker.Resume();
                        }
                        else
                        {
                            Worker.Pause();
                        }
                    };
                }
                else
                {
                    Logger.Warn("Unable to observe GameAdditionAllowed: mainModel or property was not found.");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to observe GameAdditionAllowed: {ex}");
            }
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            CheckConnection();
            if (Settings.Settings.SynchronizationEnabled && Settings.Settings.FetchChangesAtStartup)
            {
                ActivateProgressBag("LOC_Yalgrin_SimpleSync_Dialogs_Fetch",
                    progArgs => { DataProcessingService.FetchRemainingChanges(progArgs).GetAwaiter().GetResult(); });
            }

            Logger.Info("Starting connection thread...");
            ServerConnectionThread =
                new ServerConnectionThread(DataProcessingBroker, SyncBackendService, Settings, PlayniteApi);
            ServerConnectionThread.Start();
        }

        private void CheckConnection()
        {
            ActivateProgressBag("LOC_Yalgrin_SimpleSync_Dialogs_CheckConnection",
                progArgs =>
                {
                    try
                    {
                        var checkResultDto = SyncBackendService.CheckConnection(progArgs.CancelToken).GetAwaiter()
                            .GetResult();
                        if (checkResultDto != null)
                        {
                            Logger.Info($"Connection check successful, result {checkResultDto.Result}!");
                            if (!string.IsNullOrEmpty(checkResultDto.DisplayClientName))
                            {
                                Settings.UpdateDisplayName(checkResultDto.DisplayClientName);
                            }

                            if (checkResultDto.RegistrationSpecified && !checkResultDto.RegistrationValid)
                            {
                                SessionManager.CurrentSession = new SessionInfo
                                {
                                    InvalidRegistration = true,
                                    SessionId = SessionManager.CurrentSession?.SessionId,
                                    ServerVersion = SessionManager.CurrentSession?.ServerVersion
                                };
                            }

                            switch (checkResultDto.Result)
                            {
                                case CheckResult.OutdatedClient:
                                    PlayniteApi.Dialogs.ShowErrorMessage(
                                        "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_OutdatedClient",
                                        "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Error");
                                    Settings.MarkAsDisabled();
                                    break;
                                case CheckResult.OutdatedServer:
                                    PlayniteApi.Dialogs.ShowErrorMessage(
                                        "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_OutdatedServer",
                                        "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Error");
                                    Settings.MarkAsDisabled();
                                    break;
                                case CheckResult.Ok:
                                    Logger.Info("Connection test successful!");
                                    break;
                            }
                        }
                        else
                        {
                            Logger.Warn("No result returned. Is the sync server configured?");
                        }
                    }
                    catch (SecurityException ex)
                    {
                        Logger.Error(ex, "Exception while checking the connection!");

                        if (ex.Message == "SecurityException.CLIENT_ALREADY_REGISTERED")
                        {
                            PlayniteApi.Dialogs.ShowErrorMessage(
                                "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_ClientAlreadyConnected",
                                "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Error");
                        }
                        else if (ex.Message == "SecurityException.MISSING_REGISTRATION")
                        {
                            PlayniteApi.Dialogs.ShowErrorMessage(
                                "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_MissingRegistration",
                                "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Error");
                        }
                        else if (ex.Message == "SecurityException.INVALID_REGISTRATION")
                        {
                            PlayniteApi.Dialogs.ShowErrorMessage(
                                "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_InvalidRegistration",
                                "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Error");
                        }
                        else
                        {
                            PlayniteApi.Dialogs.ShowErrorMessage(
                                "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_AuthError",
                                "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Error");
                        }

                        Settings.MarkAsDisabled();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Exception while checking the connection!");

                        PlayniteApi.Dialogs.ShowErrorMessage(
                            "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_UnableToReachServer",
                            "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Error");
                        Settings.MarkAsDisabled();
                    }
                });
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            Worker?.StopAsync().GetAwaiter().GetResult();
            ServerConnectionThread?.Shutdown();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return AddSyncOption("LOC_Yalgrin_SimpleSync_Menu_SyncAll",
                progArgs => Worker.Enqueue(() => DataSynchronizationService.SyncAll(progArgs), progArgs.CancelToken)
                    .GetAwaiter().GetResult());
            yield return AddSyncOption("LOC_Yalgrin_SimpleSync_Menu_SyncFilteredGames",
                progArgs => Worker.Enqueue(() => DataSynchronizationService.SyncGames(progArgs, GetFilteredGameIds()),
                        progArgs.CancelToken).GetAwaiter()
                    .GetResult());
            yield return AddSyncOption("LOC_Yalgrin_SimpleSync_Menu_SyncSelectedGames",
                progArgs => Worker.Enqueue(() => DataSynchronizationService.SyncGames(progArgs, GetSelectedGameIds()),
                        progArgs.CancelToken).GetAwaiter()
                    .GetResult());
            yield return AddSeparator();
            yield return AddFetchOption("LOC_Yalgrin_SimpleSync_Menu_FetchAll",
                progArgs => Worker.Enqueue(() => DataProcessingService.FetchAll(progArgs), progArgs.CancelToken)
                    .GetAwaiter().GetResult());
            yield return AddFetchOption("LOC_Yalgrin_SimpleSync_Menu_FetchFilteredGames",
                progArgs => Worker.Enqueue(() => DataProcessingService.FetchGames(progArgs, GetFilteredRequestDto()),
                        progArgs.CancelToken).GetAwaiter()
                    .GetResult());
            yield return AddFetchOption("LOC_Yalgrin_SimpleSync_Menu_FetchSelectedGames",
                progArgs => Worker.Enqueue(() => DataProcessingService.FetchGames(progArgs, GetSelectedRequestDto()),
                        progArgs.CancelToken).GetAwaiter()
                    .GetResult());
            yield return AddSeparator();
            yield return AddFetchOption("LOC_Yalgrin_SimpleSync_Menu_FetchRemainingChanges",
                progArgs => Worker
                    .Enqueue(() => DataProcessingService.FetchRemainingChanges(progArgs), progArgs.CancelToken)
                    .GetAwaiter().GetResult());
        }

        private List<Guid> GetFilteredGameIds()
        {
            return PlayniteApi.MainView.UIDispatcher.Invoke(() =>
            {
                return PlayniteApi.MainView.FilteredGames.ConvertAll(g => g.Id);
            });
        }

        private List<Guid> GetSelectedGameIds()
        {
            return PlayniteApi.MainView.UIDispatcher.Invoke(() =>
            {
                return PlayniteApi.MainView.SelectedGames.ToList().ConvertAll(g => g.Id);
            });
        }

        private GameChangeRequestDto GetFilteredRequestDto()
        {
            return PlayniteApi.MainView.UIDispatcher.Invoke(() =>
            {
                var ids = new List<string>();
                var gameIds = new List<GameIdsDto>();
                foreach (var game in PlayniteApi.MainView.FilteredGames)
                {
                    ids.Add(game.Id.ToString());
                    gameIds.Add(new GameIdsDto
                    {
                        GameId = game.GameId,
                        PluginId = game.PluginId.ToString()
                    });
                }

                return new GameChangeRequestDto
                {
                    Ids = ids,
                    GameIds = gameIds
                };
            });
        }

        private GameChangeRequestDto GetSelectedRequestDto()
        {
            return PlayniteApi.MainView.UIDispatcher.Invoke(() =>
            {
                var ids = new List<string>();
                var gameIds = new List<GameIdsDto>();
                foreach (var game in PlayniteApi.MainView.SelectedGames)
                {
                    ids.Add(game.Id.ToString());
                    gameIds.Add(new GameIdsDto
                    {
                        GameId = game.GameId,
                        PluginId = game.PluginId.ToString()
                    });
                }

                return new GameChangeRequestDto
                {
                    Ids = ids,
                    GameIds = gameIds
                };
            });
        }

        private MainMenuItem AddSyncOption(string label, Action<GlobalProgressActionArgs> action)
        {
            return AddProgressBarOption(label, "LOC_Yalgrin_SimpleSync_Dialogs_Sync", action);
        }

        private MainMenuItem AddFetchOption(string label, Action<GlobalProgressActionArgs> action)
        {
            return AddProgressBarOption(label, "LOC_Yalgrin_SimpleSync_Dialogs_Fetch",
                action.Invoke);
        }

        private MainMenuItem AddProgressBarOption(string label, string progressBarLabel,
            Action<GlobalProgressActionArgs> action)
        {
            return AddOption(label, actionArgs => ActivateProgressBag(progressBarLabel, action));
        }

        private MainMenuItem AddOption(string label, Action<MainMenuItemActionArgs> action)
        {
            return new MainMenuItem
            {
                Description = ResourceProvider.GetString(label),
                MenuSection = "@Simple Sync",
                Action = action
            };
        }

        private void ActivateProgressBag(string label, Action<GlobalProgressActionArgs> action)
        {
            PlayniteApi.Dialogs.ActivateGlobalProgress(
                action,
                new GlobalProgressOptions(label, true)
                    { IsIndeterminate = false });
        }

        private static MainMenuItem AddSeparator()
        {
            return new MainMenuItem
            {
                MenuSection = "@Simple Sync",
                Description = "-"
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new SimpleSyncPluginSettingsView();
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            //not used
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            //not used
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            //not used
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            //not used
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            //not used
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            //not used
        }
    }
}