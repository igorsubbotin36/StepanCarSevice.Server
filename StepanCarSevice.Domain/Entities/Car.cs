namespace StepanCarSevice.Domain.Entities
{
    public class Car
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public required User Owner { get; set; }
        public required string WIN {  get; set; }
        public int CarModelId { get; set; }
        public required CarModel CarModel { get; set; }
        public int Year { get; set; }
        public required string Number { get; set; }
    }
}
