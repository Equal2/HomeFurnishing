using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{

    public class LedgerLineIndexViewModel
    {
        public int? LedgerLineId { get; set; }
        public string LedgerAccountName { get; set; }
        public string Amount { get; set; }

    }
}
