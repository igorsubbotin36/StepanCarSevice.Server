using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Interfaces.Services
{
    public interface IService<R, C, U>
        where R : class
        where C : class
        where U : class
    {
        Task<Result<R>> AddAsync(C model);
        Task<Result<R>> UpdateAsync(U model);
        Task<Result<List<R>>> GetAllAsync();
        Task<Result<R>> GetById(int id);
        Task<Result> DeleteByIdAsync(int Id);
    }
}
