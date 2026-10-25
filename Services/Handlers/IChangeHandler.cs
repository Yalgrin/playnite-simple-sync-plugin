using System.Threading.Tasks;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public interface IChangeHandler
    {
        Task HandleChange(ChangeMessage dto);

        Task HandleReassignedId(object dto);

        ObjectType GetHandledType();
    }
}