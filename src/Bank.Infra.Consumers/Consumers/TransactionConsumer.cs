using Bank.Domain.Entities;
using Bank.Infra.Consumers.Contracts;
using Bank.Infra.Consumers.MessageQueues;
using Bank.Infra.Consumers.Models.DTO;
using Bank.Infra.Consumers.Models.ServiceSettings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Bank.Domain.Enums.TransactionStatusEnum;
using static Bank.Infra.Consumers.Models.Enums.ProcessorResultEnum;

namespace Bank.Infra.Consumers.Consumers
{
    public class TransactionConsumer : BackgroundService
    {
        private readonly MQSettings _configuration;
        private readonly IModel _model;
        private readonly ILogger<TransactionConsumer> _logger;
        private readonly IConsumerTransactionApp _consumerTransactionApp;
        private readonly IMessageProcessorApp _messageProcessor;

        public TransactionConsumer(IOptions<MQSettings> option,
            ILogger<TransactionConsumer> logger,
            IConsumerTransactionApp consumerTransactionApp, IMessageProcessorApp messageProcessor)
        {
            _configuration = option.Value;
            _model = new QueueFactory(_configuration).CreateTransactionQueue();
            _logger = logger;
            _consumerTransactionApp = consumerTransactionApp;
            _messageProcessor = messageProcessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = await GetConsumer(_model);
            _model.BasicConsume(_configuration.TransactionQueue, false, consumer);
        }
        
        private async Task<EventingBasicConsumer> GetConsumer(IModel model)
        {
            var consumer = new EventingBasicConsumer(_model);

            consumer.Received += async (sender, eventArgs) =>
            {
                var transactionMessage = new Transaction();
                try
                {
                    var contentString = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                    transactionMessage = JsonConvert.DeserializeObject<Transaction>(contentString);
                    await _consumerTransactionApp.UpdateTransactionAsync(transactionMessage, (int)TransactionStatus.Processing, transactionMessage.Message);
                    _logger.LogInformation($"Processing message: {contentString}");

                    var retryQueueDTO = new RetryQueueDTO()
                    {
                        EventArgs = eventArgs,
                        TransactionMessage = transactionMessage,
                        ContentString = contentString,
                        Model = _model
                    };

                    var validateAccountsResult = await _messageProcessor.ValidateAccounts(transactionMessage);
                    if (validateAccountsResult == AccountsResultEnum.NotReachable)
                    {
                        await RetryQueue(retryQueueDTO);
                    }
                    else if(validateAccountsResult == AccountsResultEnum.NotAllowed)
                    {
                        await _consumerTransactionApp.UpdateTransactionAsync(transactionMessage, (int)TransactionStatus.Error, "Account not found.");
                        _model.BasicAck(eventArgs.DeliveryTag, false);
                        _logger.LogInformation($"Account not found.: {contentString}");
                    }
                    else
                    {
                        var transferResult = await _messageProcessor.TransferFunds(transactionMessage);
                        if (transferResult == AccountsResultEnum.AccountsOk)
                        {
                            await _consumerTransactionApp.UpdateTransactionAsync(transactionMessage, (int)TransactionStatus.Confirmed, transactionMessage.Message);
                            _model.BasicAck(eventArgs.DeliveryTag, false);
                            _logger.LogInformation($"Message processed: {contentString}");
                        }
                        else if (transferResult == AccountsResultEnum.NotAllowed)
                        {
                            await _consumerTransactionApp.UpdateTransactionAsync(transactionMessage, (int)TransactionStatus.Error, "Not enough balance");
                            _model.BasicAck(eventArgs.DeliveryTag, false);
                            _logger.LogInformation($"Not enough balance: {contentString}");
                        }
                        else
                        {
                            await RetryQueue(retryQueueDTO);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error in transaction consumer with exception: {e.Message} TransactionId: {transactionMessage.Id}");
                    _model.BasicReject(eventArgs.DeliveryTag, false);
                }
            };
            return consumer;
        }

        private async Task RetryQueue(RetryQueueDTO messageProcessor)
        {
            var retryHandler = new RetryHandler();
            var attempts = retryHandler.GetRetryAttempts(messageProcessor.EventArgs.BasicProperties);
            if (attempts < _configuration.RetryAttempts)
            {
                attempts++;
                var properties = messageProcessor.Model.CreateBasicProperties();
                properties.Headers = retryHandler.CopyMessageHeaders(messageProcessor.EventArgs.BasicProperties.Headers);
                retryHandler.SetRetryAttempts(properties, attempts);
                messageProcessor.Model.BasicPublish(messageProcessor.EventArgs.Exchange, messageProcessor.EventArgs.RoutingKey, properties, messageProcessor.EventArgs.Body);
                messageProcessor.Model.BasicAck(messageProcessor.EventArgs.DeliveryTag, false);
            }
            else
            {
                await _consumerTransactionApp.UpdateTransactionAsync(messageProcessor.TransactionMessage, (int)TransactionStatus.Error, "APIConta not reachable.");
                messageProcessor.Model.BasicAck(messageProcessor.EventArgs.DeliveryTag, false);
                _logger.LogInformation($"APIConta not reachable.: {messageProcessor.ContentString}");
            }
        }
    }
}
