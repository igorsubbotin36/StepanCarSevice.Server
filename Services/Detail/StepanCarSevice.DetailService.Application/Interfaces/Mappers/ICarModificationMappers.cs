using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Application.Interfaces.Mappers
{
    public interface ICarModificationMappers
    {
        CarModificationReadDto CarModificationToReadDto(CarModification carModel);
        CarModification UpdateDtoToCarModification(CarModificationUpdateDto model);
        CarModification CreateDtoToCarModification(CarModificationCreateDto model);
    }
}
