using StepanCarSevice.DetailService.Application.Interfaces.Mappers;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Application.Models.Mappers
{
    public class CarModificationMappers : ICarModificationMappers
    {
        public CarModificationReadDto CarModificationToReadDto(CarModification carModel)
        {
            throw new NotImplementedException();
        }

        public CarModification CreateDtoToCarModification(CarModificationCreateDto model)
        {
            throw new NotImplementedException();
        }

        public CarModification UpdateDtoToCarModification(CarModificationUpdateDto model)
        {
            throw new NotImplementedException();
        }
    }
}
