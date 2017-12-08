using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Machine : EntityBase
    {
        public Machine()
        {
        }

        //[Key]
        //public int MachineId { get; set; }


        //[Display (Name="Name")]
        //[MaxLength(50), Required]
        //[Index("IX_Machine_MachineName", IsUnique = true)]
        //public string MachineName { get; set; }

        //[Display(Name = "Is Active ?")]
        //public Boolean IsActive { get; set; }

        //[Display(Name = "Created By")]
        //public string CreatedBy { get; set; }

        //[Display(Name = "Modified By")]
        //public string ModifiedBy { get; set; }

        //[Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        //public DateTime CreatedDate { get; set; }

        //[Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        //public DateTime ModifiedDate { get; set; }

        [Key]
        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductID { get; set; }
        public virtual Product Product { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
