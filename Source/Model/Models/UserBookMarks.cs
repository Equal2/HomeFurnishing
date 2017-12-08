using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class UserBookMark : EntityBase, IHistoryLog
    {
        public UserBookMark()
        {
           
        }

        [Key]
        public int UserBookMarkId { get; set; }

        
        [Display(Name = "Application User"), MaxLength(128)]
        public string ApplicationUserName { get; set; }


        [ForeignKey("Menu")]
        [Display(Name = "Menu")]
        public int MenuId { get; set; }
        public virtual Menu Menu { get; set; }


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
