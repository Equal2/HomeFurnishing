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
    public class MaterialPlanLineViewModel
    {
        public int MaterialPlanLineId { get; set; }
        public int MaterialPlanHeaderId { get; set; }
        public string MaterialPlanHeaderDocNo { get; set; }
        [MaxLength(10)]
        public string GeneratedFor { get; set;}
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public string ProductName{ get; set; }
        public decimal RequiredQty { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal ExcessStockQty { get; set; }
        public decimal StockPlanQty { get; set; }
        public decimal ProdPlanQty { get; set; }
        public decimal PurchPlanQty { get; set; }
        [Display(Name = "Remark")]
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



        [Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        [Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }

        public string Specification { get; set; }

        public MaterialPlanSettingsViewModel MaterialPlanSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }


    public class MaterialPlanLineHelpListViewModel
    {
        public int MaterialPlanHeaderId { get; set; }
        public int MaterialPlanLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
    }
}
