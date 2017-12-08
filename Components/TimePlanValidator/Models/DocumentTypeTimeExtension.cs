using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimePlanValidator.Models
{
    public class DocumentTypeTimeExtension : EntityBase, IHistoryLog
    {
        public DocumentTypeTimeExtension()
        {
        }

        [Key]
        public int DocumentTypeTimeExtensionId { get; set; }        

        [ForeignKey("DocType"), Display(Name = "Doc Type")]
        public int? DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public string Type { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public int NoOfRecords { get; set; }
        public DateTime DocDate { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }


    public class DocTypeTimeLineViewModel
    {
        public decimal Days { get; set; }
    }

}
