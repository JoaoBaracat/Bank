using Bank.App;
using Bank.Domain.Apps;
using Bank.Domain.Notifications;
using Bank.Domain.Repositories;
using Bank.Infra.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bank.Infra.IoC
{
    public static class NativeInjectorBootStrapper
    {
        public static void RegisterServices(IServiceCollection services)
        {
            //App
            services.AddScoped<ITransactionApp, TransactionApp>();

            //Domain
            services.AddScoped<INotifier, Notifier>();

            //Infra Data
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
        }
    }
}
