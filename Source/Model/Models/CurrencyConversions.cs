using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class CurrencyConversions : EntityBase
    {
        public CurrencyConversions()
        {
        }

        [Key]
        public int CurrencyConversionsId { get; set; }

        public Decimal FromQty { get; set; }

        [ForeignKey("FromCurrency"), Display(Name = "From Currency")]
        public int FromCurrencyId { get; set; }
        public virtual Currency FromCurrency { get; set; }

        public Decimal ToQty { get; set; }

        [ForeignKey("ToCurrency"), Display(Name = "To Currency")]
        public int ToCurrencyId { get; set; }
        public virtual Currency ToCurrency { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
    }
}
