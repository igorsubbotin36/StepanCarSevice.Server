using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Models.DTO
{
    public record CarManufactureUpdateDto(int Id, string? Name);
    public record CarModelUpdateDto(int Id, int ManufactureId, string? Name, int? YearFrom, int? YearTo);
    public record DetailManufactureUpdateDto(int Id, string? Name);
    public record DetailUpdateDto(int Id, string? Code, string? OriginalCode, int DetailManufactureId, string? Name, decimal? Price, int? Count);
}
