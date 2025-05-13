using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public abstract class AbstractDiffChangeHandler<TEntity, TDto, TDiffDto> : AbstractChangeHandler<TEntity, TDiffDto>
        where TEntity : DatabaseObject where TDto : AbstractDto where TDiffDto : AbstractDiffDto
    {
        private readonly ILogger Logger = LogManager.GetLogger();
        private readonly AbstractChangeHandler<TEntity, TDto> _parentHandler;

        protected AbstractDiffChangeHandler(IPlayniteAPI api, AbstractMapper<TEntity, TDiffDto> mapper,
            DataSynchronizationService dataSynchronizationService,
            AbstractChangeHandler<TEntity, TDto> parentHandler) : base(api, mapper, dataSynchronizationService)
        {
            _parentHandler = parentHandler;
        }

        protected override Task HandleObjectSave(TDiffDto objectDto, long dtoObjectId)
        {
            var db = _api.Database;
            var databaseCollection = GetDatabaseCollection(db);

            TEntity oldEntity = null;
            var reassignIds = false;
            var isNew = false;

            var entity = FindDatabaseObject(databaseCollection, objectDto, ref oldEntity, ref reassignIds, ref isNew);
            if (entity == null)
            {
                return FetchWholeObjectAndHandleSave(objectDto);
            }

            var isChanged = isNew || HasEntityBeenChanged(entity, objectDto);
            _mapper.FillEntity(entity, objectDto);

            Logger.Trace($"HandleChange > isNew: {isNew},  reassignIds: {reassignIds}, isChanged: {isChanged}");
            if (!isChanged)
            {
                return Task.CompletedTask;
            }

            return UpdateDatabase(db, isNew, entity, databaseCollection, objectDto, reassignIds, oldEntity,
                dtoObjectId);
        }

        private async Task FetchWholeObjectAndHandleSave(TDiffDto objectDto)
        {
            var fullObjectDto = await GetFullObject(objectDto.BaseObjectId);
            if (fullObjectDto == null)
            {
                return;
            }

            await _parentHandler.DoHandleSave(fullObjectDto, objectDto.BaseObjectId);
        }

        protected override TEntity FindDatabaseObject(IItemCollection<TEntity> databaseCollection, TDiffDto objectDto,
            ref TEntity oldEntity,
            ref bool reassignIds, ref bool isNew)
        {
            var matchingEntities = databaseCollection.Where(e => e.Id.ToString() == objectDto.Id).ToList();
            return matchingEntities.Count > 0 ? matchingEntities[0] : null;
        }

        protected abstract Task<TDto> GetFullObject(long objectId);

        protected async Task LoadMetadata(IGameDatabaseAPI db, TEntity entity, TDiffDto objectDto,
            List<string> changeFields, string metadataName, string previousItem, Func<string, string> func)
        {
            if (changeFields.Contains(metadataName))
            {
                Logger.Trace($"Going to load and save metadata {metadataName}...");
                var fileContent = await GetObjectMetadata(objectDto.BaseObjectId, metadataName);
                if (fileContent != null)
                {
                    Logger.Trace($"Going to save metadata {metadataName}...");

                    var tempPath = Path.Combine(Path.GetTempPath(),
                        Guid.NewGuid() + "-" + objectDto.BaseObjectId + "-" + fileContent.Item2);
                    Logger.Trace($"Writing metadata {metadataName} to path {tempPath}...");
                    File.WriteAllBytes(tempPath, fileContent.Item1);

                    Logger.Trace($"Adding file to entity {entity.Id}...");
                    var newIcon = db.AddFile(tempPath, entity.Id);
                    func.Invoke(newIcon);

                    Logger.Trace($"Deleting temp file {tempPath}...");
                    File.Delete(tempPath);
                    if (previousItem != null)
                    {
                        Logger.Trace($"Removing previous file {previousItem}...");
                        db.RemoveFile(previousItem);
                    }
                }
                else
                {
                    Logger.Warn("File not found!");
                }
            }
        }
    }
}