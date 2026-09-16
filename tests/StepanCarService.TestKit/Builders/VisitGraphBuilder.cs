using StepanCarSevice.VisitService.Domain.Entities;

namespace StepanCarService.TestKit.Builders;

// Полный граф Visit для одного тенанта: производитель → модель → автомобиль (с владельцем) → визит
// (с работой и деталью визита). Числовые поля вида OwnerId/CarModelId — исходные Id из Detail-сервиса,
// на изоляцию и каскады не влияют; реальные связи — через *SnapshotId и навигационные свойства
public sealed class VisitGraphBuilder
{
    private readonly string _suffix = Guid.NewGuid().ToString("N")[..8];

    public static VisitGraphBuilder Graph() => new();

    public VisitGraph Build()
    {
        var manufacture = new ManufactureSnapshot { NameEN = $"Manufacture{_suffix}", NameRU = $"Производитель {_suffix}", Country = "RU" };

        var carModel = new CarModelSnapshot
        {
            ManufactureSnapshotId = 0,
            ManufactureSnapshot = manufacture,
            NameEN = $"Model{_suffix}",
            NameRU = $"Модель {_suffix}",
            YearFrom = 2000,
            YearTo = 2020
        };

        var owner = new OwnerSnapshot { FirstName = "Иван", SecondName = $"Тестов{_suffix}", Phone = TestData.Phone() };

        var car = new CarSnapshot
        {
            OwnerId = 0,
            OwnerSnapshotId = 0,
            OwnerSnapshot = owner,
            VIN = $"VIN{_suffix}",
            CarModelId = 0,
            CarModelSnapshotId = 0,
            CarModelSnapshot = carModel,
            Year = 2015,
            Number = $"A000AA{_suffix}"
        };

        var visit = new Visit { CarId = 0, CarSnapshotId = 0, Car = car };

        var detail = new DetailSnapshot
        {
            Code = $"CODE-{_suffix}",
            Name = $"Деталь {_suffix}",
            CarModelId = 0,
            CarModelSnapshotId = 0,
            CarModelSnapshot = carModel,
            Price = 100,
            Count = 1
        };

        var work = new Work { Name = $"Работа {_suffix}", Price = 500, Visit = visit };
        visit.Works.Add(work);

        var visitDetails = new VisitDetails { Visit = visit, DetailSnapshot = detail };

        return new VisitGraph
        {
            Manufacture = manufacture,
            CarModel = carModel,
            Owner = owner,
            Car = car,
            Visit = visit,
            Work = work,
            Detail = detail,
            VisitDetails = visitDetails
        };
    }
}

public sealed class VisitGraph
{
    public required ManufactureSnapshot Manufacture { get; init; }
    public required CarModelSnapshot CarModel { get; init; }
    public required OwnerSnapshot Owner { get; init; }
    public required CarSnapshot Car { get; init; }
    public required Visit Visit { get; init; }
    public required Work Work { get; init; }
    public required DetailSnapshot Detail { get; init; }
    public required VisitDetails VisitDetails { get; init; }
}
