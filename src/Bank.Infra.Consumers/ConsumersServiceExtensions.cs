using Bank.Domain.Apps.MessageQueues;
using Bank.Domain.Models.MQ;
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
            //consumers
            services.AddHostedService<TransactionConsumer>();
            services.AddHostedService<TransferConsumer>();

            //sender
            services.Configure<MQSettings>(configuration);
            services.AddScoped<ITransactionSendQueue, TransactionSendQueue>();
        }
    }
}
