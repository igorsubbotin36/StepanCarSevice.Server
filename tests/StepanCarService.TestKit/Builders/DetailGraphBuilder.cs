using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarService.TestKit.Builders;

// Полный граф Detail для одного тенанта: производитель → модель → модификация (двигатель + деталь через
// производителя деталей). Справочники (EngineType/TransmissionType/WheelDriveType) берутся из сида
// DbInitializer (Id = 1 по умолчанию), поэтому граф годится сразу после SeedAsync тестовой БД
public sealed class DetailGraphBuilder
{
    private readonly string _suffix = Guid.NewGuid().ToString("N")[..8];
    private int _engineTypeId = 1;
    private int _transmissionTypeId = 1;
    private int _wheelDriveTypeId = 1;

    public static DetailGraphBuilder Graph() => new();

    public DetailGraphBuilder WithEngineType(int id) { _engineTypeId = id; return this; }
    public DetailGraphBuilder WithTransmissionType(int id) { _transmissionTypeId = id; return this; }
    public DetailGraphBuilder WithWheelDriveType(int id) { _wheelDriveTypeId = id; return this; }

    public DetailGraph Build()
    {
        var manufacture = new CarManufacture { Name = $"Производитель {_suffix}" };
        var model = new CarModel { Name = $"Модель {_suffix}", Manufacture = manufacture, YearFrom = 2000, YearTo = 2020 };
        manufacture.CarModels.Add(model);

        var detailManufacture = new DetailManufacture { Name = $"Производитель деталей {_suffix}" };
        var detail = new Detail
        {
            Name = $"Деталь {_suffix}",
            Code = $"CODE-{_suffix}",
            OriginalCode = $"ORIG-{_suffix}",
            DetailManufacture = detailManufacture,
            Price = 100,
            Count = 1
        };
        detailManufacture.Details.Add(detail);

        var engine = new Engine { Name = $"Двигатель {_suffix}", EngineTypeId = _engineTypeId, CarModel = model };

        var modification = new CarModification
        {
            Name = $"Модификация {_suffix}",
            CarModel = model,
            WheelDriveTypeId = _wheelDriveTypeId,
            TransmissionTypeId = _transmissionTypeId
        };
        modification.Engines.Add(engine);
        modification.Details.Add(detail);
        model.CarModifications.Add(modification);

        return new DetailGraph
        {
            Manufacture = manufacture,
            Model = model,
            Modification = modification,
            DetailManufacture = detailManufacture,
            Detail = detail,
            Engine = engine
        };
    }
}

public sealed class DetailGraph
{
    public required CarManufacture Manufacture { get; init; }
    public required CarModel Model { get; init; }
    public required CarModification Modification { get; init; }
    public required DetailManufacture DetailManufacture { get; init; }
    public required Detail Detail { get; init; }
    public required Engine Engine { get; init; }
}
