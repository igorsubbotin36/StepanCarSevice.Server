using StepanCarService.Core.Models;
using StepanCarSevice.VisitService.Application.Interfaces;
using StepanCarSevice.VisitService.Domain.Entities;
using StepanCarSevice.VisitService.Domain.Repositories;
namespace StepanCarSevice.VisitService.Application.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;
        public CarService(ICarRepository carRepository)
        {
            _carRepository = carRepository;
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
            bool updateSuccess = await _carRepository.UpdateCarAsync(car);
            if (!updateSuccess)
                return Result.Failure(SystemErrors.DatabaseError);
            return Result.Success();
        }
    }
}
