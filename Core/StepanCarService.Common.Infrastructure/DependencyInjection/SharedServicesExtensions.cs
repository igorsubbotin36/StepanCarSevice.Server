using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Infastructure.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Infastructure.DependencyInjection
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
