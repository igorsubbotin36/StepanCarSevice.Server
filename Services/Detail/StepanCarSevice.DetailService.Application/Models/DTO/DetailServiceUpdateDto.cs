using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Models.DTO
{
    public record CarManudactureUpdateDto(int Id, string? NameEng);
    public record CarModelUpdateDto(int Id, CarManudactureUpdateDto? Manufacture, string? NameEng, int? YearFrom, int? YearTo);
    public record DetailManufactureUpdateDto(int Id, string? Name);
    public record DetailUpdateDto(int Id, string? Code, string? OriginalCode, DetailManufactureUpdateDto? DetailManufacture, string? Name, CarModelUpdateDto? CarModel, decimal? Price, int? Count);
}
