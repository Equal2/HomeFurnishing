using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class Product : EntityBase, IHistoryLog
    {
        public Product()
        {            
            //ProductSuppliers = new List<ProductSupplier>();
            //ProductBuyers = new List<ProductBuyer>();
            //ProductIncludedAccessories = new List<ProductIncludedAccessories>();
            //ProductRelatedAccessories = new List<ProductRelatedAccessories>();
            //PurchaseOrderLines = new List<PurchaseOrderLine>();
            //PurchaseGoodsReceiptLines = new List<PurchaseGoodsReceiptLine>();
            //ProductAttributes = new List<ProductAttributes>();
            //PurchaseIndentLines = new List<PurchaseIndentLine>();
            //JobOrderLine  = new List<JobOrderLine>();
            //JobOrderBom = new List<JobOrderBom>();
            //MaterialPlanLine = new List<MaterialPlanLine>();
            //ProdOrderLine = new List<ProdOrderLine>();
            //SaleOrderLine = new List<SaleOrderLine>();
            //PackingLine = new List<PackingLine>();
            //RequisitionLine = new List<RequisitionLine>();
            //StockLine = new List<StockLine>();
            //Stock = new List<Stock>();
            //StockProcess = new List<StockProcess>();
            //ProductSize = new List<ProductSize>();
        }

        [Key]
        [Display(Name = "Product Id")]
        public int ProductId { get; set; }

        [ForeignKey("DocType")]
        public int? DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [MaxLength(20, ErrorMessage = "ProductCode cannot exceed 20 characters"), Required]
        [Display(Name = "Product Code")]
        [Index("IX_Product_ProductCode", IsUnique = true)]
        public string ProductCode { get; set; }

        [MaxLength(50, ErrorMessage = "Product Name cannot exceed 50 characters"), Required]
        [Display(Name = "Product Name")]
        [Index("IX_Product_ProductName", IsUnique = true)]
        public string ProductName { get; set; }
        
        [Display(Name = "Product Description")]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]        
        public string ProductDescription { get; set; }

        [Display(Name = "Product Specification")]
        public string ProductSpecification { get; set; }

        [Display(Name = "Standard Cost")]
        public decimal? StandardCost { get; set; }


        [ForeignKey("ProductGroup")]
        [Display(Name = "Product Group")]
        public int? ProductGroupId { get; set; }        
        public virtual ProductGroup ProductGroup { get; set; }

        [ForeignKey("SalesTaxGroupProduct")]
        [Display(Name = "SalesTaxGroupProduct")]
        public int ? SalesTaxGroupProductId { get; set; }
        public virtual SalesTaxGroupProduct SalesTaxGroupProduct { get; set; }

        [Display(Name = "DrawBack Tarif fHead")]
        public int ? DrawBackTariffHeadId { get; set; }

        [ForeignKey("Unit")]
        [Display(Name = "Unit")]
        [MaxLength(3)]
        public String UnitId { get; set; }        
        public virtual Unit Unit { get; set; }

        [ForeignKey("Division")]
        [Display(Name = "Division")]
        public int DivisionId { get; set; }        
        public virtual Division Division { get; set; }

        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }

        public Decimal? StandardWeight { get; set; }
        public string Tags { get; set; }

        public Decimal? CBM { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; } 
        
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }



        [ForeignKey("DocumentType")]
        public int ? ReferenceDocTypeId { get; set; }
        public virtual DocumentType DocumentType { get; set; }
        public int ? ReferenceDocId { get; set; }
        //public ICollection<ProductSupplier> ProductSuppliers { get; set; }
        //public ICollection<ProductBuyer> ProductBuyers { get; set; }
        //public ICollection<ProductRelatedAccessories> ProductRelatedAccessories { get; set; }
        //public ICollection<ProductIncludedAccessories> ProductIncludedAccessories { get; set; }
        //public ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; }
        //public ICollection<PurchaseGoodsReceiptLine> PurchaseGoodsReceiptLines { get; set; }
        //public ICollection<ProductAttributes> ProductAttributes { get; set; }
        //public ICollection<PurchaseIndentLine> PurchaseIndentLines { get; set; }
        //public ICollection<JobOrderLine> JobOrderLine { get; set; }
        //public ICollection<JobOrderBom> JobOrderBom { get; set; }
        //public ICollection<MaterialPlanLine> MaterialPlanLine { get; set; }
        //public ICollection<ProdOrderLine> ProdOrderLine { get; set; }
        //public ICollection<SaleOrderLine> SaleOrderLine { get; set; }
        //public ICollection<PackingLine> PackingLine { get; set; }
        //public ICollection<RequisitionLine> RequisitionLine { get; set; }
        //public ICollection<StockLine> StockLine { get; set; }
        //public ICollection<Stock> Stock { get; set; }
        //public ICollection<StockProcess> StockProcess { get; set; }
        //public ICollection<ProductSize> ProductSize { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
