using Models.Company.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Company.ViewModels
{
    public class DocumentTypeViewModel
    {
        public int DocumentTypeId { get; set; }
        [MaxLength(5, ErrorMessage = "Short Name cannot exceed 5 characters"), Required]
        public string DocumentTypeShortName { get; set; }
        [MaxLength(50, ErrorMessage = "DocumentType Name cannot exceed 50 characters"), Required]
        public string DocumentTypeName { get; set; }
        [Display(Name = "Document Category")]
        [ForeignKey("DocumentCategory")]
        public int DocumentCategoryId { get; set; }
        public virtual DocumentCategory DocumentCategory { get; set; }
        public string DocumentCategoryName { get; set; }
        [MaxLength(25)]
        public string DomainName { get; set; }
        [MaxLength(20)]
        public string VoucherType { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }
        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [MaxLength(10)]
        public string Nature { get; set; }
        public int ? ReportMenuId { get; set; }
        public string IconDisplayName { get; set; }
        public string ImageFileName { get; set; }
        public string ImageFolderName { get; set; }
        public bool SupportGatePass { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        [MaxLength(50)]
        public string DatabaseTableName { get; set; }
        [MaxLength(50)]
        public string ControllerName { get; set; }

        [MaxLength(50)]
        public string ActionName { get; set; }

        [MaxLength(50)]
        public string ActionNamePendingToSubmit { get; set; }
    }
}
