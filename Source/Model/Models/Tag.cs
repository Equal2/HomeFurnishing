using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Tag : EntityBase, IHistoryLog
    {
        public Tag()
        {
            ProductTag = new List<ProductTag>();
        }

        [Key]
        public int TagId { get; set; }

        [Display(Name="Tag Name")]
        [MaxLength(50), Required]
        [Index("IX_Tag_TagName", IsUnique = true)]
        public string TagName { get; set; }

        [MaxLength(50)]
        public string TagType { get; set; }

        public ICollection<ProductTag> ProductTag { get; set; }
        
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
