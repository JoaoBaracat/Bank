using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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
