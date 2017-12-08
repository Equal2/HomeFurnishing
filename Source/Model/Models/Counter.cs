using System;
using System.ComponentModel.DataAnnotations;

namespace Model.Models
{
    public class Counter : EntityBase, IHistoryLog
    {

        [Key]
        public int CounterId { get; set; }
        public string ImageFolderName { get; set; }

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