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
using Model.ViewModels;

namespace Service
{
    public interface IMaterialPlanCancelLineService : IDisposable
    {
        MaterialPlanCancelLine Create(MaterialPlanCancelLine pt);
        void Delete(int id);
        void Delete(MaterialPlanCancelLine pt);
        MaterialPlanCancelLine Find(int id);
        IEnumerable<MaterialPlanCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanCancelLine pt);
        MaterialPlanCancelLine Add(MaterialPlanCancelLine pt);
        IEnumerable<MaterialPlanCancelLineViewModel> GetMaterialPlanCancelLineList(int id);//Material Plan HeaderId        
        Task<IEquatable<MaterialPlanCancelLine>> GetAsync();
        Task<MaterialPlanCancelLine> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        MaterialPlanSettings GetMaterialPlanSettingsForDocument(int DocTypeId, int DivisionId, int SiteId);
        IEnumerable<MaterialPlanCancelLineViewModel> GetMaterialPlanCancelForDelete(int HEaderId);//
        int GetMaxSr(int id);
        IEnumerable<MaterialPlanCancelLineViewModel> GetOrderPlanForFilters(MaterialPlanCancelFilterViewModel vm);
        IQueryable<ComboBoxResult> GetPendingPlanningHelpList(int Id, string term);
        MaterialPlanCancelLineViewModel GetMaterialPlanCancelLine(int Id);
    }

    public class MaterialPlanCancelLineService : IMaterialPlanCancelLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanCancelLine> _MaterialPlanCancelLineRepository;
        RepositoryQuery<MaterialPlanCancelLine> MaterialPlanCancelLineRepository;
        public MaterialPlanCancelLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanCancelLineRepository = new Repository<MaterialPlanCancelLine>(db);
            MaterialPlanCancelLineRepository = new RepositoryQuery<MaterialPlanCancelLine>(_MaterialPlanCancelLineRepository);
        }


        public MaterialPlanCancelLine Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanCancelLine>().Find(id);
        }

        public MaterialPlanCancelLine Create(MaterialPlanCancelLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanCancelLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanCancelLine>().Delete(id);
        }

        public void Delete(MaterialPlanCancelLine pt)
        {
            _unitOfWork.Repository<MaterialPlanCancelLine>().Delete(pt);
        }

        public void Update(MaterialPlanCancelLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanCancelLine>().Update(pt);
        }

        public IEnumerable<MaterialPlanCancelLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanCancelLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanCancelLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanCancelLineViewModel> GetMaterialPlanCancelLineList(int id)//HEaderId
        {

            return (from p in db.MaterialPlanCancelLine
                    join t2 in db.MaterialPlanLine on p.MaterialPlanLineId equals t2.MaterialPlanLineId
                    join t in db.Product on t2.ProductId equals t.ProductId
                    join u in db.Units on t.UnitId equals u.UnitId
                    where p.MaterialPlanCancelHeaderId == id
                    orderby p.Sr
                    select new MaterialPlanCancelLineViewModel
                    {
                        Qty = p.Qty,
                        MaterialPlanDocNo=t2.MaterialPlanHeader.DocNo,
                        MaterialPlanCancelHeaderId = p.MaterialPlanCancelHeaderId,
                        MaterialPlanCancelLineId = p.MaterialPlanCancelLineId,
                        ProductId = t2.ProductId,
                        ProductName = t.ProductName,
                        Remark = p.Remark,
                        UnitId = u.UnitId,
                        UnitName = u.UnitName,
                        unitDecimalPlaces = u.DecimalPlaces,
                        Dimension1Id = t2.Dimension1Id,
                        Dimension1Name = t2.Dimension1.Dimension1Name,
                        Dimension2Id = t2.Dimension2Id,
                        Dimension2Name = t2.Dimension2.Dimension2Name,
                        LockReason=t2.LockReason,
                    }
                        );

        }

        public MaterialPlanSettings GetMaterialPlanSettingsForDocument(int DocTypeId, int DivisionId, int SiteId)
        {
            return (from p in db.MaterialPlanSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }

        public MaterialPlanCancelLine Add(MaterialPlanCancelLine pt)
        {
            _unitOfWork.Repository<MaterialPlanCancelLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanCancelLine
                        orderby p.MaterialPlanCancelLineId
                        select p.MaterialPlanCancelLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanCancelLine
                        orderby p.MaterialPlanCancelLineId
                        select p.MaterialPlanCancelLineId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanCancelLine
                        orderby p.MaterialPlanCancelLineId
                        select p.MaterialPlanCancelLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanCancelLine
                        orderby p.MaterialPlanCancelLineId
                        select p.MaterialPlanCancelLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<MaterialPlanCancelLineViewModel> GetMaterialPlanCancelForDelete(int HEaderId)
        {
            return (from p in db.MaterialPlanCancelLine
                    where p.MaterialPlanCancelHeaderId == HEaderId
                    select new MaterialPlanCancelLineViewModel
                    {
                        MaterialPlanCancelHeaderId = p.MaterialPlanCancelHeaderId,
                        MaterialPlanCancelLineId = p.MaterialPlanCancelLineId,
                    }
                        );
        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.MaterialPlanCancelLine
                       where p.MaterialPlanCancelHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }

        public IQueryable<ComboBoxResult> GetPendingPlanningHelpList(int Id, string term)
        {

            var Header = new MaterialPlanCancelHeaderService(_unitOfWork).Find(Id);

            var Settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            var list = db.ViewMaterialPlanBalance.AsQueryable();

            if (!string.IsNullOrEmpty(term))
                list = list.Where(m => m.MaterialPlanNo.ToLower().Contains(term.ToLower()));

            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes))
                list = list.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));

            var Jh = list.Join(db.MaterialPlanHeader,
                m => m.MaterialPlanHeaderId,
                t => t.MaterialPlanHeaderId,
                (v, h) => new { v, h }).Where(m => m.h.BuyerId==Header.BuyerId).Select( m => m.v );

            var Result = Jh.GroupBy(m => m.MaterialPlanHeaderId,
                (k, e) => new ComboBoxResult
                {
                    id = k.ToString(),
                    text = e.Max(m => m.MaterialPlanNo + " | " + m.DocType.DocumentTypeName),
                }).OrderBy(m => m.text);

            return Result;
        }


        public IEnumerable<MaterialPlanCancelLineViewModel> GetOrderPlanForFilters(MaterialPlanCancelFilterViewModel vm)
        {
            var Header = new MaterialPlanCancelHeaderService(_unitOfWork).Find(vm.MaterialPlanCancelHeaderId);

            var Settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductId)) { ProductIdArr = vm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(vm.MaterialPlanHeaderId)) { SaleOrderIdArr = vm.MaterialPlanHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProductGroupId)) { ProductGroupIdArr = vm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            string[] Dimension1IdArr = null;
            if (!string.IsNullOrEmpty(vm.Dimension1Id)) { Dimension1IdArr = vm.Dimension1Id.Split(",".ToCharArray()); }
            else { Dimension1IdArr = new string[] { "NA" }; }

            string[] Dimension2IdArr = null;
            if (!string.IsNullOrEmpty(vm.Dimension2Id)) { Dimension2IdArr = vm.Dimension2Id.Split(",".ToCharArray()); }
            else { Dimension2IdArr = new string[] { "NA" }; }

            string[] Dimension3IdArr = null;
            if (!string.IsNullOrEmpty(vm.Dimension3Id)) { Dimension3IdArr = vm.Dimension3Id.Split(",".ToCharArray()); }
            else { Dimension3IdArr = new string[] { "NA" }; }

            string[] Dimension4IdArr = null;
            if (!string.IsNullOrEmpty(vm.Dimension4Id)) { Dimension4IdArr = vm.Dimension4Id.Split(",".ToCharArray()); }
            else { Dimension4IdArr = new string[] { "NA" }; }

            string[] ProcessIdArr = null;
            if (!string.IsNullOrEmpty(vm.ProcessId)) { ProcessIdArr = vm.ProcessId.Split(",".ToCharArray()); }
            else { ProcessIdArr = new string[] { "NA" }; }

            var Query = db.ViewMaterialPlanBalance.AsQueryable()
                .Join(db.MaterialPlanLine,
                m => m.MaterialPlanLineId,
                om => om.MaterialPlanLineId,
                (m, p) => new { m, p })
                .Join(db.Product, m => m.m.ProductId, t => t.ProductId,
               (m, j) => new
               {

                   MaterialPlanLineId = m.m.MaterialPlanLineId,
                   BalanceQty = m.m.BalanceQty,
                   MaterialPlanHeaderId = m.m.MaterialPlanHeaderId,
                   MaterialPlanNo = m.m.MaterialPlanNo,
                   ProductId = m.m.ProductId,
                   MaterialPlanDate = m.m.MaterialPlanDate,
                   DocTypeId = m.m.DocTypeId,
                   DocTypeName = m.m.DocType.DocumentTypeName,
                   ProductGroupId = j.ProductGroupId,
                   Dimension1Name = m.p.Dimension1.Dimension1Name,
                   Dimenstion2Name = m.p.Dimension2.Dimension2Name,
                   ProcessName = m.p.Process.ProcessName,
                   ProductName = j.ProductName,
                   Specification = m.p.Specification,
                   unitDecimalPlaces = j.Unit.DecimalPlaces,
                   UnitId = j.UnitId,
                   MaterialPlanDocNo = m.m.MaterialPlanNo,
                   UnitName = j.Unit.UnitName,
                   BuyerId = m.p.MaterialPlanHeader.BuyerId,
                   Dimension1Id = m.p.Dimension1Id,
                   Dimension2Id = m.p.Dimension2Id,
                   Dimension3Id = m.p.Dimension3Id,
                   Dimension4Id = m.p.Dimension4Id,
                   ProcessId = m.p.ProcessId,
               });

            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes))
                Query = Query.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));

            if (!string.IsNullOrEmpty(vm.ProductId))
                Query = Query.Where(m => ProductIdArr.Contains(m.ProductId.ToString()));

            if (!string.IsNullOrEmpty(vm.MaterialPlanHeaderId))
                Query = Query.Where(m => SaleOrderIdArr.Contains(m.MaterialPlanHeaderId.ToString()));

            if (!string.IsNullOrEmpty(vm.ProductGroupId))
                Query = Query.Where(m => ProductGroupIdArr.Contains(m.ProductGroupId.ToString()));

            if (!string.IsNullOrEmpty(vm.Dimension1Id))
                Query = Query.Where(m => Dimension1IdArr.Contains(m.Dimension1Id.ToString()));

            if (!string.IsNullOrEmpty(vm.Dimension2Id))
                Query = Query.Where(m => Dimension2IdArr.Contains(m.Dimension2Id.ToString()));

            if (!string.IsNullOrEmpty(vm.Dimension3Id))
                Query = Query.Where(m => Dimension3IdArr.Contains(m.Dimension3Id.ToString()));

            if (!string.IsNullOrEmpty(vm.Dimension4Id))
                Query = Query.Where(m => Dimension4IdArr.Contains(m.Dimension4Id.ToString()));

            if (!string.IsNullOrEmpty(vm.ProcessId))
                Query = Query.Where(m => ProcessIdArr.Contains(m.ProcessId.ToString()));

            var Result = Query.Where(m => m.BuyerId == Header.BuyerId).Select(m => new MaterialPlanCancelLineViewModel
            {
                BalanceQty = m.BalanceQty,
                Dimension1Name = m.Dimension1Name,
                Dimension2Name = m.Dimenstion2Name,
                MaterialPlanCancelHeaderDocNo = Header.DocNo,
                MaterialPlanDocNo = m.MaterialPlanDocNo,
                MaterialPlanCancelHeaderId = Header.MaterialPlanCancelHeaderId,
                MaterialPlanLineId = m.MaterialPlanLineId,
                ProcessName = m.ProcessName,
                ProductName = m.ProductName,
                Qty = m.BalanceQty,
                Specification = m.Specification,
                unitDecimalPlaces = m.unitDecimalPlaces,
                UnitId = m.UnitId,
                UnitName = m.UnitName
            });

            return Result;

        }

        public MaterialPlanCancelLineViewModel GetMaterialPlanCancelLine(int Id)
        {
            var Line = (from p in db.MaterialPlanCancelLine
                       join t in db.MaterialPlanLine on p.MaterialPlanLineId equals t.MaterialPlanLineId
                       where p.MaterialPlanCancelLineId == Id
                       select new MaterialPlanCancelLineViewModel
                       {
                           Qty = p.Qty,
                           Dimension1Id = t.Dimension1Id,
                           Dimension2Id = t.Dimension2Id,
                           Dimension1Name = t.Dimension1.Dimension1Name,
                           Dimension2Name = t.Dimension2.Dimension2Name,
                           MaterialPlanCancelHeaderId = p.MaterialPlanCancelHeaderId,
                           MaterialPlanCancelLineId = p.MaterialPlanCancelLineId,
                           MaterialPlanDocNo = t.MaterialPlanHeader.DocNo,
                           MaterialPlanLineId = t.MaterialPlanLineId,
                           ProcessId = t.ProcessId,
                           ProductId = t.ProductId,
                           Remark = p.Remark,
                           Specification = t.Specification,
                           UnitId = t.Product.UnitId,
                           unitDecimalPlaces = t.Product.Unit.DecimalPlaces,
                       }).FirstOrDefault();

            return Line;

        }

        public void Dispose()
        {
        }


        public Task<IEquatable<MaterialPlanCancelLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanCancelLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
