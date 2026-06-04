using StepanCarSevice.DetailService.Application.Interfaces.Mappers;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Models.Mappers
{
    public class CarManufactureMappers : ICarManufactureMappers
    {
        public CarManufactureReadDto CarManufactureToReadDto(CarManufacture carManufacture)
        {
            throw new NotImplementedException();
        }

        public CarManufacture CreateDtoToCarManufacture(CarManufactureCreateDto model)
        {
            throw new NotImplementedException();
        }

        public CarManufacture UpdateDtoToCarManufacture(CarManufactureUpdateDto model)
        {
            throw new NotImplementedException();
        }
    }
}
