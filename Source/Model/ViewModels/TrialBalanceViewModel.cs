using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class TrialBalanceViewModel
    {
        public int LedgerAccountGroupId { get; set; }
        public string LedgerAccountGroupName { get; set; }
        public decimal ? AmtDr { get; set; }
        public decimal ? AmtCr { get; set; }
        public string ReportType { get; set; }
        public string OpenReportType { get; set; }
        public int? Level { get; set; }
        public int? GroupLevel { get; set; }
        public int? ParentLedgerAccountGroupId { get; set; }
        public string ParentLedgerAccountGroupName { get; set; }

        public string TopParentLedgerAccountGroupName { get; set; }
        public int? LedgerAccountId { get; set; }
        public decimal? TotalAmtDr { get; set; }
        public decimal? TotalAmtCr { get; set; }

    }

    public class TrialBalanceDetailViewModel
    {
        public int LedgerAccountGroupId { get; set; }
        public string LedgerAccountGroupName { get; set; }
        public decimal? AmtDr { get; set; }
        public decimal? AmtCr { get; set; }
        public int? Level { get; set; }
        public int? ParentLedgerAccountGroupId { get; set; }
        public string ReportType { get; set; }
        public string OpenReportType { get; set; }

        public decimal? TotalAmtDr { get; set; }
        public decimal? TotalAmtCr { get; set; }
        public List<TrialBalanceDetailViewModel> ChildTrialBalanceAsPerDetailViewModel { get; set; }
    }

    public class TrialBalanceSummaryViewModel
    {
        public int LedgerAccountGroupId { get; set; }
        public string LedgerAccountGroupName { get; set; }
        public decimal ? AmtDr { get; set; }
        public decimal ? AmtCr { get; set; }
        public string OpeningDrCr { get; set; }
        public string BalanceDrCr { get; set; }
        public decimal ? Balance { get; set; }
        public decimal ? Opening { get; set; }
        public string ReportType { get; set; }
        public string OpenReportType { get; set; }
        public int? LedgerAccountId { get; set; }

        public string ParentLedgerAccountGroupName { get; set; }
        public int? GroupLevel { get; set; }
        public string TopParentLedgerAccountGroupName { get; set; }
        public decimal? TotalAmtDr { get; set; }
        public decimal? TotalAmtCr { get; set; }

        public decimal? TotalOpening { get; set; }
        public string TotalOpeningDrCr { get; set; }
        public decimal? TotalBalance { get; set; }
        public string TotalBalanceDrCr { get; set; }
    }

    public class ProfitAndLossSummaryViewModel
    {
        public int Sr { get; set; }
        public int DrGroupId { get; set; }
        public string DrParticular { get; set; }
        public string AmtDr { get; set; }
        public int CrGroupId { get; set; }
        public string CrParticular { get; set; }
        public string AmtCr { get; set; }
        public string ReportType { get; set; }
    }

    public class BalanceSheetSummaryViewModel
    {
        public Int64 ? Sr { get; set; }
        
        public int? AssetMainGroupId { get; set; }
        public int? Assetlabel { get; set; }
        public int ?  AssetGroupId { get; set; }
        public string AssetGroupName { get; set; }
        public decimal? SubAssetAmount { get; set; }
        public decimal? AssetAmount { get; set; }
        public int? LiabilityMainGroupId { get; set; }
        public int? Liabilitylabel { get; set; }
        public int ? LiabilityGroupId { get; set; }
        public string LiabilityGroupName { get; set; }
        public decimal? SubLiabilityAmount { get; set; }
        public decimal? LiabilityAmount { get; set; }
        public string ReportType { get; set; }
    }

    [Serializable()]
    public class FinancialDisplaySettings
    {
        public string ReportType { get; set; }
        public List<FinancialDisplayParameters> FinancialDisplayParameters { get; set; }
    }

    [Serializable()]
    public class FinancialDisplayParameters
    {
        public string ParameterName { get; set; }
        public bool IsApplicable { get; set; }
        public string Value { get; set; }
    }

}
