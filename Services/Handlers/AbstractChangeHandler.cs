using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public abstract class AbstractChangeHandler<TEntity, TDto> : IChangeHandler
        where TEntity : DatabaseObject where TDto : AbstractDto
    {
        private readonly ILogger Logger = LogManager.GetLogger();

        protected readonly IPlayniteAPI _api;
        protected readonly AbstractMapper<TEntity, TDto> _mapper;
        protected readonly DataSynchronizationService _dataSynchronizationService;

        protected AbstractChangeHandler(IPlayniteAPI api, AbstractMapper<TEntity, TDto> mapper,
            DataSynchronizationService dataSynchronizationService)
        {
            _api = api;
            _mapper = mapper;
            _dataSynchronizationService = dataSynchronizationService;
        }

        public async Task HandleChange(ChangeDto dto)
        {
            var objectDto = await GetObject(dto);
            if (objectDto == null)
            {
                return;
            }

            await DoHandleSave(objectDto, dto.ObjectId);
        }

        internal Task DoHandleSave(TDto objectDto, long objectId)
        {
            return objectDto.Removed ? HandleObjectRemoval(objectDto) : HandleObjectSave(objectDto, objectId);
        }

        private Task HandleObjectRemoval(TDto objectDto)
        {
            var db = _api.Database;
            if (!CanRemove(db, objectDto))
            {
                Logger.Info($"Cannot remove object {GetHandledType()} with id = {objectDto.Id}!");
                return Task.CompletedTask;
            }

            var databaseCollection = GetDatabaseCollection(db);
            var toRemove = databaseCollection
                .Where(e => e.Id.ToString() == objectDto.Id && e.Name == objectDto.Name)
                .ToList();
            Logger.Info($"Removing {toRemove.Count} objects of type {GetHandledType()}!");
            _dataSynchronizationService.RegisterGracePeriod(GetHandledType(), Guid.Parse(objectDto.Id));
            databaseCollection.Remove(toRemove);
            return Task.CompletedTask;
        }

        protected virtual Task HandleObjectSave(TDto objectDto,
            long dtoObjectId)
        {
            var db = _api.Database;
            var databaseCollection = GetDatabaseCollection(db);

            TEntity oldEntity = null;
            var reassignIds = false;
            var isNew = false;

            var entity = FindDatabaseObject(databaseCollection, objectDto, ref oldEntity, ref reassignIds, ref isNew);

            var isChanged = isNew || HasEntityBeenChanged(entity, objectDto);
            _mapper.FillEntity(entity, objectDto);

            Logger.Trace($"HandleChange > isNew: {isNew},  reassignIds: {reassignIds}, isChanged: {isChanged}");
            if (!isChanged)
            {
                Logger.Info($"Object {GetHandledType()} with id = {entity.Id} has not been changed, skipping...");
                return Task.CompletedTask;
            }

            return UpdateDatabase(db, isNew, entity, databaseCollection, objectDto, reassignIds, oldEntity,
                dtoObjectId);
        }

        protected async Task UpdateDatabase(IGameDatabaseAPI db, bool isNew, TEntity entity,
            IItemCollection<TEntity> databaseCollection, TDto objectDto, bool reassignIds, TEntity oldEntity,
            long dtoObjectId)
        {
            using (db.BufferedUpdate())
            {
                if (isNew)
                {
                    Logger.Info($"Saving new object {GetHandledType()} with id {entity.Id}...");
                    _dataSynchronizationService.RegisterGracePeriod(GetHandledType(), entity.Id);
                    databaseCollection.Add(entity);
                    bool changedAnything = FillEntityPostNewEntitySave(entity, objectDto);
                    var needsToHandleMetadata = NeedsToHandleMetadata(entity, objectDto);
                    if (needsToHandleMetadata)
                    {
                        Logger.Trace($"Handling metadata save for object {GetHandledType()} with id {entity.Id}...");
                        await HandleMetadata(db, entity, objectDto, dtoObjectId);
                    }

                    if (needsToHandleMetadata || changedAnything)
                    {
                        Logger.Trace($"Saving new object {GetHandledType()} with id {entity.Id} again...");
                        _dataSynchronizationService.RegisterGracePeriod(GetHandledType(), entity.Id);
                        databaseCollection.Update(entity);
                    }
                }
                else
                {
                    if (NeedsToHandleMetadata(entity, objectDto))
                    {
                        Logger.Trace($"Handling metadata save for object {GetHandledType()} with id {entity.Id}...");
                        await HandleMetadata(db, entity, objectDto, dtoObjectId);
                    }

                    Logger.Info($"Saving existing object {GetHandledType()} with id {entity.Id}...");
                    _dataSynchronizationService.RegisterGracePeriod(GetHandledType(), entity.Id);
                    databaseCollection.Update(entity);
                }

                if (reassignIds)
                {
                    Logger.Info(
                        $"Going to reassign ids from {oldEntity.Id} to {entity.Id} for object with type {GetHandledType()}...");
                    HandleReassigningIds(oldEntity, entity, db);

                    Logger.Trace($"Removing old object {GetHandledType()} with id {oldEntity.Id}...");
                    _dataSynchronizationService.RegisterGracePeriod(GetHandledType(), oldEntity.Id);
                    databaseCollection.Remove(oldEntity);
                }
            }
        }

        protected virtual TEntity FindDatabaseObject(IItemCollection<TEntity> databaseCollection, TDto objectDto,
            ref TEntity oldEntity,
            ref bool reassignIds, ref bool isNew)
        {
            TEntity entity;
            var matchingEntities = databaseCollection.Where(e => e.Id.ToString() == objectDto.Id).ToList();
            if (matchingEntities.Count > 0)
            {
                entity = matchingEntities[0];
            }
            else
            {
                matchingEntities = databaseCollection.Where(e => e.Name == objectDto.Name).ToList();
                if (matchingEntities.Count > 0)
                {
                    oldEntity = matchingEntities[0];
                    reassignIds = true;
                }

                entity = CreateNewInstance();
                isNew = true;
            }

            return entity;
        }

        protected virtual bool FillEntityPostNewEntitySave(TEntity entity, TDto dto)
        {
            return false;
        }

        protected virtual Task HandleMetadata(IGameDatabaseAPI db, TEntity entity, TDto objectDto, long dtoObjectId)
        {
            return Task.CompletedTask;
        }

        protected virtual bool NeedsToHandleMetadata(TEntity entity, TDto objectDto)
        {
            return false;
        }

        protected async Task LoadMetadata(IGameDatabaseAPI db, TEntity entity, long dtoObjectId, bool hasItem,
            string metadataName, string previousItem, Func<string, string> func)
        {
            if (hasItem)
            {
                Logger.Trace($"Going to load and save metadata {metadataName}...");
                var fileContent = await GetObjectMetadata(dtoObjectId, metadataName);
                if (fileContent != null)
                {
                    Logger.Trace($"Going to save metadata {metadataName}...");

                    var tempPath = Path.Combine(Path.GetTempPath(),
                        Guid.NewGuid() + "-" + dtoObjectId + "-" + fileContent.Item2);
                    Logger.Trace($"Writing metadata {metadataName} to path {tempPath}...");
                    File.WriteAllBytes(tempPath, fileContent.Item1);

                    Logger.Trace($"Adding file to entity {entity.Id}...");
                    var newFile = db.AddFile(tempPath, entity.Id);
                    func.Invoke(newFile);

                    Logger.Trace($"Deleting temp file {tempPath}...");
                    File.Delete(tempPath);
                    if (previousItem != null)
                    {
                        Logger.Trace($"Removing previous file {previousItem}...");
                        db.RemoveFile(previousItem);
                    }
                }
            }
            else
            {
                if (previousItem != null)
                {
                    Logger.Trace($"Removing existing file {previousItem}...");
                    db.RemoveFile(previousItem);
                    func.Invoke(null);
                }
            }
        }

        protected virtual Task<Tuple<byte[], string>> GetObjectMetadata(long dtoObjectId, string metadataName)
        {
            return Task.FromResult<Tuple<byte[], string>>(null);
        }

        protected virtual bool CanRemove(IGameDatabaseAPI db, TDto objectDto)
        {
            return true;
        }

        protected virtual bool HasEntityBeenChanged(TEntity entity, TDto objectDto)
        {
            return entity.Name != objectDto.Name;
        }

        protected virtual void HandleReassigningIds(TEntity oldEntity, TEntity entity, IGameDatabaseAPI db)
        {
        }

        protected abstract TEntity CreateNewInstance();

        protected abstract IItemCollection<TEntity> GetDatabaseCollection(IGameDatabaseAPI db);

        protected abstract Task<TDto> GetObject(ChangeDto dto);

        public abstract ObjectType GetHandledType();
    }
}