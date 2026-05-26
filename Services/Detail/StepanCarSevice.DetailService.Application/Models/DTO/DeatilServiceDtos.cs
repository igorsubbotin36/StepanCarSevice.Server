using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Models.DTO
{
    public record CarManudactureDto(string NameEng, string NameRu, string Country, string TenantId);
    public record CarModelDto(int ManufactureId, string NameEng, string NameRu, int YearFrom, int YearTo, string TenantId);
    public record DetailManufactureDto(string Name, string TenantId);
    public record DetailDto(string Code, string OriginalCode, int DetailManufactureId, string Name, int CarModelId, decimal Price, int Count, string TenantId);
}
