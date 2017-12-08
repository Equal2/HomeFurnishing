using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class MaterialPlanCancelHeaderViewModel
    {

        public int MaterialPlanCancelHeaderId { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        
        public DateTime DocDate { get; set; }

        [MaxLength(20)]
        public string DocNo { get; set; }
        
        public int DivisionId { get; set; }
        public string DivisionName{ get; set; }
        
        public int SiteId { get; set; }
        public string SiteName{ get; set; }
        
        public DateTime DueDate { get; set; }        
        public int ? GodownId { get; set; }
        public string GodownName{ get; set; }
        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public MaterialPlanSettingsViewModel MaterialPlanSettings { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LockReason { get; set; }
    }
}
