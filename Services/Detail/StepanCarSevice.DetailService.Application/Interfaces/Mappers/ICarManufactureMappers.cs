using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Application.Interfaces.Mappers
{
    public interface ICarManufactureMappers
    {
        CarManufactureReadDto CarManufactureToReadDto(CarManufacture carManufacture);
        CarManufacture UpdateDtoToCarManufacture(CarManufactureUpdateDto model);
        CarManufacture CreateDtoToCarManufacture (CarManufactureCreateDto model);
    }
}
