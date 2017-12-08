using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobOrderJobOrder : EntityBase
    {
        [Key]
        public int JobOrderJobOrderId { get; set; }

        [ForeignKey("JobOrderHeader")]
        public int JobOrderHeaderId { get; set; }
        public virtual JobOrderHeader JobOrderHeader { get; set; }

        [ForeignKey("GenJobOrderHeader")]
        public int GenJobOrderHeaderId { get; set; }
        public virtual JobOrderHeader GenJobOrderHeader { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
