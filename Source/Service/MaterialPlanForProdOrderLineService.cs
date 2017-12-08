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
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Service
{
    public interface IMaterialPlanForProdOrderLineService : IDisposable
    {
        MaterialPlanForProdOrderLine Create(MaterialPlanForProdOrderLine pt);
        void Delete(int id);
        void Delete(MaterialPlanForProdOrderLine pt);
        MaterialPlanForProdOrderLine Find(int id);
        IEnumerable<MaterialPlanForProdOrderLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanForProdOrderLine pt);
        MaterialPlanForProdOrderLine Add(MaterialPlanForProdOrderLine pt);
        IEnumerable<MaterialPlanForProdOrderLine> GetMaterialPlanForLineList();
        Task<IEquatable<MaterialPlanForProdOrderLine>> GetAsync();
        Task<MaterialPlanForProdOrderLine> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<MaterialPlanForProcedureViewModel> GetMaterialPlanLineFromProcedure(MaterialPlanLineListViewModel vm, string ConnectionString,string ProcName);
        IEnumerable<MaterialPlanForProdOrderLine> GetMAterialPlanForProdORderForMaterialPlan(int MaterialPlanLineId);
        int GetMaxSr(int id);
    }

    public class MaterialPlanForProdOrderLineService : IMaterialPlanForProdOrderLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanForProdOrderLine> _MaterialPlanForLineRepository;
        RepositoryQuery<MaterialPlanForProdOrderLine> MaterialPlanForLineRepository;
        public MaterialPlanForProdOrderLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanForLineRepository = new Repository<MaterialPlanForProdOrderLine>(db);
            MaterialPlanForLineRepository = new RepositoryQuery<MaterialPlanForProdOrderLine>(_MaterialPlanForLineRepository);
        }


        public MaterialPlanForProdOrderLine Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanForProdOrderLine>().Find(id);
        }

        public MaterialPlanForProdOrderLine Create(MaterialPlanForProdOrderLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanForProdOrderLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanForProdOrderLine>().Delete(id);
        }

        public void Delete(MaterialPlanForProdOrderLine pt)
        {
            _unitOfWork.Repository<MaterialPlanForProdOrderLine>().Delete(pt);
        }

        public void Update(MaterialPlanForProdOrderLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanForProdOrderLine>().Update(pt);
        }

        public IEnumerable<MaterialPlanForProcedureViewModel> GetMaterialPlanLineFromProcedure(MaterialPlanLineListViewModel vm, string ConnectionString,string ProcName)
        {
            var prodorderlinelist =  vm.MaterialPlanLineViewModel.Where(m => m.Qty > 0).Select(m=>new{m.ProdOrderLineId,m.Qty});

            int MaterialPlanHeaderId;

            if (vm.MaterialPlanLineViewModel.Count > 0)
            {
                MaterialPlanHeaderId = vm.MaterialPlanLineViewModel[0].MaterialPlanHeaderId;
            }

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ProdOrderLineId");
            dataTable.Columns.Add("Qty");


            foreach(var item in prodorderlinelist)
            {

                var dr = dataTable.NewRow();
                dr["ProdOrderLineId"] = item.ProdOrderLineId;
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
                    cmd.Parameters.AddWithValue("@MaterialPlanHeaderId", vm.MaterialPlanLineViewModel.FirstOrDefault().MaterialPlanHeaderId   );
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(ds);
                    }
                }
            }

            // IEnumerable<MaterialPlanForProcedureViewModel> SaleInvoiceprintviewmodel = db.Database.SqlQuery<MaterialPlanForProcedureViewModel>("Web.GetBomForProdOrderLine @ProductTypeId,@ProcessId,@T", new SqlParameter("@ProductTypeId", ProductTypeId), new SqlParameter("@ProcessId", ProcessId), new SqlParameter("@T", dataTable)).ToList();

            DataTable dt2 = ds.Tables[0];


            List<MaterialPlanForProcedureViewModel> temp = new List<MaterialPlanForProcedureViewModel>();

            foreach(DataRow dr in dt2.Rows)
            {
                var values = dr.ItemArray;

                MaterialPlanForProcedureViewModel line = new MaterialPlanForProcedureViewModel();
                line.ProdOrderLineId = values[0] == System.DBNull.Value ? 0 : Convert.ToInt32(values[0].ToString());
                line.ProductId = values[1] == System.DBNull.Value ? 0 : Convert.ToInt32(values[1].ToString());
                line.Dimension1Id = values[2] == System.DBNull.Value ? null : (int?)Convert.ToInt32(values[2].ToString());
                line.Dimension2Id = values[3] == System.DBNull.Value ? null : (int?)Convert.ToInt32(values[3].ToString());
                line.Dimension3Id = values[4] == System.DBNull.Value ? null : (int?)Convert.ToInt32(values[4].ToString());
                line.Dimension4Id = values[5] == System.DBNull.Value ? null : (int?)Convert.ToInt32(values[5].ToString());
                line.ProcessId = values[6] == System.DBNull.Value ? null : (int?)Convert.ToInt32(values[6].ToString());
                line.Qty = values[7] == System.DBNull.Value ? 0 : Convert.ToDecimal(values[7].ToString());
                temp.Add(line);

            }

            return temp;

        }

        public IEnumerable<MaterialPlanLineViewModel> GetMaterialPlanSummaryFromProcedure(List<MaterialPlanForProcedureViewModel> vm, string ConnectionString, string ProcName, int MaterialPlanHeaderId)
        {


            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ProductId");
            dataTable.Columns.Add("ProdOrderLineId");
            dataTable.Columns.Add("Dimension1Id");
            dataTable.Columns.Add("Dimension2Id");
            dataTable.Columns.Add("ProcessId");
            dataTable.Columns.Add("Qty");


            foreach (var item in vm)
            {
                var dr = dataTable.NewRow();
                dr["ProductId"] = item.ProductId ;
                dr["ProdOrderLineId"] = item.ProdOrderLineId;
                dr["Dimension1Id"] = item.Dimension1Id ;
                dr["Dimension2Id"] = item.Dimension2Id;
                dr["ProcessId"] = item.ProcessId ;
                dr["Qty"] = item.Qty;
                dataTable.Rows.Add(dr);

            }
            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("Web.ProcGetBomForWeavingPlanForYarnSummary"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.AddWithValue("@T", dataTable);
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
                line.UnitName = dr["UnitName"] == System.DBNull.Value ? null : dr["UnitName"].ToString();
                line.unitDecimalPlaces = dr["DecimalPlaces"] == System.DBNull.Value ? 0 : Convert.ToInt32(dr["DecimalPlaces"].ToString());
                line.RequiredQty = dr["Qty"] == System.DBNull.Value ? 0 : Convert.ToInt32(dr["Qty"].ToString());
                line.Dimension1Name = dr["Dimension1Name"] == System.DBNull.Value ? null : dr["Dimension1Name"].ToString();
                line.Dimension2Name = dr["Dimension2Name"] == System.DBNull.Value ? null : dr["Dimension2Name"].ToString();
                line.ProcessName = dr["ProcessName"] == System.DBNull.Value ? null : dr["ProcessName"].ToString();


                line.ExcessStockQty = 0;
                line.MaterialPlanHeaderId = MaterialPlanHeaderId;


                line.ProdPlanQty = (dr["PurchProd"].ToString () == "Purchase") ? 0 :  Convert.ToInt32(dr["Qty"].ToString());
                line.PurchPlanQty = (dr["PurchProd"].ToString() == "Purchase") ? Convert.ToInt32(dr["Qty"].ToString()) : 0;

                line.GeneratedFor = MaterialPlanConstants.ProdOrder;



                temp.Add(line);










            }

            return temp;

        }

     

        public IEnumerable<MaterialPlanForProdOrderLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanForProdOrderLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanForProdOrderLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanForProdOrderLine> GetMaterialPlanForLineList()
        {
            var pt = _unitOfWork.Repository<MaterialPlanForProdOrderLine>().Query().Get().OrderBy(m => m.MaterialPlanForProdOrderLineId);

            return pt;
        }

        public MaterialPlanForProdOrderLine Add(MaterialPlanForProdOrderLine pt)
        {
            _unitOfWork.Repository<MaterialPlanForProdOrderLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanForProdOrderLine
                        orderby p.MaterialPlanForProdOrderLineId
                        select p.MaterialPlanForProdOrderLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanForProdOrderLine
                        orderby p.MaterialPlanForProdOrderLineId
                        select p.MaterialPlanForProdOrderLineId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanForProdOrderLine
                        orderby p.MaterialPlanForProdOrderLineId
                        select p.MaterialPlanForProdOrderLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanForProdOrderLine
                        orderby p.MaterialPlanForProdOrderLineId
                        select p.MaterialPlanForProdOrderLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<MaterialPlanForProdOrderLine> GetMAterialPlanForProdORderForMaterialPlan(int MaterialPlanLineId)
        {
            return (from p in db.MaterialPlanForProdOrderLine
                    where p.MaterialPlanLineId == MaterialPlanLineId
                    select p);
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.MaterialPlanLine
                       where p.MaterialPlanHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<MaterialPlanForProdOrderLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanForProdOrderLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
