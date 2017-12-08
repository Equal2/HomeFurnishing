using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Data.Models;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;

namespace Reports.ViewModels
{
    public class SaleOrderAmendmentReportLayout
    {

        [DisplayFormat(DataFormatString = "{0:MMMM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "From Date")]
        public DateTime? FromDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MMMM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "To Date")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Date Filter On")]
        public string DateFilterOn { get; set; }

        [Display(Name = "Document Type")]
        public string DocumentTypeId { get; set; }

        [Display(Name = "Document Type")]
        public string DocumentType { get; set; }

        [Display(Name = "Buyer")]
        public string BuyerId { get; set; }

        [Display(Name = "Buyer")]
        public string Buyer { get; set; }

        [Display(Name = "Product Nature")]
        public string ProductNatureId { get; set; }

        [Display(Name = "Product Nature")]
        public string ProductNature { get; set; }

        [Display(Name = "Product Category")]
        public string ProductCategoryId { get; set; }

        [Display(Name = "Product Category")]
        public string ProductCategory { get; set; }

        [Display(Name = "Product Type")]
        public string ProductTypeId { get; set; }

        [Display(Name = "Product Type")]
        public string ProductType { get; set; }

        [Display(Name = "Product Collection")]
        public string ProductCollectionId { get; set; }

        [Display(Name = "Product Collection")]
        public string ProductCollection { get; set; }

        [Display(Name = "Product Quality")]
        public string ProductQualityId { get; set; }

        [Display(Name = "Product Quality")]
        public string ProductQuality { get; set; }

        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }

        [Display(Name = "Product Group")]
        public string ProductGroup { get; set; }

        [Display(Name = "Product Style")]
        public string ProductStyleId { get; set; }

        [Display(Name = "Product Style")]
        public string ProductStyle { get; set; }

        [Display(Name = "Product Design")]
        public string ProductDesignId { get; set; }

        [Display(Name = "Product Design")]
        public string ProductDesign { get; set; }

        [Display(Name = "Product Shape")]
        public string ProductShapeId { get; set; }

        [Display(Name = "Product Shape")]
        public string ProductShape { get; set; }

        [Display(Name = "Product Size")]
        public string ProductSizeId { get; set; }

        [Display(Name = "Product Size")]
        public string ProductSize { get; set; }

        [Display(Name = "Product Invoice Group")]
        public string ProductInvoiceGroupId { get; set; }

        [Display(Name = "Product Invoice Group")]
        public string ProductInvoiceGroup { get; set; }

        [Display(Name = "Product Custom Group")]
        public string ProductCustomGroupId { get; set; }

        [Display(Name = "Product Custom Group")]
        public string ProductCustomGroup { get; set; }

        [Display(Name = "Product Tag")]
        public string ProductTagId { get; set; }

        [Display(Name = "Product Tag")]
        public string ProductTag { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }

        [Display(Name = "Product")]
        public string Product { get; set; }

        [Display(Name = "Site")]
        public string SiteId { get; set; }

        [Display(Name = "Site")]
        public string Site { get; set; }

        [Display(Name = "Division")]
        public string DivisionId { get; set; }

        [Display(Name = "Division")]
        public string Division { get; set; }

        [Display(Name = "Sale Order")]
        public string SaleOrder { get; set; }

        [Display(Name = "Sale Order")]
        public string SaleOrderHeaderId { get; set; }

        [Display(Name = "Amendment No")]
        public string SaleOrderAmendmentHeader { get; set; }

        [Display(Name = "Amendment No")]
        public string SaleOrderAmendmentHeaderId { get; set; }
    }
}
