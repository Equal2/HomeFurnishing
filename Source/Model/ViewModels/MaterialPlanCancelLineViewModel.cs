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
    public class MaterialPlanCancelLineViewModel
    {
        public int MaterialPlanCancelLineId { get; set; }
        public int MaterialPlanCancelHeaderId { get; set; }
        public string MaterialPlanCancelHeaderDocNo { get; set; }
        public int MaterialPlanLineId { get; set; }
        public string MaterialPlanDocNo { get; set; }
       
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public string ProductName{ get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public string Remark { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public int ? unitDecimalPlaces { get; set; }

        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public MaterialPlanSettingsViewModel MaterialPlanSettings { get; set; }
        public string LockReason { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }


    public class MaterialPlanCancelLineHelpListViewModel
    {
        public int MaterialPlanCancelHeaderId { get; set; }
        public int MaterialPlanCancelLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
    }
}
