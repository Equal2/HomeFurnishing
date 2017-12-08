using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Dimension4Types : EntityBase, IHistoryLog
    {
        public Dimension4Types()
        {
        }

        [Key]
        public int Dimension4TypeId { get; set; }

        [Display(Name="Dimension4 Type")]
        [MaxLength(50, ErrorMessage = "Dimension4Type Name cannot exceed 50 characters"), Required]
        [Index("IX_Dimension4Type_Dimension4TypeName", IsUnique = true)]
        public string Dimension4TypeName { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
