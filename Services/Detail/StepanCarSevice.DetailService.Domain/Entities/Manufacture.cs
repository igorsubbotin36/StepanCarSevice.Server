using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class Manufacture
    {
        [Key]
        public int Id { get; set; }
        public required string NameEN { get; set; }
        public required string NameRU { get; set; }
        public required string Country { get; set; }
    }
}
