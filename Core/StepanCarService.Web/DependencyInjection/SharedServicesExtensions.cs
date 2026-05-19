using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StepanCarService.Core.Interfaces;
using StepanCarService.Web.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Web.DependencyInjection
{
    public static class SharedServicesExtensions
    {
        public static IServiceCollection AddSharedServices(
        this IServiceCollection services)
        {
            services.AddScoped<IErrorMapper, ErrorMapper>();
            return services;
        }
    }
}
