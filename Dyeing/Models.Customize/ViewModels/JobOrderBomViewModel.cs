using System.ComponentModel.DataAnnotations;

namespace Models.Customize.ViewModels
{
    public class ProcGetBomForWeavingViewModel
    {
        public int ProductId { get; set; }
        public int ? Dimension1Id { get; set; }
        public int ? Dimension2Id{ get; set; }
        public int? CostCenterId { get; set; }
        public int ProcessId { get; set; }
        public decimal Qty { get; set; }


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
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public byte UnitDecimalPlaces { get; set; }
    }
}
