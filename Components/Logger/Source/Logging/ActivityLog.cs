using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Components.Logging
{
    public class ActivityLog : EntityBase
    {
        [Key]
        public int ActivityLogId { get; set; }


        public int? DocTypeId { get; set; }

        public int DocId { get; set; }
        public string DocNo { get; set; }
        public DateTime ? DocDate { get; set; }
        public int ? DocLineId { get; set; }

        [ForeignKey("ActivityTypeT")]
        public int ActivityType { get; set; }
        public virtual ActivityType ActivityTypeT { get; set; }

        public string Narration { get; set; }

        public string UserRemark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Upload Date")]
        public DateTime ? UploadDate { get; set; }

        [Display(Name = "Transaction Id")]
        [MaxLength(50)]
        public string TransactionId { get; set; }

        [Display(Name = "Transaction Error")]
        public string TransactionError { get; set; }
        
        public string Modifications { get; set; }
        [MaxLength(50)]
        public string ControllerName { get; set; }
        [MaxLength(50)]
        public string ActionName { get; set; }
        public int DocStatus { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
    