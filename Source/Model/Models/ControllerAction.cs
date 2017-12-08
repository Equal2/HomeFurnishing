using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class ControllerAction : EntityBase, IHistoryLog
    {
        public ControllerAction()
        {
            //DocumentTypes = new List<DocumentType>();
        }

        [Key]
        public int ControllerActionId { get; set; }


        [Display(Name = "Controller")]
        [ForeignKey("Controller")]
        public int ? ControllerId { get; set; }
        public virtual MvcController Controller { get; set; }

        [Display(Name = "Controller Name")]
        [MaxLength(100, ErrorMessage = "{0} cannot exceed {1} characters")]
        [Index("IX_ControllerAction_ControllerName", IsUnique = true, Order = 1)]
        public string ControllerName { get; set; }

        [Display(Name = "Action Name")]
        [MaxLength(100, ErrorMessage = "{0} cannot exceed {1} characters"), Required]
        [Index("IX_ControllerAction_ControllerName", IsUnique = true,Order =2)]
        public string ActionName { get; set; }
        public string PubModuleName { get; set; }

        public string DisplayName { get; set; }

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
