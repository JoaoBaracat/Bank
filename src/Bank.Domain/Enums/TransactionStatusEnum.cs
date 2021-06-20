using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Bank.Domain.Enums
{
    public class TransactionStatusEnum
    {
        public enum TransactionStatus
        {
            [Description("In Queue")]
            InQueue = 0,
            [Description("Processing")]
            Processing = 1,
            [Description("Confirmed")]
            Confirmed = 2,
            [Description("Error")]
            Error = 3
        }    
    }
}
