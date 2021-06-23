using System;

namespace Bank.Domain.Models.APIConta
{
    public class BalanceAdjustment
    {
        public Guid TransactionId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Value { get; set; }
        public string Type { get; set; }

    }
}
