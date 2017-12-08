using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using Model.ViewModels;



namespace Model.ViewModel
{
    public class WeavingReceiveQACombinedViewModel
    {
        public int JobReceiveHeaderId { get; set; }
        public int JobReceiveLineId { get; set; }

        public int JobReceiveQAHeaderId { get; set; }

        public int JobReceiveQALineId { get; set; }
        public int JobReceiveQAAttributeId { get; set; }


        public int StockHeaderId { get; set; }

        public int StockId { get; set; }

        

        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public int GodownId { get; set; }
        public int ProcessId { get; set; }
        public int JobWorkerId { get; set; }
        public int? JobReceiveById { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public string LotNo { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductQualityName { get; set; }
        public int JobOrderLineId { get; set; }
        public string JobOrderHeaderDocNo { get; set; }
        public string CostCenterNo { get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }
        public string UnitId { get; set; }
        public string DealUnitId { get; set; }
        public Decimal UnitConversionMultiplier { get; set; }
        public decimal DealQty { get; set; }
        public decimal StandardWeight { get; set; }
        public decimal Weight { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal? PenaltyRate { get; set; }
        public decimal? PenaltyAmt { get; set; }

        public int? ReferenceDocId { get; set; }
        public int? ReferenceDocTypeId { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<QAGroupLineLineViewModel> QAGroupLine { get; set; }
        public JobReceiveSettingsViewModel JobReceiveSettings { get; set; }
        public JobReceiveQASettingsViewModel JobReceiveQASettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        public Decimal? OrderLength { get; set; }
        public Decimal? OrderWidth { get; set; }

        public Decimal? Length { get; set; }
        public Decimal? Width { get; set; }
        public Decimal? Height { get; set; }
        public int? DimensionUnitDecimalPlaces { get; set; }

        public Decimal? XRate { get; set; }
        public string OMSId { get; set; }

    }

    public class WeavingReceiveQACombinedViewModel_ByProductUid
    {
        public int JobReceiveHeaderId { get; set; }
        public int JobReceiveLineId { get; set; }

        public int JobReceiveQAHeaderId { get; set; }

        public int JobReceiveQALineId { get; set; }
        public int JobReceiveQAAttributeId { get; set; }


        public int StockHeaderId { get; set; }

        public int StockId { get; set; }



        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public int GodownId { get; set; }
        public int ProcessId { get; set; }
        public int JobWorkerId { get; set; }
        public int? JobReceiveById { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public string LotNo { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CostcenterName { get; set; }        
        public string ProductCategoryName { get; set; }
        public string ProductQualityName { get; set; }
        public string ProductGroupName { get; set; }
        public string ColourName { get; set; }
        public string SiteName { get; set; }
        public string PONo { get; set; }
        public string PaymentSlipNo { get; set; }
        public string InvoiceNo { get; set; }
        public string RollNo { get; set; }
        public string InvoiceParty { get; set; }
        public int JobOrderLineId { get; set; }
        public string JobOrderHeaderDocNo { get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }
        public string UnitId { get; set; }
        public string DealUnitId { get; set; }
        public Decimal UnitConversionMultiplier { get; set; }
        public decimal DealQty { get; set; }
        public decimal StandardWeight { get; set; }
        public decimal Weight { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal? PenaltyRate { get; set; }
        public decimal? PenaltyAmt { get; set; }

        public decimal? TDSCom { get; set; }
        public decimal? NetAmount { get; set; }

        public int? ReferenceDocId { get; set; }
        public int? ReferenceDocTypeId { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<QAGroupLineLineViewModel> QAGroupLine { get; set; }
        public JobReceiveSettingsViewModel JobReceiveSettings { get; set; }
        public JobReceiveQASettingsViewModel JobReceiveQASettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        public Decimal? OrderLength { get; set; }
        public Decimal? OrderWidth { get; set; }

        public Decimal? Length { get; set; }
        public Decimal? Width { get; set; }
        public Decimal? Height { get; set; }
        public int? DimensionUnitDecimalPlaces { get; set; }

        public Decimal? XRate { get; set; }
        public string OMSId { get; set; }
        public string Message { get; set; }

    }

}
