namespace StepanCarSevice.Domain.Entities
{
    public class Manufacture
    {
        public int Id { get; set; }
        public required string NameEN { get; set; }
        public required string NameRU { get; set; }
        public required string Country { get; set; }
    }
}
