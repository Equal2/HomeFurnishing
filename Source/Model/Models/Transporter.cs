using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Core.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class Transporter : EntityBase
    {
        public Transporter()
        {
           
        }

        [Key]
        [ForeignKey("Person"), Display(Name = "Person")]
        public int PersonID { get; set; }
        public virtual Person Person { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
