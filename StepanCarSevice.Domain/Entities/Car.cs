using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.Domain.Entities
{
    public class Car
    {
        [Key]
        public int Id { get; set; }
        public required int OwnerId { get; set; }
        public required User Owner { get; set; }
        public required string WIN {  get; set; }
        public required int CarModelId { get; set; }
        public required CarModel CarModel { get; set; }
        public required int Year { get; set; }
        public required string Number { get; set; }
    }
}
