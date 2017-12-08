using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity ;
using Microsoft.AspNet.Identity.EntityFramework ;
namespace Model.Models
{
    public class RolesMenu : EntityBase, IHistoryLog
    {

        [Key]
        public int RolesMenuId { get; set; }
       
        [Display(Name = "Role"),MaxLength(128)]
        [ForeignKey("Role")]
        public string RoleId { get; set; }        
        public virtual IdentityRole Role { get; set; }

        [Display(Name = "Menu")]
        [ForeignKey("Menu")]
        public int MenuId { get; set; }
        public virtual Menu Menu{ get; set; }

        public bool FullHeaderPermission { get; set; }
        public bool FullLinePermission { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_PurchaseOrderHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_PurchaseOrderHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }


        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
    }
}
