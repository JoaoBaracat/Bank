using Bank.App;
using Bank.Domain.Apps;
using Bank.Domain.Notifications;
using Bank.Domain.Repositories;
using Bank.Infra.Consumers;
using Bank.Infra.Data.Repositories;
using Bank.Infra.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bank.Infra.IoC
{
    public static class NativeInjectorBootStrapper
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            //App
            services.AddScoped<ITransactionApp, TransactionApp>();

            //Domain
            services.AddScoped<INotifier, Notifier>();

            //Infra Data
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            //Infra Identity
            services.AddIdentityServices(configuration);

            //Infra Consumers
            services.AddQueueConfig(configuration);
        }
    }
}
