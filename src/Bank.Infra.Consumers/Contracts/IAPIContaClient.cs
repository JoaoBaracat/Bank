using Bank.Infra.Consumers.Models.APIConta;
using System.Threading.Tasks;

namespace Bank.Domain.Apps.Services
{
    public interface IAPIContaClient
    {
        Task<Account> GetAccountByNumberAsync(string accountNumber);
        Task<BalanceAdjustmentResponse> PostTransferAsync(BalanceAdjustment balanceAdjustment);
    }
}
