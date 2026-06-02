using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Models.DTO
{
    public record CarManudactureCreateDto(string NameEng);
    public record CarModelCreateDto(CarManudactureCreateDto Manufacture, string NameEng, int YearFrom, int YearTo);
    public record DetailManufactureCreateDto(string Name);
    public record DetailCreateDto(string Code, string OriginalCode, DetailManufactureCreateDto DetailManufacture, string Name, CarModelCreateDto CarModel, decimal Price, int Count);
}
