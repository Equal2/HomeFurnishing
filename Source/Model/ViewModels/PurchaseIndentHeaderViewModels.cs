using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class PurchaseIndentHeaderViewModel 
    {

        [Key]
        [Display(Name = "Purchase Indent Id")]
        public int PurchaseIndentHeaderId { get; set; }

        [Display(Name = "Indent Type"), Required]
        [ForeignKey("DocType")]
        [Index("IX_PurchaseIndentHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
        public string DocumentTypeName { get; set; }

        [Display(Name = "Indent Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocDate { get; set; }

        [Display(Name = "Indent No"), Required, MaxLength(20)]
        [Index("IX_PurchaseIndentHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_PurchaseIndentHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        
        public int ? ReasonId { get; set; }
        public string ReasonName { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        [ForeignKey("MaterialPlanHeader")]
        public int? MaterialPlanHeaderId { get; set; }
        public string MaterialPlanDocNo { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }
        [Display(Name="Department")]
        public string DepartmentName { get; set; }

        public PurchaseIndentSettingsViewModel PurchaseIndentSettings { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FirstName { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string LockReason { get; set; }

    }

    public class PurchaseIndentHeaderViewModelWithLog: PurchaseIndentHeaderViewModel
    {
        [Required,MinLength(20)]
        public string LogReason { get; set; }
    }
    public class PurchaseIndentMasterDetailModel
    {
        public int PurchaseIndentHeaderId { get; set; }
        public PurchaseIndentHeaderViewModelWithLog PurchaseIndentHeaderViewModelWithLog { get; set; }
        public List<PurchaseIndentLineViewModel> PurchaseIndentLineViewModel { get; set; }
    }
    public class PurchaseIndentHeaderListViewModel
    {
        public int PurchaseIndentHeaderId { get; set; }
        public int PurchaseIndentLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
    }
   
}
