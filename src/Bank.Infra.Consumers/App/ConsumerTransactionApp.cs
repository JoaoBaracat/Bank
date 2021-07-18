using Bank.Domain.Apps;
using Bank.Domain.Entities;
using Bank.Infra.Consumers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Bank.Infra.Consumers.App
{
    public class ConsumerTransactionApp : IConsumerTransactionApp
    {
        private readonly IServiceProvider _serviceProvider;

        public ConsumerTransactionApp(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var transactionApp = scope.ServiceProvider.GetRequiredService<ITransactionApp>();
                var transactionToUpdate = await transactionApp.GetById(transaction.Id);
                if (transactionToUpdate != null)
                {
                    transactionToUpdate.Status = transaction.Status;
                    transactionToUpdate.Message = transaction.Message;
                    await transactionApp.Update(transactionToUpdate.Id, transactionToUpdate);
                }
            }
        }
    }

}
