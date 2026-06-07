using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Application.Interfaces.Mappers
{
    public interface ICarModelMappers
    {
        CarModelReadDto CarModelToReadDto(CarModel carModel);
        CarModel UpdateDtoToCarModel(CarModelUpdateDto model);
        CarModel CreateDtoToCarModel(CarModelCreateDto model);
    }
}
