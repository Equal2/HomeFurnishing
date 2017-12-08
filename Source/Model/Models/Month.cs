using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surya.India.Model.Models
{
    public class Month : EntityBase
    {
        [Key]
        public int MonthId { get; set; }

        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "Month Name cannot exceed 50 characters"), Required]
        [Index("IX_Month_MonthName", IsUnique = true)]
        public string MonthName { get; set; }
    }
}
