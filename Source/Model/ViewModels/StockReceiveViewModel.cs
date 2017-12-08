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

    public class RequisitionFiltersForReceive
    {
        public int StockHeaderId { get; set; }
        public int PersonId { get; set; }
        public string ProductId { get; set; }
        public string ProductGroupId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }
        public string CostCenterId { get; set; }
        public string RequisitionHeaderId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }


    public class StockProcessFiltersForReceive
    {
        public int StockHeaderId { get; set; }
        public int PersonId { get; set; }
        public string ProductId { get; set; }
        public string ProductGroupId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }
        public string CostCenterId { get; set; }
        public string StockProcessInHeaderId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }



    public class StockReceiveLineViewModel
    {
        public int StockLineId { get; set; }

        public int StockHeaderId { get; set; }
        public string StockHeaderDocNo { get; set; }

        [Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public string ProductUidIdName { get; set; }

        [Display(Name = "Product"), Required]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "From Process")]
        public int? FromProcessId { get; set; }
        public string FromProcessName { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal QtyRec { get; set; }
        public decimal QtyIss { get; set; }
        public decimal Qty { get; set; }
        public Decimal? RequisitionBalanceQty { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Request No")]
        public int? RequisitionLineId { get; set; }
        public string RequisitionHeaderDocNo { get; set; }

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
        public decimal? Rate { get; set; }

        public decimal? Amount { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public byte? UnitDecimalPlaces { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public int? ToCostCenterId { get; set; }
        public string ToCostCenterName { get; set; }
        public int? RequiredProductId { get; set; }
        public string RequiredProductName { get; set; }
        public StockHeaderSettingsViewModel StockHeaderSettings { get; set; }
        public int? GodownId { get; set; }
        public int? FromGodownId { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public Decimal BalanceQty { get; set; }

        [Display(Name = "Person")]
        public int? PersonId { get; set; }
        public string PersonName { get; set; }

        public int? ReferenceDocTypeId { get; set; }

        public int? ReferenceDocId { get; set; }

        public int? ReferenceDocLineId { get; set; }
    }

    public class StockReceiveMasterDetailModel
    {
        public List<StockReceiveLineViewModel> StockLineViewModel { get; set; }
        public StockHeaderSettingsViewModel StockHeaderSettings { get; set; }
    }

}
