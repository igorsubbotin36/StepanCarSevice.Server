using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Core.Entities
{
    abstract public class AppCommonEntity
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
