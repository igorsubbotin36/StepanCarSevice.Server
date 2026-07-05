using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Application.Interfaces.Mappers
{
    public interface IDetailMappers
    {
        DetailReadDto DetailToReadDto(Detail detail);
        Detail UpdateDtoToDetail(DetailUpdateDto model);
        Detail CreateDtoToDetail(DetailCreateDto model);
    }
}
