using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.SDK;
using SimpleSyncPlugin.Exceptions;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services
{
    public class SyncBackendClient
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private const int SupportedApiVersion = 1;

        private readonly IPlayniteAPI _api;
        private readonly HttpClient _httpClient;
        private readonly HttpClient _longTimeoutHttpClient;
        private readonly CancellationTokenSource _shutdownCts;
        private readonly List<Stream> _heldStreams = new List<Stream>();

        public string ServerAddress { get; private set; }
        public RegisteredClientInfo ClientInfo { get; private set; }
        public bool ShouldShutdown { get; private set; }
        public CancellationToken ShutdownToken => _shutdownCts.Token;

        public SyncBackendClient(IPlayniteAPI api, string serverAddress, RegisteredClientInfo clientInfo)
        {
            _api = api;
            ServerAddress = serverAddress;
            ClientInfo = clientInfo;
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

            if (clientInfo != null)
            {
                _httpClient.DefaultRequestHeaders.Add("X-Client-Id", clientInfo.ClientId);
                _httpClient.DefaultRequestHeaders.Add("X-Client-Token", clientInfo.ClientToken);
                _longTimeoutHttpClient.DefaultRequestHeaders.Add("X-Client-Id", clientInfo.ClientId);
                _longTimeoutHttpClient.DefaultRequestHeaders.Add("X-Client-Token", clientInfo.ClientToken);
            }

            _shutdownCts = new CancellationTokenSource();
            ShouldShutdown = false;
        }

        public Task<RegisteredClientDto> RegisterClient(RegistrationRequestDto requestDto,
            CancellationToken cancellationToken = default)
        {
            Logger.Debug(
                $"Registering client {requestDto.DisplayName} with supported API version {SupportedApiVersion}...");
            var request = new RegistrationRequestDto
            {
                DisplayName = requestDto.DisplayName,
                SupportedApiVersion = SupportedApiVersion
            };
            return DoJsonRequest<RegisteredClientDto>(HttpMethod.Post, "/api/client/register", cancellationToken,
                request);
        }

        public Task<CheckResultDto> CheckConnection(CancellationToken cancellationToken = default)
        {
            Logger.Debug("Checking connection...");
            var request = new CheckRequestDto { SupportedApiVersion = SupportedApiVersion };
            return DoJsonRequest<CheckResultDto>(HttpMethod.Post, "/api/client/check",
                cancellationToken, request);
        }

        public Task EnableChangeStream(CancellationToken cancellationToken = default)
        {
            Logger.Debug("Enabling change stream...");
            return DoJsonRequest(HttpMethod.Post, "/api/client/enable-change-stream", cancellationToken);
        }

        public Task DisableChangeStream(CancellationToken cancellationToken = default)
        {
            Logger.Debug("Disabling change stream...");
            return DoJsonRequest(HttpMethod.Post, "/api/client/disable-change-stream", cancellationToken);
        }

        public Task<CategoryDto> GetCategory(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<CategoryDto>(id, "category", cancellationToken);
        }

        public Task SaveCategory(CategoryDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "category", cancellationToken);
        }

        public Task DeleteCategory(CategoryDto category, CancellationToken cancellationToken = default)
        {
            return DeleteObject(category, "category", cancellationToken);
        }

        public Task<GenreDto> GetGenre(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<GenreDto>(id, "genre", cancellationToken);
        }

        public Task SaveGenre(GenreDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "genre", cancellationToken);
        }

        public Task DeleteGenre(GenreDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "genre", cancellationToken);
        }

        public Task<PlatformDto> GetPlatform(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<PlatformDto>(id, "platform", cancellationToken);
        }

        public Task<PlatformDiffDto> GetPlatformDiff(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<PlatformDiffDto>(id, "platform-diff", cancellationToken);
        }

        public Task<Tuple<byte[], string>> GetPlatformMetadata(long id, string filename,
            CancellationToken cancellationToken = default)
        {
            return GetMetadata(id, filename, "platform-metadata", cancellationToken);
        }

        public Task SavePlatform(PlatformDto dto, string icon, string coverImage, string backgroundImage,
            CancellationToken cancellationToken = default)
        {
            return SaveObjectWithMetadata(dto, "platform", icon, coverImage, backgroundImage, cancellationToken);
        }

        public Task SavePlatformDiff(PlatformDiffDto dto, string icon, string coverImage, string backgroundImage,
            CancellationToken cancellationToken = default)
        {
            return SaveObjectDiffWithMetadata(dto, "platform-diff", icon, coverImage, backgroundImage,
                cancellationToken);
        }

        public Task DeletePlatform(PlatformDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "platform", cancellationToken);
        }

        public Task<CompanyDto> GetCompany(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<CompanyDto>(id, "company", cancellationToken);
        }

        public Task SaveCompany(CompanyDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "company", cancellationToken);
        }

        public Task DeleteCompany(CompanyDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "company", cancellationToken);
        }

        public Task<FeatureDto> GetFeature(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<FeatureDto>(id, "feature", cancellationToken);
        }

        public Task SaveFeature(FeatureDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "feature", cancellationToken);
        }

        public Task DeleteFeature(FeatureDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "feature", cancellationToken);
        }

        public Task<TagDto> GetTag(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<TagDto>(id, "tag", cancellationToken);
        }

        public Task SaveTag(TagDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "tag", cancellationToken);
        }

        public Task DeleteTag(TagDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "tag", cancellationToken);
        }

        public Task<SeriesDto> GetSeries(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<SeriesDto>(id, "series", cancellationToken);
        }

        public Task SaveSeries(SeriesDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "series", cancellationToken);
        }

        public Task DeleteSeries(SeriesDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "series", cancellationToken);
        }

        public Task<AgeRatingDto> GetAgeRating(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<AgeRatingDto>(id, "age-rating", cancellationToken);
        }

        public Task SaveAgeRating(AgeRatingDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "age-rating", cancellationToken);
        }

        public Task DeleteAgeRating(AgeRatingDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "age-rating", cancellationToken);
        }

        public Task<RegionDto> GetRegion(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<RegionDto>(id, "region", cancellationToken);
        }

        public Task SaveRegion(RegionDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "region", cancellationToken);
        }

        public Task DeleteRegion(RegionDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "region", cancellationToken);
        }

        public Task<SourceDto> GetSource(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<SourceDto>(id, "source", cancellationToken);
        }

        public Task SaveSource(SourceDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "source", cancellationToken);
        }

        public Task DeleteSource(SourceDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "source", cancellationToken);
        }

        public Task<CompletionStatusDto> GetCompletionStatus(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<CompletionStatusDto>(id, "completion-status", cancellationToken);
        }

        public Task SaveCompletionStatus(CompletionStatusDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "completion-status", cancellationToken);
        }

        public Task DeleteCompletionStatus(CompletionStatusDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "completion-status", cancellationToken);
        }

        public Task<FilterPresetDto> GetFilterPreset(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<FilterPresetDto>(id, "filter-preset", cancellationToken);
        }

        public Task SaveFilterPreset(FilterPresetDto dto, CancellationToken cancellationToken = default)
        {
            return SaveObject(dto, "filter-preset", cancellationToken);
        }

        public Task DeleteFilterPreset(FilterPresetDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "filter-preset", cancellationToken);
        }

        public Task<GameDto> GetGame(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<GameDto>(id, "game", cancellationToken);
        }

        public Task<GameDiffDto> GetGameDiff(long id, CancellationToken cancellationToken = default)
        {
            return GetObject<GameDiffDto>(id, "game-diff", cancellationToken);
        }

        public Task<Tuple<byte[], string>> GetGameMetadata(long id, string filename,
            CancellationToken cancellationToken = default)
        {
            return GetMetadata(id, filename, "game-metadata", cancellationToken);
        }

        public Task SaveGame(GameDto dto, string icon, string coverImage, string backgroundImage,
            CancellationToken cancellationToken = default)
        {
            return SaveObjectWithMetadata(dto, "game", icon, coverImage, backgroundImage, cancellationToken);
        }

        public Task SaveGameDiff(GameDiffDto dto, string icon, string coverImage, string backgroundImage,
            CancellationToken cancellationToken = default)
        {
            return SaveObjectDiffWithMetadata(dto, "game-diff", icon, coverImage, backgroundImage, cancellationToken);
        }

        public Task DeleteGame(GameDto dto, CancellationToken cancellationToken = default)
        {
            return DeleteObject(dto, "game", cancellationToken);
        }

        public Task<Stream> Connect(long lastProcessedId, CancellationToken cancellationToken = default)
        {
            Logger.Debug($"Connecting to the SSE endpoint...");
            return DoStreamRequest(HttpMethod.Post, $"/api/client/connect?lastChangeId={lastProcessedId}",
                cancellationToken);
        }

        public Task<List<ChangeMessage>> FetchAll(CancellationToken cancellationToken = default)
        {
            Logger.Debug($"Fetching all changes...");
            return DoJsonRequest<List<ChangeMessage>>(HttpMethod.Get, "/api/change/all", cancellationToken);
        }

        public Task<List<ChangeMessage>> FetchRemainingChanges(long lastProcessedId,
            CancellationToken cancellationToken = default)
        {
            Logger.Debug($"Fetching remaining changes (last processed id: {lastProcessedId})...");
            return DoJsonRequest<List<ChangeMessage>>(HttpMethod.Get, $"/api/change?lastChangeId={lastProcessedId}",
                cancellationToken);
        }

        public Task<List<ChangeMessage>> FetchGames(GameChangeRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            return DoJsonRequest<List<ChangeMessage>>(HttpMethod.Post, $"/api/change/games", cancellationToken, dto);
        }

        private Task<T> GetObject<T>(long id, string objectPath, CancellationToken cancellationToken = default)
            where T : class
        {
            Logger.Debug($"Fetching object \"{objectPath}\" with id = \"{id}\"");
            return DoJsonRequest<T>(HttpMethod.Get, $"/api/{objectPath}/{id}", cancellationToken);
        }

        private Task<Tuple<byte[], string>> GetMetadata(long id, string filename, string objectPath,
            CancellationToken cancellationToken = default)
        {
            Logger.Debug($"Fetching file \"{filename}\" for object \"{objectPath}\" with id = \"{id}\"");
            return DoMetadataRequest(HttpMethod.Get, $"/api/{objectPath}/{id}/{filename}", cancellationToken);
        }

        private Task SaveObject<T>(T entity, string objectPath, CancellationToken cancellationToken = default)
            where T : AbstractDto
        {
            Logger.Debug($"Saving object \"{objectPath}\" with id = \"{entity.Id}\"");
            return DoJsonRequest(HttpMethod.Post, $"/api/{objectPath}/save", cancellationToken, entity);
        }

        private Task SaveObjectWithMetadata<T>(T dto,
            string objectPath, string icon, string coverImage, string backgroundImage,
            CancellationToken cancellationToken = default) where T : AbstractDto
        {
            Logger.Debug($"Saving object \"{objectPath}\" with id = \"{dto.Id}\" including metadata");
            var content = new MultipartFormDataContent();
            content.Add(CreateJsonContent(dto), "dto");
            AddFileToMultipartRequest(content, icon, "Icon");
            AddFileToMultipartRequest(content, coverImage, "CoverImage");
            AddFileToMultipartRequest(content, backgroundImage, "BackgroundImage");
            return DoRequest(HttpMethod.Post, $"/api/{objectPath}/save", cancellationToken, content);
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

        private Task SaveObjectDiffWithMetadata<T>(T dto,
            string objectPath, string icon, string coverImage, string backgroundImage,
            CancellationToken cancellationToken = default) where T : AbstractDiffDto
        {
            Logger.Debug($"Saving object diff \"{objectPath}\" with id = \"{dto.Id}\" including metadata");
            var content = new MultipartFormDataContent();
            content.Add(CreateJsonContent(dto), "dto");
            AddMetadataToDiffMultipartRequest(content, dto, icon, "Icon");
            AddMetadataToDiffMultipartRequest(content, dto, coverImage, "CoverImage");
            AddMetadataToDiffMultipartRequest(content, dto, backgroundImage, "BackgroundImage");
            return DoRequest(HttpMethod.Post, $"/api/{objectPath}/save", cancellationToken, content);
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

        private Task DeleteObject<T>(T dto, string objectPath, CancellationToken cancellationToken = default)
            where T : AbstractDto
        {
            Logger.Debug($"Deleting object \"{objectPath}\" with id = \"{dto.Id}\"");
            return DoJsonRequest(HttpMethod.Post, $"/api/{objectPath}/delete", cancellationToken, dto);
        }

        private async Task<Tuple<byte[], string>> DoMetadataRequest(HttpMethod method, string uri,
            CancellationToken cancellationToken = default, object bodyObject = null)
        {
            var request = new HttpRequestMessage(method, uri);
            var sessionId = SessionManager.CurrentSession?.SessionId;
            if (sessionId != null)
            {
                request.Headers.Add("X-Session-Id", sessionId);
            }

            if (bodyObject != null)
            {
                request.Content = CreateJsonContent(bodyObject);
            }

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCts.Token, cancellationToken);
            var mergedToken = linkedCts.Token;
            var response = await _httpClient.SendAsync(request, mergedToken);
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

        private async Task<Stream> DoStreamRequest(HttpMethod method, string uri,
            CancellationToken cancellationToken = default, object bodyObject = null)
        {
            var request = new HttpRequestMessage(method, uri);
            var sessionId = SessionManager.CurrentSession?.SessionId;
            if (sessionId != null)
            {
                request.Headers.Add("X-Session-Id", sessionId);
            }

            if (bodyObject != null)
            {
                request.Content = CreateJsonContent(bodyObject);
            }

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCts.Token, cancellationToken);
            CancellationToken mergedToken = linkedCts.Token;
            var response =
                await _longTimeoutHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                    mergedToken);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                lock (_heldStreams)
                {
                    _heldStreams.Add(stream);
                }

                return stream;
            }

            await TryToExtractError(response);
            return null;
        }

        private async Task DoJsonRequest(HttpMethod method, string uri, CancellationToken cancellationToken = default,
            object bodyObject = null)
        {
            var request = new HttpRequestMessage(method, uri);
            var sessionId = SessionManager.CurrentSession?.SessionId;
            if (sessionId != null)
            {
                request.Headers.Add("X-Session-Id", sessionId);
            }

            if (bodyObject != null)
            {
                request.Content = CreateJsonContent(bodyObject);
            }

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCts.Token, cancellationToken);
            var response = await _httpClient.SendAsync(request, linkedCts.Token);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            await TryToExtractError(response);
        }

        private async Task<T> DoJsonRequest<T>(HttpMethod method, string uri,
            CancellationToken cancellationToken = default,
            object bodyObject = null) where T : class
        {
            var request = new HttpRequestMessage(method, uri);
            var sessionId = SessionManager.CurrentSession?.SessionId;
            if (sessionId != null)
            {
                request.Headers.Add("X-Session-Id", sessionId);
            }

            if (bodyObject != null)
            {
                request.Content = CreateJsonContent(bodyObject);
            }

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCts.Token, cancellationToken);
            var response = await _httpClient.SendAsync(request, linkedCts.Token);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return string.IsNullOrEmpty(result) ? null : JsonConvert.DeserializeObject<T>(result);
            }

            await TryToExtractError(response);
            return null;
        }

        private async Task DoRequest(HttpMethod method, string uri, CancellationToken cancellationToken = default,
            HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, uri);
            var sessionId = SessionManager.CurrentSession?.SessionId;
            if (sessionId != null)
            {
                request.Headers.Add("X-Session-Id", sessionId);
            }

            if (content != null)
            {
                request.Content = content;
            }

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCts.Token, cancellationToken);
            var response = await _httpClient.SendAsync(request, linkedCts.Token);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            await TryToExtractError(response);
        }

        private static StringContent CreateJsonContent<T>(T dto) where T : class
        {
            var contentToSave = SerializeObject(dto);
            return new StringContent(
                contentToSave,
                Encoding.UTF8,
                "application/json");
        }

        private static async Task TryToExtractError(HttpResponseMessage response)
        {
            var error = await DeserializeError(response);
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
            else if (response.StatusCode == HttpStatusCode.Unauthorized ||
                     response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new AuthException(response.StatusCode.ToString());
            }

            throw new HttpStatusException(response.StatusCode, error?.Message ?? "Unexpected error!");
        }

        private static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private static async Task<ErrorDto> DeserializeError(HttpResponseMessage response)
        {
            try
            {
                return JsonConvert.DeserializeObject<ErrorDto>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to deserialize error body!");
                return null;
            }
        }

        public void Shutdown()
        {
            Logger.Trace("Requesting client shutdown...");

            ShouldShutdown = true;

            var cts = _shutdownCts;
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }

            lock (_heldStreams)
            {
                foreach (var stream in _heldStreams)
                {
                    try
                    {
                        stream.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to dispose stream!");
                    }
                }
            }
        }
    }
}