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
    public class DetailMappers : IDetailMappers
    {
        public Detail CreateDtoToDetail(DetailCreateDto model)
        {
            throw new NotImplementedException();
        }

        public DetailReadDto DetailToReadDto(Detail detail)
        {
            throw new NotImplementedException();
        }

        public Detail UpdateDtoToDetail(DetailUpdateDto model)
        {
            throw new NotImplementedException();
        }
    }
}
