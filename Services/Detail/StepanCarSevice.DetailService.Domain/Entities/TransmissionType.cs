using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class TransmissionType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
