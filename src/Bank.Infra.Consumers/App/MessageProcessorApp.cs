using Bank.Domain.Apps.Services;
using Bank.Infra.Consumers.Consumers;
using Bank.Infra.Consumers.Contracts;
using Bank.Infra.Consumers.Models.APIConta;
using Bank.Infra.Consumers.Models.DTO;
using Bank.Infra.Consumers.Models.ServiceSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Threading.Tasks;
using static Bank.Domain.Enums.TransactionStatusEnum;
using static Bank.Domain.Enums.TransactionTypeEnum;

namespace Bank.Infra.Consumers.App
{
    public class MessageProcessorApp : IMessageProcessorApp
    {
        private readonly MQSettings _configuration;
        private readonly ILogger<MessageProcessorApp> _logger;
        private readonly IAPIContaClient _apiContaClient;
        private readonly IConsumerTransactionApp _consumerTransactionApp;
        public MessageProcessorApp(IOptions<MQSettings> option,
            ILogger<MessageProcessorApp> logger,
            IAPIContaClient apiContaClient,
            IConsumerTransactionApp consumerTransactionApp)
        {
            _configuration = option.Value;
            _logger = logger;
            _apiContaClient = apiContaClient;
            _consumerTransactionApp = consumerTransactionApp;
        }

        public async Task ProcessMessage(MessageProcessorDTO messageProcessor)
        {

            var accountsExists = await ValidateAccounts(messageProcessor);
            if (accountsExists)
            {                
                await TransferFunds(messageProcessor);
            }
        }

        public async Task TransferFunds(MessageProcessorDTO messageProcessor)
        {
            var transferOrigin = new BalanceAdjustment(messageProcessor.TransactionMessage, TransactionType.Debit);
            var originResponse = await _apiContaClient.PostTransferAsync(transferOrigin);
            if (originResponse.Response == "Success")
            {
                var processed = false;
                var transferDestination = new BalanceAdjustment(messageProcessor.TransactionMessage, TransactionType.Credit);
                while (!processed)
                {
                    var destinationResponse = await _apiContaClient.PostTransferAsync(transferDestination);
                    if (destinationResponse.Response == "Success")
                    {
                        messageProcessor.TransactionMessage.Status = (int)TransactionStatus.Confirmed;
                        await _consumerTransactionApp.UpdateTransactionAsync(messageProcessor.TransactionMessage);
                        processed = true;
                        messageProcessor.Model.BasicAck(messageProcessor.EventArgs.DeliveryTag, false);
                        _logger.LogInformation($"Message processed: {messageProcessor.ContentString}");
                    }
                }
            }
            else if (originResponse.Response == "Not enough balance")
            {
                messageProcessor.TransactionMessage.Status = (int)TransactionStatus.Error;
                messageProcessor.TransactionMessage.Message = originResponse.Response;
                await _consumerTransactionApp.UpdateTransactionAsync(messageProcessor.TransactionMessage);
                messageProcessor.Model.BasicAck(messageProcessor.EventArgs.DeliveryTag, false);
                _logger.LogInformation($"{messageProcessor.TransactionMessage.Message}: {messageProcessor.ContentString}");
            }
            else
            {
                await RetryQueue(messageProcessor);
            }
        }

        public async Task<bool> ValidateAccounts(MessageProcessorDTO messageProcessor)
        {
            var accountOrigin = await _apiContaClient.GetAccountByNumberAsync(messageProcessor.TransactionMessage.AccountOrigin);
            var accountDestination = await _apiContaClient.GetAccountByNumberAsync(messageProcessor.TransactionMessage.AccountDestination);
            if (accountOrigin == null || accountDestination == null)
            {
                await RetryQueue(messageProcessor);
                return false;
            }
            else if (accountOrigin.Id == 0 || accountDestination.Id == 0)
            {
                messageProcessor.TransactionMessage.Status = (int)TransactionStatus.Error;
                messageProcessor.TransactionMessage.Message = "Account not found.";
                await _consumerTransactionApp.UpdateTransactionAsync(messageProcessor.TransactionMessage);
                messageProcessor.Model.BasicAck(messageProcessor.EventArgs.DeliveryTag, false);
                _logger.LogInformation($"{messageProcessor.TransactionMessage.Message}: {messageProcessor.ContentString}");
                return false;
            }
            return true;
        }

        public async Task RetryQueue(MessageProcessorDTO messageProcessor)
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
                messageProcessor.TransactionMessage.Status = (int)TransactionStatus.Error;
                messageProcessor.TransactionMessage.Message = "APIConta not reachable.";
                await _consumerTransactionApp.UpdateTransactionAsync(messageProcessor.TransactionMessage);
                messageProcessor.Model.BasicAck(messageProcessor.EventArgs.DeliveryTag, false);
                _logger.LogInformation($"{messageProcessor.TransactionMessage.Message}: {messageProcessor.ContentString}");
            }
        }
    }
}
