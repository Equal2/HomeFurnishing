using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class Currency : EntityBase, IHistoryLog
    {

        [Key]
        public int ID { get; set; }

        [MaxLength(20, ErrorMessage = "Currency Name cannot exceed 20 characters"), Required]
        [Index("IX_Currency_Name", IsUnique = true)]
        public string Name { get; set; }

        [MaxLength(5, ErrorMessage = "Symbol cannot exceed 5 characters"), Required]
        public string Symbol { get; set; }

        [MaxLength(20, ErrorMessage = "Fraction Name cannot exceed 20 characters")]
        public string FractionName { get; set; }

        public int? FractionUnits { get; set; }

        [MaxLength(1, ErrorMessage = "FractionSymbol cannot exceed 1 characters")]
        public string FractionSymbol { get; set; }

        [Display (Name ="Current Base Currency Rate")]
        public decimal BaseCurrencyRate { get; set; }
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
