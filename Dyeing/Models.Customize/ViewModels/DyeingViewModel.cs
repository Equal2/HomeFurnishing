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
    
    public partial class DyeingViewModel
    {
        public int JobReceiveHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Document Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), MaxLength(20)]
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The JobWorker field is required")]
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "The OrderBy field is required")]
        public int JobReceiveById { get; set; }
        public string JobReceiveByName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The Process field is required")]
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        public string DyeingType { get; set; }

        public int PersonProcessId { get; set; }
        public int? MachineId { get; set; }
        public string MachineName { get; set; }

        public int GodownId { get; set; }
        public string GodownName { get; set; }


        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public string LotNo { get; set; }

        public Decimal Qty { get; set; }
        public Decimal BalanceQty { get; set; }
        public Decimal RecipeQty { get; set; }

        public Decimal LoadingTime { get; set; }

        public string UnitId { get; set; }

        public int StockHeaderId { get; set; }

        public int JobOrderLineId { get; set; }
        public string JobOrderNo { get; set; }

        public DateTime? StartDateTime { get; set; }

        public int StartDateTimeHour { get; set; }
        public int StartDateTimeMinute { get; set; }

        public DateTime? CompletedDateTime { get; set; }

        public int CompletedDateTimeHour { get; set; }
        public int CompletedDateTimeMinute { get; set; }




        public string ModifiedBy { get; set; }

        public int? CreditDays { get; set; }

        public bool IsQCRequired { get; set; }
        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DyeingListViewModel
    {
        public int JobReceiveHeaderId { get; set; }
        public int JobReceiveLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
        public decimal BalanceQty { get; set; }


    }
}
