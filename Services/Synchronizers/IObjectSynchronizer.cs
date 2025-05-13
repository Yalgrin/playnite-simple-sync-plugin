using System;
using System.Collections.Generic;
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

        void RegisterGracePeriod(Guid id);
    }
}