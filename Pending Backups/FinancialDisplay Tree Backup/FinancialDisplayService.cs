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
    public interface IFinancialDisplayService : IDisposable
    {
        IEnumerable<TrialBalanceViewModel> GetTrialBalance(FinancialDisplaySettings Settings);
        IEnumerable<TrialBalanceSummaryViewModel> GetTrialBalanceSummary(FinancialDisplaySettings Settings);
        List<TrialBalanceAsPerDetailViewModel> GetTrialBalanceAsPerDetail(FinancialDisplaySettings Settings);
        IEnumerable<SubTrialBalanceViewModel> GetSubTrialBalance(FinancialDisplaySettings Settings);
        IEnumerable<SubTrialBalanceSummaryViewModel> GetSubTrialBalanceSummary(FinancialDisplaySettings Settings);
        IEnumerable<LedgerBalanceViewModel> GetLedgerBalance(FinancialDisplaySettings Settings);
    }

    public class FinancialDisplayService : IFinancialDisplayService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public static TrialBalanceAsPerDetailViewModel TempVar = new TrialBalanceAsPerDetailViewModel();

        public FinancialDisplayService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;            
        }


        public IEnumerable<TrialBalanceViewModel> GetTrialBalance(FinancialDisplaySettings Settings)
        {
            var SiteSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var LedgerAccountGroupSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "LedgerAccountGroup" select H).FirstOrDefault();


            string SiteId = SiteSetting.Value;
            string DivisionId = DivisionSetting.Value;
            string FromDate = FromDateSetting.Value;
            string ToDate = ToDateSetting.Value;
            string LedgerAccountGroup = LedgerAccountGroupSetting.Value;


            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterLedgerAccountGroup = new SqlParameter("@LedgerAccountGroup", LedgerAccountGroup);


            string mCondStr = "";
            if (SiteId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))";
            if (DivisionId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))";
            if (FromDate != null) mCondStr = mCondStr + " AND LH.DocDate >= @FromDate";
            if (ToDate != null) mCondStr = mCondStr + " AND LH.DocDate <= @ToDate";
            if (LedgerAccountGroup != null && LedgerAccountGroup != "") mCondStr = mCondStr + " AND LAG.LedgerAccountGroupId = @LedgerAccountGroup";

            string mBalanceCondStr = "";
            //if (ShowZeroBalance != null) mCondStr = "HAVING sum(isnull(H.AmtDr,0))  <> sum(isnull(H.AmtCr,0))";

            string mQry = @"SELECT LAG.LedgerAccountGroupId, max(LAG.LedgerAccountGroupName) AS LedgerAccountGroupName, 
                            CASE WHEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) > 0 THEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) ELSE NULL  END AS AmtDr,
                            CASE WHEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) < 0 THEN abs(sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0))) ELSE NULL END AS AmtCr,    
                            'Trial Balance' AS ReportType
                            FROM web.LedgerHeaders LH  WITH (Nolock)
                            LEFT JOIN web.Ledgers H WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = H.LedgerAccountId 
                            LEFT JOIN web.LedgerAccountGroups LAG  WITH (Nolock) ON LAG.LedgerAccountGroupId = LA.LedgerAccountGroupId 
                            WHERE LAG.LedgerAccountGroupId IS NOT NULL  " + mCondStr +
                            @" GROUP BY LAG.LedgerAccountGroupId " +
                            mBalanceCondStr +
                            "Order By LedgerAccountGroupName";

            IEnumerable<TrialBalanceViewModel> TrialBalanceList;
            TrialBalanceList = db.Database.SqlQuery<TrialBalanceViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterLedgerAccountGroup).ToList();

            return TrialBalanceList;

        }

        public IEnumerable<TrialBalanceSummaryViewModel> GetTrialBalanceSummary(FinancialDisplaySettings Settings)
        {
            var SiteSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var LedgerAccountGroupSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "LedgerAccountGroup" select H).FirstOrDefault();


            string SiteId = SiteSetting.Value;
            string DivisionId = DivisionSetting.Value;
            string FromDate = FromDateSetting.Value;
            string ToDate = ToDateSetting.Value;
            string LedgerAccountGroup = LedgerAccountGroupSetting.Value;

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterLedgerAccountGroup = new SqlParameter("@LedgerAccountGroup", LedgerAccountGroup);



            string mCondStr = "";
            if (SiteId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))";
            if (DivisionId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))";
            if (LedgerAccountGroup != null && LedgerAccountGroup != "") mCondStr = mCondStr + " AND LAG.LedgerAccountGroupId = @LedgerAccountGroup";

            string mOpeningDateCondStr = "";
            if (FromDate != null) mOpeningDateCondStr = mOpeningDateCondStr + " AND LH.DocDate < @FromDate ";


            string mDateCondStr = "";
            if (FromDate != null) mDateCondStr = mDateCondStr + " AND LH.DocDate >= @FromDate ";
            if (ToDate != null) mDateCondStr = mDateCondStr + " AND LH.DocDate <= @ToDate ";


            string mQry = @"SELECT VMain.LedgerAccountGroupId,  max(LAG.LedgerAccountGroupName) AS LedgerAccountGroupName, 
                        CASE WHEN abs(Sum(Isnull(VMain.Opening,0))) =0 THEN NULL ELSE abs(Sum(Isnull(VMain.Opening,0))) END AS Opening, 
                        CASE WHEN Sum(Isnull(VMain.Opening,0)) =0 THEN NULL ELSE Sum(Isnull(VMain.Opening,0)) END AS OpeningValue, 
                        CASE WHEN Sum(Isnull(VMain.Opening,0)) >= 0 THEN 'Dr' ELSE 'Cr' END AS OpeningType, 
                        CASE WHEN Sum(isnull(Vmain.AmtDr,0)) = 0 THEN NULL ELSE Sum(isnull(Vmain.AmtDr,0)) END AS AmtDr, 
                        CASE WHEN sum(isnull(VMain.AmtCr,0)) = 0 THEN NULL ELSE sum(isnull(VMain.AmtCr,0)) END AS AmtCr,
                        abs(Sum(Isnull(VMain.Opening,0)) + Sum(isnull(Vmain.AmtDr,0)) - sum(isnull(VMain.AmtCr,0))) AS Balance,
                        Sum(Isnull(VMain.Opening,0)) + Sum(isnull(Vmain.AmtDr,0)) - sum(isnull(VMain.AmtCr,0)) AS BalanceValue,
                        CASE WHEN ( Sum(Isnull(VMain.Opening,0)) + Sum(isnull(Vmain.AmtDr,0)) - sum(isnull(VMain.AmtCr,0))) >= 0 THEN 'Dr' ELSE 'Cr' END AS BalanceType,
                        'Trial Balance' AS ReportType
                        FROM
                        (
                            SELECT LA.LedgerAccountGroupId,  sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) AS Opening,
                            0 AS AmtDr,0 AS  AmtCr    
                            FROM web.LedgerHeaders LH  WITH (Nolock)
                            LEFT JOIN web.Ledgers H WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = H.LedgerAccountId 
                            WHERE LA.LedgerAccountGroupId IS NOT NULL   " + mCondStr + mOpeningDateCondStr +
                            @" GROUP BY LA.LedgerAccountGroupId 
                            
                            UNION ALL 

                            SELECT LA.LedgerAccountGroupId, 0 AS Opening,
                            sum(isnull(H.AmtDr,0)) AS AmtDr,
                            sum(isnull(H.AmtCr,0)) AS AmtCr    
                            FROM web.Ledgers H WITH (Nolock)
                            LEFT JOIN web.LedgerHeaders LH WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = H.LedgerAccountId 
                            LEFT JOIN web.LedgerAccountGroups LAG  WITH (Nolock) ON LAG.LedgerAccountGroupId = LA.LedgerAccountGroupId 
                            WHERE LA.LedgerAccountGroupId IS NOT NULL  " + mCondStr + mDateCondStr +
                            @"GROUP BY LA.LedgerAccountGroupId 
                        ) AS VMain
                        LEFT JOIN web.LedgerAccountGroups LAG  WITH (Nolock) ON LAG.LedgerAccountGroupId = VMain.LedgerAccountGroupId 
                        GROUP BY VMain.LedgerAccountGroupId
                        HAVING Sum(Isnull(VMain.Opening,0)) <> 0 OR Sum(isnull(Vmain.AmtDr,0)) <> 0 OR Sum(isnull(Vmain.AmtDr,0)) <> 0
                        Order By max(LAG.LedgerAccountGroupName)";

            IEnumerable<TrialBalanceSummaryViewModel> TrialBalanceSummaryList = db.Database.SqlQuery<TrialBalanceSummaryViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterLedgerAccountGroup).ToList();

            return TrialBalanceSummaryList;

        }


        public IEnumerable<SubTrialBalanceViewModel> GetSubTrialBalance(FinancialDisplaySettings Settings)
        {
            var SiteSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var LedgerAccountGroupSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "LedgerAccountGroup" select H).FirstOrDefault();


            string SiteId = SiteSetting.Value;
            string DivisionId = DivisionSetting.Value;
            string FromDate = FromDateSetting.Value;
            string ToDate = ToDateSetting.Value;
            string LedgerAccountGroup = LedgerAccountGroupSetting.Value;

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterLedgerAccountGroup = new SqlParameter("@LedgerAccountGroup", LedgerAccountGroup);


            string mCondStr = "";
            if (SiteId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))";
            if (DivisionId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))";
            if (FromDate != null) mCondStr = mCondStr + " AND LH.DocDate >= @FromDate";
            if (ToDate != null) mCondStr = mCondStr + " AND LH.DocDate <= @ToDate";
            if (LedgerAccountGroup != null && LedgerAccountGroup != "") mCondStr = mCondStr + " AND LAG.LedgerAccountGroupId = @LedgerAccountGroup";

            string mBalanceCondStr = "";
            //if (ShowZeroBalance != null) mCondStr = "HAVING sum(isnull(H.AmtDr,0))  <> sum(isnull(H.AmtCr,0))";

            string mQry = @"SELECT LA.LedgerAccountId, max(LA.LedgerAccountName + ',' + LA.LedgerAccountSuffix) AS LedgerAccountName, max(LAG.LedgerAccountGroupName) AS LedgerAccountGroupName, 
                            CASE WHEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) > 0 THEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) ELSE NULL  END AS AmtDr,
                            CASE WHEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) < 0 THEN abs(sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0))) ELSE NULL END AS AmtCr,
                            'Sub Trial Balance' AS ReportType    
                            FROM web.Ledgers H  WITH (Nolock) 
                            LEFT JOIN web.LedgerHeaders LH WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = H.LedgerAccountId 
                            LEFT JOIN web.LedgerAccountGroups LAG  WITH (Nolock) ON LAG.LedgerAccountGroupId = LA.LedgerAccountGroupId 
                            WHERE LA.LedgerAccountId IS NOT NULL " + mCondStr +
                            @" GROUP BY LA.LedgerAccountId  " +
                            mBalanceCondStr +
                            "Order By LedgerAccountName ";

            IEnumerable<SubTrialBalanceViewModel> SubTrialBalanceList = db.Database.SqlQuery<SubTrialBalanceViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterLedgerAccountGroup).ToList();

            return SubTrialBalanceList;

        }


        public IEnumerable<SubTrialBalanceSummaryViewModel> GetSubTrialBalanceSummary(FinancialDisplaySettings Settings)
        {
            var SiteSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var LedgerAccountGroupSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "LedgerAccountGroup" select H).FirstOrDefault();


            string SiteId = SiteSetting.Value;
            string DivisionId = DivisionSetting.Value;
            string FromDate = FromDateSetting.Value;
            string ToDate = ToDateSetting.Value;
            string LedgerAccountGroup = LedgerAccountGroupSetting.Value;

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterLedgerAccountGroup = new SqlParameter("@LedgerAccountGroup", LedgerAccountGroup);


            string mCondStr = "";
            if (SiteId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))";
            if (DivisionId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))";
            if (LedgerAccountGroup != null && LedgerAccountGroup != "") mCondStr = mCondStr + " AND LAG.LedgerAccountGroupId = @LedgerAccountGroup";

            string mOpeningDateCondStr = "";
            if (FromDate != null) mOpeningDateCondStr = mOpeningDateCondStr + " AND LH.DocDate < @FromDate ";


            string mDateCondStr = "";
            if (FromDate != null) mDateCondStr = mDateCondStr + " AND LH.DocDate >= @FromDate ";
            if (ToDate != null) mDateCondStr = mDateCondStr + " AND LH.DocDate <= @ToDate ";


            string mQry = @"SELECT VMain.LedgerAccountId,  max(LA.LedgerAccountName + ',' + LA.LedgerAccountSuffix )   AS LedgerAccountName, max(LAG.LedgerAccountGroupName) AS LedgerAccountGroupName, 
                            CASE WHEN abs(Sum(Isnull(VMain.Opening,0))) = 0 THEN NULL ELSE abs(Sum(Isnull(VMain.Opening,0))) END AS Opening, 
                            CASE WHEN Sum(Isnull(VMain.Opening,0)) = 0 THEN NULL ELSE Sum(Isnull(VMain.Opening,0)) END AS OpeningValue, 
                            CASE WHEN Sum(Isnull(VMain.Opening,0)) >= 0 THEN 'Dr' ELSE 'Cr' END AS OpeningType, 
                            CASE WHEN Sum(isnull(Vmain.AmtDr,0)) = 0 THEN NULL ELSE Sum(isnull(Vmain.AmtDr,0)) END AS AmtDr, CASE WHEN sum(isnull(VMain.AmtCr,0)) = 0 THEN NULL ELSE sum(isnull(VMain.AmtCr,0)) END AS AmtCr,
                            abs(Sum(Isnull(VMain.Opening,0)) + Sum(isnull(Vmain.AmtDr,0)) - sum(isnull(VMain.AmtCr,0))) AS Balance,
                            Sum(Isnull(VMain.Opening,0)) + Sum(isnull(Vmain.AmtDr,0)) - sum(isnull(VMain.AmtCr,0)) AS BalanceValue,
                            CASE WHEN ( Sum(Isnull(VMain.Opening,0)) + Sum(isnull(Vmain.AmtDr,0)) - sum(isnull(VMain.AmtCr,0))) >= 0 THEN 'Dr' ELSE 'Cr' END AS BalanceType,
                            'Sub Trial Balance' AS ReportType
                            FROM
                            (
                            SELECT H.LedgerAccountId ,  sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) AS Opening,
                            0 AS AmtDr,0 AS  AmtCr    
                            FROM web.LedgerHeaders LH  WITH (Nolock)
                            LEFT JOIN web.Ledgers H WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = H.LedgerAccountId 
                            LEFT JOIN web.LedgerAccountGroups LAG  WITH (Nolock) ON LAG.LedgerAccountGroupId = LA.LedgerAccountGroupId 
                            WHERE H.LedgerAccountId  IS NOT NULL " + mCondStr + mOpeningDateCondStr +
                            @" GROUP BY H.LedgerAccountId 
                            HAVING sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) <> 0
                            
                            UNION ALL 

                            SELECT H.LedgerAccountId, 0 AS Opening,
                            sum(isnull(H.AmtDr,0)) AS AmtDr,sum(isnull(H.AmtCr,0)) AS AmtCr
                            FROM web.LedgerHeaders LH  WITH (Nolock)
                            LEFT JOIN  web.Ledgers H  WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = H.LedgerAccountId 
                            LEFT JOIN web.LedgerAccountGroups LAG  WITH (Nolock) ON LAG.LedgerAccountGroupId = LA.LedgerAccountGroupId 
                            WHERE H.LedgerAccountId IS NOT NULL " + mCondStr + mDateCondStr +
                            @" GROUP BY H.LedgerAccountId 
                            ) AS VMain
                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = VMain.LedgerAccountId 
                            LEFT JOIN web.LedgerAccountGroups LAG  WITH (Nolock) ON LAG.LedgerAccountGroupId = LA.LedgerAccountGroupId 
                            Where 1=1 
                            GROUP BY VMain.LedgerAccountId
                            Order By max(LA.LedgerAccountName + ',' + LA.LedgerAccountSuffix) ";



            IEnumerable<SubTrialBalanceSummaryViewModel> SubTrialBalanceSummaryList = db.Database.SqlQuery<SubTrialBalanceSummaryViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterLedgerAccountGroup).ToList();

            return SubTrialBalanceSummaryList;

        }

        public IEnumerable<LedgerBalanceViewModel> GetLedgerBalance(FinancialDisplaySettings Settings)
        {
            var SiteSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var LedgerAccountSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "LedgerAccount" select H).FirstOrDefault();


            string SiteId = SiteSetting.Value;
            string DivisionId = DivisionSetting.Value;
            string FromDate = FromDateSetting.Value;
            string ToDate = ToDateSetting.Value;
            string LedgerAccount = LedgerAccountSetting.Value;


            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterLedgerAccount = new SqlParameter("@LedgerAccount", LedgerAccount);



            string mCondStr = "";
            if (SiteId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))";
            if (DivisionId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))";
            if (LedgerAccount != null && LedgerAccount != "") mCondStr = mCondStr + " AND LA.LedgerAccountId = @LedgerAccount";

            string mOpeningDateCondStr = "";
            if (FromDate != null) mOpeningDateCondStr = mOpeningDateCondStr + " AND LH.DocDate < @FromDate ";


            string mDateCondStr = "";
            if (FromDate != null) mDateCondStr = mDateCondStr + " AND LH.DocDate >= @FromDate ";
            if (ToDate != null) mDateCondStr = mDateCondStr + " AND LH.DocDate <= @ToDate ";

            int ShowContraAccount = 1;


            string mQry = @"SELECT VMain.LedgerAccountId, VMain.LedgerHeaderId, VMain.DocHeaderId, VMain.DocTypeId,  VMain.LedgerAccountName,VMain.PersonId, VMain.ContraLedgerAccountName, 
                            D.DivisionShortCode + S.SiteShortCode + '-' + VMain.DocumentTypeShortName + '-' + VMain.DocNo As DocNo, VMain.DocumentTypeShortName, REPLACE(CONVERT(VARCHAR(11),VMain.DocDate,106), ' ','/')   AS DocDate, IsNull(VMain.Narration,'') As Narration, VMain.LedgerId, 
                            CASE WHEN VMain.AmtDr = 0 THEN NULL ELSE VMain.AmtDr END AS AmtDr, CASE WHEN VMain.AmtCr = 0 THEN NULL ELSE VMain.AmtCr END AS AmtCr,  
                            abs(sum(isnull(VMain.AmtDr,0)) OVER( PARTITION BY VMain.LedgerAccountId  ORDER BY VMain.DocDate, VMain.DocTypeId,  VMain.DocNo, VMain.LedgerId ) - sum(isnull(VMain.AmtCr,0)) OVER( PARTITION BY VMain.LedgerAccountId  ORDER BY VMain.DocDate, VMain.DocTypeId,  VMain.DocNo ,VMain.LedgerId ))  AS Balance,
                            CASE WHEN sum(isnull(VMain.AmtDr,0)) OVER( ORDER BY VMain.DocDate, VMain.DocTypeId,  VMain.DocNo ,VMain.LedgerId ) - sum(isnull(VMain.AmtCr,0)) OVER( PARTITION BY VMain.LedgerAccountId  ORDER BY VMain.DocDate, VMain.DocTypeId,  VMain.DocNo ,VMain.LedgerId )  >= 0 THEN 'Dr' ELSE 'Cr' END  AS BalanceType ,
                            'Ledgers' AS ReportType,
                            VMain.LedgerAccountName AS LedgerAccountText,
                            S.SiteName AS SiteText,D.DivisionName AS DivisionText,VMain.CostCenterName  AS CostCenterName
                            FROM
                            (
	                            SELECT Max(LH.SiteId) AS SiteId, Max(LH.DivisionId) AS DivisionId, H.LedgerAccountId, 0 AS LedgerHeaderId, 0 AS DocHeaderId, 0 AS DocTypeId, Max(LA.LedgerAccountName) AS LedgerAccountName,max(LA.PersonId) AS PersonId,  'Opening' AS ContraLedgerAccountName, 'Opening' AS DocNo, 'Opening' AS DocumentTypeShortName, @FromDate AS DocDate, 'Opening' AS Narration,
	                            'Opening' AS Narration1, 'Opening' AS Narration2, 0 AS LedgerId, 
	                            CASE WHEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) > 0 THEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) ELSE 0 END AS AmtDr,
	                            CASE WHEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) < 0 THEN abs(sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0))) ELSE 0 END AS AmtCr,
	                            NULL AS DomainName, NULL AS ControllerActionId ,'Opening' AS CostCenterName
	                            FROM web.LedgerHeaders LH   WITH (Nolock) 
	                            LEFT JOIN  web.Ledgers H WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
	                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = H.LedgerAccountId 
	                            WHERE H.LedgerAccountId IS NOT NULL " + mCondStr + mOpeningDateCondStr +
	                            @" GROUP BY H.LedgerAccountId
	 
	                            UNION ALL 
	
	                            SELECT LH.SiteId, LH.DivisionId, H.LedgerAccountId,  H.LedgerHeaderId, IsNull(LH.DocHeaderId,H.LedgerHeaderId) AS DocHeaderId,  LH.DocTypeId,  LA.LedgerAccountName,LA.PersonId, CLA.LedgerAccountName AS ContraLedgerAccountName, LH.DocNo, DT.DocumentTypeShortName, LH.DocDate  AS DocDate, 
	                            CASE When " + ShowContraAccount + @" <> 0 And CLA.LedgerAccountName Is Not Null 
			                            Then '<Strong>' + CLA.LedgerAccountName + '</Strong>' + '</br>' + 
				                            CASE When Lh.PartyDocNo Is Not Null THen 'Party Doc No : ' + LH.PartyDocNo + ', ' Else '' End + 
				                            CASE When Lh.PartyDocDate Is Not Null THen 'Party Doc Date : ' +  REPLACE(CONVERT(VARCHAR(11),LH.PartyDocDate,106), ' ','/')  + '</br>' Else '' End + 
				                            CASE When H.ChqNo Is Not Null THen 'Chq No.: ' + H.ChqNo + ', ' Else '' End + 
				                            CASE When H.ChqDate Is Not Null THen 'Chq Date: ' + REPLACE(CONVERT(VARCHAR(11),H.ChqDate,106), ' ','/') + '</br>' Else '' End +  
				                            ' (' + IsNull(LH.Narration,'') + ')'  
		                            Else 
			                            CASE When Lh.PartyDocNo Is Not Null THen 'Party Doc No : ' + LH.PartyDocNo + ', ' Else '' End + 
				                            CASE When Lh.PartyDocDate Is Not Null THen 'Party Doc Date : ' +  REPLACE(CONVERT(VARCHAR(11),LH.PartyDocDate,106), ' ','/')  + '</br>' Else '' End + 
				                            CASE When H.ChqNo Is Not Null THen 'Chq No.: ' + H.ChqNo + ', ' Else '' End + 
				                            CASE When H.ChqDate Is Not Null THen 'Chq Date: ' + REPLACE(CONVERT(VARCHAR(11),H.ChqDate,106), ' ','/') + '</br>' Else '' End +  
				                            ' (' + IsNull(LH.Narration,'') + ')'  
		                            End As Narration,
		
		
	                            CLA.LedgerAccountName AS Narration1, H.Narration AS Narration2,
	                            H.LedgerId, H.AmtDr, H.AmtCr, DT.DomainName, DT.ControllerActionId ,C.CostCenterName      
	                            FROM web.Ledgers H  WITH (Nolock) 
	                            LEFT JOIN web.LedgerHeaders LH WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
	                            LEFT JOIN web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId = LH.DocTypeId 
	                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = H.LedgerAccountId 
	                            LEFT JOIN web.LedgerAccounts CLA  WITH (Nolock) ON CLA.LedgerAccountId = H.ContraLedgerAccountId  
	                            LEFT JOIN Web.CostCenters C WITH (Nolock) ON C.CostCenterId=H.CostCenterId
	                            WHERE LA.LedgerAccountId IS NOT NULL " + mCondStr + mDateCondStr +
                            @" ) VMain 
                            LEFT JOIN Web.Sites S ON S.SiteId = VMain.SiteId
                            LEFT JOIN Web.Divisions D ON D.DivisionId = VMain.DivisionId ";


            IEnumerable<LedgerBalanceViewModel> LedgerBalanceList = db.Database.SqlQuery<LedgerBalanceViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterLedgerAccount).ToList();

            return LedgerBalanceList;

        }

        public List<TrialBalanceAsPerDetailViewModel> GetTrialBalanceAsPerDetail(FinancialDisplaySettings Settings)
        {
            var SiteSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var LedgerAccountGroupSetting = (from H in Settings.FinancialDisplayParameters where H.ParameterName == "LedgerAccountGroup" select H).FirstOrDefault();


            string SiteId = SiteSetting.Value;
            string DivisionId = DivisionSetting.Value;
            string FromDate = FromDateSetting.Value;
            string ToDate = ToDateSetting.Value;
            string LedgerAccountGroup = LedgerAccountGroupSetting.Value;


            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterLedgerAccountGroup = new SqlParameter("@LedgerAccountGroup", LedgerAccountGroup);


            string mCondStr = "";
            if (SiteId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))";
            if (DivisionId != null) mCondStr = mCondStr + " AND LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))";
            if (FromDate != null) mCondStr = mCondStr + " AND LH.DocDate >= @FromDate";
            if (ToDate != null) mCondStr = mCondStr + " AND LH.DocDate <= @ToDate";
            if (LedgerAccountGroup != null && LedgerAccountGroup != "") mCondStr = mCondStr + " AND LAG.LedgerAccountGroupId = @LedgerAccountGroup";

            string mBalanceCondStr = "";
            //if (ShowZeroBalance != null) mCondStr = "HAVING sum(isnull(H.AmtDr,0))  <> sum(isnull(H.AmtCr,0))";


            string mQry = @"WITH CTE_LedgerBalance AS 
                            (
	                            SELECT LAG.LedgerAccountGroupId, max(LAG.LedgerAccountGroupName) AS LedgerAccountGroupName, Max(Lag.ParentLedgerAccountGroupId) AS ParentLedgerAccountGroupId,
	                            Sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) AS Balance,
	                            'Trial Balance' AS ReportType
	                            FROM web.LedgerHeaders LH  WITH (Nolock)
	                            LEFT JOIN web.Ledgers H WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
	                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = H.LedgerAccountId 
	                            LEFT JOIN web.LedgerAccountGroups LAG  WITH (Nolock) ON LAG.LedgerAccountGroupId = LA.LedgerAccountGroupId 
	                            WHERE LAG.LedgerAccountGroupId IS NOT NULL   AND LH.DocDate >=  '01/Apr/2017' 
	                            AND LH.DocDate <= '30/Sep/2017'
	                            --AND LAG.LedgerAccountGroupName LIKE '%Sundry Debtors%'
	                            GROUP BY LAG.LedgerAccountGroupId 
                            ),--SELECT CASE WHEN balance>0 THEN balance END AS dr FROM cte_LedgerBalance, 
                            CTE_LedgerAccountGroup AS 
                            (
	                            SELECT L.LedgerAccountGroupId AS BaseLedgerAccountGroupId, L.LedgerAccountGroupId, L.LedgerAccountGroupName, L.ParentLedgerAccountGroupId AS ParentLedgerAccountGroupId, 0 AS [level]
	                            FROM CTE_LedgerBalance L
	                            UNION ALL
	                            SELECT H.BaseLedgerAccountGroupId, L.LedgerAccountGroupId, L.LedgerAccountGroupName, L.ParentLedgerAccountGroupId, H.level + 1
	                            FROM CTE_LedgerAccountGroup H 
	                            INNER JOIN CTE_LedgerBalance L ON H.ParentLedgerAccountGroupId = L.LedgerAccountGroupId
                            ),
                            CTE_LedgerBalanceTotals AS 
                            (
	                            SELECT IsNull(Sum(VTotals.AmtDr),0) AS TotalAmtDr, IsNull(Sum(VTotals.AmtCr),0) AS TotalAmtCr
	                            FROM (
		                            SELECT L.LedgerAccountGroupId, Max(L.LedgerAccountGroupName) AS LedgerAccountGroupName, Max(L.ParentLedgerAccountGroupId) AS ParentLedgerAccountGroupId, 
		                            CASE WHEN Sum(isnull(Lb.Balance,0)) > 0 THEN Sum(isnull(Lb.Balance,0)) ELSE NULL  END AS AmtDr,
		                            CASE WHEN Sum(isnull(Lb.Balance,0)) < 0 THEN abs(Sum(isnull(Lb.Balance,0))) ELSE NULL END AS AmtCr
		                            FROM CTE_LedgerAccountGroup L 
		                            LEFT JOIN CTE_LedgerBalance Lb ON L.BaseLedgerAccountGroupId = Lb.LedgerAccountGroupId 
		                            WHERE L.ParentLedgerAccountGroupId IS NULL
		                            GROUP BY L.LedgerAccountGroupId
	                            ) AS VTotals
                            )

                            SELECT L.LedgerAccountGroupId, Max(L.LedgerAccountGroupName)  AS LedgerAccountGroupName, Max(L.ParentLedgerAccountGroupId) AS ParentLedgerAccountGroupId, 
                            CASE WHEN Sum(isnull(Lb.Balance,0)) > 0 THEN Sum(isnull(Lb.Balance,0)) ELSE NULL  END AS AmtDr,
                            CASE WHEN Sum(isnull(Lb.Balance,0)) < 0 THEN abs(Sum(isnull(Lb.Balance,0))) ELSE NULL END AS AmtCr,
                            Max(IsNull(Lbt.TotalAmtDr,0)) AS TotalAmtDr, Max(IsNull(Lbt.TotalAmtCr,0)) AS TotalAmtCr
                            FROM CTE_LedgerAccountGroup L 
                            LEFT JOIN CTE_LedgerBalance Lb ON L.BaseLedgerAccountGroupId = Lb.LedgerAccountGroupId 
                            LEFT JOIN CTE_LedgerBalanceTotals Lbt ON 1=1
                            GROUP BY L.LedgerAccountGroupId

                            UNION ALL 

                            SELECT LA.LedgerAccountId AS LedgerAccountGroupId, max(LA.LedgerAccountName) AS LedgerAccountGroupName, 
                            Max(LA.LedgerAccountGroupId) AS ParentLedgerAccountGroupId,
                            CASE WHEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) > 0 THEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) ELSE NULL  END AS AmtDr,
                            CASE WHEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) < 0 THEN abs(sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0))) ELSE NULL END AS AmtCr,
                            Max(IsNull(Lbt.TotalAmtDr,0)) AS TotalAmtDr, Max(IsNull(Lbt.TotalAmtCr,0)) AS TotalAmtCr
                            FROM web.LedgerHeaders LH  WITH (Nolock)
                            LEFT JOIN web.Ledgers H WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
                            LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = H.LedgerAccountId 
                            LEFT JOIN CTE_LedgerBalanceTotals Lbt ON 1=1
                            LEFT JOIN (
	                            SELECT Ag.ParentLedgerAccountGroupId
	                            FROM Web.LedgerAccountGroups Ag
	                            WHERE Ag.ParentLedgerAccountGroupId IS NOT NULL
	                            GROUP BY Ag.ParentLedgerAccountGroupId
                            ) AS V1 ON La.LedgerAccountGroupId = V1.ParentLedgerAccountGroupId
                            WHERE LH.DocDate >=  '01/Apr/2017' 
                            AND LH.DocDate <= '30/Sep/2017'
                            AND V1.ParentLedgerAccountGroupId IS NOT NULL
                            GROUP BY LA.LedgerAccountId ";



            //OR Ag.LedgerAccountGroupName = 'Sundry Debtors (Group)' OR  Pag.LedgerAccountGroupName = 'Sundry Debtors (Group)')

            IEnumerable<TrialBalanceAsPerDetailViewModel> TrialBalanceList = db.Database.SqlQuery<TrialBalanceAsPerDetailViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterLedgerAccountGroup).ToList();

            List<TrialBalanceAsPerDetailViewModel> temp = GenerateTreeStructureForList(TrialBalanceList.ToList());

            //List<TrialBalanceAsPerDetailViewModel> temp1 = temp.Where(i => i.ChildTrialBalanceAsPerDetailViewModel != null && i.ParentLedgerAccountGroupId == null).ToList();

            return temp;

        }

        public static List<TrialBalanceAsPerDetailViewModel> GenerateTreeStructureForList(List<TrialBalanceAsPerDetailViewModel> MainList)
        {
            List<TrialBalanceAsPerDetailViewModel> RetLsit = new List<TrialBalanceAsPerDetailViewModel>();

            foreach(var ListContent in MainList)
            {
                TempVar = ListContent;
                AddNodesRecursively(TempVar, MainList);
                if (ListContent.ParentLedgerAccountGroupId == null)
                    RetLsit.Add(ListContent);
                TempVar = null;
            }

            return RetLsit;
        }


        public static TrialBalanceAsPerDetailViewModel AddNodesRecursively(TrialBalanceAsPerDetailViewModel ListContent, List<TrialBalanceAsPerDetailViewModel> MainList)
        {
            var current = MainList.Where(o => o.ParentLedgerAccountGroupId == ListContent.LedgerAccountGroupId && o.LedgerAccountGroupId != ListContent.LedgerAccountGroupId);
            foreach (var item in current)
            {
                var child = new TrialBalanceAsPerDetailViewModel();
                child.LedgerAccountGroupId = item.LedgerAccountGroupId;
                child.LedgerAccountGroupName = item.LedgerAccountGroupName;
                child.AmtDr = item.AmtDr;
                child.AmtCr = item.AmtCr;
                child.Level = item.Level;
                child.ParentLedgerAccountGroupId = ListContent.LedgerAccountGroupId;
                child.ReportType = item.ReportType;

                if (ListContent.ChildTrialBalanceAsPerDetailViewModel == null)
                    ListContent.ChildTrialBalanceAsPerDetailViewModel = new List<TrialBalanceAsPerDetailViewModel>();
                ListContent.ChildTrialBalanceAsPerDetailViewModel.Add(child);
                if (item.LedgerAccountGroupId != ListContent.LedgerAccountGroupId)
                    AddNodesRecursively(child, MainList);
            }
            return ListContent;
        }



        static List<TrialBalanceAsPerDetailViewModel> BuildTree(List<TrialBalanceAsPerDetailViewModel> items)
        {
            items.ForEach(i => i.ChildTrialBalanceAsPerDetailViewModel = items.Where(ch => ch.ParentLedgerAccountGroupId == i.LedgerAccountGroupId).ToList());
            return items.Where(i => i.ParentLedgerAccountGroupId == i.LedgerAccountGroupId).ToList();
        }


        public void Dispose()
        {
        }
    }


}
