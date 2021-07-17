using Bank.Domain.Apps.MessageQueues;
using Bank.Domain.Apps.Services;
using Bank.Domain.Models.MQ;
using Bank.Infra.Consumers.APIConta;
using Bank.Infra.Consumers.Consumers;
using Bank.Infra.Consumers.MessageQueues;
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

            //sender
            services.Configure<MQSettings>(configuration);
            services.AddSingleton<ITransactionSendQueue, TransactionSendQueue>();
        }
    }
}
