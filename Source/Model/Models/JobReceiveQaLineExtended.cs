using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobReceiveQALineExtended : EntityBase
    {
        [Key]
        [ForeignKey("JobReceiveQALine")]
        [Display(Name = "JobReceiveQALine")]
        public int JobReceiveQALineId { get; set; }
        public virtual JobReceiveQALine JobReceiveQALine { get; set; }

        public Decimal? Length { get; set; }
        public Decimal? Width { get; set; }
        public Decimal? Height { get; set; }
    }
}

