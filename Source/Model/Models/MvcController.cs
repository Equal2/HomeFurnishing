using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MvcController : EntityBase, IHistoryLog
    {

        [Key]
        public int ControllerId { get; set; }

        [Display (Name="Controller Name")]
        [MaxLength(100, ErrorMessage = "{0} cannot exceed {1} characters"), Required]
        [Index("IX_Controller_ControllerName", IsUnique = true,Order =1)]
        public string ControllerName { get; set; }

        [Display(Name = "Parent Controller")]
        [ForeignKey("ParentController")]
        public int? ParentControllerId { get; set; }        
        public virtual MvcController ParentController { get; set; }

        [MaxLength(30),Index("IX_Controller_ControllerName", IsUnique = true, Order = 2)]
        public string PubModuleName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

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
