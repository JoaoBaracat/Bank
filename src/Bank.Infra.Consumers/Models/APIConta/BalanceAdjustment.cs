using Bank.Domain.Entities;
using Bank.Domain.Enums;
using System;
using static Bank.Domain.Enums.TransactionTypeEnum;

namespace Bank.Infra.Consumers.Models.APIConta
{
    public class BalanceAdjustment
    {
        public BalanceAdjustment(Transaction transaction, TransactionType type)
        {
            TransactionId = transaction.Id;
            AccountNumber = type == TransactionType.Debit ? transaction.AccountOrigin : transaction.AccountDestination;
            Value = transaction.Value;
            Type = Enumerations.GetEnumDescription(type);
        }

        public Guid TransactionId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Value { get; set; }
        public string Type { get; set; }

    }
}
