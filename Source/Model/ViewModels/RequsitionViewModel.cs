using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class RequisitionHeaderViewModel
    {        
        [Display(Name = "Requisition Id")]
        public int RequisitionHeaderId { get; set; }

        [Display(Name = "Requisition Type"), Required]     
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Requisition Date"), Required]        
        public DateTime DocDate { get; set; }

        [Display(Name = "Requisition No"), Required, MaxLength(20)]        
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Person")]        
        public int PersonId { get; set; }
        public string PersonName { get; set; }

        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }

        [Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public MaterialRequestSettingsViewModel MaterialRequestSettings { get; set; }

        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string LockReason { get; set; }
    }

    public class RequisitionLineViewModel
    {
        public int RequisitionLineId { get; set; }

        [Display(Name = "Purchase Indent")]        
        public int RequisitionHeaderId { get; set; }
        public string RequisitionHeaderDocNo { get; set; }

        [Display(Name = "Product"), Required]        
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }



        [Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        [Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }


        [MaxLength(50)]
        public string Specification { get; set; }

        public DateTime? DueDate { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public MaterialRequestSettingsViewModel MaterialRequestSettings { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string CostCenterName { get; set; }
        public int ? CostCenterId { get; set; }
        public string LockReason { get; set; }

    }


    public class RequisitionHelpListViewModel
    {
        public int RequisitionHeaderId { get; set; }
        public string DocNo { get; set; }
    }

    public class MaterialRequestBalanceSummaryViewModel
    {
        public int RequisitionHeaderId { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DueDate { get; set; }
        public string DocNo { get; set; }
        public int DateDiff { get; set; }
        public string CosCenterName { get; set; }
        public string JobWorkerName { get; set; }
        public string Design { get; set; }
        public int UndReqShade { get; set; }
        public decimal UndReqShadeQty { get; set; }
        public int UndIssueShade { get; set; }
        public decimal UndIssueShadeQty { get; set; }
        public int DyedReqShade { get; set; }
        public decimal DyedReqShadeQty { get; set; }
        public int DyedIssueShade { get; set; }
        public decimal DyedIssueShadeQty { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
    }


}
