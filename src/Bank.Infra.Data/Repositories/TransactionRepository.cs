using Bank.Domain.Entities;
using Bank.Domain.Repositories;
using Bank.Infra.Data.Contexts;

namespace Bank.Infra.Data.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(BankDbContext context) : base(context)
        {

        }
    }
}
