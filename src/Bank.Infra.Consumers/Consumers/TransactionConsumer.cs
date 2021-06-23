using Bank.Domain.Entities;
using Bank.Domain.Enums;
using Bank.Domain.Models.APIConta;
using Bank.Domain.Models.MQ;
using Bank.Infra.Consumers.APIConta;
using Bank.Infra.Consumers.MessageQueues;
using Bank.Infra.Data.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Bank.Domain.Enums.TransactionStatusEnum;
using static Bank.Domain.Enums.TransactionTypeEnum;

namespace Bank.Infra.Consumers.Consumers
{
    public class TransactionConsumer : BackgroundService
    {
        private readonly MQSettings _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _model;
        private TansferSendQueue _transfer;
        private APIContaClient _apiConta;

        public TransactionConsumer(IOptions<MQSettings> option, IServiceProvider serviceProvider)
        {
            _configuration = option.Value;
            _serviceProvider = serviceProvider;
            _model = new QueueFactory(_configuration).CreateTransactionQueue();
            _apiConta = new APIContaClient(_configuration);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_model);

            consumer.Received += (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonConvert.DeserializeObject<Transaction>(contentString);
                
                var accountOrigin = _apiConta.GetAccountByNumber(message.AccountOrigin);
                var accountDestination = _apiConta.GetAccountByNumber(message.AccountDestination);
                
                if (accountOrigin == null || accountDestination == null)
                {
                    var retryHandler = new RetryHandler();
                    var attempts = retryHandler.GetRetryAttempts(eventArgs.BasicProperties);
                    if (attempts < 3)
                    {
                        attempts++;
                        var properties = _model.CreateBasicProperties();
                        properties.Headers = retryHandler.CopyMessageHeaders(eventArgs.BasicProperties.Headers);
                        retryHandler.SetRetryAttempts(properties, attempts);
                        _model.BasicPublish(eventArgs.Exchange, eventArgs.RoutingKey, properties, eventArgs.Body);
                        _model.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        message.Status = (int)TransactionStatus.Error;
                        message.Message = "APIConta not reachable.";
                        UpdateTransaction(message);
                        _model.BasicAck(eventArgs.DeliveryTag, false);
                    }
                }
                else if (accountOrigin.Id == 0 || accountDestination.Id == 0)
                {
                    message.Status = (int)TransactionStatus.Error;
                    message.Message = "Account not found.";
                    UpdateTransaction(message);
                    _model.BasicAck(eventArgs.DeliveryTag, false);
                }
                else if (accountOrigin.Balance < message.Value)
                {
                    message.Status = (int)TransactionStatus.Error;
                    message.Message = "Not enough balance in the origin account.";
                    UpdateTransaction(message);
                    _model.BasicAck(eventArgs.DeliveryTag, false);
                }
                else
                {
                    var transferOrigin = new BalanceAdjustment()
                    {
                        TransactionId = message.Id,
                        AccountNumber = message.AccountOrigin,
                        Value = message.Value,
                        Type = Enumerations.GetEnumDescription(TransactionType.Debit)
                    };
                    var transferDestination = new BalanceAdjustment()
                    {
                        TransactionId = message.Id,
                        AccountNumber = message.AccountDestination,
                        Value = message.Value,
                        Type = Enumerations.GetEnumDescription(TransactionType.Credit)
                    };
                    
                    var transfers = new List<BalanceAdjustment>();
                    transfers.Add(transferOrigin);
                    transfers.Add(transferDestination);
                    _transfer = new TansferSendQueue(_configuration);
                    _transfer.SendQueue(JsonConvert.SerializeObject(transfers));

                    message.Status = (int)TransactionStatus.Processing;
                    UpdateTransaction(message);

                    _model.BasicAck(eventArgs.DeliveryTag, false);
                }
            };
            
            _model.BasicConsume(_configuration.TransactionQueue, false, consumer);
            return Task.CompletedTask;
        }

        private void UpdateTransaction(Transaction transaction)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                var transactionToUpdate = context.Transactions.Find(transaction.Id);
                transactionToUpdate.Status = transaction.Status;
                transactionToUpdate.Message = transaction.Message;
                context.Update(transactionToUpdate);
                context.SaveChanges();
            }
        }
    }
}
