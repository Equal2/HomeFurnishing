using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class BankReconciliationViewModel
    {
        public int LedgerId { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string AccountName { get; set; }
        public string Narration { get; set; }
        public string ChqNo { get; set; }
        public DateTime? ChqDate { get; set; }
        public Decimal? AmtDr { get; set; }
        public Decimal? AmtCr { get; set; }
        public DateTime? BankDate { get; set; }
    }

    public class BankReconciliationIndexViewModel
    {
        public int LedgerAccountId { get; set; }
        public string LedgerAccountName { get; set; }
    }
}
