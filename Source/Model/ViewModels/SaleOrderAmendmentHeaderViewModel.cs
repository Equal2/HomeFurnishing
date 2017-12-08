using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class SaleOrderAmendmentHeaderViewModel
    {
        public int SaleOrderAmendmentHeaderId { get; set; }
        
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        public DateTime DocDate { get; set; }

        [MaxLength(10)]
        public string DocNo { get; set; }
        
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        
        public int SiteId { get; set; }
        public string SiteName { get; set; }        
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
        
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }

        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

    }

    public class SaleOrderAmendmentHeaderIndexViewModel
    {
        public int SaleOrderAmendmentHeaderId { get; set; }
        public string DocNo { get; set; }
        public String DocTypeName { get; set; }
        public DateTime DocDate { get; set; }
        public string BuyerName { get; set; }
        public string ReasonName { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string ModifiedBy { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
    }

    public class SaleOrderAmendmentFilterViewModel
    {
        public int SaleOrderAmendmentHeaderId { get; set; }
        public int BuyerId { get; set; }
        [Display(Name = "Sale Order No")]
        public string SaleOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
    }
    public class SaleOrderAmendmentMasterDetailModel
    {
        public List<SaleOrderQtyAmendmentLineViewModel> SaleOrderQtyAmendmentViewModel { get; set; }
    }
}
