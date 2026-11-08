using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Services;

namespace SimpleSyncPlugin.Threading
{
    public class DataProcessingBroker
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly ConcurrentDictionary<ObjectType, ConcurrentQueue<SaveEntityData>> _saveEntityQueue =
            new ConcurrentDictionary<ObjectType, ConcurrentQueue<SaveEntityData>>();

        private readonly DataProcessingService _service;
        private readonly DataSynchronizationService _synchronizationService;
        private readonly SimpleSyncWorker _worker;

        public DataProcessingBroker(DataProcessingService service, DataSynchronizationService synchronizationService,
            SimpleSyncWorker worker)
        {
            _service = service;
            _synchronizationService = synchronizationService;
            _worker = worker;

            foreach (ObjectType type in Enum.GetValues(typeof(ObjectType)))
            {
                _saveEntityQueue[type] = new ConcurrentQueue<SaveEntityData>();
            }
        }

        public void SubmitChange(ChangeMessage change)
        {
            _worker.Enqueue(() => _service.ProcessChange(change));
        }

        public void SubmitObjectSave<T>(ObjectType type, T entity) where T : DatabaseObject
        {
            lock (_saveEntityQueue)
            {
                _saveEntityQueue[type].Enqueue(new SaveEntityData { Entity = entity });
            }

            EnqueueSendOperation(type);
        }

        public void SubmitObjectSaveList<T>(ObjectType type, List<T> entities) where T : DatabaseObject
        {
            lock (_saveEntityQueue)
            {
                foreach (var entity in entities)
                {
                    _saveEntityQueue[type].Enqueue(new SaveEntityData { Entity = entity });
                }
            }

            EnqueueSendOperation(type);
        }

        public void SubmitObjectSaveDiff<T>(ObjectType type, T oldEntity, T newEntity) where T : DatabaseObject
        {
            lock (_saveEntityQueue)
            {
                _saveEntityQueue[type].Enqueue(new SaveEntityDiffData { OldEntity = oldEntity, Entity = newEntity });
            }

            EnqueueSendOperation(type);
        }

        public void SubmitObjectDelete<T>(ObjectType type, T entity) where T : DatabaseObject
        {
            lock (_saveEntityQueue)
            {
                _saveEntityQueue[type].Enqueue(new DeleteEntityData { Entity = entity });
            }

            EnqueueSendOperation(type);
        }

        public void SubmitObjectDeleteList<T>(ObjectType type, List<T> entities) where T : DatabaseObject
        {
            lock (_saveEntityQueue)
            {
                foreach (var entity in entities)
                {
                    _saveEntityQueue[type].Enqueue(new DeleteEntityData { Entity = entity });
                }
            }

            EnqueueSendOperation(type);
        }

        private void EnqueueSendOperation(ObjectType type)
        {
            _worker.Enqueue(async () =>
            {
                var entities = new List<SaveEntityData>();
                var entitiesToDelete = new List<DeleteEntityData>();
                var entityChanges = new List<SaveEntityDiffData>();

                lock (_saveEntityQueue)
                {
                    while (_saveEntityQueue[type].TryDequeue(out SaveEntityData data))
                    {
                        if (data is SaveEntityDiffData diffData)
                        {
                            entityChanges.Add(diffData);
                        }
                        else if (data is DeleteEntityData deleteData)
                        {
                            entitiesToDelete.Add(deleteData);
                        }
                        else
                        {
                            entities.Add(data);
                        }
                    }
                }

                if (entitiesToDelete.Count > 0)
                {
                    Logger.Debug($"Going to delete {entitiesToDelete.Count} entities of type {type}");
                    foreach (var data in entitiesToDelete)
                    {
                        entities.RemoveAll(e => e.Entity.Id == data.Entity.Id);
                        entityChanges.RemoveAll(e => e.Entity.Id == data.Entity.Id || e.OldEntity.Id == data.Entity.Id);
                        await _synchronizationService.DeleteObject(type, data.Entity);
                    }
                }

                Dictionary<Guid, int> entityCounts = new Dictionary<Guid, int>();
                List<Guid> idsToSendCompletely = new List<Guid>();

                foreach (var data in entities)
                {
                    if (!idsToSendCompletely.Contains(data.Entity.Id))
                    {
                        idsToSendCompletely.Add(data.Entity.Id);
                    }

                    entityCounts[data.Entity.Id] =
                        entityCounts.TryGetValue(data.Entity.Id, out var count) ? count + 1 : 1;
                }

                foreach (var data in entityChanges)
                {
                    entityCounts[data.Entity.Id] =
                        entityCounts.TryGetValue(data.Entity.Id, out var count) ? count + 1 : 1;
                }

                foreach (var pair in entityCounts.Where(e =>
                             e.Value > 1 || (type != ObjectType.Game && type != ObjectType.Platform)))
                {
                    if (!idsToSendCompletely.Contains(pair.Key))
                    {
                        idsToSendCompletely.Add(pair.Key);
                    }
                }

                entityChanges.RemoveAll(e => idsToSendCompletely.Contains(e.Entity.Id));

                if (idsToSendCompletely.Count > 0)
                {
                    Logger.Debug($"Going to save completely {idsToSendCompletely.Count} entities of type {type}");
                    foreach (var id in idsToSendCompletely)
                    {
                        await _synchronizationService.SaveObjectById(type, id);
                    }
                }

                if (entityChanges.Count > 0)
                {
                    Logger.Debug($"Going to save {entityChanges.Count} entity changes of type {type}");
                    foreach (var change in entityChanges)
                    {
                        await _synchronizationService.SaveObjectDiff(type, change.OldEntity, change.Entity);
                    }
                }
            });
        }
    }

    internal class SaveEntityData
    {
        public DatabaseObject Entity;
    }

    internal class DeleteEntityData : SaveEntityData
    {
    }

    internal class SaveEntityDiffData : SaveEntityData
    {
        public DatabaseObject OldEntity;
    }
}