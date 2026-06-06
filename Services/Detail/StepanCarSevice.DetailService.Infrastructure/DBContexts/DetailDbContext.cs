using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Infrastructure.DBContexts
{
    public class DetailDbContext : TenantBaseDbContext
    {
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<CarModification> CarModifications { get; set; }
        public DbSet<Engine> Engines { get; set; }
        public DbSet<EngineType> EngineTypes { get; set; }
        public DbSet<WheelDriveType> WheelDriveTypes { get; set; }
        public DbSet<TransmissionType> TransmissionTypes { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<CarManufacture> CarManufactures { get; set; }
        public DbSet<DetailManufacture> DetailManufactures { get; set; }

        public DetailDbContext(DbContextOptions<DetailDbContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

                object value = optionsBuilder.UseNpgsql(connectionString);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== 1. CarManufacture -> CarModel (один ко многим) ==========
            modelBuilder.Entity<CarManufacture>()
                .HasMany(m => m.CarModels)
                .WithOne(c => c.Manufacture)
                .HasForeignKey(c => c.ManufactureId)
                .OnDelete(DeleteBehavior.Restrict); // Осторожно: не удаляем модели при удалении производителя

            // ========== 2. CarModel -> CarModification (один ко многим) ==========
            modelBuilder.Entity<CarModel>()
                .HasMany(m => m.CarModifications)
                .WithOne(c => c.CarModel)
                .HasForeignKey(c => c.CarModelId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== 3. CarModification -> WheelDriveType (многие к одному) ==========
            modelBuilder.Entity<CarModification>()
                .HasOne(c => c.WheelDriveType)
                .WithMany() // у WheelDriveType нет коллекции CarModifications, но если добавите – укажите .WithMany(w => w.CarModifications)
                .HasForeignKey(c => c.WheelDriveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== 4. CarModification -> TransmissionType (многие к одному) ==========
            modelBuilder.Entity<CarModification>()
                .HasOne(c => c.TransmissionType)
                .WithMany()
                .HasForeignKey(c => c.TransmissionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== 5. CarModification <-> Engine (многие ко многим) ==========
            // Таблица связи: CarModificationEngine
            modelBuilder.Entity<CarModification>()
                .HasMany(c => c.Engines)
                .WithMany(e => e.CarModifications)
                .UsingEntity<Dictionary<string, object>>(
                    "CarModificationEngine",
                    j => j.HasOne<Engine>().WithMany().HasForeignKey("EngineId"),
                    j => j.HasOne<CarModification>().WithMany().HasForeignKey("CarModificationId"),
                    j =>
                    {
                        j.HasKey("CarModificationId", "EngineId");
                        j.ToTable("CarModificationEngines"); // явное имя таблицы
                    });

            // ========== 6. CarModification <-> Detail (многие ко многим) ==========
            // Таблица связи: CarModificationDetail
            modelBuilder.Entity<CarModification>()
                .HasMany(c => c.Details)
                .WithMany(d => d.CarModifications)
                .UsingEntity<Dictionary<string, object>>(
                    "CarModificationDetail",
                    j => j.HasOne<Detail>().WithMany().HasForeignKey("DetailId"),
                    j => j.HasOne<CarModification>().WithMany().HasForeignKey("CarModificationId"),
                    j =>
                    {
                        j.HasKey("CarModificationId", "DetailId");
                        j.ToTable("CarModificationDetails");
                    });

            // ========== 7. Detail -> DetailManufacture (многие к одному) ==========
            modelBuilder.Entity<Detail>()
                .HasOne(d => d.DetailManufacture)
                .WithMany(dm => dm.Details)
                .HasForeignKey(d => d.DetailManufactureId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== 8. Самореферентные связи Detail (оригиналы и альтернативы) ==========
            // Оригиналы: текущая деталь может ссылаться на несколько оригинальных деталей
            modelBuilder.Entity<Detail>()
                .HasMany(d => d.OriginalDetails)
                .WithMany() // обратная навигация не указана, поэтому WithMany() без параметра
                .UsingEntity<Dictionary<string, object>>(
                    "DetailOriginalLinks",
                    j => j.HasOne<Detail>().WithMany().HasForeignKey("OriginalDetailId"),
                    j => j.HasOne<Detail>().WithMany().HasForeignKey("DetailId"),
                    j =>
                    {
                        j.HasKey("DetailId", "OriginalDetailId");
                        j.ToTable("DetailOriginalRelations");
                    });

            // Альтернативы: текущая деталь может иметь несколько альтернативных деталей
            modelBuilder.Entity<Detail>()
                .HasMany(d => d.AlternativeDetails)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "DetailAlternativeLinks",
                    j => j.HasOne<Detail>().WithMany().HasForeignKey("AlternativeDetailId"),
                    j => j.HasOne<Detail>().WithMany().HasForeignKey("DetailId"),
                    j =>
                    {
                        j.HasKey("DetailId", "AlternativeDetailId");
                        j.ToTable("DetailAlternativeRelations");
                    });

            // ========== 9. Engine -> EngineType (многие к одному) ==========
            modelBuilder.Entity<Engine>()
                .HasOne(e => e.EngineType)
                .WithMany() // у EngineType нет коллекции Engine, можно добавить при необходимости
                .HasForeignKey(e => e.EngineTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== 10. Настройка связей с TenantInfoEntity ==========
            // Предполагается, что у TenantInfoEntity есть первичный ключ TenantId (string)
            // и, возможно, коллекции для каждой сущности (CarManufactures, CarModels и т.д.)
            // Если коллекций нет, используем .WithMany()
            modelBuilder.Entity<CarManufacture>()
                .HasOne(e => e.Tenant)
                .WithMany() // .WithMany(t => t.CarManufactures) если коллекция есть
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarModel>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarModification>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Detail>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetailManufacture>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Engine>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EngineType>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransmissionType>()
                .HasOne(t => t.Tenant)
                .WithMany()
                .HasForeignKey(t => t.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WheelDriveType>()
                .HasOne(w => w.Tenant)
                .WithMany()
                .HasForeignKey(w => w.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== 11. Индексы для ускорения фильтрации по TenantId ==========
            modelBuilder.Entity<CarManufacture>().HasIndex(e => e.TenantId);
            modelBuilder.Entity<CarModel>().HasIndex(e => e.TenantId);
            modelBuilder.Entity<CarModification>().HasIndex(e => e.TenantId);
            modelBuilder.Entity<Detail>().HasIndex(e => e.TenantId);
            modelBuilder.Entity<DetailManufacture>().HasIndex(e => e.TenantId);
            modelBuilder.Entity<Engine>().HasIndex(e => e.TenantId);
            modelBuilder.Entity<EngineType>().HasIndex(e => e.TenantId);
            modelBuilder.Entity<TransmissionType>().HasIndex(e => e.TenantId);
            modelBuilder.Entity<WheelDriveType>().HasIndex(e => e.TenantId);

            // Дополнительные индексы на внешние ключи (для часто используемых JOIN)
            modelBuilder.Entity<CarModel>().HasIndex(e => e.ManufactureId);
            modelBuilder.Entity<CarModification>().HasIndex(e => e.CarModelId);
            modelBuilder.Entity<CarModification>().HasIndex(e => e.WheelDriveTypeId);
            modelBuilder.Entity<CarModification>().HasIndex(e => e.TransmissionTypeId);
            modelBuilder.Entity<Detail>().HasIndex(e => e.DetailManufactureId);
            modelBuilder.Entity<Engine>().HasIndex(e => e.CarModelId);
            modelBuilder.Entity<Engine>().HasIndex(e => e.EngineTypeId);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DetailDbContext).Assembly);
        }
    }
}
