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
    [Table("ViewProdOrderHeader")]
    public class ViewProdOrderHeader
    {
        [Key]
        public int ProdOrderHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public DateTime DueDate { get; set; }
        public int Status { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal TotalQty { get; set; }
    }
    
    
    [Table("ViewProdOrderLine")]
    public class ViewProdOrderLine
    {
        [Key]
        public int ProdOrderLineId { get; set; }
        public int ProdOrderHeaderId { get; set; }
        public decimal Qty { get; set; }
        public int ProductId { get; set; }
        public string Remark { get; set; }
    }

    [Table("ViewProdOrderBalance")]
    public class ViewProdOrderBalance :EntityBase
    {
        [Key]
        public int ProdOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProdOrderHeaderId { get; set; }
        public string ProdOrderNo { get; set; }
                
        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public DateTime IndentDate { get; set; }
        
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

        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public int ? ReferenceDocLineId { get; set; }
        public int ? ReferenceDocTypeId { get; set; }
        public int? BuyerId { get; set; }
    }

    [Table("ViewProdOrderBalanceForMPlan")]
    public class ViewProdOrderBalanceForMPlan
    {
        [Key]
        public int ProdOrderLineId { get; set; }
        public int DocTypeId { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProdOrderHeaderId { get; set; }
        public string ProdOrderNo { get; set; }
        public int ProductId { get; set; }
        public DateTime IndentDate { get; set; }
    }

    [Table("ViewSizeinCms")]
    public class ViewSizeinCms
    {
        [Key]
        public int SizeId { get; set; }
        public string SizeName { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Area { get; set; }
    }

    [Table("ViewRugSize")]
    public class ViewRugSize
    {
        [Key]
        public int ProductId { get; set; }       
        public int ? StandardSizeID { get; set; }
        public string StandardSizeName { get; set; }
        public decimal? StandardSizeArea { get; set; }
        public int ? ManufaturingSizeID { get; set; }
        public string ManufaturingSizeName { get; set; }
        public decimal? ManufaturingSizeArea { get; set; }
        public int ? FinishingSizeID { get; set; }
        public string FinishingSizeName { get; set; }
        public decimal? FinishingSizeArea { get; set; }
        public int? StencilSizeId { get; set; }
        public string StencilSizeName { get; set; }
        public decimal? StencilSizeArea { get; set; }
        public int? MapSizeId { get; set; }
        public string MapSizeName { get; set; }
        public decimal? MapSizeArea { get; set; }
    }

    [Table("ViewRugArea")]
    public class ViewRugArea
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int SizeId { get; set; }
        public int ProductShapeId { get; set; }
        public string SizeName { get; set; }
        public decimal Area { get; set; }
        public string UnitId { get; set; }
        public decimal SqYardPerPcs { get; set; }
    }
}
