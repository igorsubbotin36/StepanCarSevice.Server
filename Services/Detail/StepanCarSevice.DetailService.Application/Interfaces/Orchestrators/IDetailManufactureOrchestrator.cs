using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Interfaces.Orchestrators
{
    public interface IDetailManufactureOrchestrator
    {
        Task<Result<DetailManufactureReadDto>> UpdateAsync(DetailManufactureUpdateDto model);
        Task<Result<DetailManufactureReadDto>> AddAsync(DetailManufactureCreateDto model);
    }
}
