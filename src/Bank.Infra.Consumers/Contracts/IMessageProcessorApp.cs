using Bank.Infra.Consumers.Models.DTO;
using System.Threading.Tasks;

namespace Bank.Infra.Consumers.Contracts
{
    public interface IMessageProcessorApp
    {
        Task ProcessMessage(MessageProcessorDTO messageProcessor);
        Task TransferFunds(MessageProcessorDTO messageProcessor);
        Task<bool> ValidateAccounts(MessageProcessorDTO messageProcessor);
        Task RetryQueue(MessageProcessorDTO messageProcessor);
    }
}
