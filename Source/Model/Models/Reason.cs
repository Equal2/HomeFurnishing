using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Reason : EntityBase, IHistoryLog
    {
        public Reason()
        {
        }

        [Key]
        public int ReasonId { get; set; }

        [Display(Name="Reason")]
        [MaxLength(100, ErrorMessage = "Reason Name cannot exceed 100 characters"), Required]
        [Index("IX_Reason_ReasonName", IsUnique = true, Order = 1)]
        public string ReasonName { get; set; }

        [ForeignKey("DocumentCategory")]
        [Index("IX_Reason_ReasonName", IsUnique = true, Order = 2)]
        public int DocumentCategoryId { get; set; }
        public virtual DocumentCategory DocumentCategory { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        

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
