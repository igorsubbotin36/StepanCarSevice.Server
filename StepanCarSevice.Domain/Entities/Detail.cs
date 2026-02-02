namespace StepanCarSevice.Domain.Entities
{
    public class Detail
    {
        public int Id { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        public required int CarModelId { get; set; }
        public required CarModel CarModel { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
    }
}
