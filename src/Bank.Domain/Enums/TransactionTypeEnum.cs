using System.ComponentModel;

namespace Bank.Domain.Enums
{
    public class TransactionTypeEnum
    {
        public enum TransactionType
        {
            [Description("Debit")]
            Debit = 0,
            [Description("Credit")]
            Credit = 1
        }
    }
}
