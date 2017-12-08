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
    public class PurchaseIndentLineViewModel
    {
        [Key]
        public int PurchaseIndentLineId { get; set; }

        [Display(Name = "Purchase Indent")]
        [ForeignKey("PurchaseIndentHeader")]
        public int PurchaseIndentHeaderId { get; set; }
        public virtual PurchaseIndentHeader PurchaseIndentHeader { get; set; }

        [ForeignKey("MaterialPlanLine")]
        public int ? MaterialPlanLineId { get; set; }
        public virtual MaterialPlanForSaleOrderLine MaterialPlanLine { get; set; }
        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ? DueDate { get; set; }
        public string Specification { get; set; }

        [Display(Name = "Product"), Required,Range(1,int.MaxValue,ErrorMessage="Product field is required")]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "Qty"), Required]
        public decimal Qty { get; set; }

        public decimal PlanBalanceQty { get; set; }

        public string MaterialPlanHeaderDocNo { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }
        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Dimension1")]
        [ForeignKey("Dimension1")]
        public int ? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        [ForeignKey("Dimension2")]
        public int ? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }
        public string Dimension2Name { get; set; }
        public string UnitId { get; set; }
        public PurchaseIndentSettingsViewModel PurchIndentSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public int MyProperty { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public int ? PlanDocTypeId { get; set; }
        public int ? PlanHeaderId { get; set; }
        public string LockReason { get; set; }

    }

    public class PurchaseIndentLineListViewModel
    {
        public int PurchaseIndentHeaderId { get; set; }
        public int PurchaseIndentLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
    }

    public class PurchaseIndentLineFilterViewModel
    {
        public int PurchaseIndentHeaderId { get; set; }

        [Display(Name = "Material Plan No")]
        public string MaterialPlanHeaderId { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        
    }

}
