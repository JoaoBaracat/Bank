using Bank.Domain.Apps.Services;
using Bank.Domain.Entities;
using Bank.Infra.Consumers.Contracts;
using Bank.Infra.Consumers.Models.APIConta;
using System.Threading.Tasks;
using static Bank.Domain.Enums.TransactionTypeEnum;
using static Bank.Infra.Consumers.Models.Enums.ProcessorResultEnum;

namespace Bank.Infra.Consumers.App
{
    public class MessageProcessorApp : IMessageProcessorApp
    {
        private readonly IAPIContaClient _apiContaClient;
        public MessageProcessorApp(IAPIContaClient apiContaClient)
        {
            _apiContaClient = apiContaClient;
        }

        public async Task<AccountsResultEnum> TransferFunds(Transaction transaction)
        {
            var transferOrigin = new BalanceAdjustment(transaction, TransactionType.Debit);
            var originResponse = await _apiContaClient.PostTransferAsync(transferOrigin);
            if (originResponse.Response == "Success")
            {
                var transferDestination = new BalanceAdjustment(transaction, TransactionType.Credit);
                while (true)
                {
                    var destinationResponse = await _apiContaClient.PostTransferAsync(transferDestination);
                    if (destinationResponse.Response == "Success")
                    {
                        return AccountsResultEnum.AccountsOk;
                    }
                }
            }
            else if (originResponse.Response == "Not enough balance")
            {
                return AccountsResultEnum.NotAllowed;
            }
            return AccountsResultEnum.NotReachable;
        }

        public async Task<AccountsResultEnum> ValidateAccounts(Transaction transaction)
        {
            var accountOrigin = await _apiContaClient.GetAccountByNumberAsync(transaction.AccountOrigin);
            var accountDestination = await _apiContaClient.GetAccountByNumberAsync(transaction.AccountDestination);
            if (accountOrigin == null || accountDestination == null)
            {
                return AccountsResultEnum.NotReachable;
            }
            else if (accountOrigin.Id == 0 || accountDestination.Id == 0)
            {
                return AccountsResultEnum.NotAllowed;
            }
            return AccountsResultEnum.AccountsOk;
        }
    }
}
