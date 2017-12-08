using Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DatabaseViews
{

    [Table("ViewMaterialRequestBalance")]
    public class ViewMaterialRequestBalance
    {
        [Key]
        public int RequisitionLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int RequisitionHeaderId { get; set; }
        public string MaterialRequestNo { get; set; }
        public int PersonId { get; set; }
        public int ProductId { get; set; }
        public int ?  Dimension1Id { get; set; }
        public int ? Dimension2Id { get; set; }
        public DateTime MaterialRequestDate { get; set; }
    }

    [Table("_Roles")]
    public class ViewRoles
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    [Table("ViewRequisitionBalance")]
    public class ViewRequisitionBalance
    {
        [Key]
        public int RequisitionLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int RequisitionHeaderId { get; set; }
        public string RequisitionNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
        public DateTime OrderDate { get; set; }


        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }



        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }


        [ForeignKey("CostCenter")]
        public int ? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }


    }

}
