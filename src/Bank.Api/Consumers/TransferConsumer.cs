using Bank.App.APIConta;
using Bank.App.MessageQueues;
using Bank.Domain.Enums;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Bank.Domain.Enums.TransactionStatusEnum;
using static Bank.Domain.Enums.TransactionTypeEnum;

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
                var message = JsonConvert.DeserializeObject<BalanceAdjustment>(contentString);
                if (_apiConta.PostTransfer(message) || message.Type == Enumerations.GetEnumDescription(TransactionType.Credit))
                {
                    UpdateTransaction(message.TransactionId);
                    _model.BasicAck(eventArgs.DeliveryTag, false);
                }
                else
                {
                    _model.BasicReject(eventArgs.DeliveryTag, true);
                }                
            };
            _model.BasicConsume(_configuration.TranferQueue, false, consumer);
            return Task.CompletedTask;
        }

        public void UpdateTransaction(Guid transactionId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                var transactionToUpdate = context.Transactions.Find(transactionId);
                transactionToUpdate.Status = (int)TransactionStatus.Confirmed;
                context.Update(transactionToUpdate);
                context.SaveChanges();
            }
        }
    }
}
