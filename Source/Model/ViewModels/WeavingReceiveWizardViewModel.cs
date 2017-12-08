using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    [Serializable]
    public class WeavingReceiveWizardViewModel
    {
        public string DocNo { get; set; }
        public string Date { get; set; }
        public int  JobWorkerId { get; set; }
        public int? CostCenterId { get; set; }
        public string JobWorkerName { get; set; }
        public string DesignName { get; set; }
        public string Colour { get; set; }
        public string Size { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Qty { get; set; }
        public decimal OtherQty { get; set; }
        public decimal ? Area { get; set; }
        public int JobOrderLineId { get; set; }
        public string ProductUidIdName { get; set; }
        public string FromProductUidName { get; set; }
        public string ToProductUidName { get; set; }
        public decimal Rate { get; set; }
        public decimal Incentive { get; set; }
        public decimal AreaPerPc { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public string OtherDealUnitId { get; set; }
        public string OtherDealUnitName { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public decimal OtherAmount { get; set; }
    }

    public class WeavingReceiveWizardMasterDetailModel
    {
        public List<WeavingReceiveWizardViewModel> WeavingReceiveWizardViewModel { get; set; }
    }

}
