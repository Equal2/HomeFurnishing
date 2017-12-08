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
    
    public partial class RecipeHeaderViewModel
    {
        public int JobOrderHeaderId { get; set; }
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

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DueDate { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "The JobWorker field is required")]
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }


        public int PersonId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The OrderBy field is required")]
        public int? OrderById { get; set; }
        public string OrderByName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The Process field is required")]
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        public int PersonProcessId { get; set; }
        public int? MachineId { get; set; }
        public string MachineName { get; set; }

        public int GodownId { get; set; }
        public string GodownName { get; set; }


        public int Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public string LotNo { get; set; }

        public Decimal Qty { get; set; }
        public Decimal BalanceQty { get; set; }

        public string UnitId { get; set; }

        public int StockHeaderId { get; set; }

        public int ProdOrderLineId { get; set; }
        public string ProdOrderNo { get; set; }

        public Decimal? TestingQty { get; set; }

        public Decimal? SubRecipeQty { get; set; }


        public string ModifiedBy { get; set; }

        public int? CreditDays { get; set; }


        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class RecipeHeaderListViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public int JobOrderLineId { get; set; }
        public string DocNo { get; set; }
        public string RequestDocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
        public decimal BalanceQty { get; set; }


    }

    public class RecipeLineFilterViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Production Order")]
        public string ProdOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }


        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }

        public string DealUnitId { get; set; }
        public decimal Rate { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }

    }

    public class RecipeLineListViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public int JobOrderLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
        public Decimal? BalanceQty { get; set; }
    }

    public class RecipeMasterDetailModel
    {
        public List<JobOrderLineViewModel> JobOrderLineViewModel { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }
    }

}
