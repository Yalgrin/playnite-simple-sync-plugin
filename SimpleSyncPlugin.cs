using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
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
        private DataProcessingThread DataProcessingThread { get; set; }
        private DataFetchThread DataFetchThread { get; set; }
        private SyncBackendService SyncBackendService { get; }
        private DataProcessingService DataProcessingService { get; }
        private DataSynchronizationService DataSynchronizationService { get; }

        public override Guid Id { get; } = Guid.Parse("9780d304-9b7e-4f83-bd8c-e1838dae2f2e");

        public SimpleSyncPlugin(IPlayniteAPI api) : base(api)
        {
            Settings = new SimpleSyncPluginSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            Logger.Info("Initializing SimpleSyncPlugin...");
            SyncBackendService = new SyncBackendService(api, Settings);
            DataSynchronizationService = new DataSynchronizationService(api, Settings, SyncBackendService);
            DataProcessingService =
                new DataProcessingService(api, Settings, SyncBackendService, DataSynchronizationService);
            DataSynchronizationService.RegisterListeners();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (Settings.Settings.FetchChangesAtStartup)
            {
                ActivateProgressBag("LOC_Yalgrin_SimpleSync_Dialogs_Fetch",
                    progArgs =>
                    {
                        FetchWithLocks(() => DataProcessingService.FetchRemainingChanges(progArgs).Wait());
                    });
            }

            Logger.Info("Starting processing and fetch threads...");
            DataProcessingThread = new DataProcessingThread(DataProcessingService);
            DataProcessingThread.Start();
            DataFetchThread = new DataFetchThread(DataProcessingThread, SyncBackendService, Settings);
            DataFetchThread.Start();
        }


        private void FetchWithLocks(Action action)
        {
            try
            {
                DataProcessingThread?.LockForProcessing();
                action.Invoke();
            }
            finally
            {
                DataProcessingThread?.UnlockForProcessing();
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            DataProcessingThread?.Shutdown();
            DataFetchThread?.Shutdown();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return AddSyncOption("LOC_Yalgrin_SimpleSync_Menu_SyncAll",
                progArgs => DataSynchronizationService.SyncAll(progArgs).Wait());
            yield return AddSyncOption("LOC_Yalgrin_SimpleSync_Menu_SyncFilteredGames",
                progArgs => DataSynchronizationService.SyncGames(progArgs, GetFilteredGameIds()).Wait());
            yield return AddSyncOption("LOC_Yalgrin_SimpleSync_Menu_SyncSelectedGames",
                progArgs => DataSynchronizationService.SyncGames(progArgs, GetSelectedGameIds()).Wait());
            yield return AddSeparator();
            yield return AddFetchOption("LOC_Yalgrin_SimpleSync_Menu_FetchAll",
                progArgs => DataProcessingService.FetchAll(progArgs).Wait());
            yield return AddFetchOption("LOC_Yalgrin_SimpleSync_Menu_FetchFilteredGames",
                progArgs => DataProcessingService.FetchGames(progArgs, GetFilteredRequestDto()).Wait());
            yield return AddFetchOption("LOC_Yalgrin_SimpleSync_Menu_FetchSelectedGames",
                progArgs => DataProcessingService.FetchGames(progArgs, GetSelectedRequestDto()).Wait());
            yield return AddSeparator();
            yield return AddFetchOption("LOC_Yalgrin_SimpleSync_Menu_FetchRemainingChanges",
                progArgs => DataProcessingService.FetchRemainingChanges(progArgs).Wait());
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
                progArgs => FetchWithLocks(() => action.Invoke(progArgs)));
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