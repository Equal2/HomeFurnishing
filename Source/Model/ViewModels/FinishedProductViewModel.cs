using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModel
{
    public class FinishedProductViewModel
    {

        [Key]
        [Display(Name = "Product Id")]
        public int ProductId { get; set; }

        [MaxLength(20, ErrorMessage = "ProductCode cannot exceed 20 characters"), Required]
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; }

        [MaxLength(50, ErrorMessage = "Product Name cannot exceed 50 characters"), Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        public int SizeId { get; set; }
        public virtual Size Size { get; set; }
        [ForeignKey("ProductCategory")]
        [Display(Name = "Product Category"),Required()]
        [Range(1, int.MaxValue, ErrorMessage = "Product Category field is required")]
        public int ProductCategoryId { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }

        [ForeignKey("ProductGroup"),Required]
        [Range(1, int.MaxValue, ErrorMessage = "Product Group field is required")]
        [Display(Name = "Product Group")]
        public int ProductGroupId { get; set; }
        public virtual ProductGroup ProductGroup { get; set; }
        [ForeignKey("Sample")]
        [Display(Name = "Sample")]
        public int? SampleId { get; set; }
        public virtual Product Sample { get; set; }
        [ForeignKey("ProductCollection")]
        [Display(Name = "Product Collection")]
        public int? ProductCollectionId { get; set; }
        public virtual ProductCollection ProductCollection { get; set; }
        [Display(Name = "Counter No.")]
        public int? CounterNo { get; set; }
        [ForeignKey("ProductQuality")]
        [Display(Name = "Product Quality")]
        public int? ProductQualityId { get; set; }
        public virtual ProductQuality ProductQuality { get; set; }
        [ForeignKey("OriginCountry")]
        [Display(Name = "OriginCountry")]
        public int OriginCountryId { get; set; }
        public virtual Country OriginCountry { get; set; }
        [ForeignKey("ProductDesign")]
        [Display(Name = "Product Design")]
        public int? ProductDesignId { get; set; }
        public virtual ProductDesign ProductDesign { get; set; }
        [ForeignKey("ProductInvoiceGroup")]
        [Display(Name = "Product Invoice Group")]
        public int? ProductInvoiceGroupId { get; set; }
        public virtual ProductInvoiceGroup ProductInvoiceGroup { get; set; }
        [ForeignKey("ProductDesignPattern"),Required]
        [Display(Name = "Design Pattern")]
        public int? ProductDesignPatternId { get; set; }
        public virtual ProductDesignPattern ProductDesignPattern { get; set; }
        [ForeignKey("DrawBackTariffHead")]
        [Display(Name = "DrawBack Tarif fHead")]
        public int? DrawBackTariffHeadId { get; set; }
        public virtual DrawBackTariffHead DrawBackTariffHead { get; set; }
        [ForeignKey("ProductStyle")]
        [Display(Name = "ProductStyle")]
        public int? ProductStyleId { get; set; }
        public virtual ProductStyle ProductStyle { get; set; }

        [ForeignKey("DescriptionOfGoods")]
        [Display(Name = "DescriptionOfGoods")]
        public int? DescriptionOfGoodsId { get; set; }
        public virtual DescriptionOfGoods DescriptionOfGoods { get; set; }

        [ForeignKey("Colour")]
        [Display(Name = "Colour")]
        public int? ColourId { get; set; }
        public virtual Colour Colour { get; set; }
        [Display(Name = "Standard Cost")]
        public decimal? StandardCost { get; set; }
        public Decimal? StandardWeight { get; set; }
        [ForeignKey("ProductManufacturer")]
        [Display(Name = "ProductManufacturer")]
        public int? ProductManufacturerId { get; set; }
        public virtual ProductManufacturer ProductManufacturer { get; set; }
        [ForeignKey("ProcessSequenceHeader")]
        [Display(Name = "Process Sequence"),Required]
        public int? ProcessSequenceHeaderId { get; set; }
        public virtual ProcessSequenceHeader ProcessSequenceHeader { get; set; }
        public Decimal? CBM { get; set; }
        public string Tags { get; set; }
        [ForeignKey("FaceContent")]
        [Display(Name = "Face Content"),Required]
        public int? FaceContentId { get; set; }
        public virtual ProductContentHeader FaceContent { get; set; }
        [ForeignKey("Content")]
        [Display(Name = "Content"),Required]
        public int? ContentId { get; set; }
        public virtual ProductContentHeader Content { get; set; }
        [ForeignKey("Division")]
        [Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        public string ProductSpecification { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }
        public bool IsSample { get; set; }
        public string ProductionRemark { get; set; }
    }
}
