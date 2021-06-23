using Bank.Domain.Apps;
using Bank.Domain.Apps.MessageQueues;
using Bank.Domain.Entities;
using Bank.Domain.Entities.Validations;
using Bank.Domain.Notifications;
using Bank.Domain.Repositories;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Bank.App
{
    public class TransactionApp : AppBase, ITransactionApp
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionSendQueue _transactionSendQueue;

        public TransactionApp(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, INotifier notifier, ITransactionSendQueue transactionSendQueue) : base(unitOfWork, notifier)
        {
            _transactionRepository = transactionRepository;
            _transactionSendQueue = transactionSendQueue;
        }

        public async Task<Transaction> GetById(Guid id)
        {
            var transaction = await _transactionRepository.GetById(id);
            if (transaction == null)
            {
                Notify($"The transaction {id} was not found.");
                return null;
            }

            return transaction;
        }
        
        public async Task<Transaction> Create(Transaction transaction)
        {
            if (!Validate(new TransactionValidation(), transaction))
            {
                return null;
            }
            _transactionRepository.Create(transaction);
            await UnitOfWork.Save();
            _transactionSendQueue.SendQueue(JsonConvert.SerializeObject(transaction));
            return transaction;
        }

        public async Task Update(Guid id, Transaction transaction)
        {
            if (id != transaction.Id)
            {
                Notify($"The supplied ids {id} and {transaction.Id} are differents.");
                return;
            }

            if (!Validate(new TransactionValidation(), transaction))
            {
                return;
            }

            var transactionToUpdate = await GetById(id);
            if (transactionToUpdate == null)
            {
                Notify($"The transaction {id} not found.");
                return;
            }

            transactionToUpdate.AccountOrigin = transaction.AccountOrigin;
            transactionToUpdate.AccountDestination = transaction.AccountDestination;
            transactionToUpdate.Value = transaction.Value;
            transactionToUpdate.Status = transaction.Status;

            _transactionRepository.Update(transactionToUpdate);

            await UnitOfWork.Save();
        }
    }
}
