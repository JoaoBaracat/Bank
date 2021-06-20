using System.Threading.Tasks;

namespace Bank.Domain.Repositories
{
    public interface IUnitOfWork
    {
        public Task<int> Save();

    }
}
