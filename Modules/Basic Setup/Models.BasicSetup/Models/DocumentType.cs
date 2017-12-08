using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Company.Models
{
    public class DocumentType : EntityBase, IHistoryLog
    {
        [Key]
        public int DocumentTypeId { get; set; }

        [Display(Name = "Document Type Short Name")]
        [MaxLength(5, ErrorMessage = "Short Name cannot exceed 5 characters"), Required]
        [Index("IX_DocumentType_DocumentTypeShortName", IsUnique = true)]
        public string DocumentTypeShortName { get; set; }

        [Display (Name="Document Type")]
        [MaxLength(50, ErrorMessage = "DocumentType Name cannot exceed 50 characters"), Required]
        [Index("IX_DocumentType_DocumentTypeName", IsUnique = true)]
        public string DocumentTypeName { get; set; }

        [ForeignKey("DocumentCategory")]
        [Display(Name = "Document Category")]
        public int DocumentCategoryId { get; set; }
        public virtual DocumentCategory DocumentCategory { get; set; }
        [MaxLength(25)]
        public string DomainName { get; set; }

        [MaxLength (20)]
        public string VoucherType { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "ReportMenu")]
        public int? ReportMenuId { get; set; }

        [MaxLength(10)]
        public string Nature { get; set; }
        public string IconDisplayName { get; set; }
        public string ImageFileName { get; set; }
        public string ImageFolderName { get; set; }
        public bool SupportGatePass { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string DatabaseTableName{ get; set; }
        
        [MaxLength(50)]
        public string ControllerName{ get; set; }

        [MaxLength(50)]
        public string ActionName { get; set; }
        
        [MaxLength(50)]
        public string ActionNamePendingToSubmit { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }



    }
}
