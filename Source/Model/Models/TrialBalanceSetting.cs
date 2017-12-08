using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class TrialBalanceSetting : EntityBase
    {
        [Key]
        public int TrialBalanceSettingId { get; set; }
        public string UserName { get; set; }
        public DateTime ?  FromDate { get; set; }
        public DateTime ? ToDate { get; set; }
        public string DivisionIds { get; set; }
        public bool ShowZeroBalance { get; set; }
        public bool ShowContraAccount { get; set; }
        public string SiteIds { get; set; }
        public string CostCenter { get; set; }
        public string DisplayType { get; set; }
        public string DrCr { get; set; }
    }
}
