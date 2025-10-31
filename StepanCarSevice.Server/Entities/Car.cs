namespace StepanCarService.Server.Entities
{
    public class Car
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public string WIN {  get; set; }
        public int CarModelId { get; set; }
        public CarModel CarModel { get; set; }
        public int Year { get; set; }
        public string Number { get; set; }
    }
}
