using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Interfaces.Mappers
{
    public interface IDetailMappers
    {
        DetailReadDto DetailToReadDto(Detail detail);
        Detail UpdateDtoToDetail(DetailUpdateDto model);
        Detail CreateDtoToDetail(DetailCreateDto model);
    }
}
