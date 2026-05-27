using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Models.DTO
{
    public record CarManudactureUpdateDto(int Id, string? NameEng, string? NameRu, string? Country, string TenantId);
    public record CarModelUpdateDto(int Id, int? ManufactureId, string? NameEng, string? NameRu, int? YearFrom, int? YearTo, string TenantId);
    public record DetailManufactureUpdateDto(int Id, string? Name, string? TenantId);
    public record DetailUpdateDto(int Id, string? Code, string? OriginalCode, int? DetailManufactureId, string? Name, int? CarModelId, decimal? Price, int? Count, string TenantId);
}
