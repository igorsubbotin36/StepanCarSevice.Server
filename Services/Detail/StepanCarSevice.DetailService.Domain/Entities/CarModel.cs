using System.ComponentModel.DataAnnotations;
namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class CarModel
    {
        [Key]
        public int Id { get; set; }
        public int ManufacturerId { get; set; }
        public required Manufacture Manufacture { get; set; }
        public required string NameEN { get; set; }
        public required string NameRU { get; set; }
        public int YearFrom { get; set; }
        public int YearTo { get; set; }

    }
}
