using Bank.Domain.Apps.MessageQueues;
using Bank.Domain.Apps.Services;
using Bank.Infra.Consumers.APIConta;
using Bank.Infra.Consumers.App;
using Bank.Infra.Consumers.Consumers;
using Bank.Infra.Consumers.Contracts;
using Bank.Infra.Consumers.MessageQueues;
using Bank.Infra.Consumers.Models.ServiceSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bank.Infra.Consumers
{
    public static class ConsumersServiceExtensions
    {
        public static void AddQueueConfig(this IServiceCollection services, IConfiguration configuration)
        {
            //consumer
            services.AddHostedService<TransactionConsumer>();
            services.AddSingleton<IAPIContaClient, APIContaClient>();
            services.AddSingleton<IConsumerTransactionApp, ConsumerTransactionApp>();
            services.AddSingleton<IMessageProcessorApp, MessageProcessorApp>();

            //sender
            services.Configure<MQSettings>(configuration);
            services.AddSingleton<ITransactionSendQueue, TransactionSendQueue>();
        }
    }
}
