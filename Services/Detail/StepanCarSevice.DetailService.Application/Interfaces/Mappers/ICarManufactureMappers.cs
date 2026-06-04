using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Interfaces.Mappers
{
    public interface ICarManufactureMappers
    {
        CarManufactureReadDto CarManufactureToReadDto(CarManufacture carManufacture);
        CarManufacture UpdateDtoToCarManufacture(CarManufactureUpdateDto model);
        CarManufacture CreateDtoToCarManufacture (CarManufactureCreateDto model);
    }
}
