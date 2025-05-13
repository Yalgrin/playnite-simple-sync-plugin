using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class CompletionStatusMapper : AbstractMapper<CompletionStatus, CompletionStatusDto>
    {
        public CompletionStatusDto ToDto(CompletionStatus entity)
        {
            return new CompletionStatusDto
            {
                Id = entity.Id.ToString(),
                Name = entity.Name
            };
        }
    }
}