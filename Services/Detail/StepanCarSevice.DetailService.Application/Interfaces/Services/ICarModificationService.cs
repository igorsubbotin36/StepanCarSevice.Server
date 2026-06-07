using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;

namespace StepanCarSevice.DetailService.Application.Interfaces.Services
{
    public interface ICarModificationService : IService<CarModificationReadDto, CarModificationCreateDto, CarModificationUpdateDto>
    {
        Task<Result<List<CarModificationReadDto>>> GetAllModificationsByModelId(int modelId);
    }
}
