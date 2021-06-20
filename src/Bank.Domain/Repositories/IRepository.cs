using Bank.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Bank.Domain.Repositories
{
    public interface IRepository<TEntity> where TEntity : Entity
    {
        Task<TEntity> GetById(Guid id);
        void Create(TEntity entity);
        void Update(TEntity entity);
    }
}
