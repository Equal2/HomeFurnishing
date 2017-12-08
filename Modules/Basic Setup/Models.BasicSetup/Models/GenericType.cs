using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace Models.BasicSetup.Models
{
    public class GenericType : EntityBase, IHistoryLog
    {

        [Key]
        public int GenericTypeId { get; set; }

        [MaxLength(50, ErrorMessage = "{0} Name cannot exceed {1} characters"), Required]
        [Display(Name = "Type")]
        [Index("IX_GenericType_GenericTypeName", IsUnique = true)]
        public string GenericTypeName { get; set; }        

        public string Category { get; set;}        

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; } 

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
