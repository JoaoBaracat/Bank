using Bank.Domain.Entities;
using Bank.Domain.Repositories;
using Bank.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Bank.Infra.Data.Repositories
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity, new()
    {
        protected readonly DbSet<TEntity> _entities;

        protected Repository(BankDbContext context)
        {
            _entities = context.Set<TEntity>();
        }

        public async Task<TEntity> GetById(Guid id)
        {
            return await _entities.FindAsync(id);
        }

        public void Create(TEntity entity)
        {
            _entities.Add(entity);
        }

        public void Update(TEntity entity)
        {
            _entities.Update(entity);
        }
    }
}
