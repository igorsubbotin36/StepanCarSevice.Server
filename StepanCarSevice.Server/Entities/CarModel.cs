namespace StepanCarSevice.Server.Entities
{
    public class CarModel
    {
        public int Id { get; set; }
        public int ManufacturerId { get; set; }
        public Manufacture Manufacture { get; set; }
        public string NameEN { get; set; }
        public string NameRU { get; set; }
        public int YearFrom { get; set; }
        public int YearTo { get; set; }

    }
}
