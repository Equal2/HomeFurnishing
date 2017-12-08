using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class FinishedProduct : Product
    {
        public FinishedProduct()
        {            
        }

        public bool IsSample { get; set; }

        //[ForeignKey("ProductCategory")]
        //[Display(Name = "Product Category")]
        //public int? ProductCategoryId { get; set; }
        //public virtual ProductCategory ProductCategory { get; set; }

        [ForeignKey("ProductCollection")]
        [Display(Name = "Product Collection")]
        public int? ProductCollectionId { get; set; }
        public virtual ProductCollection ProductCollection { get; set; }

        [ForeignKey("ProductQuality")]
        [Display(Name = "Product Quality")]
        public int? ProductQualityId { get; set; }
        public virtual ProductQuality ProductQuality { get; set; }

        [ForeignKey("ProductDesign")]
        [Display(Name = "Product Design")]
        public int? ProductDesignId { get; set; }
        public virtual ProductDesign ProductDesign { get; set; }

        [ForeignKey("ProductDesignPattern")]
        [Display(Name = "Design Pattern")]
        public int? ProductDesignPatternId { get; set; }
        public virtual ProductDesignPattern ProductDesignPattern { get; set; }

        [ForeignKey("Colour")]
        [Display(Name = "Colour")]
        public int? ColourId { get; set; }
        public virtual Colour Colour { get; set; }

        [ForeignKey("FaceContent")]
        [Display(Name = "Face Content")]
        public int? FaceContentId { get; set; }
        public virtual ProductContentHeader FaceContent { get; set; }


        [ForeignKey("Content")]
        [Display(Name = "Content")]
        public int? ContentId { get; set; }
        public virtual ProductContentHeader Content { get; set; }


        [ForeignKey("ProductInvoiceGroup")]
        [Display(Name = "Product Invoice Group")]
        public int? ProductInvoiceGroupId { get; set; }
        public virtual ProductInvoiceGroup ProductInvoiceGroup { get; set; }

        [ForeignKey("ProcessSequenceHeader")]
        [Display(Name = "Product Process Sequence")]
        public int? ProcessSequenceHeaderId { get; set; }
        public virtual ProcessSequenceHeader ProcessSequenceHeader { get; set; }

        [ForeignKey("ProductManufacturer")]
        [Display(Name = "ProductManufacturer")]
        public int? ProductManufacturerId { get; set; }
        public virtual ProductManufacturer ProductManufacturer { get; set; }

        [ForeignKey("ProductStyle")]
        [Display(Name = "ProductStyle")]
        public int? ProductStyleId { get; set; }
        public virtual ProductStyle ProductStyle { get; set; }

        [ForeignKey("DescriptionOfGoods")]
        [Display(Name = "DescriptionOfGoods")]
        public int? DescriptionOfGoodsId { get; set; }
        public virtual DescriptionOfGoods DescriptionOfGoods { get; set; }

        [ForeignKey("OriginCountry")]
        [Display(Name = "OriginCountry")]
        public int ? OriginCountryId { get; set; }
        public virtual Country OriginCountry { get; set; }

        [ForeignKey("ProductShape")]
        [Display(Name = "Product Shape")]
        public int? ProductShapeId { get; set; }
        public virtual ProductType ProductShape { get; set; }


        [ForeignKey("Sample")]
        [Display(Name = "Sample")]
        public int? SampleId { get; set; }
        public virtual Product Sample { get; set; }

        
        [Display(Name = "Counter No.")]
        public int? CounterNo { get; set; }
        
        [Display(Name = "Discontinued Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DiscontinuedDate { get; set; }

        [Display(Name = "Map Scale")]
        public int? MapScale { get; set; }

        [Display(Name = "Trace Type")]
        public string TraceType { get; set; }

        [Display(Name = "Map Type")]
        public string MapType { get; set; }
        public string ProductionRemark { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
