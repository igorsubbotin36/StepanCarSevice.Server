using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Models.DTO
{
    public record CarManufactureReadDto(int Id, string Name);
    public record CarModelReadDto(int Id, int ManufactureId, string NameEng, string NameRu, int YearFrom, int YearTo);
    public record DetailManufactureReadDto(int Id, string Name);
    public record DetailReadDto(int Id, string Code, string OriginalCode, int DetailManufactureId, string Name, decimal Price, int Count);
}
