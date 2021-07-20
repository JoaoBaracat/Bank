using Bank.App;
using Bank.Domain.Apps.MessageQueues;
using Bank.Domain.Entities;
using Bank.Domain.Notifications;
using Bank.Infra.Consumers.Models.ServiceSettings;
using Bank.Infra.Data.Contexts;
using Bank.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Bank.Domain.Enums.TransactionStatusEnum;

namespace Bank.Test
{
    public class TransactionAppTests
    {
        private TransactionApp _transactionApp;
        private DbContextOptions<BankDbContext> _options;
        private BankDbContext _context;
        private Guid _guidWrongModel;
        private Guid _guidModel;
        private IOptions<MQSettings> _option;
        private readonly Mock<ITransactionSendQueue> _transactionSendQueueMock;

        public TransactionAppTests()
        {
            _guidWrongModel = Guid.NewGuid();
            _guidModel = Guid.NewGuid();
            _option = Options.Create(new MQSettings()
            {
                MQHostName = "localhost",
                MQUserName = "guest",
                MQPassword = "guest",
                TransactionQueue = "BankTransactionQueueTest",
                Exchange = "BankExchangeTest",
                DeadLetterQueue = "BankDeadLetterQueueTest",
                DeadLetterExchange = "BankDeadLetterExchangeTest",
                RetryAttempts = 3,
                APIContaSettings = new APIContaSettings()
                {
                    Url = "http://localhost:5000",
                    GetEndPoint = "/api/Account/",
                    PostEndPoint = "/api/Account/",
                }
            });
            _transactionSendQueueMock = new Mock<ITransactionSendQueue>();
            _transactionSendQueueMock.Setup(x => x.SendQueue(It.IsAny<string>()));
        }

        private IEnumerable<Transaction> TransactionsList()
        {
            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(new Transaction { Id = _guidModel, AccountOrigin = "74323858", AccountDestination = "47246054", Value = 10 });
            transactions.Add(new Transaction { Id = Guid.NewGuid(), AccountOrigin = "46646271", AccountDestination = "53244144", Value = 12.1M });
            transactions.Add(new Transaction { Id = Guid.NewGuid(), AccountOrigin = "53244144", AccountDestination = "74323858", Value = 13.5M });
            transactions.Add(new Transaction { Id = _guidWrongModel, AccountOrigin = "23814220", AccountDestination = "23814220", Value = 0 });
            return transactions;
        }

        [Fact]
        public async Task ShouldCreateTransaction()
        {
            _options = new DbContextOptionsBuilder<BankDbContext>()
              .UseInMemoryDatabase(databaseName: "ShouldCreateTransaction")
              .Options;
            _context = new BankDbContext(_options);

            _transactionApp = new TransactionApp(new TransactionRepository(_context), 
                new UnitOfWork(_context), 
                new Notifier(),
                _transactionSendQueueMock.Object);
            foreach (var transaction in TransactionsList().ToList())
            {
                await _transactionApp.Create(transaction);
            }
            var created = await _transactionApp.GetById(_guidModel);
            
            Assert.Equal("74323858", created.AccountOrigin);
            Assert.Equal("47246054", created.AccountDestination);
            Assert.Equal(10, created.Value);
        }

        [Fact]
        public async Task ShouldNotCreateTransaction()
        {
            _options = new DbContextOptionsBuilder<BankDbContext>()
              .UseInMemoryDatabase(databaseName: "ShouldNotCreateTransaction")
              .Options;
            _context = new BankDbContext(_options);

            _transactionApp = new TransactionApp(new TransactionRepository(_context), 
                new UnitOfWork(_context), 
                new Notifier(),
                _transactionSendQueueMock.Object);
            foreach (var transaction in TransactionsList().ToList())
            {
                await _transactionApp.Create(transaction);
            }
            var notCreated = await _transactionApp.GetById(_guidWrongModel);
            
            Assert.Null(notCreated);
        }


        [Fact]
        public async Task ShouldGetTransactionInQueue()
        {
            _options = new DbContextOptionsBuilder<BankDbContext>()
              .UseInMemoryDatabase(databaseName: "ShouldGetTransactionInQueue")
              .Options;
            _context = new BankDbContext(_options);

            _transactionApp = new TransactionApp(new TransactionRepository(_context), 
                new UnitOfWork(_context), 
                new Notifier(),
                _transactionSendQueueMock.Object);
            foreach (var transaction in TransactionsList().ToList())
            {
                await _transactionApp.Create(transaction);
            }
            var created = await _transactionApp.GetById(_guidModel);
            
            Assert.Equal((int)TransactionStatus.InQueue, created.Status);
        }
    }
}
