using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public interface IObjectSynchronizer
    {
        void RegisterListeners();

        ObjectType GetHandledType();

        int GetElementCount(IGameDatabaseAPI db);

        Task SyncAll(IGameDatabaseAPI db, GlobalProgressActionArgs progArgs);

        Task SyncSelected(IGameDatabaseAPI db, GlobalProgressActionArgs progArgs, List<Guid> ids);

        bool IsNotInGracePeriod(Guid id);

        void RegisterGracePeriod(Guid id);

        Task SaveObject(object entity, CancellationToken cancellationToken = default);

        Task SaveObjectById(Guid id, CancellationToken cancellationToken = default);

        Task SaveDiffObject(object oldEntity, object newEntity, CancellationToken cancellationToken = default);

        Task DeleteObject(object entity, CancellationToken cancellationToken = default);
    }
}