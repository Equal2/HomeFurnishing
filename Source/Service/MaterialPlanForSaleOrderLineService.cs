using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;

using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModel;
using System.Data.SqlClient;
using Model.ViewModels;
using System.Configuration;
using System.Data;
namespace Service
{
    public interface IMaterialPlanForSaleOrderLineService : IDisposable
    {
        MaterialPlanForSaleOrderLine Create(MaterialPlanForSaleOrderLine pt);
        void Delete(int id);
        void Delete(MaterialPlanForSaleOrderLine pt);
        MaterialPlanForSaleOrderLine Find(int id);
        IEnumerable<MaterialPlanForSaleOrderLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanForSaleOrderLine pt);
        MaterialPlanForSaleOrderLine Add(MaterialPlanForSaleOrderLine pt);
        IEnumerable<MaterialPlanForSaleOrderLine> GetMaterialPlanForLineList();
        Task<IEquatable<MaterialPlanForSaleOrderLine>> GetAsync();
        Task<MaterialPlanForSaleOrderLine> FindAsync(int id);
        IEnumerable<MaterialPlanForSaleOrderViewModel> GetLineListForIndex(int id);//HeaderId
        IEnumerable<MaterialPlanForSaleOrderViewModel> GetSaleOrdersForFilters(MaterialPlanForLineFilterViewModel svm);
        IEnumerable<MaterialPlanForSaleOrderViewModel> GetProdOrdersForFilters(MaterialPlanLineForProductionFilterViewModel svm);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<MaterialPlanForSaleOrderLine> GetSaleOrderLineForMaterialPlanLine(int MaterialPlanLineId);
        IEnumerable<MaterialPlanLineViewModel> GetMaterialPlanSummaryFromProcedure(MaterialPlanLineListViewModel vm, string ConnectionString, string ProcName);
    }

    public class MaterialPlanForSaleOrderLineService : IMaterialPlanForSaleOrderLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanForSaleOrderLine> _MaterialPlanForLineRepository;
        RepositoryQuery<MaterialPlanForSaleOrderLine> MaterialPlanForLineRepository;
        public MaterialPlanForSaleOrderLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanForLineRepository = new Repository<MaterialPlanForSaleOrderLine>(db);
            MaterialPlanForLineRepository = new RepositoryQuery<MaterialPlanForSaleOrderLine>(_MaterialPlanForLineRepository);
        }


        public MaterialPlanForSaleOrderLine Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanForSaleOrderLine>().Find(id);
        }
        public IEnumerable<MaterialPlanForSaleOrderLine> GetSaleOrderLineForMaterialPlanLine(int MaterialPlanLineId)
        {
            return (from p in db.MaterialPlanForSaleOrderLine
                    where p.MaterialPlanLineId == MaterialPlanLineId
                    select p
                        ).ToList();
        }

        public MaterialPlanForSaleOrderLine Create(MaterialPlanForSaleOrderLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanForSaleOrderLine>().Insert(pt);
            return pt;
        }
        public IEnumerable<MaterialPlanForSaleOrderViewModel> GetLineListForIndex(int id)
        {
            //return (from p in db.MaterialPlanForSaleOrderLine
            //        where p.MaterialPlanHeaderId == id
            //        orderby p.MaterialPlanForLineId
            //        select new MaterialPlanForLineViewModel
            //        {
            //            DeliveryQty = p.DeliveryQty,
            //            DeliveryUnitId = p.DeliveryUnitId,
            //            DueDate = p.DueDate,
            //            MaterialPlanForLineId
            //            = p.MaterialPlanForLineId,
            //            MaterialPlanHeaderDocNo = p.MaterialPlanHeader.DocNo,
            //            MaterialPlanHeaderId = p.MaterialPlanHeaderId,
            //            Qty = p.Qty,
            //            Remark = p.Remark,
            //        }
            //           );
            throw new NotImplementedException();
        }

        public IEnumerable<MaterialPlanForSaleOrderViewModel> GetSaleOrdersForFilters(MaterialPlanForLineFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.SaleOrderHeaderId)) { SaleOrderIdArr = svm.SaleOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            string[] Dimension1IdArr = null;
            if (!string.IsNullOrEmpty(svm.Dimension1Id)) { Dimension1IdArr = svm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dimension1IdArr = new string[] { "NA" }; }

            string[] Dimension2IdArr = null;
            if (!string.IsNullOrEmpty(svm.Dimension2Id)) { Dimension2IdArr = svm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dimension2IdArr = new string[] { "NA" }; }

            string[] BuyerIdArr = null;
            if (!string.IsNullOrEmpty(svm.BuyerId)) { BuyerIdArr = svm.BuyerId.Split(",".ToCharArray()); }
            else { BuyerIdArr = new string[] { "NA" }; }


            var Header = new MaterialPlanHeaderService(_unitOfWork).Find(svm.MaterialPlanHeaderId);            

            var settings= new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            string ProcName = settings.PendingProdOrderList;

            if (string.IsNullOrEmpty(ProcName))
                throw new Exception("Pending ProdOrders Not Configured");

            SqlParameter SqlParameterDocType = new SqlParameter("@PlanningDocumentType", svm.DocTypeId);
            SqlParameter SqlParameterSite = new SqlParameter("@Site", Header.SiteId);
            SqlParameter SqlParameterDivision = new SqlParameter("@Division", Header.DivisionId);
            SqlParameter SqlParameterBuyer = new SqlParameter("@BuyerId", (Header.BuyerId.HasValue ? Header.BuyerId : (object)DBNull.Value));

            IEnumerable<PendingSaleOrderFromProc> PendingSaleOrders = db.Database.SqlQuery<PendingSaleOrderFromProc>(" " + ProcName + " @PlanningDocumentType, @Site, @Division, @BuyerId", SqlParameterDocType, SqlParameterSite, SqlParameterDivision,SqlParameterBuyer).ToList();

            //var ProductIds=(PendingSaleOrders.Select(m=>m.ProductId)).ToArray();

            //var ProductGroupIds = (from p in db.Product.Where(m => ProductIdArr.Contains(m.ProductId.ToString()))
            //                       select p.ProductGroupId).ToArray();


            var resu = (from p in PendingSaleOrders                       
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                               && (string.IsNullOrEmpty(svm.SaleOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleOrderHeaderId.ToString()))
                               && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(p.ProductGroupId.ToString()))
                               && (string.IsNullOrEmpty(svm.Dimension1Id) ? 1 == 1 : Dimension1IdArr.Contains(p.Dimension1Id.ToString()))
                               && (string.IsNullOrEmpty(svm.Dimension2Id) ? 1 == 1 : Dimension2IdArr.Contains(p.Dimension2Id.ToString()))
                               && (string.IsNullOrEmpty(svm.BuyerId) ? 1 == 1 : BuyerIdArr.Contains(p.BuyerId.ToString()))
                               && p.BalanceQty > 0
                               orderby p.Sr
                        select new MaterialPlanForSaleOrderViewModel
                        {
                            BalanceQtyForPlan = p.BalanceQty,
                            Qty = p.BalanceQty,
                            SaleOrderDocNo = p.SaleOrderNo,
                            ProductId = p.ProductId,
                            ProductName = p.ProductName,
                            Dimension1Id = p.Dimension1Id,
                            Dimension1Name = p.Dimension1Name,
                            Dimension2Id = p.Dimension2Id,
                            Dimension2Name = p.Dimension2Name,
                            MaterialPlanHeaderId = svm.MaterialPlanHeaderId,
                            SaleOrderLineId = p.SaleOrderLineId,  
                            Specification=p.Specification,
                            UnitName=p.UnitName,
                            BomDetailExists =p.BomDetailExists,
                        }).ToList();
            return resu;
        }

        public IEnumerable<MaterialPlanForSaleOrderViewModel> GetProdOrdersForFilters(MaterialPlanLineForProductionFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProdOrderHeaderId)) { SaleOrderIdArr = svm.ProdOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var Header = new MaterialPlanHeaderService(_unitOfWork).Find(svm.MaterialPlanHeaderId);

            string ProcName = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId).PendingProdOrderList;

            SqlParameter SqlParameterProductId = new SqlParameter("@MaterialPlanHeaderId", svm.MaterialPlanHeaderId);            

            IEnumerable<ProdOrderBalanceViewModel> StockAvailableForPacking = db.Database.SqlQuery<ProdOrderBalanceViewModel>("" + ProcName + " @MaterialPlanHeaderId", SqlParameterProductId).ToList();

            var ProductIds = StockAvailableForPacking.Select(m => m.ProductId).ToArray();
            var ProdOrderLineIds = StockAvailableForPacking.Select(m => m.ProdOrderLineId).ToArray();


            var temp1 = from p in StockAvailableForPacking
                         where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                                     && (string.IsNullOrEmpty(svm.ProdOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.ProdOrderHeaderId.ToString()))
                                     && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(p.ProductGroupId.ToString()))
                                     && p.BalanceQty > 0 && (svm.DocDate.HasValue ? p.ProdOrderDate.Date <= svm.DocDate.Value.Date : 1 == 1)
                         select new MaterialPlanForSaleOrderViewModel
                         {
                             BalanceQtyForPlan = p.BalanceQty,
                             Qty = p.BalanceQty,
                             ProdOrderDocNo = p.ProdOrderNo,
                             ProductId = p.ProductId,
                             ProductName = p.ProductName,
                             MaterialPlanHeaderId = svm.MaterialPlanHeaderId,
                             ProdOrderLineId = p.ProdOrderLineId,
                             Dimension1Id = p.Dimension1Id,
                             Dimension2Id = p.Dimension2Id,
                             Dimension1Name = p.Dimension1Name,
                             Dimension2Name = p.Dimension2Name,
                             UnitName = p.UnitName,
                             ProcessId = p.ProcessId,
                             ProcessName = p.ProcessName,
                             BomDetailExists = p.IsBomExist,
                         };


            var temp2 = (from p in StockAvailableForPacking                                                
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(p.ProductId.ToString()))
                                    && (string.IsNullOrEmpty(svm.ProdOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.ProdOrderHeaderId.ToString()))
                                    && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(p.ProductGroupId.ToString()))
                                    && p.BalanceQty > 0  && (svm.DocDate.HasValue ?  p.ProdOrderDate.Date <= svm.DocDate.Value.Date : 1==1)
                        select new MaterialPlanForSaleOrderViewModel
                        {
                            BalanceQtyForPlan = p.BalanceQty,
                            Qty = p.BalanceQty,
                            ProdOrderDocNo = p.ProdOrderNo,
                            ProductId = p.ProductId,
                            ProductName = p.ProductName,
                            MaterialPlanHeaderId = svm.MaterialPlanHeaderId,
                            ProdOrderLineId = p.ProdOrderLineId,
                            Dimension1Id = p.Dimension1Id,
                            Dimension2Id = p.Dimension2Id,
                            Dimension1Name = p.Dimension1Name,
                            Dimension2Name = p.Dimension2Name,
                            UnitName = p.UnitName,
                            ProcessId = p.ProcessId,
                            ProcessName = p.ProcessName,
                            BomDetailExists = p.IsBomExist,
                        }).ToList();


            var DocTypeIds = StockAvailableForPacking.Select(m => m.DocTypeId).ToArray();

            return temp2;
        }
        public IEnumerable<MaterialPlanLineViewModel> GetMaterialPlanSummaryFromProcedure(MaterialPlanLineListViewModel vm, string ConnectionString, string ProcName)
        {
            var prodorderlinelist = vm.MaterialPlanLineViewModel.Where(m => m.Qty > 0).Select(m => new { m.SaleOrderLineId, m.ProductId, m.Qty });

            int MaterialPlanHeaderId = 0;

            if (vm.MaterialPlanLineViewModel.Count > 0)
            {
                MaterialPlanHeaderId = vm.MaterialPlanLineViewModel[0].MaterialPlanHeaderId;
            }

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("SaleOrderLineId");
            dataTable.Columns.Add("ProductId");
            dataTable.Columns.Add("Qty");


            foreach (var item in prodorderlinelist)
            {

                var dr = dataTable.NewRow();
                dr["SaleOrderLineId"] = item.SaleOrderLineId;
                dr["ProductId"] = item.ProductId;
                dr["Qty"] = item.Qty;
                dataTable.Rows.Add(dr);

            }
            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand(ProcName))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@T", dataTable);
                    cmd.Parameters.AddWithValue("@MaterialPlanHeaderId", vm.MaterialPlanLineViewModel.FirstOrDefault().MaterialPlanHeaderId);
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(ds);
                    }
                }
            }

            DataTable dt2 = ds.Tables[0];

            List<MaterialPlanLineViewModel> temp = new List<MaterialPlanLineViewModel>();

            foreach (DataRow dr in dt2.Rows)
            {

                MaterialPlanLineViewModel line = new MaterialPlanLineViewModel();

                line.ProductId = dr["ProductId"] == System.DBNull.Value ? 0 : Convert.ToInt32(dr["ProductId"].ToString());
                line.ProcessId = dr["ProcessId"] == System.DBNull.Value ? null : (int?)Convert.ToInt32(dr["ProcessId"].ToString());
                line.Dimension1Id = dr["Dimension1Id"] == System.DBNull.Value ? null : (int?)Convert.ToInt32(dr["Dimension1Id"]);
                line.Dimension2Id = dr["Dimension2Id"] == System.DBNull.Value ? null : (int?)Convert.ToInt32(dr["Dimension2Id"]);


                line.ProductName = dr["ProductName"] == System.DBNull.Value ? null : dr["ProductName"].ToString();
                line.Specification = dr["Specification"] == System.DBNull.Value ? null : dr["Specification"].ToString();
                line.UnitName = dr["UnitName"] == System.DBNull.Value ? null : dr["UnitName"].ToString();
                line.unitDecimalPlaces = dr["DecimalPlaces"] == System.DBNull.Value ? 0 : Convert.ToInt32(dr["DecimalPlaces"].ToString());
                line.RequiredQty = dr["Qty"] == System.DBNull.Value ? 0 : Convert.ToDecimal(dr["Qty"].ToString());
                line.Dimension1Name = dr["Dimension1Name"] == System.DBNull.Value ? null : dr["Dimension1Name"].ToString();
                line.Dimension2Name = dr["Dimension2Name"] == System.DBNull.Value ? null : dr["Dimension2Name"].ToString();
                line.ProcessName = dr["ProcessName"] == System.DBNull.Value ? null : dr["ProcessName"].ToString();


                line.ExcessStockQty = 0;
                line.MaterialPlanHeaderId = MaterialPlanHeaderId;


                line.ProdPlanQty = (dr["PurchProd"].ToString() == "Purchase") ? 0 : Convert.ToDecimal(dr["Qty"].ToString());
                line.PurchPlanQty = (dr["PurchProd"].ToString() == "Purchase") ? Convert.ToDecimal(dr["Qty"].ToString()) : 0;

                line.GeneratedFor = MaterialPlanConstants.SaleOrder;



                temp.Add(line);
            }

            return temp;

        }


        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanForSaleOrderLine>().Delete(id);
        }

        public void Delete(MaterialPlanForSaleOrderLine pt)
        {
            _unitOfWork.Repository<MaterialPlanForSaleOrderLine>().Delete(pt);
        }

        public void Update(MaterialPlanForSaleOrderLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanForSaleOrderLine>().Update(pt);
        }

        public IEnumerable<MaterialPlanForSaleOrderLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanForSaleOrderLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanForSaleOrderLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanForSaleOrderLine> GetMaterialPlanForLineList()
        {
            var pt = _unitOfWork.Repository<MaterialPlanForSaleOrderLine>().Query().Get().OrderBy(m => m.MaterialPlanForSaleOrderLineId);

            return pt;
        }

        public MaterialPlanForSaleOrderLine Add(MaterialPlanForSaleOrderLine pt)
        {
            _unitOfWork.Repository<MaterialPlanForSaleOrderLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanForSaleOrderLine
                        orderby p.MaterialPlanForSaleOrderLineId
                        select p.MaterialPlanForSaleOrderLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanForSaleOrderLine
                        orderby p.MaterialPlanForSaleOrderLineId
                        select p.MaterialPlanForSaleOrderLineId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.MaterialPlanForSaleOrderLine
                        orderby p.MaterialPlanForSaleOrderLineId
                        select p.MaterialPlanForSaleOrderLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanForSaleOrderLine
                        orderby p.MaterialPlanForSaleOrderLineId
                        select p.MaterialPlanForSaleOrderLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<MaterialPlanForSaleOrderLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanForSaleOrderLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
