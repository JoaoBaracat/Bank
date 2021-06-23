using Bank.App.APIConta;
using Bank.App.MessageQueues;
using Bank.Domain.Models.APIConta;
using Bank.Domain.Models.MQ;
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

namespace Bank.Api.Consumers
{
    public class TransferConsumer : BackgroundService
    {
        private readonly MQSettings _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _model;
        private APIContaClient _apiConta;

        public TransferConsumer(IOptions<MQSettings> option, IServiceProvider serviceProvider)
        {
            _configuration = option.Value;
            _serviceProvider = serviceProvider;
            _model = new QueueFactory(_configuration).CreateTransferQueue();
            _apiConta = new APIContaClient(_configuration);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_model);
            consumer.Received += (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonConvert.DeserializeObject<List<BalanceAdjustment>>(contentString);
                var accountOrigin = _apiConta.GetAccountByNumber(message[0].AccountNumber);
                if (accountOrigin != null)
                {
                    if (accountOrigin.Balance < message[0].Value)
                    {
                        UpdateTransaction(message[0].TransactionId, "Not enough balance in the origin account.", (int)TransactionStatus.Error);
                        _model.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        foreach (var item in message)
                        {
                            var processed = false;
                            while (!processed)
                            {
                                if (_apiConta.PostTransfer(item))
                                {
                                    processed = true;
                                }
                            }
                        }
                        UpdateTransaction(message[0].TransactionId);
                        _model.BasicAck(eventArgs.DeliveryTag, false);
                    }
                }
                else
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
                        UpdateTransaction(message[0].TransactionId, "APIConta not reachable.", (int)TransactionStatus.Error);
                        _model.BasicAck(eventArgs.DeliveryTag, false);
                    }
                }
            };
            _model.BasicConsume(_configuration.TranferQueue, false, consumer);
            return Task.CompletedTask;
        }

        private void UpdateTransaction(Guid transactionId, string message = null, int transactionStatus = -1)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                var transactionToUpdate = context.Transactions.Find(transactionId);
                transactionToUpdate.Message = string.IsNullOrEmpty(message) ? transactionToUpdate.Message : message;
                transactionToUpdate.Status = transactionStatus < 0 ? (int)TransactionStatus.Confirmed : transactionStatus;
                context.Update(transactionToUpdate);
                context.SaveChanges();
            }
        }
    }
}
