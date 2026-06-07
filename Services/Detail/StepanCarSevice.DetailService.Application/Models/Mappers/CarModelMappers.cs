using StepanCarSevice.DetailService.Application.Interfaces.Mappers;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Application.Models.Mappers
{
    public class CarModelMappers : ICarModelMappers
    {
        public CarModelReadDto CarManufactureToReadDto(CarModel carModel)
        {
            throw new NotImplementedException();
        }

        public CarModelReadDto CarModelToReadDto(CarModel carModel)
        {
            throw new NotImplementedException();
        }

        public CarModel CreateDtoToCarModel(CarModelCreateDto model)
        {
            throw new NotImplementedException();
        }

        public CarModel UpdateDtoToCarModel(CarModelUpdateDto model)
        {
            throw new NotImplementedException();
        }
    }
}
