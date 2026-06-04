using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Interfaces.Orchestrators
{
    public interface IDetailOrchestrator
    {
        Task<Result<DetailReadDto>> UpdateAsync(DetailUpdateDto model);
        Task<Result<DetailReadDto>> AddAsync(DetailCreateDto model);
    }
}
