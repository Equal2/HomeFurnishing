using System;
using System.ComponentModel.DataAnnotations;

namespace Models.BasicSetup.ViewModels
{
    public class StockHeaderViewModel
    {
        public int StockHeaderId { get; set; }

        public int? DocHeaderId { get; set; }

        [Display(Name = "Doc Type"), Required]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Doc Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Doc No"), Required, MaxLength(20)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Currency")]
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }

        [Display(Name = "Person"), Required]
        public int? PersonId { get; set; }
        public string PersonName { get; set; }

        [Display(Name = "Process")]
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "From Godown")]
        public int? FromGodownId { get; set; }
        public string FromGodownName { get; set; }
        public int? GatePassHeaderId { get; set; }
        public string GatePassDocNo { get; set; }
        public int GatePassStatus { get; set; }
        public DateTime? GatePassDocDate { get; set; }

        [Display(Name = "Godown")]
        public int? GodownId { get; set; }
        public string GodownName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }


        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }

        [Display(Name = "Machine")]
        public int? MachineId { get; set; }
        public string MachineName { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string LockReason { get; set; }
    }
}
