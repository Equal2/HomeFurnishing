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

namespace Service
{
    public interface IProdOrderLineStatusService : IDisposable
    {
        ProdOrderLineStatus Create(ProdOrderLineStatus pt);
        void Delete(int id);
        void Delete(ProdOrderLineStatus pt);
        ProdOrderLineStatus Find(int id);
        IEnumerable<ProdOrderLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProdOrderLineStatus pt);
        ProdOrderLineStatus Add(ProdOrderLineStatus pt);
        IEnumerable<ProdOrderLineStatus> GetProdOrderLineStatusList();
        Task<IEquatable<ProdOrderLineStatus>> GetAsync();
        Task<ProdOrderLineStatus> FindAsync(int id);
        void CreateLineStatus(int id);
    }

    public class ProdOrderLineStatusService : IProdOrderLineStatusService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProdOrderLineStatus> _ProdOrderLineStatusRepository;
        RepositoryQuery<ProdOrderLineStatus> ProdOrderLineStatusRepository;
        public ProdOrderLineStatusService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProdOrderLineStatusRepository = new Repository<ProdOrderLineStatus>(db);
            ProdOrderLineStatusRepository = new RepositoryQuery<ProdOrderLineStatus>(_ProdOrderLineStatusRepository);
        }


        public ProdOrderLineStatus Find(int id)
        {
            return _unitOfWork.Repository<ProdOrderLineStatus>().Find(id);
        }

        public ProdOrderLineStatus Create(ProdOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProdOrderLineStatus>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProdOrderLineStatus>().Delete(id);
        }

        public void Delete(ProdOrderLineStatus pt)
        {
            _unitOfWork.Repository<ProdOrderLineStatus>().Delete(pt);
        }

        public void Update(ProdOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProdOrderLineStatus>().Update(pt);
        }

        public IEnumerable<ProdOrderLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProdOrderLineStatus>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProdOrderLineStatus> GetProdOrderLineStatusList()
        {
            var pt = _unitOfWork.Repository<ProdOrderLineStatus>().Query().Get();

            return pt;
        }

        public ProdOrderLineStatus Add(ProdOrderLineStatus pt)
        {
            _unitOfWork.Repository<ProdOrderLineStatus>().Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id)
        {
            ProdOrderLineStatus Stat = new ProdOrderLineStatus();
            Stat.ProdOrderLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            ProdOrderLineStatus Stat = Find(id);
            Delete(Stat);
        }


        //CancelQtyUpdate Functions

        public void UpdateProdQtyCancelMultiple(Dictionary<int, decimal> Qty, DateTime DocDate)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            var LineAndQty = (from p in db.ProdOrderCancelLine
                              where (IsdA).Contains(p.ProdOrderLineId)
                              group p by p.ProdOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();

            var LineStatus = (from p in db.ProdOrderLineStatus
                              where IsdA.Contains(p.ProdOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.CancelQty = Qty[item.ProdOrderLineId.Value] + (LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault().Qty);
                item.CancelDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Update(item);
            }
        }


        public void UpdateProdQtyOnCancel(int id, int CancelLineId, DateTime DocDate, decimal Qty,ref ApplicationDbContext context,bool IsDbBased)
        {

            var temp = (from p in db.ProdOrderCancelLine
                        where p.ProdOrderLineId == id && p.ProdOrderCancelLineId != CancelLineId
                        join t in db.ProdOrderCancelHeader on p.ProdOrderCancelHeaderId equals t.ProdOrderCancelHeaderId
                        select new
                        {
                            Qty = p.Qty,
                            Date = t.DocDate,
                        }).ToList();
            decimal qty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(ProdStatusQtyConstants.CancelQty, qty, date, id,ref context,IsDbBased);

        }


        public void DeleteProdQtyOnCancelMultiple(int id)
        {

            var LineAndQty = (from t in db.ProdOrderCancelLine
                              where t.ProdOrderCancelHeaderId == id
                              group t by t.ProdOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();

            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId).ToArray();

            var ProdOrderLineStatus = (from p in db.ProdOrderLineStatus
                                       where IsdA2.Contains(p.ProdOrderLineId.Value)
                                       select p
                                        ).ToList();

            foreach (var item in ProdOrderLineStatus)
            {
                item.CancelQty = item.CancelQty - (LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                Update(item);
            }

        }




        //UpdateQty on JobOrder
        public void UpdateProdQtyJobOrderMultiple(Dictionary<int, decimal> Qty, DateTime DocDate,ref ApplicationDbContext Context)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            var LineAndQty = (from p in Context.JobOrderLine
                              where (IsdA).Contains(p.ProdOrderLineId.Value)
                              group p by p.ProdOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();

            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            var LineStatus = (from p in Context.ProdOrderLineStatus
                              where IsdA.Contains(p.ProdOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.JobOrderQty = Qty[item.ProdOrderLineId.Value] + (LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault().Qty);
                item.JobOrderDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.ProdOrderLineStatus.Add(item);
            }


        }

        public void UpdateProdQtyJobOrderMultipleDB(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            var LineAndQty = (from p in Context.JobOrderLine
                              where (IsdA).Contains(p.ProdOrderLineId.Value)
                              group p by p.ProdOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();

            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            var LineStatus = (from p in Context.ProdOrderLineStatus
                              where IsdA.Contains(p.ProdOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.JobOrderQty = Qty[item.ProdOrderLineId.Value] + (LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault().Qty);
                item.JobOrderDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                //Update(item);
                Context.ProdOrderLineStatus.Add(item);
            }


        }

        public void UpdateProdQtyOnJobOrder(int id, int JobOrderLineId, DateTime DocDate, decimal Qty,ref ApplicationDbContext Context,bool IsDBbased)
        {

            var temp = (from p in db.JobOrderLine
                        where p.ProdOrderLineId == id && p.JobOrderLineId != JobOrderLineId
                        join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                        select new
                        {
                            Qty = p.Qty,
                            Date = t.DocDate,
                        }).ToList();
            decimal qty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(ProdStatusQtyConstants.JobOrderQty, qty, date, id,ref Context,IsDBbased);

        }


        public void DeleteProdQtyOnJobOrderMultiple(int id, ref ApplicationDbContext context, bool DbBased)
        {
            var LineAndQty = (from t in context.JobOrderLine
                              where t.JobOrderHeaderId == id && t.ProdOrderLineId != null
                              group t by t.ProdOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();

            if (LineAndQty != null) 
            { 
                int[] IsdA2 = null;
                IsdA2 = LineAndQty.Select(m => m.LineId.Value).ToArray();

                var ProdOrderLineStatus = (from p in context.ProdOrderLineStatus
                                           where IsdA2.Contains(p.ProdOrderLineId.Value)
                                           select p
                                            ).ToList();

                foreach (var item in ProdOrderLineStatus)
                {
                    item.JobOrderQty = item.JobOrderQty - (LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault().Qty);
                    item.ObjectState = Model.ObjectState.Modified;
                    if (!DbBased)
                        Update(item);
                    else
                        context.ProdOrderLineStatus.Add(item);
                }
            }
        }

        public void DeleteProdQtyOnJobOrderMultipleDb(int id, ref ApplicationDbContext Context)
        {

            var LineAndQty = (from t in Context.JobOrderLine
                              where t.JobOrderHeaderId == id
                              group t by t.ProdOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();


            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId.Value).ToArray();

            var ProdOrderLineStatus = (from p in Context.ProdOrderLineStatus
                                       where IsdA2.Contains(p.ProdOrderLineId.Value)
                                       select p
                                        ).ToList();

            foreach (var item in ProdOrderLineStatus)
            {
                item.JobOrderQty = item.JobOrderQty - (LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.ProdOrderLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                Context.ProdOrderLineStatus.Add(item);
            }
        }


        public void UpdateStatusQty(string QtyType, decimal Qty, DateTime date, int Id,ref ApplicationDbContext context,bool IsDBbased)
        {
            if(IsDBbased)
            {
                ProdOrderLineStatus Stat = (from p in context.ProdOrderLineStatus
                                            where p.ProdOrderLineId == Id
                                            select p).FirstOrDefault();

                switch (QtyType)
                {
                    case ProdStatusQtyConstants.CancelQty:
                        {
                            Stat.CancelQty = Qty;
                            Stat.CancelDate = date;
                            break;
                        }
                    case ProdStatusQtyConstants.AmendmentQty:
                        {
                            Stat.AmendmentQty = Qty;
                            Stat.AmendmentDate = date;
                            break;
                        }
                    case ProdStatusQtyConstants.JobOrderQty:
                        {
                            Stat.JobOrderQty = Qty;
                            Stat.JobOrderDate = date;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                Stat.ObjectState = Model.ObjectState.Modified;
                context.ProdOrderLineStatus.Add(Stat);
            }
            else
            { 

            ProdOrderLineStatus Stat = Find(Id);

            switch (QtyType)
            {
                case ProdStatusQtyConstants.CancelQty:
                    {
                        Stat.CancelQty = Qty;
                        Stat.CancelDate = date;
                        break;
                    }
                case ProdStatusQtyConstants.AmendmentQty:
                    {
                        Stat.AmendmentQty = Qty;
                        Stat.AmendmentDate = date;
                        break;
                    }
                case ProdStatusQtyConstants.JobOrderQty:
                    {
                        Stat.JobOrderQty = Qty;
                        Stat.JobOrderDate = date;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            Update(Stat);

            }
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<ProdOrderLineStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProdOrderLineStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
