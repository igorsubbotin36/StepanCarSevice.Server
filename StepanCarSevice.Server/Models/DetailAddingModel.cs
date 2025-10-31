using StepanCarSevice.Server.Entities;

namespace StepanCarSevice.Server.Models
{
    public class DetailAddingModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int CarModelId { get; set; }
        public CarModel CarModel { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
    }
}
