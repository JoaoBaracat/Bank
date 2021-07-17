using Bank.Domain.Models.APIConta;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Domain.Apps.Services
{
    public interface IAPIContaClient
    {
        Task<Account> GetAccountByNumberAsync(string accountNumber);
        Task<BalanceAdjustmentResponse> PostTransferAsync(BalanceAdjustment balanceAdjustment);
    }
}
