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
    public interface IStockProcessDisplayService : IDisposable
    {
       
        IEnumerable<StockInHandViewModel> GetStockProcessDisplay(int id, string UserName);     

        IEnumerable<StockLedgerViewModel> GetStockProcessLedger(int? ProductId, int? Dim1, int? Dim2, int? Dim3, int? Dim4, int? Process, string LotNo, int? Godown, string UserName);
    }

    public class StockProcessDisplayService : IStockProcessDisplayService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public StockProcessDisplayService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;            
        }
      

       

        public IEnumerable<StockInHandViewModel> GetStockProcessDisplay(int id, string UserName)
        {

            var Settings = new StockInHandSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            SqlParameter SqlParameterGroupOn;
            SqlParameter SqlParameterSiteId;
            SqlParameter SqlParameterToDate;
            SqlParameter SqlParameterFromDate;
            SqlParameter SqlParameterShowBalance;

            if (string.IsNullOrEmpty(Settings.GroupOn))
                SqlParameterGroupOn = new SqlParameter("@GroupOn", DBNull.Value);
            else
                SqlParameterGroupOn = new SqlParameter("@GroupOn", Settings.GroupOn);

            if (string.IsNullOrEmpty(Settings.SiteIds))
                SqlParameterSiteId = new SqlParameter("@Site", DBNull.Value);
            else
                SqlParameterSiteId = new SqlParameter("@Site", Settings.SiteIds);

            //if (string.IsNullOrEmpty(Settings.DivisionIds))
            //    SqlParameterDivisionId = new SqlParameter("@Division", DBNull.Value);
            //else
            //    SqlParameterDivisionId = new SqlParameter("@Division", Settings.DivisionIds);

            if (!Settings.ToDate.HasValue)
                SqlParameterToDate = new SqlParameter("@ToDate", DBNull.Value);
            else
                SqlParameterToDate = new SqlParameter("@ToDate", Settings.ToDate.Value.ToString("dd/MMM/yyyy"));

            if (!Settings.FromDate.HasValue)
                SqlParameterFromDate = new SqlParameter("@FromDate", DBNull.Value);
            else
                SqlParameterFromDate = new SqlParameter("@FromDate", Settings.FromDate.Value.ToString("dd/MMM/yyyy"));

            SqlParameter SqlParameterProdType = new SqlParameter("@ProductType", id);

            if (string.IsNullOrEmpty(Settings.ShowBalance) || Settings.ShowBalance == StockInHandShowBalanceConstants.All)
                SqlParameterShowBalance = new SqlParameter("@ShowBalance", DBNull.Value);
            else
                SqlParameterShowBalance = new SqlParameter("@ShowBalance", Settings.ShowBalance);


            SqlParameter SqlParameterProduct = new SqlParameter("@Product", DBNull.Value);
            SqlParameter SqlParameterGodown = new SqlParameter("@Godown", DBNull.Value);
            SqlParameter SqlParameterProcess = new SqlParameter("@Process", DBNull.Value);
            SqlParameter SqlParameterDimension1 = new SqlParameter("@Dimension1", DBNull.Value);
            SqlParameter SqlParameterDimension2 = new SqlParameter("@Dimension2", DBNull.Value);
            SqlParameter SqlParameterDimension3 = new SqlParameter("@Dimension3", DBNull.Value);
            SqlParameter SqlParameterDimension4 = new SqlParameter("@Dimension4", DBNull.Value);
            SqlParameter SqlParameterProdNature = new SqlParameter("@ProductNature", DBNull.Value);
            SqlParameter SqlParameterProdGroup = new SqlParameter("@ProductGroup", DBNull.Value);
            SqlParameter SqlParameterProdCustomGroup = new SqlParameter("@ProductCustomGroup", DBNull.Value);

            IEnumerable<StockInHandViewModel> StockInHandList = db.Database.SqlQuery<StockInHandViewModel>("Web.spStockProcess_ForMultipleProductDisplay  @ProductType, @Site, @FromDate, @ToDate, @GroupOn, @ShowBalance, @Product, @Godown, @Process, @Dimension1, @Dimension2,@Dimension3, @Dimension4, @ProductNature, @ProductGroup, @ProductCustomGroup", SqlParameterProdType, SqlParameterSiteId, SqlParameterFromDate, SqlParameterToDate, SqlParameterGroupOn, SqlParameterShowBalance, SqlParameterProduct, SqlParameterGodown, SqlParameterProcess, SqlParameterDimension1, SqlParameterDimension2, SqlParameterDimension3, SqlParameterDimension4, SqlParameterProdNature, SqlParameterProdGroup, SqlParameterProdCustomGroup).ToList();

            return StockInHandList;

        }       

        public IEnumerable<StockLedgerViewModel> GetStockProcessLedger(int? ProductId, int? Dim1, int? Dim2, int? Dim3, int? Dim4, int? Process, string LotNo, int? Godown, string UserName)
        {


            var Settings = new StockInHandSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            SqlParameter SqlParameterGroupOn;
            SqlParameter SqlParameterSiteId;
            SqlParameter SqlParameterDivisionId;
            SqlParameter SqlParameterToDate;
            SqlParameter SqlParameterFromDate;
            //SqlParameter SqlParameterShowBalance;
            SqlParameter SqlParameterProduct;
            SqlParameter SqlParameterGodown;
            SqlParameter SqlParameterProcess;
            SqlParameter SqlParameterDimension1;
            SqlParameter SqlParameterDimension2;
            SqlParameter SqlParameterDimension3;
            SqlParameter SqlParameterDimension4;

            if (string.IsNullOrEmpty(Settings.GroupOn))
                SqlParameterGroupOn = new SqlParameter("@GroupOn", DBNull.Value);
            else
                SqlParameterGroupOn = new SqlParameter("@GroupOn", Settings.GroupOn);

            if (string.IsNullOrEmpty(Settings.SiteIds))
                SqlParameterSiteId = new SqlParameter("@Site", DBNull.Value);
            else
                SqlParameterSiteId = new SqlParameter("@Site", Settings.SiteIds);

            if (string.IsNullOrEmpty(Settings.DivisionIds))
                SqlParameterDivisionId = new SqlParameter("@Division", DBNull.Value);
            else
                SqlParameterDivisionId = new SqlParameter("@Division", Settings.DivisionIds);

            if (!Settings.ToDate.HasValue)
                SqlParameterToDate = new SqlParameter("@ToDate", DBNull.Value);
            else
                SqlParameterToDate = new SqlParameter("@ToDate", Settings.ToDate.Value.ToString("dd/MMM/yyyy"));

            if (!Settings.FromDate.HasValue)
                SqlParameterFromDate = new SqlParameter("@FromDate", DBNull.Value);
            else
                SqlParameterFromDate = new SqlParameter("@FromDate", Settings.FromDate.Value.ToString("dd/MMM/yyyy"));


            //if (string.IsNullOrEmpty(Settings.ShowBalance))
            //    SqlParameterShowBalance = new SqlParameter("@ShowBalance", DBNull.Value);
            //else
            //    SqlParameterShowBalance = new SqlParameter("@ShowBalance", Settings.ShowBalance);

            if (!ProductId.HasValue)
                SqlParameterProduct = new SqlParameter("@Product", DBNull.Value);
            else
                SqlParameterProduct = new SqlParameter("@Product", ProductId);

            if ((!Godown.HasValue))
                SqlParameterGodown = new SqlParameter("@Godown", DBNull.Value);
            else
                SqlParameterGodown = new SqlParameter("@Godown", Godown);

            if ((!Process.HasValue))
                SqlParameterProcess = new SqlParameter("@Process", DBNull.Value);
            else
                SqlParameterProcess = new SqlParameter("@Process", Process);

            if ((!Dim1.HasValue))
                SqlParameterDimension1 = new SqlParameter("@Dimension1", DBNull.Value);
            else
                SqlParameterDimension1 = new SqlParameter("@Dimension1", Dim1);

            if ((!Dim2.HasValue))
                SqlParameterDimension2 = new SqlParameter("@Dimension2", DBNull.Value);
            else
                SqlParameterDimension2 = new SqlParameter("@Dimension2", Dim2);

            if ((!Dim3.HasValue))
                SqlParameterDimension3 = new SqlParameter("@Dimension3", DBNull.Value);
            else
                SqlParameterDimension3 = new SqlParameter("@Dimension3", Dim3);

            if ((!Dim4.HasValue))
                SqlParameterDimension4 = new SqlParameter("@Dimension4", DBNull.Value);
            else
                SqlParameterDimension4 = new SqlParameter("@Dimension4", Dim4);


            SqlParameter SqlParameterProdNature = new SqlParameter("@ProductNature", DBNull.Value);
            SqlParameter SqlParameterProdGroup = new SqlParameter("@ProductGroup", DBNull.Value);
            SqlParameter SqlParameterProdCustomGroup = new SqlParameter("@ProductCustomGroup", DBNull.Value);


            IEnumerable<StockLedgerViewModel> StockInHandList = db.Database.SqlQuery<StockLedgerViewModel>("Web.spStockProcessLedger  @Site, @Division, @FromDate, @ToDate, @GroupOn, @Product, @Godown, @Process, @Dimension1, @Dimension2, @Dimension3, @Dimension4", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterGroupOn, SqlParameterProduct, SqlParameterGodown, SqlParameterProcess, SqlParameterDimension1, SqlParameterDimension2, SqlParameterDimension3, SqlParameterDimension4).ToList();

            return StockInHandList;


        }

        public IEnumerable<StockLedgerViewModel> GetStockLederBalance(int id,string UserName)
        {

            var Settings = new StockInHandSettingService(_unitOfWork).GetTrailBalanceSetting(UserName);

            string SiteId = Settings.SiteIds;
            string DivisionId = Settings.DivisionIds;
            string FromDate = Settings.FromDate.HasValue ? Settings.FromDate.Value.ToString("dd/MMM/yyyy") : "";
            string ToDate = Settings.ToDate.HasValue ? Settings.ToDate.Value.ToString("dd/MMM/yyyy") : "";

            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value );
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value );
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterStockLederAccountId = new SqlParameter("@StockLederAccountId", id);

            IEnumerable<StockLedgerViewModel> StockInHandList = db.Database.SqlQuery<StockLedgerViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spStockLeder @Site, @Division, @FromDate, @ToDate, @StockLederAccountId", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterStockLederAccountId).ToList();

            return StockInHandList;

        }

        public void Dispose()
        {
        }
    }
}
