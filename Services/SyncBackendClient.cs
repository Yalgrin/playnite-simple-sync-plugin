using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.SDK;
using SimpleSyncPlugin.Exceptions;
using SimpleSyncPlugin.Extensions;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services
{
    public class SyncBackendClient
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private const string ClientErrorId = "Yalgrin-SimpleSyncPlugin-ClientError";
        private const string HttpErrorId = "Yalgrin-SimpleSyncPlugin-HttpError";
        private const string ForceFetchRequiredId = "Yalgrin-SimpleSyncPlugin-ForceFetchRequired";

        private readonly IPlayniteAPI _api;
        private readonly HttpClient _httpClient;
        private readonly HttpClient _longTimeoutHttpClient;
        private readonly Guid _clientId;

        public string ServerAddress { get; private set; }
        public bool ShouldShutdown { get; private set; }

        public SyncBackendClient(IPlayniteAPI api, string serverAddress, Guid clientId)
        {
            _api = api;
            ServerAddress = serverAddress;
            var baseAddress = new Uri(serverAddress);

            _httpClient = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(20)
            };
            _longTimeoutHttpClient = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromMinutes(60)
            };
            _clientId = clientId;
            ShouldShutdown = false;
        }

        public Task<string> TestConnection()
        {
            return FetchStringUsingGetRequest("/api/health");
        }

        public Task<CategoryDto> GetCategory(long id)
        {
            return GetObject<CategoryDto>(id, "category");
        }

        public Task SaveCategory(CategoryDto dto)
        {
            return SaveObject(dto, "category");
        }

        public Task DeleteCategory(CategoryDto category)
        {
            return DeleteObject(category, "category");
        }

        public Task<GenreDto> GetGenre(long id)
        {
            return GetObject<GenreDto>(id, "genre");
        }

        public Task SaveGenre(GenreDto dto)
        {
            return SaveObject(dto, "genre");
        }

        public Task DeleteGenre(GenreDto dto)
        {
            return DeleteObject(dto, "genre");
        }

        public Task<PlatformDto> GetPlatform(long id)
        {
            return GetObject<PlatformDto>(id, "platform");
        }

        public Task<PlatformDiffDto> GetPlatformDiff(long id)
        {
            return GetObject<PlatformDiffDto>(id, "platform-diff");
        }

        public Task<Tuple<byte[], string>> GetPlatformMetadata(long id, string filename)
        {
            return GetMetadata(id, filename, "platform-metadata");
        }

        public Task SavePlatform(PlatformDto dto, string icon, string coverImage, string backgroundImage)
        {
            return SaveObjectWithMetadata(dto, "platform", icon, coverImage, backgroundImage);
        }

        public Task SavePlatformDiff(PlatformDiffDto dto, string icon, string coverImage, string backgroundImage)
        {
            return SaveObjectDiffWithMetadata(dto, "platform-diff", icon, coverImage, backgroundImage);
        }

        public Task DeletePlatform(PlatformDto dto)
        {
            return DeleteObject(dto, "platform");
        }

        public Task<CompanyDto> GetCompany(long id)
        {
            return GetObject<CompanyDto>(id, "company");
        }

        public Task SaveCompany(CompanyDto dto)
        {
            return SaveObject(dto, "company");
        }

        public Task DeleteCompany(CompanyDto dto)
        {
            return DeleteObject(dto, "company");
        }

        public Task<FeatureDto> GetFeature(long id)
        {
            return GetObject<FeatureDto>(id, "feature");
        }

        public Task SaveFeature(FeatureDto dto)
        {
            return SaveObject(dto, "feature");
        }

        public Task DeleteFeature(FeatureDto dto)
        {
            return DeleteObject(dto, "feature");
        }

        public Task<TagDto> GetTag(long id)
        {
            return GetObject<TagDto>(id, "tag");
        }

        public Task SaveTag(TagDto dto)
        {
            return SaveObject(dto, "tag");
        }

        public Task DeleteTag(TagDto dto)
        {
            return DeleteObject(dto, "tag");
        }

        public Task<SeriesDto> GetSeries(long id)
        {
            return GetObject<SeriesDto>(id, "series");
        }

        public Task SaveSeries(SeriesDto dto)
        {
            return SaveObject(dto, "series");
        }

        public Task DeleteSeries(SeriesDto dto)
        {
            return DeleteObject(dto, "series");
        }

        public Task<AgeRatingDto> GetAgeRating(long id)
        {
            return GetObject<AgeRatingDto>(id, "age-rating");
        }

        public Task SaveAgeRating(AgeRatingDto dto)
        {
            return SaveObject(dto, "age-rating");
        }

        public Task DeleteAgeRating(AgeRatingDto dto)
        {
            return DeleteObject(dto, "age-rating");
        }

        public Task<RegionDto> GetRegion(long id)
        {
            return GetObject<RegionDto>(id, "region");
        }

        public Task SaveRegion(RegionDto dto)
        {
            return SaveObject(dto, "region");
        }

        public Task DeleteRegion(RegionDto dto)
        {
            return DeleteObject(dto, "region");
        }

        public Task<SourceDto> GetSource(long id)
        {
            return GetObject<SourceDto>(id, "source");
        }

        public Task SaveSource(SourceDto dto)
        {
            return SaveObject(dto, "source");
        }

        public Task DeleteSource(SourceDto dto)
        {
            return DeleteObject(dto, "source");
        }

        public Task<CompletionStatusDto> GetCompletionStatus(long id)
        {
            return GetObject<CompletionStatusDto>(id, "completion-status");
        }

        public Task SaveCompletionStatus(CompletionStatusDto dto)
        {
            return SaveObject(dto, "completion-status");
        }

        public Task DeleteCompletionStatus(CompletionStatusDto dto)
        {
            return DeleteObject(dto, "completion-status");
        }

        public Task<FilterPresetDto> GetFilterPreset(long id)
        {
            return GetObject<FilterPresetDto>(id, "filter-preset");
        }

        public Task SaveFilterPreset(FilterPresetDto dto)
        {
            return SaveObject(dto, "filter-preset");
        }

        public Task DeleteFilterPreset(FilterPresetDto dto)
        {
            return DeleteObject(dto, "filter-preset");
        }

        public Task<GameDto> GetGame(long id)
        {
            return GetObject<GameDto>(id, "game");
        }

        public Task<GameDiffDto> GetGameDiff(long id)
        {
            return GetObject<GameDiffDto>(id, "game-diff");
        }

        public Task<Tuple<byte[], string>> GetGameMetadata(long id, string filename)
        {
            return GetMetadata(id, filename, "game-metadata");
        }

        public Task SaveGame(GameDto dto, string icon, string coverImage, string backgroundImage)
        {
            return SaveObjectWithMetadata(dto, "game", icon, coverImage, backgroundImage);
        }

        public Task SaveGameDiff(GameDiffDto dto, string icon, string coverImage, string backgroundImage)
        {
            return SaveObjectDiffWithMetadata(dto, "game-diff", icon, coverImage, backgroundImage);
        }

        public Task DeleteGame(GameDto dto)
        {
            return DeleteObject(dto, "game");
        }

        public async Task<Stream> GetStream(long lastProcessedId)
        {
            try
            {
                return await _longTimeoutHttpClient.GetStreamAsync(
                    $"/api/change/stream?lastChangeId={lastProcessedId}");
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to get stream!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to get stream!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to get stream!");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        public async Task<List<ChangeDto>> FetchAll()
        {
            try
            {
                return await FetchObjectUsingGetRequest<List<ChangeDto>>("/api/change/all");
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to get all objects!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to get all objects!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to get all objects!");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        public async Task<List<ChangeDto>> FetchRemainingChanges(long lastProcessedId)
        {
            try
            {
                return await FetchObjectUsingGetRequest<List<ChangeDto>>($"/api/change?lastChangeId={lastProcessedId}");
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to get remaining changes!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to get remaining changes!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to get remaining changes!");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        public async Task<List<ChangeDto>> FetchGames(GameChangeRequestDto dto)
        {
            try
            {
                return await SendPostRequestWithResult<GameChangeRequestDto, List<ChangeDto>>($"/api/change/games",
                    dto);
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to get remaining changes!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to get remaining changes!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to get remaining changes!");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        private async Task<T> GetObject<T>(long id, string objectPath) where T : class
        {
            try
            {
                return await FetchObjectUsingGetRequest<T>($"/api/{objectPath}/{id}");
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to get object \"{objectPath}\" with id = {id}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to get object \"{objectPath}\" with id = {id}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to get object \"{objectPath}\" with id = {id}");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        private async Task<Tuple<byte[], string>> GetMetadata(long id, string filename, string objectPath)
        {
            try
            {
                return await FetchMetadataUsingGetMethod($"/api/{objectPath}/{id}/{filename}");
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to get metadata \"{objectPath}\" with id = {id} and filename = {filename}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to get metadata \"{objectPath}\" with id = {id} and filename = {filename}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to get metadata \"{objectPath}\" with id = {id} and filename = {filename}");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        private async Task<Tuple<byte[], string>> FetchMetadataUsingGetMethod(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                var resultContent = response.Content;
                var fileName = resultContent.Headers.ContentDisposition.FileName;
                if (fileName != null && fileName.StartsWith("\"") && fileName.EndsWith("\""))
                {
                    fileName = fileName.Substring(1, fileName.Length - 2);
                }

                return new Tuple<byte[], string>(await resultContent.ReadAsByteArrayAsync(), fileName);
            }

            await TryToExtractError(response);
            return null;
        }

        private async Task SaveObject<T>(T entity, string objectPath) where T : AbstractDto
        {
            try
            {
                await SendPostRequest($"/api/{objectPath}/save?clientId={_clientId}", entity);
            }
            catch (ForceFetchRequiredException ex)
            {
                Logger.Error(ex, $"Force fetch required for object \"{objectPath}\" with id = {entity.Id}");
                _api.Notifications.Add(new NotificationMessage(ForceFetchRequiredId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_ForceFetchRequired"), NotificationType.Error));
                throw;
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to save object \"{objectPath}\" with id = {entity.Id}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to save object \"{objectPath}\" with id = {entity.Id}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to save object \"{objectPath}\" with id = {entity.Id}");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        private async Task SaveObjectWithMetadata<T>(T dto,
            string objectPath, string icon, string coverImage, string backgroundImage) where T : AbstractDto
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(CreateJsonContent(dto), "dto");
                AddFileToMultipartRequest(content, icon, "Icon");
                AddFileToMultipartRequest(content, coverImage, "CoverImage");
                AddFileToMultipartRequest(content, backgroundImage, "BackgroundImage");
                await DoSendPostRequest($"/api/{objectPath}/save?clientId={_clientId}", content);
            }
            catch (ForceFetchRequiredException ex)
            {
                Logger.Error(ex, $"Force fetch required for object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(ForceFetchRequiredId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_ForceFetchRequired"), NotificationType.Error));
                throw;
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to save object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to save object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to save object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        private void AddFileToMultipartRequest(MultipartFormDataContent multipartContent, string localFileName,
            string metadataName)
        {
            if (localFileName == null)
            {
                return;
            }

            var fullFilePath = _api.Database.GetFullFilePath(localFileName);
            try
            {
                var bytes = File.ReadAllBytes(fullFilePath);
                multipartContent.Add(new ByteArrayContent(bytes), "files",
                    metadataName + Path.GetExtension(localFileName));
            }
            catch (FileNotFoundException e)
            {
                Logger.Error(e, $"Failed to load file: \"{fullFilePath}\"!");
            }
        }

        private async Task SaveObjectDiffWithMetadata<T>(T dto,
            string objectPath, string icon, string coverImage, string backgroundImage) where T : AbstractDiffDto
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(CreateJsonContent(dto), "dto");
                AddMetadataToDiffMultipartRequest(content, dto, icon, "Icon");
                AddMetadataToDiffMultipartRequest(content, dto, coverImage, "CoverImage");
                AddMetadataToDiffMultipartRequest(content, dto, backgroundImage, "BackgroundImage");
                await DoSendPostRequest($"/api/{objectPath}/save?clientId={_clientId}", content);
            }
            catch (ManualSynchronizationRequiredException ex)
            {
                Logger.Error(ex, $"Manual synchronization required for object \"{objectPath}\" with id = {dto.Id}");
                throw;
            }
            catch (ForceFetchRequiredException ex)
            {
                Logger.Error(ex, $"Force fetch required for object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(ForceFetchRequiredId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_ForceFetchRequired"), NotificationType.Error));
                throw;
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to save object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to save object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to save object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        private void AddMetadataToDiffMultipartRequest(MultipartFormDataContent multipartContent,
            AbstractDiffDto dto, string localFileName, string metadataName)
        {
            if (localFileName == null || !dto.ChangedFields.Contains(metadataName))
            {
                return;
            }

            var fullFilePath = _api.Database.GetFullFilePath(localFileName);
            try
            {
                var bytes = File.ReadAllBytes(fullFilePath);
                multipartContent.Add(new ByteArrayContent(bytes), "files",
                    metadataName + Path.GetExtension(localFileName));
            }
            catch (FileNotFoundException e)
            {
                Logger.Error(e, $"Failed to load file: \"{fullFilePath}\"!");
            }
        }

        private async Task DeleteObject<T>(T dto, string objectPath) where T : AbstractDto
        {
            try
            {
                await SendPostRequest($"/api/{objectPath}/delete?clientId={_clientId}", dto);
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Failed to delete object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Failed to delete object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to delete object \"{objectPath}\" with id = {dto.Id}");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        private async Task<T> FetchObjectUsingGetRequest<T>(string uri) where T : class
        {
            var response = await _httpClient.GetAsync(uri);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(result);
            }

            await TryToExtractError(response);
            return null;
        }

        private async Task<string> FetchStringUsingGetRequest(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return result;
            }

            await TryToExtractError(response);
            return null;
        }

        private Task SendPostRequest<T>(string uri, T bodyObject) where T : class
        {
            return DoSendPostRequest(uri, CreateJsonContent(bodyObject));
        }

        private Task<TR> SendPostRequestWithResult<T, TR>(string uri, T bodyObject) where T : class where TR : class
        {
            return DoSendPostRequestWithResult<TR>(uri, CreateJsonContent(bodyObject));
        }

        private async Task DoSendPostRequest(string uri, HttpContent content)
        {
            var response = await _httpClient.PostAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            await TryToExtractError(response);
        }

        private async Task<T> DoSendPostRequestWithResult<T>(string uri, HttpContent content) where T : class
        {
            var response = await _httpClient.PostAsync(uri, content);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(result);
            }

            await TryToExtractError(response);
            return null;
        }

        private static async Task TryToExtractError(HttpResponseMessage response)
        {
            ErrorDto error = await DeserializeError(response);
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                switch (error?.Message)
                {
                    case "manualSyncRequired":
                        throw new ManualSynchronizationRequiredException();
                    case "forceFetchRequired":
                        throw new ForceFetchRequiredException();
                }
            }

            throw new HttpStatusException(response.StatusCode, error?.Message ?? "Unexpected error!");
        }

        private static async Task<ErrorDto> DeserializeError(HttpResponseMessage response)
        {
            try
            {
                return JsonConvert.DeserializeObject<ErrorDto>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to deserialize error body!");
                return null;
            }
        }

        private static StringContent CreateJsonContent<T>(T dto) where T : class
        {
            var contentToSave = SerializeObject(dto);
            return new StringContent(
                contentToSave,
                Encoding.UTF8,
                "application/json");
        }

        private static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private string GetLocalizedString(string key)
        {
            return _api.GetLocalizedString(key);
        }

        public void Shutdown()
        {
            Logger.Trace("Requesting client shutdown...");
            ShouldShutdown = true;
        }
    }
}