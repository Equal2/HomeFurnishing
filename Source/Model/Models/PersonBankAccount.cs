using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Spatial;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PersonBankAccount : EntityBase, IHistoryLog
    {
        [Key]
        public int PersonBankAccountID { get; set; }        

        [ForeignKey("Person"), Display(Name = "Person Name")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
        
        [MaxLength(200)]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [MaxLength(500)]
        [Display(Name = "Bank Branch")]
        public string BankBranch { get; set; }

        [MaxLength(50)]
        [Display(Name = "Bank Code")]
        public string BankCode { get; set; }

        [MaxLength(20)]
        [Display(Name = "Account No")]
        public string AccountNo { get; set; }

        [MaxLength(100)]
        [Display(Name = "Account Holder Name")]
        public string AccountHolderName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
               
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
