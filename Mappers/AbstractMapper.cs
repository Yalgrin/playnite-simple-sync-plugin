using System;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public abstract class AbstractMapper<TEntity, TDto> where TEntity : DatabaseObject where TDto : AbstractDto
    {
        public virtual void FillEntity(TEntity entity, TDto dto)
        {
            entity.Id = new Guid(dto.Id);
            entity.Name = dto.Name;
        }
    }
}