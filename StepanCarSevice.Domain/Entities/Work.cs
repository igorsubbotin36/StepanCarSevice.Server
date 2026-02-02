namespace StepanCarSevice.Domain.Entities
{
    public class Work
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
    }
}
