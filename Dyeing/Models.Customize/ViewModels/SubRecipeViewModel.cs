using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.BasicSetup.ViewModels;
using Models.Customize.Models;

namespace Models.Customize.ViewModels
{

    public partial class SubRecipeViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public Decimal Qty { get; set; }
        public Decimal? BalanceQty { get; set; }


    }
}
