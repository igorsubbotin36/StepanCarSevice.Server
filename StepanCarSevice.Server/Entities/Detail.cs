namespace StepanCarSevice.Server.Entities
{
    public class Detail
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int CarModelId { get; set; }
        public CarModel CarModel { get; set; }
        public decimal Price { get; set; }
        public bool InStock { get; set; }
    }
}
