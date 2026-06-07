using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Infrastructure.DBContexts.Inits
{
    public class DbInitializer
    {
        public static void Init(DetailDbContext dbContext)
        {
            if (!dbContext.TransmissionTypes.Any())
            {
                var transmissonTypes = new List<TransmissionType>
                {
                    new TransmissionType { Id = 1, Name = "МКПП"},
                    new TransmissionType { Id = 2, Name = "АКПП"},
                    new TransmissionType { Id = 3, Name = "Вариатор"},
                    new TransmissionType { Id = 4, Name = "Робот"}
                };
                dbContext.TransmissionTypes.AddRange(transmissonTypes);
            }

            if (!dbContext.EngineTypes.Any())
            {
                var engineTypes = new List<EngineType>
                {
                    new EngineType { Id = 1, Name = "Бензин"},
                    new EngineType { Id = 2, Name = "Дизель"},
                    new EngineType { Id = 3, Name = "Электро"},
                    new EngineType { Id = 4, Name = "Гибрид"}
                };
                dbContext.EngineTypes.AddRange(engineTypes);
            }

            if (!dbContext.WheelDriveTypes.Any())
            {
                var wheelDriveTypes = new List<WheelDriveType>
                {
                    new WheelDriveType { Id = 1, Name = "Передний"},
                    new WheelDriveType { Id = 2, Name = "Задний"},
                    new WheelDriveType { Id = 3, Name = "Полный"}
                };
                dbContext.WheelDriveTypes.AddRange(wheelDriveTypes);
            }

            dbContext.SaveChanges();
        }
    }
}
