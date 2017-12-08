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
    public class CarpetMasterViewModel
    {
        [Required(ErrorMessage="The Design field is required.")]
        public string ProductGroupName { get; set; }
        [ForeignKey("ProductCategory")]
        [Display(Name = "Product Category"), Required(ErrorMessage = "The Construction field is required."), Range(1, int.MaxValue, ErrorMessage = "The Construction field is required.")]
        public int? ProductCategoryId { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }

        [ForeignKey("ProductGroup")]
        [Display(Name = "Product Group")]
        public int ProductGroupId { get; set; }
        public virtual ProductGroup ProductGroup { get; set; }

        [ForeignKey("ProductCollection")]
        [Display(Name = "Product Collection"),Required]
        public int? ProductCollectionId { get; set; }
        public virtual ProductCollection ProductCollection { get; set; }

        [ForeignKey("ProductQuality")]
        [Display(Name = "Product Quality"),Required]
        public int? ProductQualityId { get; set; }
        public virtual ProductQuality ProductQuality { get; set; }

        [ForeignKey("ProductDesign"),Required(ErrorMessage="The ColourWays field is required")]
        [Display(Name = "Product Design")]
        public int? ProductDesignId { get; set; }
        public virtual ProductDesign ProductDesign { get; set; }

        [ForeignKey("ProductInvoiceGroup")]
        [Display(Name = "Product Invoice Group")]
        public int? ProductInvoiceGroupId { get; set; }
        public virtual ProductInvoiceGroup ProductInvoiceGroup { get; set; }
        [ForeignKey("ProductManufacturer")]
        [Display(Name = "ProductManufacturer")]
        public int? ProductManufacturerId { get; set; }
        public virtual ProductManufacturer ProductManufacturer { get; set; }

        [ForeignKey("ProductStyle")]
        [Display(Name = "ProductStyle")]
        public int? ProductStyleId { get; set; }
        public virtual ProductStyle ProductStyle { get; set; }
        [ForeignKey("DrawBackTariffHead")]
        [Display(Name = "DrawBack Tarif fHead")]
        public int? DrawBackTariffHeadId { get; set; }
        public virtual DrawBackTariffHead DrawBackTariffHead { get; set; }
        [ForeignKey("Unit")]
        [Display(Name = "Unit")]
        [MaxLength(3)]
        public String UnitId { get; set; }
        public virtual Unit Unit { get; set; }

        [ForeignKey("Division")]
        [Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site")]
        [Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("ProcessSequenceHeader")]
        [Display(Name = "Product Process Sequence")]
        public int? ProcessSequenceHeaderId { get; set; }
        public virtual ProcessSequenceHeader ProcessSequenceHeader { get; set; }

        [ForeignKey("ProductDesignPattern")]
        [Display(Name = "Design Pattern")]
        public int? ProductDesignPatternId { get; set; }
        public virtual ProductDesignPattern ProductDesignPattern { get; set; }
        public int StandardSizeId { get; set; }

        public int ManufacturingSizeId { get; set; }

        public int FinishingSizeId { get; set; }

        public int StencilSizeId { get; set; }
 
        public int MapSizeId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public string Tags { get; set; }

        [ForeignKey("FaceContent")]
        [Display(Name = "Face Content")]
        public int? FaceContentId { get; set; }
        public virtual ProductContentHeader FaceContent { get; set; }
        public bool IsSample { get; set; }

        [ForeignKey("Content")]
        [Display(Name = "Content")]
        public int? ContentId { get; set; }
        public virtual ProductContentHeader Content { get; set; }
        public int ColourId { get; set; }
        public string ColourIdList { get; set; }
 
        [ForeignKey("Country"),Required,Display(Name="Origin Country")]
        public int? OriginCountryId { get; set; }
        public virtual Country Country { get; set; }

        [ForeignKey("ProductShape")]
        [Display(Name = "Product Shape")]
        public int? ProductShapeId { get; set; }
        public virtual ProductType ProductShape { get; set; }


        public decimal? GrossWeight { get; set; }
        public decimal ? StandardWeight { get; set; }
        public decimal? StandardCost { get; set; }
        [ForeignKey("DescriptionOfGoods"),Display(Name="Description of Goods")]
        public int ? DescriptionOfGoodsId { get; set; }
        public virtual DescriptionOfGoods DescriptionOfGoods { get; set; }
        public List<ProductTypeAttributeViewModel> ProductTypeAttributes { get; set; }
        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }
        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [ForeignKey("Sample")]
        [Display(Name = "Sample")]
        public int? SampleId { get; set; }
        public virtual Product Sample { get; set; }


        [Display(Name = "Counter No.")]
        public int? CounterNo { get; set; }

        [Display(Name = "Trace Type")]
        public string TraceType { get; set; }

        [Display(Name = "Map Type")]
        public string MapType { get; set; }

        [Display(Name = "Map Scale")]
        public int? MapScale { get; set; }

        public string ProductionRemark { get; set; }

        public int ProductSupplierId { get; set; }
        public int ? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int? LeadTime { get; set; }
        public decimal? MinimumOrderQty { get; set; }
        public decimal? MaximumOrderQty { get; set; }
        public decimal? Cost { get; set; }
        public Decimal? CBM { get; set; }

        public Boolean IsBomCreated { get; set; }

        public int? SalesTaxProductCodeId { get; set; }
        public string SalesTaxProductCodeName { get; set; }
        public CarpetSkuSettingsViewModel CarpetSkuSettings { get; set; }

    }

    public class CarpetIndexViewModel
    {
        public int ProductId { get; set; }
        public int ProductGroupId { get; set; }
        public int ProductCategoryId { get; set; }
        public string ProductName { get; set; }
        public string ProductGroupName { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductDesignName { get; set; }
        public int ProductCollectionId { get; set; }
        public string ProductCollectionName { get; set; }
        public int ProductDesignId { get; set; }        
    }
}
