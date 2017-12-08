using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class SaleOrderCancelHeaderViewModel
    {
        [Key]
        public int SaleOrderCancelHeaderId { get; set; }
        [Required(ErrorMessage="Please select a Document Type")]
        public int DocumentTypeId { get; set; }
        [Required(ErrorMessage="Please select Cancel Date"),DisplayFormat(DataFormatString="dd/MMM/yyyy")]
        public DateTime DocDate { get; set; }       
        public int? SaleOrderHeaderId { get; set; }
        [Required(ErrorMessage="Please select a Reason")]
        public int ReasonId { get; set; }
        [Required(ErrorMessage="CancelNo field is required")]
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        [MinLength(20,ErrorMessage="Minimum 20 Characters"),Required]
        public string Remark { get; set; }
        public int ? Status { get; set; }
        public string BuyerId { get; set; }
        [Display(Name="Sale Order No")]
        public string SaleOrderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string ModifiedBy { get; set; }

    }

    public class SaleOrderCancelHeaderIndexViewModel
    {
        [Key]
        public int SaleOrderCancelHeaderId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Cancel Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Cancel Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [ForeignKey("Reason"), Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public virtual Reason Reason { get; set; }
        public SaleOrderSettingsViewModel SaleOrderSettings { get; set; }

        [MaxLength(20)]
        public string DocNo { get; set; }

        [ForeignKey("Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        public int Status { get; set; }

        [MinLength(30), Required]
        public string Remark { get; set; }
        public string ReasonName { get; set; }
        public string DivisionName { get; set; }
        public string SiteName { get; set; }

        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string ModifiedBy { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SaleOrderCancelHeaderDetailsViewModel
    {
        public int SaleOrderCancelHeaderId { get; set; }
        public int DocumentTypeId { get; set; }
        [Display(Name = "Entry Type")]
        public string DocumentTypeName { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        [Display(Name = "Cancel Date")]
        public DateTime DocDate { get; set; }
        public int? SaleOrderHeaderId { get; set; }
        public string Buyer { get; set; }
        [Display(Name = "Cancel No"),Required]
        public string DocNo { get; set; }
        public int? DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int? SiteId { get; set; }
        public string SiteName { get; set; }
        public int? Status { get; set; }
        public int ReasonId { get; set; }
        public string Reason { get; set; }
        [MinLength(20), Required(ErrorMessage = "This Field is required")]
        public string LogReason { get; set; }
        [MinLength(20, ErrorMessage = "Minimum 20 Characters"), Required]
        public string Remark { get; set; }
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
    }

    public class SaleOrderCancelMasterDetailModel
    {
        public int SaleOrderCancelHeaderId { get; set; }
        public SaleOrderCancelHeaderDetailsViewModel SaleOrderCancelHeaderDetailsViewModel { get; set; }
        public SaleOrderCancelHeader SaleOrderCancelHeader { get; set; }
        public List<SaleOrderCancelLine> SaleOrderCancelLines { get; set; }
        public List<SaleOrderCancelLineViewModel> SaleOrderCancelViewModels { get; set; }
    }
    public class SaleOrderCancelFilterViewModel
    {
        public int SaleOrderCancelHeaderId { get; set; }
        public int BuyerId { get; set; }
        [Display(Name = "Sale Order No")]
        public string SaleOrderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }

        public string Dimension1Id { get; set; }

        public string Dimension2Id { get; set; }

        public SaleOrderSettingsViewModel SaleOrderSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }


        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
    }
}
