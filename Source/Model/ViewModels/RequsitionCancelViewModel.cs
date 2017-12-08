using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class RequisitionCancelHeaderViewModel
    {        
        [Display(Name = "Cancel Id")]
        public int RequisitionCancelHeaderId { get; set; }

        [Display(Name = "Cancel Type"), Required]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Cancel Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocDate { get; set; }

        [Display(Name = "Cancel No"), Required, MaxLength(20)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Person")]
        public int PersonId { get; set; }
        public string PersonName { get; set; }


        [Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public RequisitionSettingsViewModel RequisitionSettings { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }


    }

    public class RequisitionCancelLineViewModel
    {
        public int RequisitionCancelLineId { get; set; }

        [Display(Name = "Purchase Indent")]
        public int RequisitionCancelHeaderId { get; set; }
        public string RequisitionCancelHeaderDocNo { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "Request Line")]
        public int RequisitionLineId { get; set; }
        public string RequisitionDocNo { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }

        [MaxLength(50)]
        public string Specification { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public int unitDecimalPlaces { get; set; }
        public RequisitionSettingsViewModel RequisitionSettings { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string LockReason { get; set; }
    }

    public class RequisitionCancelFilterViewModel
    {
        public int RequisitionCancelHeaderId { get; set; }
        public int PersonId { get; set; }
        public string RequisitionId { get; set; }
        public string CostCenterId { get; set; }
        public string ProductId { get; set; }
        public string ProductGroupId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
    }
    public class RequisitionCancelListModel
    {
        public List<RequisitionCancelLineViewModel> RequisitionCancelViewModels { get; set; }
    }

    public class RequisitionCancelProductHelpList
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int RequisitionLineId { get; set; }
        public string RequisitionDocNo { get; set; }
        public decimal BalanceQty { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
    }

}
