using Bank.Domain.Entities;
using Bank.Infra.Data.Contexts;
using System;

namespace Bank.Api.IntegrationTest.Base
{
    public class Utilities
    {
        public static void InitializeDbForTests(BankDbContext context)
        {
            context.Transactions.Add(new Transaction() { Id = Guid.Parse("fda81897-9c25-4d18-9347-023e9a5d4f1b"), AccountOrigin = "532156", AccountDestination = "561321", Value = 5.21M });
            context.Transactions.Add(new Transaction() { Id = Guid.NewGuid(), AccountOrigin = "984546", AccountDestination = "561321", Value = 5.21M });
            context.SaveChanges();
        }
    }
}
