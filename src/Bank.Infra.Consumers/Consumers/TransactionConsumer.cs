using Bank.Domain.Entities;
using Bank.Domain.Enums;
using Bank.Domain.Models.APIConta;
using Bank.Domain.Models.MQ;
using Bank.Infra.Consumers.APIConta;
using Bank.Infra.Consumers.MessageQueues;
using Bank.Infra.Data.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private APIContaClient _apiConta;
        private readonly ILogger<TransactionConsumer> _logger;

        public TransactionConsumer(IOptions<MQSettings> option, IServiceProvider serviceProvider, ILogger<TransactionConsumer> logger)
        {
            _configuration = option.Value;
            _serviceProvider = serviceProvider;
            _model = new QueueFactory(_configuration).CreateTransactionQueue();
            _apiConta = new APIContaClient(_configuration);
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_model);

            consumer.Received += async (sender, eventArgs) =>
            {                
                try
                {
                    var contentArray = eventArgs.Body.ToArray();
                    var contentString = Encoding.UTF8.GetString(contentArray);
                    var message = JsonConvert.DeserializeObject<Transaction>(contentString);
                    message.Status = (int)TransactionStatus.Processing;
                    await UpdateTransaction(message);
                    _logger.LogInformation($"Processing message: {contentString}");

                    var accountOrigin = await _apiConta.GetAccountByNumber(message.AccountOrigin);
                    var accountDestination = await _apiConta.GetAccountByNumber(message.AccountDestination);
                    if (accountOrigin == null || accountDestination == null)
                    {
                        var retryHandler = new RetryHandler();
                        var attempts = retryHandler.GetRetryAttempts(eventArgs.BasicProperties);
                        if (attempts < _configuration.RetryAttempts)
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
                            await UpdateTransaction(message);
                            _model.BasicAck(eventArgs.DeliveryTag, false);
                            _logger.LogInformation($"{message.Message}: {contentString}");
                        }
                    }
                    else if (accountOrigin.Id == 0 || accountDestination.Id == 0)
                    {
                        message.Status = (int)TransactionStatus.Error;
                        message.Message = "Account not found.";
                        await UpdateTransaction(message);
                        _model.BasicAck(eventArgs.DeliveryTag, false);
                        _logger.LogInformation($"{message.Message}: {contentString}");
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

                        var originResponse = await _apiConta.PostTransfer(transferOrigin);
                        if (originResponse.Response == "Success")
                        {
                            var processed = false;
                            while (!processed)
                            {
                                var destinationResponse = await _apiConta.PostTransfer(transferDestination);
                                if (destinationResponse.Response == "Success")
                                {
                                    message.Status = (int)TransactionStatus.Confirmed;
                                    await UpdateTransaction(message);
                                    processed = true;
                                    _model.BasicAck(eventArgs.DeliveryTag, false);
                                    _logger.LogInformation($"Message processed: {contentString}");
                                }
                            }
                        }
                        else if (originResponse.Response == "Not enough balance")
                        {
                            message.Status = (int)TransactionStatus.Error;
                            message.Message = originResponse.Response;
                            await UpdateTransaction(message);
                            _model.BasicAck(eventArgs.DeliveryTag, false);
                            _logger.LogInformation($"{message.Message}: {contentString}");
                        }
                        else
                        {
                            var retryHandler = new RetryHandler();
                            var attempts = retryHandler.GetRetryAttempts(eventArgs.BasicProperties);
                            if (attempts < _configuration.RetryAttempts)
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
                                await UpdateTransaction(message);
                                _model.BasicAck(eventArgs.DeliveryTag, false);
                                _logger.LogInformation($"{message.Message}: {contentString}");
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    _logger.LogError($"Error in transaction consumer with exception: {e.Message}");
                    _model.BasicReject(eventArgs.DeliveryTag, false);
                }
            };
            _model.BasicConsume(_configuration.TransactionQueue, false, consumer);
        }

        private async Task UpdateTransaction(Transaction transaction)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                var transactionToUpdate = await context.Transactions.FindAsync(transaction.Id);
                transactionToUpdate.Status = transaction.Status;
                transactionToUpdate.Message = transaction.Message;
                context.Update(transactionToUpdate);
                await context.SaveChangesAsync();
            }
        }
    }
}
