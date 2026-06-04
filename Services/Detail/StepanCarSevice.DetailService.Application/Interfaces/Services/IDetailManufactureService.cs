using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Interfaces.Services
{
    public interface IDetailManufactureService
    {
        Task<Result<DetailManufactureReadDto>> AddAsync(DetailManufactureCreateDto model);
        Task<Result<DetailManufactureReadDto>> UpdateAsync(DetailManufactureUpdateDto model);
        Task<Result<DetailManufactureReadDto>> GetByNameOrCreateAsync(DetailManufactureCreateDto model);
        Task<Result<List<DetailManufactureReadDto>>> GetAllAsync();
        Task<Result<DetailManufactureReadDto>> GetByNameAsync(string name);
        Task<Result> DeleteByIdAsync(int Id);
    }
}
