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
                    transactionMessage.Status = (int)TransactionStatus.Processing;

                    await _consumerTransactionApp.UpdateTransactionAsync(transactionMessage);
                    _logger.LogInformation($"Processing message: {contentString}");
                    await _messageProcessor.ProcessMessage(new MessageProcessorDTO() { 
                        EventArgs = eventArgs, 
                        TransactionMessage = transactionMessage, 
                        ContentString = contentString ,
                        Model = _model
                    });                    
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error in transaction consumer with exception: {e.Message} TransactionId: {transactionMessage.Id}");
                    _model.BasicReject(eventArgs.DeliveryTag, false);
                }
            };
            return consumer;
        }
    }
}
