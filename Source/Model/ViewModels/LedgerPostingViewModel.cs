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
    public class LedgerPostingViewModel
    {
        public int LedgerAccountId { get; set; }

        public int? ContraLedgerAccountId { get; set; }
        public Byte? ContraLedgerAccountWeightage { get; set; }

        public int? CostCenterId { get; set; }

        public Decimal AmtDr { get; set; }

        public Decimal AmtCr { get; set; }

        public int? ReferenceDocLineId { get; set; }

    }

}
