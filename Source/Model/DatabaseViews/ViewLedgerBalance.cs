using Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DatabaseViews
{

    [Table("ViewLedgerBalance")]
    public class ViewLedgerBalance
    {
        [Key]
        public int LedgerId { get; set; }
        public int LedgerHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public int Priority { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public int LedgerAccountId { get; set; }
        public string Nature { get; set; }
        public Decimal Balance { get; set; }
        
    }
}
