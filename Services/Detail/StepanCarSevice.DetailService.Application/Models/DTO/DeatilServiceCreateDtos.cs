namespace StepanCarSevice.DetailService.Application.Models.DTO
{
    public record CarManufactureCreateDto(string NameEng);
    public record CarModelCreateDto(int ManufactureId, string Name, int YearFrom, int YearTo);
    public record CarModificationCreateDto(string Name, int CarModelId, string? EngineValue, int EngineTypeId, string EngineName, int EnginePower, int WheelDriveTypeId, int TransmissionTypeId);
    public record DetailManufactureCreateDto(string Name);
    public record DetailCreateDto(string Code, string OriginalCode, int DetailManufactureId, string Name, decimal Price, int Count);
}
