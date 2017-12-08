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
    public interface ISaleOrderLineStatusService : IDisposable
    {
        SaleOrderLineStatus Create(SaleOrderLineStatus pt);
        void Delete(int id);
        void Delete(SaleOrderLineStatus pt);
        SaleOrderLineStatus Find(int id);
        IEnumerable<SaleOrderLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleOrderLineStatus pt);
        SaleOrderLineStatus Add(SaleOrderLineStatus pt);
        IEnumerable<SaleOrderLineStatus> GetSaleOrderLineStatusList();

        // IEnumerable<SaleOrderLineStatus> GetSaleOrderLineStatusList(int buyerId);
        Task<IEquatable<SaleOrderLineStatus>> GetAsync();
        Task<SaleOrderLineStatus> FindAsync(int id);
        void CreateLineStatus(int id);
    }

    public class SaleOrderLineStatusService : ISaleOrderLineStatusService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleOrderLineStatus> _SaleOrderLineStatusRepository;
        RepositoryQuery<SaleOrderLineStatus> SaleOrderLineStatusRepository;
        public SaleOrderLineStatusService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleOrderLineStatusRepository = new Repository<SaleOrderLineStatus>(db);
            SaleOrderLineStatusRepository = new RepositoryQuery<SaleOrderLineStatus>(_SaleOrderLineStatusRepository);
        }


        public SaleOrderLineStatus Find(int id)
        {
            return _unitOfWork.Repository<SaleOrderLineStatus>().Find(id);
        }

        public SaleOrderLineStatus Create(SaleOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleOrderLineStatus>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleOrderLineStatus>().Delete(id);
        }

        public void Delete(SaleOrderLineStatus pt)
        {
            _unitOfWork.Repository<SaleOrderLineStatus>().Delete(pt);
        }

        public void Update(SaleOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleOrderLineStatus>().Update(pt);
        }

        public IEnumerable<SaleOrderLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleOrderLineStatus>()
                .Query()                             
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<SaleOrderLineStatus> GetSaleOrderLineStatusList()
        {
            var pt = _unitOfWork.Repository<SaleOrderLineStatus>().Query().Get();

            return pt;
        }    

        public SaleOrderLineStatus Add(SaleOrderLineStatus pt)
        {
            _unitOfWork.Repository<SaleOrderLineStatus>().Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id)
        {
            SaleOrderLineStatus Stat = new SaleOrderLineStatus();
            Stat.SaleOrderLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            SaleOrderLineStatus Stat = Find(id);
            Delete(Stat);
        }


        //CancelQtyUpdate Functions

        public void UpdateSaleQtyCancelMultiple(Dictionary<int,decimal> Qty,DateTime DocDate)
        {

            int[] IsdA=null;
            IsdA = Qty.Select(m=>m.Key).ToArray();


            var LineAndQty = (from p in db.SaleOrderCancelLine
                       where (IsdA).Contains(p.SaleOrderLineId)
                       group p by p.SaleOrderLineId into g
                       select new
                       {
                           LineId = g.Key,
                           Qty = g.Sum(m => m.Qty),
                       }).ToList();

             string Ids2 = null;
             Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

             var LineStatus = (from p in db.SaleOrderLineStatus
                              where IsdA.Contains(p.SaleOrderLineId.Value)
                              select p).ToList();

            foreach(var item in LineStatus)
            {
                item.CancelQty = Qty[item.SaleOrderLineId.Value] + (LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault().Qty);
                item.CancelDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Update(item);
            }
        }


        public void UpdateSaleQtyOnCancel(int id,int CancelLineId,DateTime DocDate,decimal Qty)
        {

            var temp = (from p in db.SaleOrderCancelLine
                       where p.SaleOrderLineId == id && p.SaleOrderCancelLineId!=CancelLineId
                       join t in db.SaleOrderCancelHeader on p.SaleOrderCancelHeaderId equals t.SaleOrderCancelHeaderId
                       select new
                       {
                           Qty = p.Qty,
                           Date = t.DocDate,
                       }).ToList();
            decimal qty ;
            DateTime date;
            if(temp.Count==0)
            {
                qty = Qty;
                date = DocDate;
            }
            else { 
            qty= temp.Sum(m => m.Qty) + Qty;
             date= temp.Max(m => m.Date);
            }
            UpdateStatusQty(SaleStatusQtyConstants.CancelQty, qty, date, id);

        }


        public void DeleteSaleQtyOnCancelMultiple(int id)
        {

            var LineAndQty = (from t in db.SaleOrderCancelLine
                        where t.SaleOrderCancelHeaderId == id
                        group t by t.SaleOrderLineId into g
                        select new
                        {
                            LineId = g.Key,
                            Qty = g.Sum(m => m.Qty)
                        }).ToList();



            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m=>m.LineId).ToArray();

            var SaleOrderLineStatus = (from p in db.SaleOrderLineStatus
                                      where IsdA2.Contains(p.SaleOrderLineId.Value)
                                      select p
                                        ).ToList();

            foreach (var item in SaleOrderLineStatus)
            {
                item.CancelQty = item.CancelQty - (LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                Update(item);
            }

        }


        //InvoiceQtyUpdate Functions

        public void UpdateSaleQtyInvoiceMultiple(Dictionary<int, decimal> Qty, DateTime DocDate)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m=>m.Key).ToArray();


            var LineAndQty = (from p in db.SaleInvoiceLine
                              where (IsdA).Contains(p.SaleOrderLineId.Value)
                              group p by p.SaleOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();

            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            var LineStatus = (from p in db.SaleOrderLineStatus
                              where IsdA.Contains(p.SaleOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.InvoiceQty = Qty[item.SaleOrderLineId.Value] + (LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault().Qty);
                item.ShipQty = item.InvoiceQty;
                item.InvoiceDate = DocDate;
                item.ShipDate = item.InvoiceDate;
                item.ObjectState = Model.ObjectState.Modified;
                Update(item);
            }
        }


        public void UpdateSaleQtyOnInvoice(int id, int InvoiceLineId, DateTime DocDate, decimal Qty)
        {

            var temp = (from p in db.SaleInvoiceLine
                        where p.SaleOrderLineId == id && p.SaleInvoiceLineId != InvoiceLineId
                        join t in db.SaleInvoiceHeader on p.SaleInvoiceHeaderId equals t.SaleInvoiceHeaderId
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
            UpdateStatusQty(SaleStatusQtyConstants.InvoiceQty, qty, date, id);

        }


        public void DeleteSaleQtyOnInvoiceMultiple(int id)
        {

            var LineAndQty = (from t in db.SaleInvoiceLine
                              where t.SaleInvoiceHeaderId == id
                              group t by t.SaleOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();


            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m=>m.LineId.Value).ToArray();

            using (ApplicationDbContext con=new ApplicationDbContext())
            {

                con.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");

                var SaleOrderLineStatus = (from p in con.SaleOrderLineStatus
                                           where IsdA2.Contains(p.SaleOrderLineId.Value)
                                           select p
                                        ).ToList();

                foreach (var item in SaleOrderLineStatus)
                {
                    item.InvoiceQty = item.InvoiceQty - (LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault().Qty);
                    item.ShipQty = item.InvoiceQty;
                    item.ObjectState = Model.ObjectState.Modified;
                    Update(item);
                }

            }
            

        }






        //AmendmentQtyUpdate Functions

        public void UpdateSaleQtyAmendmentMultiple(Dictionary<int, decimal> Qty, DateTime DocDate)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m=>m.Key).ToArray();


            var LineAndQty = (from p in db.SaleOrderQtyAmendmentLine
                              where (IsdA).Contains(p.SaleOrderLineId)
                              group p by p.SaleOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();

            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            var LineStatus = (from p in db.SaleOrderLineStatus
                              where IsdA.Contains(p.SaleOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.AmendmentQty = Qty[item.SaleOrderLineId.Value] + (LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault().Qty);
                item.CancelDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Update(item);
            }
        }


        public void UpdateSaleQtyOnAmendment(int id, int AmendmentLineId, DateTime DocDate, decimal Qty)
        {

            var temp = (from p in db.SaleOrderQtyAmendmentLine
                        where p.SaleOrderLineId == id && p.SaleOrderQtyAmendmentLineId != AmendmentLineId
                        join t in db.SaleOrderAmendmentHeader on p.SaleOrderAmendmentHeaderId equals t.SaleOrderAmendmentHeaderId
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
            UpdateStatusQty(SaleStatusQtyConstants.AmendmentQty, qty, date, id);

        }


        public void DeleteSaleQtyOnAmendmentMultiple(int id)
        {

            var LineAndQty = (from t in db.SaleOrderQtyAmendmentLine
                              where t.SaleOrderAmendmentHeaderId == id
                              group t by t.SaleOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();

            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m=>m.LineId).ToArray();

            var SaleOrderLineStatus = (from p in db.SaleOrderLineStatus
                                       where IsdA2.Contains(p.SaleOrderLineId.Value)
                                       select p
                                        ).ToList();

            foreach (var item in SaleOrderLineStatus)
            {
                item.AmendmentQty = item.AmendmentQty - (LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.SaleOrderLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                Update(item);
            }

        }



        public void UpdateStatusQty(string QtyType,decimal Qty,DateTime date,int Id)
        {

            SaleOrderLineStatus Stat = Find(Id);

            switch(QtyType)
            {
                case SaleStatusQtyConstants.CancelQty:
                    {
                        Stat.CancelQty = Qty;
                        Stat.CancelDate = date;
                        break;
                    }              
                case SaleStatusQtyConstants.InvoiceQty:
                    {
                        Stat.InvoiceQty = Qty;
                        Stat.InvoiceDate = date;
                        Stat.ShipQty = Stat.InvoiceQty;
                        Stat.ShipDate = Stat.InvoiceDate;
                        break;
                    }
                case SaleStatusQtyConstants.AmendmentQty:
                    {
                        Stat.AmendmentQty = Qty;
                        Stat.AmendmentDate = date;
                        break;
                    } 
                default:
                    {
                        break;
                    }
            }

            Update(Stat);


        }

     
        public void Dispose()
        {
        }


        public Task<IEquatable<SaleOrderLineStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleOrderLineStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
