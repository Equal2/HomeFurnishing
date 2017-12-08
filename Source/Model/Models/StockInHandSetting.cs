using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class StockInHandSetting : EntityBase
    {
        [Key]
        public int StockInHandSettingId { get; set; }
        public string UserName { get; set; }
        public DateTime ?  FromDate { get; set; }
        public DateTime ? ToDate { get; set; }
        public string DivisionIds { get; set; }
        public string SiteIds { get; set; }
        public string GroupOn { get; set; }
        public string ShowBalance { get; set; }

        [Display(Name = "Show Opening")]
        public Boolean ShowOpening { get; set; }
        public int? ProductTypeId { get; set; }

        public string TableName { get; set; }



    }
}
