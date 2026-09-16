using Finbuckle.MultiTenant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.Common.Infastructure.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Infastructure.DependencyInjection
{
    public static class MultitenantHostStrategyExtensions
    {
        public static IServiceCollection AddSharedMultitenantHostStrategy<T>(
        this IServiceCollection services) where T : TenantBaseDbContext
        {
            services.AddMultiTenant<TenantInfoEntity>()
                .WithHostStrategy()
                .WithStore<CaseInsensitiveTenantStore<T>>(ServiceLifetime.Scoped);
            services.AddSingleton<HostStrategyMarker>();
            return services;
        }
    }

    public sealed class HostStrategyMarker
    {
    }
}
