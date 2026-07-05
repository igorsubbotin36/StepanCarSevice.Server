using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Application.Interfaces.Mappers
{
    public interface IDetailManufactureMappers
    {
        DetailManufactureReadDto DetailManufactureToReadDto(DetailManufacture detailManufacture);
        DetailManufacture UpdateDtoToDetailManufacture(DetailManufactureUpdateDto model);
        DetailManufacture CreateDtoToDetailManufacture(DetailManufactureCreateDto model);
    }
}
