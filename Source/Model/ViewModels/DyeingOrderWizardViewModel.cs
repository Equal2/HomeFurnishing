using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class DyeingOrderWizardViewModel
    {
        public int ProdOrderHeaderId { get; set; }
        public string BuyerCode { get; set; }
        public string Dimension2Name { get; set; }
        public string ProductList { get; set; }
        public string Dimension1List { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceQty { get; set; }
        public string ProdOrderLineIdList { get; set; }
        public int ProdOrderLineId { get; set; }
        public string ProdOrderNo { get; set; }
        public string ProdOrderDate { get; set; }
        public DateTime DocDate { get; set; }
    }
    [Serializable]
    public class WeavingOrderWizardViewModel
    {
        public int DocTypeId { get; set; }
        public string DocNo { get; set; }
        //public DateTime DocDate { get; set; }
        public string Date { get; set; }
        public string DueDate { get; set; }
        //public DateTime ? DueDate { get; set; }
        public int ? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string Quality { get; set; }
        public string DesignName { get; set; }
        public string DesignPatternName { get; set; }
        public string Size { get; set; }
        public string Colour { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Qty { get; set; }
        public decimal OtherQty { get; set; }
        public decimal ? Area { get; set; }
        public int ProdOrderLineId { get; set; }
        public int? RefDocTypeId { get; set; }
        public int? RefDocLineId { get; set; }
        public int ? FirstBarCode { get; set; }
        public string ProductUidIdName { get; set; }
        public string SelectedBarCodes { get; set; }
        public decimal Rate { get; set; }
        public decimal Loss { get; set; }
        public decimal Incentive { get; set; }
        public decimal AreaPerPc { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public string OtherDealUnitId { get; set; }
        public string OtherDealUnitName { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public decimal OtherAmount { get; set; }
        public int? ProductCategoryId { get; set; }
    }

    public class WeavingOrderWizardMasterDetailModel
    {
       public List<WeavingOrderWizardViewModel> WeavingOrderWizardViewModel { get; set; }
    }

}
