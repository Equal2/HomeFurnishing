using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Dimension3Types : EntityBase, IHistoryLog
    {
        public Dimension3Types()
        {
        }

        [Key]
        public int Dimension3TypeId { get; set; }

        [Display(Name="Dimension3 Type")]
        [MaxLength(50, ErrorMessage = "Dimension3Type Name cannot exceed 50 characters"), Required]
        [Index("IX_Dimension3Type_Dimension3TypeName", IsUnique = true)]
        public string Dimension3TypeName { get; set; }

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
