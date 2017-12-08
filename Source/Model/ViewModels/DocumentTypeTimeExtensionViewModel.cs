using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class DocumentTypeTimeExtensionViewModel
    {
        public int DocumentTypeTimeExtensionId { get; set; }        

        [Required]
        public int? DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        [Required]
        public string Type { get; set; }
        public decimal Days { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Reason { get; set; }
        public int NoOfRecords { get; set; }
        [Required]
        public DateTime DocDate { get; set; }        

    }

    public class DocumentUniqueId
    {
        public DateTime DocDate { get; set; }
        public int DocTypeId { get; set; }
        public string DocNo { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int ? GatePassHeaderId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int Status { get; set; }
        public string LockReason { get; set; }
    }

}
