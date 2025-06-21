namespace StepanCarSevice.Server.Entities
{
    public class Visit
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public string? DateFrom  { get; set; }
        public string? DateTo { get; set; }
        public List<Work>? Works { get; set; }
        public List<Detail>? UsedDetails { get; set; }
    }
}
