using Finbuckle.MultiTenant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.Common.Infastructure.Mappers;
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
    public static class SharedServicesExtensions
    {
        public static IServiceCollection AddSharedServices<T>(
        this IServiceCollection services,
        IConfiguration configuration) where T : TenantBaseDbContext
        {
            services.AddSharedPostgreSQL<T>(configuration);
            services.AddSharedJwtAuthentication(configuration);

            services.AddScoped<IUnitOfWork, UnitsOfWork<T>>();
            services.AddScoped<IErrorMapper, ErrorMapper>();
            services.AddScoped<ITenantRepository, TenantBaseRepository<T>>();
            return services;
        }
    }
}
