using Bank.Domain.Entities;
using System.Threading.Tasks;

namespace Bank.Infra.Consumers.Contracts
{
    public interface IConsumerTransactionApp
    {
        Task UpdateTransactionAsync(Transaction transaction);
    }
}
