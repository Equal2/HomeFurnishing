using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Dimension2Types : EntityBase, IHistoryLog
    {
        public Dimension2Types()
        {
        }

        [Key]
        public int Dimension2TypeId { get; set; }

        [Display(Name="Dimension2 Type")]
        [MaxLength(50, ErrorMessage = "Dimension2Type Name cannot exceed 50 characters"), Required]
        [Index("IX_Dimension2Type_Dimension2TypeName", IsUnique = true)]
        public string Dimension2TypeName { get; set; }

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
