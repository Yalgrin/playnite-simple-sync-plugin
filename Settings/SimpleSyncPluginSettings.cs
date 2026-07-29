using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Playnite.SDK;
using Playnite.SDK.Data;
using SimpleSyncPlugin.Extensions;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Services;
using SimpleSyncPlugin.Threading;

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

    public class RegisteredClientInfo : ObservableObject
    {
        private string _clientId = "";
        private string _clientName = "";
        private string _clientToken = "";

        public string ClientId
        {
            get => _clientId;
            set => SetValue(ref _clientId, value);
        }

        public string ClientName
        {
            get => _clientName;
            set => SetValue(ref _clientName, value);
        }

        public string ClientToken
        {
            get => _clientToken;
            set => SetValue(ref _clientToken, value);
        }

        public RegisteredClientInfo Clone()
        {
            return new RegisteredClientInfo()
            {
                _clientId = _clientId,
                _clientName = _clientName,
                _clientToken = _clientToken
            };
        }
    }

    public class SimpleSyncPluginSettingsViewModel : ObservableObject, ISettings
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly SimpleSyncPlugin _plugin;
        private SimpleSyncPluginSettings EditingClone { get; set; }

        private SimpleSyncPluginSettings _settings;
        private RegisteredClientInfo _clientInfo;
        private string _statusMessage;

        public SimpleSyncPluginSettings Settings
        {
            get => _settings;
            private set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public RegisteredClientInfo ClientInfo
        {
            get => _clientInfo;
            private set
            {
                _clientInfo = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetValue(ref _statusMessage, value);
        }

        public ICommand TestConnectionCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand ChangeNameCommand { get; private set; }


        public SimpleSyncPluginSettingsViewModel(SimpleSyncPlugin plugin)
        {
            this._plugin = plugin;

            var savedSettings = plugin.LoadPluginSettings<SimpleSyncPluginSettings>();

            Settings = savedSettings ?? new SimpleSyncPluginSettings();
            ClientInfo = LoadAuthInfo() ?? new RegisteredClientInfo();
            RegisterCommand = new RelayCommand(ExecuteRegisterCommand);
            ChangeNameCommand = new RelayCommand(ExecuteChangeNameCommand);
            TestConnectionCommand = new RelayCommand(ExecuteTestConnectionCommand);

            SessionManager.CurrentSessionChanged += (sender, args) => { UpdateStatusMessage(); };

            UpdateStatusMessage();
        }

        private void UpdateStatusMessage()
        {
            var msg = "";
            if (string.IsNullOrEmpty(ClientInfo.ClientId))
            {
                msg = GetLocalizedString("LOC_Yalgrin_SimpleSync_Settings_NotRegistered");
            }
            else
            {
                msg = string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Settings_RegisteredAs"),
                    ClientInfo.ClientName);
            }

            msg += " ";

            if (SessionManager.CurrentSession?.SessionId != null)
            {
                msg += GetLocalizedString("LOC_Yalgrin_SimpleSync_Settings_SessionExists");
            }
            else
            {
                msg += GetLocalizedString("LOC_Yalgrin_SimpleSync_Settings_NoSession");
            }

            StatusMessage = msg;
        }

        private RegisteredClientInfo LoadAuthInfo()
        {
            var userDataPath = _plugin.GetPluginUserDataPath();
            var authInfoPath = Path.Combine(userDataPath, "AuthInfo.json");
            return File.Exists(authInfoPath) ? Serialization.FromJsonFile<RegisteredClientInfo>(authInfoPath) : null;
        }

        private void SaveAuthInfo(RegisteredClientInfo clientInfo)
        {
            var userDataPath = _plugin.GetPluginUserDataPath();
            var authInfoPath = Path.Combine(userDataPath, "AuthInfo.json");
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }

            var strConf = Serialization.ToJson(clientInfo, true);
            File.WriteAllText(authInfoPath, strConf);
            ClientInfo = clientInfo;
            UpdateStatusMessage();
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

        public void MarkAsDisabled()
        {
            Logger.Debug("Disabling the synchronization...");
            BeginEdit();
            Settings.SynchronizationEnabled = false;
            EndEdit();
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

        private void ExecuteRegisterCommand()
        {
            var api = _plugin.PlayniteApi;
            if (!string.IsNullOrEmpty(ClientInfo?.ClientId))
            {
                var messageBoxResult = api.Dialogs.ShowMessage(
                    "LOC_Yalgrin_SimpleSync_Dialogs_Register_AlreadyRegistered",
                    "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Label",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var result = api.Dialogs.SelectString("LOC_Yalgrin_SimpleSync_Dialogs_Register_EnterName",
                "LOC_Yalgrin_SimpleSync_Dialogs_Register_Caption", ClientInfo?.ClientName ?? Environment.MachineName);
            if (!result.Result)
            {
                return;
            }

            var stringResult = result.SelectedString;
            if (string.IsNullOrEmpty(stringResult))
            {
                return;
            }

            var success = false;
            api.Dialogs.ActivateGlobalProgress(async args =>
                {
                    var client = new SyncBackendClient(api, Settings.SyncServerAddress, null);
                    try
                    {
                        var clientDto = await client.RegisterClient(new RegistrationRequestDto
                            { DisplayName = stringResult });
                        SaveAuthInfo(new RegisteredClientInfo
                        {
                            ClientId = clientDto.ClientId,
                            ClientName = clientDto.DisplayName,
                            ClientToken = clientDto.ClientToken
                        });
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Failed to register client {stringResult}.");
                        success = false;
                    }
                },
                new GlobalProgressOptions("LOC_Yalgrin_SimpleSync_Dialogs_Register_Progress", true)
                    { IsIndeterminate = true });
            if (!success)
            {
                api.Dialogs.ShowErrorMessage("LOC_Yalgrin_SimpleSync_Dialogs_Register_Error",
                    "LOC_Yalgrin_SimpleSync_Dialogs_Register_Caption");
            }
        }

        private void ExecuteChangeNameCommand()
        {
            var api = _plugin.PlayniteApi;
            if (string.IsNullOrEmpty(ClientInfo?.ClientId))
            {
                api.Dialogs.ShowErrorMessage("LOC_Yalgrin_SimpleSync_Dialogs_ChangeName_NotRegistered",
                    "LOC_Yalgrin_SimpleSync_Dialogs_ChangeName_Caption");
                return;
            }

            var result = api.Dialogs.SelectString("LOC_Yalgrin_SimpleSync_Dialogs_ChangeName_EnterName",
                "LOC_Yalgrin_SimpleSync_Dialogs_ChangeName_Caption", ClientInfo?.ClientName ?? Environment.MachineName);
            if (!result.Result)
            {
                return;
            }

            var stringResult = result.SelectedString;
            if (string.IsNullOrEmpty(stringResult))
            {
                return;
            }

            var success = false;
            api.Dialogs.ActivateGlobalProgress(async args =>
                {
                    var client = new SyncBackendClient(api, Settings.SyncServerAddress, ClientInfo);
                    try
                    {
                        await client.ChangeName(stringResult);
                        SaveAuthInfo(new RegisteredClientInfo
                        {
                            ClientId = ClientInfo.ClientId,
                            ClientName = stringResult,
                            ClientToken = ClientInfo.ClientToken
                        });
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Failed to change client name to {stringResult}.");
                        success = false;
                    }
                },
                new GlobalProgressOptions("LOC_Yalgrin_SimpleSync_Dialogs_ChangeName_Progress", true)
                    { IsIndeterminate = true });
            if (!success)
            {
                api.Dialogs.ShowErrorMessage("LOC_Yalgrin_SimpleSync_Dialogs_ChangeName_Error",
                    "LOC_Yalgrin_SimpleSync_Dialogs_ChangeName_Caption");
            }
        }


        private void ExecuteTestConnectionCommand()
        {
            var api = _plugin.PlayniteApi;
            if (string.IsNullOrEmpty(ClientInfo?.ClientId))
            {
                api.Dialogs.ShowErrorMessage("LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_NotRegistered",
                    "LOC_Yalgrin_SimpleSync_Dialogs_TestConnection_Label");
                return;
            }

            Logger.Info($"Testing connection to server {Settings.SyncServerAddress}...");
            CheckResult? checkResult = null;
            api.Dialogs.ActivateGlobalProgress(async args =>
                {
                    try
                    {
                        var result = await new SyncBackendClient(api, Settings.SyncServerAddress, ClientInfo)
                            .CheckConnection();
                        checkResult = result?.Result;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Exception while checking connection!");
                        checkResult = null;
                    }
                },
                new GlobalProgressOptions("LOC_Yalgrin_SimpleSync_Dialogs_TestConnection", true)
                    { IsIndeterminate = true });
            if (checkResult != null && checkResult == CheckResult.Ok)
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

        private string GetLocalizedString(string key)
        {
            return _plugin.PlayniteApi.GetLocalizedString(key);
        }
    }
}