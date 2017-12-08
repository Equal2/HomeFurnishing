using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class SaleOrderCancelLineViewModel
    {
        [Key]
        public int SaleOrderCancelLineId { get; set; }        
        public int SaleOrderCancelHeaderId { get; set; }      
        [Required(ErrorMessage="Please Select a Sale Order")]
        public int ? SaleOrderLineId { get; set; }

        public string SaleOrderHeaderDocNo { get; set; }
        public string DocumentTypeName { get; set; }
        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }
        public string Reason { get; set; }
        public string Remark { get; set; }

        [Display(Name="Remark")]
        public string LineRemark { get; set; }

        public int BuyerId { get; set; }
        [Display(Name="Buyer")]
        public string BuyerName { get; set; }
        
        [Required]
        public decimal Qty { get; set; }
        [Required(ErrorMessage="Please Select a Product")]
        public int ? ProductId { get; set; }
        [Display(Name="Product")]
        public string ProductName { get; set; }


        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }


        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }


        public decimal OrderQty { get; set; }
        public decimal BalanceQty { get; set; }

        [Display(Name="Sale Order No.")]
        public string DocNo { get; set; }
        [Display(Name="Cancel No")]
        public string CancelNo { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
        public int unitDecimalPlaces { get; set; }
        public string LockReason { get; set; }

        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }

        public SaleOrderSettingsViewModel SaleOrderSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }


    }
}
