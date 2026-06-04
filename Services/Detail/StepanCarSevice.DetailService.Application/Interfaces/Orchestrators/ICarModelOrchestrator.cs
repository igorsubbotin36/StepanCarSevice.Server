using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Interfaces.Orchestrators
{
    public interface ICarModelOrchestrator
    {
        Task<Result<CarModelReadDto>> UpdateAsync(CarModelUpdateDto model);
        Task<Result<CarModelReadDto>> AddAsync(CarModelCreateDto model);
    }
}
