using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class JobOrderCancelHeaderViewModel
    {
        [Key]
        public int JobOrderCancelHeaderId { get; set; }

        [Display(Name = "Cancel Type"), Required]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Cancel Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Cancel No"), Required, MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters")]
        public string DocNo { get; set; }

        [Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "JobWorker Name")]
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }

        public int ? GodownId { get; set; }
        public string GodownName { get; set; }

        [Display(Name = "Remark"), Required]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        public int ProcessId { get; set; }
        public string ProcessName { get; set; }        
        public int OrderById { get; set; }
        public string OrderByName { get; set; }
        public int? ReviewCount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public int ? StockHeaderId { get; set; }

    }

    public class JobOrderCancelHeaderIndexViewModel
    {
        public int JobOrderCancelHeaderId { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string JobWorkerName { get; set; }
        public string ReasonName { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string ModifiedBy { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public decimal? TotalQty { get; set; }
        public int? DecimalPlaces { get; set; }
    }

    public class JobOrderCancelFilterViewModel
    {
        public int JobOrderCancelHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Sale Order No")]
        public string JobOrderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }
    }
    public class JobOrderCancelMasterDetailModel
    {
        public List<JobOrderCancelLineViewModel> JobOrderCancelViewModels { get; set; }
    }




    public class JobOrderCancelLineViewModel
    {

        [Key]
        public int JobOrderCancelLineId { get; set; }

        [Display(Name = "Job Order Cancel"), Required]
        public int JobOrderCancelHeaderId { get; set; }
        public string JobOrderCancelHeaderDocNo { get; set; }

        [Display(Name = "Job Order"), Required]
        public int JobOrderLineId { get; set; }
        public string JobOrderDocNo { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }
        public decimal OrderQty { get; set; }
        public decimal BalanceQty { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }


        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }





        [MaxLength(50)]
        public string Specification { get; set; }
        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }
        public DateTime? DueDate { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public string BarCodes { get; set; }
        public int ? StockId { get; set; }
        public int ? StockProcessId { get; set; }
        public int ? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public int? GodownId { get; set; }
        public string LockReason { get; set; }
    }


    public class BarCodeSequenceViewModel
    {
        public string ProductName { get; set; }
        public int JobOrderLineId { get; set; }
        public int JobOrderCancelHeaderId { get; set; }
        public int JobOrderInspectionRequestHeaderId { get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProductUidId { get; set; }
        public string ProductUidIds { get; set; }
        public string FirstBarCode { get; set; }
        public string ProductUidIdName { get; set; }
    }

    public class BarCodeSequenceListViewModel
    {
        public List<BarCodeSequenceViewModel> BarCodeSequenceViewModel { get; set; }
    }


    public class BarCodeSequenceViewModelForReceive
    {
        public string ProductName { get; set; }
        public int JobOrderLineId { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobOrdLineIds { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProductUidId { get; set; }
        public string ProductUidIds { get; set; }
        public string FirstBarCode { get; set; }
        public string ProductUidIdName { get; set; }
        public string CostCenterName { get; set; }
        public string JobReceiveType { get; set; }
        public int ? ProdOrderLineId { get; set; }
    }

    //public class BarCodeSequenceViewModelForReceivePost
    //{
    //    public string ProductName { get; set; }
    //    public int JobOrderLineId { get; set; }
    //    public int TJOLID { get; set; }
    //    public int JobReceiveHeaderId { get; set; }
    //    public decimal Qty { get; set; }
    //    public decimal BalanceQty { get; set; }
    //    public int ProductUidId { get; set; }
    //    public string ProductUidIds { get; set; }
    //    public string FirstBarCode { get; set; }
    //    public string ProductUidIdName { get; set; }
    //}

    [Serializable]
    public class BarCodeSequenceListViewModelForReceive
    {
        public List<BarCodeSequenceViewModelForReceive> BarCodeSequenceViewModel { get; set; }
        public List<BarCodeSequenceViewModelForReceive> BarCodeSequenceViewModelPost { get; set; }
    }
    //public class BarCodeSequenceListViewModelForReceivePost
    //{
    //    public List<BarCodeSequenceViewModelForReceivePost> BarCodeSequenceViewModelPost { get; set; }
    //}


}
