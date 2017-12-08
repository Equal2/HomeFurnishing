using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class SalesTaxGroupParty : EntityBase, IHistoryLog
    {
        public SalesTaxGroupParty()
        {
        }

        [Key]
        public int SalesTaxGroupPartyId { get; set; }

        [Display(Name="SalesTaxGroupParty")]
        [MaxLength(50, ErrorMessage = "SalesTaxGroupParty Name cannot exceed 50 characters"), Required]
        [Index("IX_SalesTaxGroupParty_SalesTaxGroupPartyName", IsUnique = true)]
        public string SalesTaxGroupPartyName { get; set; }
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
