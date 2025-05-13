using System.Threading.Tasks;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public interface IChangeHandler
    {
        Task HandleChange(ChangeDto dto);

        ObjectType GetHandledType();
    }
}