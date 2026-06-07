using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class EngineType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
