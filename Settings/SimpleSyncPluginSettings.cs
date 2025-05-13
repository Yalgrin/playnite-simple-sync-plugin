using System;
using System.Collections.Generic;
using System.Windows.Input;
using Playnite.SDK;
using Playnite.SDK.Data;
using SimpleSyncPlugin.Services;

namespace SimpleSyncPlugin.Settings
{
    public class SimpleSyncPluginSettings : ObservableObject
    {
        private bool _synchronizationEnabled = false;
        private string _syncServerAddress = "http://localhost:8093";
        private long _lastProcessedId = 0;
        private bool _sendLiveChanges = false;
        private bool _fetchLiveChanges = false;
        private bool _fetchChangesAtStartup = false;

        public bool SynchronizationEnabled
        {
            get => _synchronizationEnabled;
            set => SetValue(ref _synchronizationEnabled, value);
        }

        public string SyncServerAddress
        {
            get => _syncServerAddress;
            set => SetValue(ref _syncServerAddress, value);
        }

        public long LastProcessedId
        {
            get => _lastProcessedId;
            set => SetValue(ref _lastProcessedId, value);
        }

        public bool SendLiveChanges
        {
            get => _sendLiveChanges;
            set => SetValue(ref _sendLiveChanges, value);
        }

        public bool FetchLiveChanges
        {
            get => _fetchLiveChanges;
            set => SetValue(ref _fetchLiveChanges, value);
        }

        public bool FetchChangesAtStartup
        {
            get => _fetchChangesAtStartup;
            set => SetValue(ref _fetchChangesAtStartup, value);
        }
    }

    public class SimpleSyncPluginSettingsViewModel : ObservableObject, ISettings
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly SimpleSyncPlugin _plugin;
        private SimpleSyncPluginSettings EditingClone { get; set; }

        private SimpleSyncPluginSettings _settings;

        public SimpleSyncPluginSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public ICommand TestConnectionCommand { get; private set; }

        public SimpleSyncPluginSettingsViewModel(SimpleSyncPlugin plugin)
        {
            this._plugin = plugin;

            var savedSettings = plugin.LoadPluginSettings<SimpleSyncPluginSettings>();

            Settings = savedSettings ?? new SimpleSyncPluginSettings();
            TestConnectionCommand = new RelayCommand(ExecuteTestConnectionCommand);
        }

        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings = EditingClone;
        }

        public void EndEdit()
        {
            _plugin.SavePluginSettings(Settings);
            OnPropertyChanged();
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        public void UpdateLastProcessedId(long? id)
        {
            if (id != null && id > Settings.LastProcessedId)
            {
                Logger.Trace($"Updating last processed id to {id}...");
                BeginEdit();
                Settings.LastProcessedId = (long)id;
                EndEdit();
            }
        }


        private void ExecuteTestConnectionCommand()
        {
            Logger.Info($"Testing connection to server {Settings.SyncServerAddress}...");
            var api = _plugin.PlayniteApi;
            bool connectionOk = false;
            api.Dialogs.ActivateGlobalProgress(async args =>
                {
                    try
                    {
                        var result = await new SyncBackendClient(api, Settings.SyncServerAddress, Guid.Empty)
                            .TestConnection();
                        if (result == "OK")
                        {
                            connectionOk = true;
                        }
                        else
                        {
                            connectionOk = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Exception while checking connection!");
                        connectionOk = false;
                    }
                },
                new GlobalProgressOptions("LOC_Yalgrin_SimpleSync_Dialogs_TestConnection", true)
                    { IsIndeterminate = true });
            if (connectionOk)
            {
                api.Dialogs.ShowMessage("LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Ok",
                    "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Label");
            }
            else
            {
                api.Dialogs.ShowErrorMessage("LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Error",
                    "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Label");
            }
        }
    }
}