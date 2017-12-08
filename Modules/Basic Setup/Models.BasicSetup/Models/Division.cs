using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Company.Models
{
    public class Division : EntityBase, IHistoryLog
    {
        
        [Key]
        [Display(Name = "Division Id")]
        public int DivisionId { get; set; }

        [MaxLength(50,ErrorMessage="Division Name cannot exceed 50 characters"), Required]
        [Index("IX_Division_Division", IsUnique = true)]
        [Display(Name = "Division Name")]
        public string DivisionName { get; set; }

        [MaxLength(250, ErrorMessage = "{0} can't exceed {1} characters")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [MaxLength(20, ErrorMessage = "{0} can't exceed {1} characters")]
        [Display(Name = "LST No")]
        public string LstNo { get; set; }

        [MaxLength(20, ErrorMessage = "{0} can't exceed {1} characters")]
        [Display(Name = "CST No")]
        public string CstNo { get; set; }
        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }

        public int? CompanyId { get; set; }

        [MaxLength(25)]
        public string ThemeColour { get; set; }       

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

    }
}
