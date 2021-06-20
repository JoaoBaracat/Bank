using Bank.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Bank.Domain.Apps
{
    public interface IApp<TEntity> where TEntity : Entity
    {
        Task<TEntity> GetById(Guid id);
        Task<TEntity> Create(TEntity entity);
        Task Update(Guid id, TEntity entity);
    }
}
