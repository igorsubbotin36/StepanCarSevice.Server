using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarSevice.VisitService.Application.Interfaces;
using StepanCarSevice.VisitService.Domain.Entities;
using StepanCarSevice.VisitService.Domain.Repositories;
namespace StepanCarSevice.VisitService.Application.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;
        private readonly ILogger<CarService> _logger;
        public CarService(ICarRepository carRepository,
            ILogger<CarService> logger)
        {
            _carRepository = carRepository;
            _logger = logger;
        }
        public async Task<Result<CarSnapshot>> GetCarByVinAsync(string win)
        {
            CarSnapshot car = await _carRepository.GetCarByVinAsync(win);
            if (car == null)
                return Result.Failure<CarSnapshot>(ModelErrors.ModelNotFound);
            return Result.Success(car);
        }

        public async Task<Result<List<CarSnapshot>>> GetCarsByUserAsync(string phone)
        {
            List<CarSnapshot> list = await _carRepository.GetCarsByUserAsync(phone);
            if (list == null || list.Count == 0)
                return Result.Failure<List<CarSnapshot>>(ModelErrors.ModelNotFound);
            return Result.Success(list);
        }

        public async Task<Result> UpdateCarAsync(CarSnapshot car)
        {
            if (car == null)
                return Result.Failure(ModelErrors.ModelNotFound);
            try
            {
                await _carRepository.UpdateCarAsync(car);
                _logger.LogInformation($"{car.Id} информация обновлена");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{car.Id} ошибка при обновлении информации о автомобиле");
                return Result.Failure(SystemErrors.DatabaseError);
            }
            
        }
    }
}
