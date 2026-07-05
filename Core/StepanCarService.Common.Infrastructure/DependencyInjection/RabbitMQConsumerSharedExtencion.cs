using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.Common.Infastructure.Messaging;
using StepanCarService.Common.Infastructure.Messaging.Handlers;
using StepanCarService.Common.Infastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Infastructure.DependencyInjection
{
    public static class RabbitMQConsumerSharedExtencion
    {
        public static IServiceCollection AddRabbitMQConsumer<T>(
        this IServiceCollection services,
        IConfiguration configuration) where T : TenantBaseDbContext
        {
            services.Configure<RabbitMQConsumerSetting>(
                configuration.GetSection("RabbitMQ"));
            services.AddScoped<TenantBaseRepository<T>>();
            services.AddScoped<ITenantRegisteredHandler, TenantRegisteredHandler<TenantBaseRepository<T>>>();
            services.AddHostedService<TenantEventsConsumer>();
            return services;
        }
    }
}
