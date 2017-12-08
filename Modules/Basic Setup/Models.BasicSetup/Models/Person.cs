using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Company.Models;

namespace Models.BasicSetup.Models
{
    public class Person : EntityBase, IHistoryLog
    {      
        [Key]
        public int PersonID { get; set; }

        [Display(Name = "Doc Type"), Required]
        [ForeignKey("DocType")]       
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [MaxLength(100,ErrorMessage ="{0} can not exceed {1} characters"), Required]        
        public string Name { get; set; }

        [MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters"), Required]        
        public string Suffix { get; set; }

        [Index("IX_Person_Code", IsUnique = true)]
        [MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Code { get; set; }

        [MaxLength(500, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Description { get; set; }

        [MaxLength(11, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Phone { get; set; }

        [MaxLength(10, ErrorMessage = "{0} can not exceed {1} characters")]
        [Index("IX_Person_Mobile", IsUnique = true)]
        public string Mobile { get; set; }

        [MaxLength(100, ErrorMessage = "{0} can not exceed {1} characters")]
        [Index("IX_Person_Email", IsUnique = true)]
        public string Email { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        public string Tags { get; set; }

        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }
        public bool IsSisterConcern { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

        [MaxLength(20)]
        public string Nature { get; set; }
        public int Status { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

    }
}
