using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Interfaces.Mappers
{
    public interface IDetailManufactureMappers
    {
        DetailManufactureReadDto DetailManufactureToReadDto(DetailManufacture detailManufacture);
        DetailManufacture UpdateDtoToDetailManufacture(DetailManufactureUpdateDto model);
        DetailManufacture CreateDtoToDetailManufacture(DetailManufactureCreateDto model);
    }
}
