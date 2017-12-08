using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MenuSite : EntityBase, IHistoryLog
    {

        [Key]
        public int DocumentTypeSiteId { get; set; }


        [ForeignKey("Menu")]
        [Display(Name = "Menu")]
        public int MenuId { get; set; }
        public virtual Menu Menu { get; set; }


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
