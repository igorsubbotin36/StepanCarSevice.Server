using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;

namespace StepanCarSevice.DetailService.Infrastructure.Services
{
    public class DetailParserService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public DetailParserService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var scope = _scopeFactory.CreateScope();
            var manufactureRepository = scope.ServiceProvider.GetRequiredService<IRepository<CarManufacture>>();
            var carModelRepository = scope.ServiceProvider.GetRequiredService<IRepository<CarModel>>();
            var detailRepository = scope.ServiceProvider.GetRequiredService<IDetailRepository>();
            while (true)
            {
                //парсим Exist раз в неделю


                Task.Delay(TimeSpan.FromDays(7)).GetAwaiter().GetResult();
            }
        }
    }
}
