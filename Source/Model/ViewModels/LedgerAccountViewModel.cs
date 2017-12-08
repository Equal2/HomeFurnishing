using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class LedgerAccountViewModel : EntityBase, IHistoryLog
    {
        [Key]
        public int LedgerAccountId { get; set; }

        public string LedgerAccountName { get; set; }

        public string LedgerAccountSuffix { get; set; }

        public int? PersonId { get; set; }
        public string PersonName { get; set; }

        public int LedgerAccountGroupId { get; set; }
        public string LedgerAccountGroupName { get; set; }

        public int? SalesTaxGroupProductId { get; set; }
        public string SalesTaxGroupProductName { get; set; }


        public Boolean IsActive { get; set; }
        public Boolean IsSystemDefine { get; set; } 

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string OMSId { get; set; }
    }
}
