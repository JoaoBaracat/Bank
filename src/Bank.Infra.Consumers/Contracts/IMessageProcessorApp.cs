using Bank.Domain.Entities;
using System.Threading.Tasks;
using static Bank.Infra.Consumers.Models.Enums.ProcessorResultEnum;

namespace Bank.Infra.Consumers.Contracts
{
    public interface IMessageProcessorApp
    {
        Task<AccountsResultEnum> TransferFunds(Transaction transaction);
        Task<AccountsResultEnum> ValidateAccounts(Transaction transaction);
    }
}
