using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class ProcGetBomForWeavingViewModel
    {
        public int ProductId { get; set; }
        public int ? Dimension1Id { get; set; }
        public int ? Dimension2Id{ get; set; }
        public int? Dimension3Id { get; set; }
        public int? Dimension4Id { get; set; }
        public int? CostCenterId { get; set; }
        public int ProcessId { get; set; }
        public decimal Qty { get; set; }
        public decimal? BOMQty { get; set; }


    }

    public class JobOrderBomViewModel
    {
        public int JobOrderBomId { get; set; }        
        public int JobOrderHeaderId { get; set; }
        public string JobOrderHeaderDocNo { get; set; }        
        public int? JobOrderLineId { get; set; }                
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Qty { get; set; }

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

        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public byte UnitDecimalPlaces { get; set; }
    }
}
