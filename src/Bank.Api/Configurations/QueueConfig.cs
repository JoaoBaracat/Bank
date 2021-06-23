using Bank.Api.Consumers;
using Bank.App.MessageQueues;
using Bank.Domain.Apps.MessageQueues;
using Bank.Domain.Models.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bank.Api.Configurations
{

    public static class QueueConfig
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
