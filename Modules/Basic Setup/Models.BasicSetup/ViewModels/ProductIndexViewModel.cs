using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.ViewModels
{
    public class ProductIndexViewModel
    {
        [Key]
        [Display(Name = "Product Id")]
        public int ProductId { get; set; }

        [Display(Name = "Universal Product Code")]
        public string ProductCode { get; set; }

        [MaxLength(50), Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Product Description")]
        [MaxLength(1000)]
        public string ProductDescription { get; set; }

        public byte[] Photo { get; set; }

        [Display(Name = "Product Group")]
        public string ProductGroupName { get; set; }

        [Display(Name = "Product Construction")]
        public string ProductConstructionName { get; set; }

        [Display(Name = "Product Collection")]
        public string ProductCollectionName { get; set; }

        [Display(Name = "Unit")]
        [MaxLength(3)]
        public String UnitName { get; set; }

        [Display(Name = "Division")]
        public string DivisionName { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
    }

    public class ProductViewModel
    {
        [Key]
        [Display(Name = "Product Id")]
        public int ProductId { get; set; }

        [MaxLength(20), Required]
        [Display(Name = "Product Code")]
        [Index("IX_Product_ProductCode", IsUnique = true)]
        public string ProductCode { get; set; }

        [MaxLength(50), Required]
        [Display(Name = "Product Name")]
        [Index("IX_Product_ProductName", IsUnique = true)]
        public string ProductName { get; set; }

        [Display(Name = "Product Description")]
        [MaxLength(1000)]
        public string ProductDescription { get; set; }

        [Display(Name = "Standard Cost")]
        public decimal? StandardCost { get; set; }

        [Display(Name = "Product Category")]
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }

        [Display(Name = "Product Group")]
        public int ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }

        [Display(Name = "Product Collection")]
        public int? ProductCollectionId { get; set; }
        public string ProductCollectionName { get; set; }

        [Display(Name = "Product Quality")]
        public int? ProductQualityId { get; set; }
        public string ProductQualityName { get; set; }

        [Display(Name = "Product Design")]
        public int? ProductDesignId { get; set; }
        public string ProductDesignName { get; set; }

        [Display(Name = "Product Shape")]
        public int? ProductShapeId { get; set; }
        public string ProductShapeName { get; set; }

        [Display(Name = "Colour")]
        public int? ColourId { get; set; }
        public string ColourName { get; set; }

        [Display(Name = "Description Of Goods")]
        public int? DescriptionOfGoodsId { get; set; }
        public string DescriptionOfGoodsName { get; set; }

        [Display(Name = "Product Invoice Group")]
        public int? ProductInvoiceGroupId { get; set; }
        public string ProductInvoiceGroupName { get; set; }

        [Display(Name = "Product Process Sequence")]
        public int? ProcessSequenceHeaderId { get; set; }
        public string ProcessSequenceHeaderName { get; set; }

        [Display(Name = "ProductManufacturer")]
        public int? ProductManufacturerId { get; set; }
        public string ProductManufacturerName { get; set; }

        [Display(Name = "ProductStyle")]
        public int? ProductStyleId { get; set; }
        public string ProductStyleName { get; set; }

        [Display(Name = "SalesTaxGroupProduct")]
        public int? SalesTaxGroupProductId { get; set; }
        public string SalesTaxGroupProductName { get; set; }

        [Display(Name = "DrawBack Tarif fHead")]
        public int? DrawBackTariffHeadId { get; set; }
        public string DrawBackTariffHeadName { get; set; }

        [Display(Name = "Unit")]
        [MaxLength(3)]
        public String UnitId { get; set; }
        public string UnitName { get; set; }

        [Display(Name = "Division")]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Discontinued Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DiscontinuedDate { get; set; }

        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }

        public bool IsSample { get; set; }
        public string SampleName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }
        public Decimal? StandardWeight { get; set; }
        public int? OriginCountryId { get; set; }
        public string Tags { get; set; }
        public string ProductSpecification { get; set; }
        public int DT_RowId { get; set; }

        [Display(Name = "Map Scale")]
        public int? MapScale { get; set; }

        [Display(Name = "Trace Type")]
        public string TraceType { get; set; }

        [Display(Name = "Map Type")]
        public string MapType { get; set; }
        public decimal ? Rate { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Incentive { get; set; }
        public decimal? Loss { get; set; }
        public decimal? Weight { get; set; }
        public int ? ProductRateGroupId { get; set; }
        public decimal NField1 { get; set; }
        public string ProductionRemark { get; set; }
        public Decimal? CBM { get; set; }
        public int ProductSKUId { get; set; }
    }

    public class MaterialViewModel : ProductViewModel
    {
        public int? ProductTypeId { get; set; }
        [Display(Name = "Minimum Order Qty")]
        public Decimal? MinimumOrderQty { get; set; }

        [Display(Name = "ReOrder Level")]
        public Decimal? ReOrderLevel { get; set; }
        [ForeignKey("Godown")]
        [Display(Name = "Godown")]
        public int? GodownId { get; set; }
        public string GodownName { get; set; }

        [Display(Name = "Bin Location")]
        [MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters")]
        public string BinLocation { get; set; }
        [ForeignKey("Site")]
        [Display(Name = "Site")]
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        [Display(Name = "LOT Management")]
        public Boolean LotManagement { get; set; }
        public int ProductSiteDetailId { get; set; }
        public int ? ReferenceDocId { get; set; }
    }
}
