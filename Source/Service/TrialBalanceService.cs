using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModel;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace Service
{
    public interface ITrialBalanceService : IDisposable
    {
        IEnumerable<TrialBalanceViewModel> GetTrialBalance(string UserName);
        IEnumerable<TrialBalanceSummaryViewModel> GetTrialBalanceSummary(string UserName);
        IEnumerable<ProfitAndLossSummaryViewModel> GetProfitAndLossSummary(string UserName);
        IEnumerable<BalanceSheetSummaryViewModel> GetBalanceSheetSummary(string UserName);
        IEnumerable<SubTrialBalanceViewModel> GetSubTrialBalance(int ? id, string UserName);
        IEnumerable<SubTrialBalanceSummaryViewModel> GetSubTrialBalanceSummary(int ? id, string UserName);
        IEnumerable<LedgerBalanceViewModel> GetLedgerBalance(int id,string UserName);
        IEnumerable<LedgerAccountGroupBalanceViewModel> GetLedgerGroupBalance(int? id, string UserName);
    }

    public class TrialBalanceService : ITrialBalanceService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public TrialBalanceService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;            
        }


        public IEnumerable<TrialBalanceViewModel> GetTrialBalance(string UserName)
        {

            var Settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            string SiteId = Settings.SiteIds;
            string DivisionId = Settings.DivisionIds;
            string AsOnDate = Settings.ToDate.HasValue ? Settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            string CostCenter = Settings.CostCenter;
            bool ShowZeroBalance = Settings.ShowZeroBalance;

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterDate = new SqlParameter("@AsOnDate", AsOnDate);
            SqlParameter SqlParameterCostCenter = new SqlParameter("@CostCenter", !string.IsNullOrEmpty(CostCenter) ? CostCenter : (object)DBNull.Value);
            SqlParameter SqlParameterShowZeroBalance = new SqlParameter("@ShowZeroBalance", ShowZeroBalance);

            IEnumerable<TrialBalanceViewModel> TrialBalanceList;

            TrialBalanceList = db.Database.SqlQuery<TrialBalanceViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spTrialBalance @Site, @Division, @AsOnDate, @CostCenter, @ShowZeroBalance", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterDate, SqlParameterCostCenter, SqlParameterShowZeroBalance).ToList();

            return TrialBalanceList;

        }

        public IEnumerable<TrialBalanceSummaryViewModel> GetTrialBalanceSummary(string UserName)
        {

            var Settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            string SiteId = Settings.SiteIds;
            string DivisionId = Settings.DivisionIds;
            string FromDate = Settings.FromDate.HasValue ? Settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
            string ToDate = Settings.ToDate.HasValue ? Settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            string CostCenter = Settings.CostCenter;

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);


            SqlParameter SqlParameterCostCenter = new SqlParameter("@CostCenter", !string.IsNullOrEmpty(CostCenter) ? CostCenter : (object)DBNull.Value);

            IEnumerable<TrialBalanceSummaryViewModel> TrialBalanceList;

            TrialBalanceList = db.Database.SqlQuery<TrialBalanceSummaryViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spTrialBalanceSummary @Site, @Division, @FromDate, @ToDate, @CostCenter", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterCostCenter).ToList();


            return TrialBalanceList;

        }

        public IEnumerable<ProfitAndLossSummaryViewModel> GetProfitAndLossSummary(string UserName)
        {

            var Settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            string SiteId = Settings.SiteIds;
            string DivisionId = Settings.DivisionIds;
            string FromDate = Settings.FromDate.HasValue ? Settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
            string ToDate = Settings.ToDate.HasValue ? Settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            string CostCenter = Settings.CostCenter;

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterCostCenter = new SqlParameter("@CostCenter", !string.IsNullOrEmpty(CostCenter) ? CostCenter : (object)DBNull.Value);

            IEnumerable<ProfitAndLossSummaryViewModel> TrialBalanceList = db.Database.SqlQuery<ProfitAndLossSummaryViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spProfitAndLoss @Site, @Division, @FromDate, @ToDate", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate).ToList();

            return TrialBalanceList;

        }

        public IEnumerable<BalanceSheetSummaryViewModel> GetBalanceSheetSummary(string UserName)
        {

            var Settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            string SiteId = Settings.SiteIds;
            string DivisionId = Settings.DivisionIds;
            string FromDate = Settings.ToDate.HasValue ? Settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
            string ToDate = Settings.ToDate.HasValue ? Settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            string CostCenter = Settings.CostCenter;

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterCostCenter = new SqlParameter("@CostCenter", !string.IsNullOrEmpty(CostCenter) ? CostCenter : (object)DBNull.Value);

            IEnumerable<BalanceSheetSummaryViewModel> BalanceSheetList = db.Database.SqlQuery<BalanceSheetSummaryViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spBalanceSheet @Site, @Division, @FromDate, @ToDate", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate).ToList();

            return BalanceSheetList;

        }

        public IEnumerable<SubTrialBalanceViewModel> GetSubTrialBalance(int ? id, string UserName)
        {

            var Settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            string SiteId = Settings.SiteIds;
            string DivisionId = Settings.DivisionIds;
            string AsOnDate = Settings.ToDate.HasValue ? Settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            string CostCenter = Settings.CostCenter;
            bool ShowZeroBalance = Settings.ShowZeroBalance;
            string LedgerAccountGroupId = null;
            if (id.HasValue && id.Value > 0)
            {
                LedgerAccountGroupId = id.ToString();
            }


            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterDate = new SqlParameter("@AsOnDate", AsOnDate);
            SqlParameter SqlParameterLedgerAccountGroupId = new SqlParameter("LedgerAccountGroupId", !string.IsNullOrEmpty(LedgerAccountGroupId) ? LedgerAccountGroupId : (object)DBNull.Value);
            SqlParameter SqlParameterCostCenter = new SqlParameter("@CostCenter", !string.IsNullOrEmpty(CostCenter) ? CostCenter : (object)DBNull.Value);
            SqlParameter SqlParameterShowZeroBalance = new SqlParameter("@ShowZeroBalance", ShowZeroBalance);

            IEnumerable<SubTrialBalanceViewModel> TrialBalanceList = db.Database.SqlQuery<SubTrialBalanceViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spSubTrialBalance @Site, @Division, @AsOnDate, @LedgerAccountGroupId, @CostCenter, @ShowZeroBalance", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterDate, SqlParameterLedgerAccountGroupId, SqlParameterCostCenter, SqlParameterShowZeroBalance).ToList();


            return TrialBalanceList;
        }


        public IEnumerable<LedgerAccountGroupBalanceViewModel> GetLedgerGroupBalance(int? id, string UserName)
        {

            var Settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            string SiteId = Settings.SiteIds;
            string DivisionId = Settings.DivisionIds;
            string AsOnDate = Settings.ToDate.HasValue ? Settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            string CostCenter = Settings.CostCenter;
            string LedgerAccountGroupId = null;
            if (id.HasValue && id.Value > 0)
            {
                LedgerAccountGroupId = id.ToString();
            }

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterDate = new SqlParameter("@AsOnDate", AsOnDate);
            SqlParameter SqlParameterLedgerAccountGroupId = new SqlParameter("LedgerAccountGroupId", !string.IsNullOrEmpty(LedgerAccountGroupId) ? LedgerAccountGroupId : (object)DBNull.Value);
            SqlParameter SqlParameterCostCenter = new SqlParameter("@CostCenter", !string.IsNullOrEmpty(CostCenter) ? CostCenter : (object)DBNull.Value);

            IEnumerable<LedgerAccountGroupBalanceViewModel> TrialBalanceList = db.Database.SqlQuery<LedgerAccountGroupBalanceViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spLedgerAccountGroupBalance @Site, @Division, @AsOnDate, @LedgerAccountGroupId, @CostCenter", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterDate, SqlParameterLedgerAccountGroupId, SqlParameterCostCenter).ToList();
            return TrialBalanceList;
        }

        

        public IEnumerable<SubTrialBalanceSummaryViewModel> GetSubTrialBalanceSummary(int ? id, string UserName)
        {

            var Settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            string SiteId = Settings.SiteIds;
            string DivisionId = Settings.DivisionIds;
            string FromDate = Settings.FromDate.HasValue ? Settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
            string ToDate = Settings.ToDate.HasValue ? Settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            string CostCenter = Settings.CostCenter;
            string LedgerAccountGroupId = null;
            if (id.HasValue && id.Value > 0)
            {
                LedgerAccountGroupId = id.ToString();
            }

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterLedgerAccountGroupId = new SqlParameter("LedgerAccountGroupId", !string.IsNullOrEmpty(LedgerAccountGroupId) ? LedgerAccountGroupId : (object)DBNull.Value);
            SqlParameter SqlParameterCostCenter = new SqlParameter("@CostCenter", !string.IsNullOrEmpty(CostCenter) ? CostCenter : (object)DBNull.Value);


            IEnumerable<SubTrialBalanceSummaryViewModel> TrialBalanceList = db.Database.SqlQuery<SubTrialBalanceSummaryViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spSubTrialBalanceSummary @Site, @Division, @FromDate, @ToDate, @LedgerAccountGroupId, @CostCenter", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterLedgerAccountGroupId, SqlParameterCostCenter).ToList();

            return TrialBalanceList;
        }


        public IEnumerable<LedgerBalanceViewModel> GetLedgerBalance(int id,string UserName)
        {

            var Settings = new TrialBalanceSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            string SiteId = Settings.SiteIds;
            string DivisionId = Settings.DivisionIds;
            string FromDate = Settings.FromDate.HasValue ? Settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
            string ToDate = Settings.ToDate.HasValue ? Settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";
            string CostCenter = Settings.CostCenter;
            bool ShowContraAccount = Settings.ShowContraAccount;

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterLedgerAccountId = new SqlParameter("@LedgerAccountId", id);
            SqlParameter SqlParameterCostCenter = new SqlParameter("@CostCenter", !string.IsNullOrEmpty(CostCenter) ? CostCenter : (object)DBNull.Value);
            SqlParameter SqlParameterShowContraAccount = new SqlParameter("@ShowContraAccount", ShowContraAccount);

            IEnumerable<LedgerBalanceViewModel> TrialBalanceList = db.Database.SqlQuery<LedgerBalanceViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spLedger @Site, @Division, @FromDate, @ToDate, @LedgerAccountId, @CostCenter, @ShowContraAccount", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterLedgerAccountId, SqlParameterCostCenter, SqlParameterShowContraAccount).ToList();


            return TrialBalanceList;

        }

        public void Dispose()
        {
        }
    }
}
