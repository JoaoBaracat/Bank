using Bank.Domain.Repositories;
using Bank.Infra.Data.Contexts;
using System.Threading.Tasks;

namespace Bank.Infra.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BankDbContext _context;

        public UnitOfWork(BankDbContext context)
        {
            _context = context;
        }

        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
