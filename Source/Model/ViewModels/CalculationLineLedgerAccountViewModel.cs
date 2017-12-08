using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;

namespace Model.ViewModel
{
    public class CalculationLineLedgerAccountViewModel
    {
        public int CalculationLineLedgerAccountId { get; set; }        
        public int CalculationId { get; set; }
        public string CalculationName { get; set; }


        public int CalculationProductId { get; set; }
        public string CalculationProductName { get; set; }


        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }


        public int? LedgerAccountDrId { get; set; }
        public string LedgerAccountDrName { get; set; }

        
        public int? LedgerAccountCrId { get; set; }
        public string LedgerAccountCrName { get; set; }

        public int? ContraLedgerAccountId { get; set; }
        public string ContraLedgerAccountName { get; set; }



        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }

        public int? Sr { get; set; }


    }

}
