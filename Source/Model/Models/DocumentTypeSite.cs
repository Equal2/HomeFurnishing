using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class DocumentTypeSite : EntityBase, IHistoryLog
    {

        [Key]
        public int DocumentTypeSiteId { get; set; }


        [ForeignKey("DocumentType")]
        [Display(Name = "Document Type")]
        public int DocumentTypeId { get; set; }
        public virtual DocumentType DocumentType { get; set; }


        [ForeignKey("Site")]
        [Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

     
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
