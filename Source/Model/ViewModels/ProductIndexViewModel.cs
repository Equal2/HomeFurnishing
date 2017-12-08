using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
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
        public DateTime? DiscontinueDate { get; set; }
    }

    public class ProductViewModel
    {
        [Key]
        [Display(Name = "Product Id")]
        public int ProductId { get; set; }

        [MaxLength(50), Required]
        [Display(Name = "Product Code")]
        [Index("IX_Product_ProductCode", IsUnique = true)]
        public string ProductCode { get; set; }

        [MaxLength(100), Required]
        [Display(Name = "Product Name")]
        [Index("IX_Product_ProductName", IsUnique = true)]
        public string ProductName { get; set; }

        [Display(Name = "Product Description")]
        [MaxLength(1000)]
        public string ProductDescription { get; set; }

        [Display(Name = "Standard Cost")]
        public decimal? StandardCost { get; set; }
        public decimal? SaleRate { get; set; }

        [ForeignKey("ProductCategory")]
        [Display(Name = "Product Category")]
        public int? ProductCategoryId { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
        public string ProductCategoryName { get; set; }

        [ForeignKey("ProductGroup")]
        [Display(Name = "Product Group")]
        public int ProductGroupId { get; set; }
        public virtual ProductGroup ProductGroup { get; set; }
        public string ProductGroupName { get; set; }

        [ForeignKey("ProductCollection")]
        [Display(Name = "Product Collection")]
        public int? ProductCollectionId { get; set; }
        public virtual ProductCollection ProductCollection { get; set; }
        public string ProductCollectionName { get; set; }

        [ForeignKey("ProductQuality")]
        [Display(Name = "Product Quality")]
        public int? ProductQualityId { get; set; }
        public virtual ProductQuality ProductQuality { get; set; }
        public string ProductQualityName { get; set; }

        [ForeignKey("ProductDesign")]
        [Display(Name = "Product Design")]
        public int? ProductDesignId { get; set; }
        public virtual ProductDesign ProductDesign { get; set; }
        public string ProductDesignName { get; set; }

        [ForeignKey("ProductShape")]
        [Display(Name = "Product Shape")]
        public int? ProductShapeId { get; set; }
        public virtual ProductShape ProductShape { get; set; }
        public string ProductShapeName { get; set; }


        [ForeignKey("Colour")]
        [Display(Name = "Colour")]
        public int? ColourId { get; set; }
        public virtual Colour Colour { get; set; }
        public string ColourName { get; set; }

        [ForeignKey("DescriptionOfGoods")]
        [Display(Name = "Description Of Goods")]
        public int? DescriptionOfGoodsId { get; set; }
        public virtual DescriptionOfGoods DescriptionOfGoods { get; set; }
        public string DescriptionOfGoodsName { get; set; }

        [ForeignKey("ProductInvoiceGroup")]
        [Display(Name = "Product Invoice Group")]
        public int? ProductInvoiceGroupId { get; set; }
        public virtual ProductInvoiceGroup ProductInvoiceGroup { get; set; }
        public string ProductInvoiceGroupName { get; set; }

        [ForeignKey("ProcessSequenceHeader")]
        [Display(Name = "Product Process Sequence")]
        public int? ProcessSequenceHeaderId { get; set; }
        public virtual ProcessSequenceHeader ProcessSequenceHeader { get; set; }
        public string ProcessSequenceHeaderName { get; set; }

        [ForeignKey("ProductManufacturer")]
        [Display(Name = "ProductManufacturer")]
        public int? ProductManufacturerId { get; set; }
        public virtual ProductManufacturer ProductManufacturer { get; set; }
        public string ProductManufacturerName { get; set; }

        [ForeignKey("ProductStyle")]
        [Display(Name = "ProductStyle")]
        public int? ProductStyleId { get; set; }
        public virtual ProductStyle ProductStyle { get; set; }
        public string ProductStyleName { get; set; }

        [ForeignKey("SalesTaxGroupProduct")]
        [Display(Name = "SalesTaxGroupProduct")]
        public int? SalesTaxGroupProductId { get; set; }
        public virtual SalesTaxGroupProduct SalesTaxGroupProduct { get; set; }
        public string SalesTaxGroupProductName { get; set; }

        [ForeignKey("DrawBackTariffHead")]
        [Display(Name = "DrawBack Tarif fHead")]
        public int? DrawBackTariffHeadId { get; set; }
        public virtual DrawBackTariffHead DrawBackTariffHead { get; set; }
        public string DrawBackTariffHeadName { get; set; }

        [ForeignKey("Unit")]
        [Display(Name = "Unit")]
        [MaxLength(3)]
        public String UnitId { get; set; }
        public virtual Unit Unit { get; set; }
        public string UnitName { get; set; }

        [ForeignKey("Division")]
        [Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
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
        public virtual Country OriginCountry { get; set; }
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

        public Decimal? ProfitMargin { get; set; }
        public Decimal? CarryingCost { get; set; }
        public int ProductSKUId { get; set; }


        public int? DefaultDimension1Id { get; set; }
        public string DefaultDimension1Name { get; set; }
        public int? DefaultDimension2Id { get; set; }
        public string DefaultDimension2Name { get; set; }

        public int? DefaultDimension3Id { get; set; }
        public string DefaultDimension3Name { get; set; }

        public int? DefaultDimension4Id { get; set; }
        public string DefaultDimension4Name { get; set; }

        public int? SalesTaxProductCodeId { get; set; }
        public string SalesTaxProductCodeName { get; set; }

        public DateTime? DiscontinueDate { get; set; }
        public string DiscontinueReason { get; set; }



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
        public virtual Godown Godown { get; set; }


        [ForeignKey("BinLocation")]
        [Display(Name = "BinLocation")]
        public int? BinLocationId { get; set; }
        public virtual BinLocation BinLocation { get; set; }


        public int? SalesTaxProductCodeId { get; set; }


        [ForeignKey("Site")]
        [Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        [Display(Name = "LOT Management")]
        public Boolean LotManagement { get; set; }
        public int ProductSiteDetailId { get; set; }
        public int ? ReferenceDocId { get; set; }
        public ProductTypeSettingsViewModel ProductTypeSettings { get; set; }
        public List<ProductTypeAttributeViewModel> ProductTypeAttributes { get; set; }
    }
}
