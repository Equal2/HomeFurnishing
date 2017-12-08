using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class PersonRateGroup : EntityBase, IHistoryLog
    {

        [Key]
        public int PersonRateGroupId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Person Rate Group")]
        [MaxLength(50), Required]
        [Index("IX_PersonRateGroup_PersonRateGroupName", IsUnique = true,Order=1)]
        public string PersonRateGroupName { get; set; }


        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_PersonRateGroup_PersonRateGroupName", IsUnique = true, Order = 2)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }


        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_PersonRateGroup_PersonRateGroupName", IsUnique = true, Order = 3)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        public string Processes { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }


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
