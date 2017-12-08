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
    public interface IJobOrderLineStatusService : IDisposable
    {
        JobOrderLineStatus Create(JobOrderLineStatus pt);
        void Delete(int id);
        void Delete(JobOrderLineStatus pt);
        JobOrderLineStatus Find(int id);
        IEnumerable<JobOrderLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderLineStatus pt);
        JobOrderLineStatus Add(JobOrderLineStatus pt);
        IEnumerable<JobOrderLineStatus> GetJobOrderLineStatusList();

        // IEnumerable<JobOrderLineStatus> GetJobOrderLineStatusList(int buyerId);
        Task<IEquatable<JobOrderLineStatus>> GetAsync();
        Task<JobOrderLineStatus> FindAsync(int id);
        void CreateLineStatus(int id, ref ApplicationDbContext context, bool IsDBbased);
        void DeleteJobQtyOnCancelMultipleDB(int id, ref ApplicationDbContext context);
    }

    public class JobOrderLineStatusService : IJobOrderLineStatusService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobOrderLineStatus> _JobOrderLineStatusRepository;
        RepositoryQuery<JobOrderLineStatus> JobOrderLineStatusRepository;
        public JobOrderLineStatusService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderLineStatusRepository = new Repository<JobOrderLineStatus>(db);
            JobOrderLineStatusRepository = new RepositoryQuery<JobOrderLineStatus>(_JobOrderLineStatusRepository);
        }


        public JobOrderLineStatus Find(int id)
        {
            return _unitOfWork.Repository<JobOrderLineStatus>().Find(id);
        }

        public JobOrderLineStatus Create(JobOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderLineStatus>().Insert(pt);
            return pt;
        }

        public JobOrderLineStatus DBCreate(JobOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            db.JobOrderLineStatus.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderLineStatus>().Delete(id);
        }

        public void Delete(JobOrderLineStatus pt)
        {
            _unitOfWork.Repository<JobOrderLineStatus>().Delete(pt);
        }

        public void Update(JobOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderLineStatus>().Update(pt);
        }

        public IEnumerable<JobOrderLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobOrderLineStatus>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobOrderLineStatus> GetJobOrderLineStatusList()
        {
            var pt = _unitOfWork.Repository<JobOrderLineStatus>().Query().Get();

            return pt;
        }

        public JobOrderLineStatus Add(JobOrderLineStatus pt)
        {
            _unitOfWork.Repository<JobOrderLineStatus>().Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id, ref ApplicationDbContext context, bool IsDBbased)
        {
            JobOrderLineStatus Stat = new JobOrderLineStatus();
            Stat.JobOrderLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            if (IsDBbased)
                context.JobOrderLineStatus.Add(Stat);
            else
                Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            JobOrderLineStatus Stat = Find(id);
            Delete(Stat);
        }


        //CancelQtyUpdate Functions

        public void UpdateJobQtyCancelMultiple(Dictionary<int, decimal> Qty, DateTime DocDate)
        {
            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            var temp = (from p in db.JobOrderCancelLine
                        where (IsdA).Contains(p.JobOrderLineId)
                        group p by p.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key,
                            Qty = g.Sum(m => m.Qty),
                        }).ToList();


            var UnitConVMuls = (from p in db.JobOrderLine
                                where (IsdA).Contains(p.JobOrderLineId)
                                select new
                                {
                                    LineId = p.JobOrderLineId,
                                    UnitConvMul = p.UnitConversionMultiplier
                                }).ToList();


            var LineStatus = (from p in db.JobOrderLineStatus
                              where IsdA.Contains(p.JobOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.CancelQty = Qty[item.JobOrderLineId.Value] + (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.CancelDealQty = item.CancelQty * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.CancelDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Update(item);
            }


        }

        public void UpdateJobQtyCancelMultipleDB(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {
            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            var temp = (from p in Context.JobOrderCancelLine
                        where (IsdA).Contains(p.JobOrderLineId)
                        group p by p.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key,
                            Qty = g.Sum(m => m.Qty),
                        }).ToList();


            var UnitConVMuls = (from p in Context.JobOrderLine
                                where (IsdA).Contains(p.JobOrderLineId)
                                select new
                                {
                                    LineId = p.JobOrderLineId,
                                    UnitConvMul = p.UnitConversionMultiplier
                                }).ToList();


            var LineStatus = (from p in Context.JobOrderLineStatus
                              where IsdA.Contains(p.JobOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.CancelQty = Qty[item.JobOrderLineId.Value] + (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.CancelDealQty = item.CancelQty * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.CancelDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobOrderLineStatus.Add(item);
            }


        }


        public void UpdateJobQtyOnCancel(int id, int CancelLineId, DateTime DocDate, decimal Qty, ref ApplicationDbContext Context, bool IsDbBased)
        {

            var UnitConvMul = (from p in Context.JobOrderLine
                               where p.JobOrderLineId == id
                               select p.UnitConversionMultiplier).FirstOrDefault();

            var temp = (from p in Context.JobOrderCancelLine
                        where p.JobOrderLineId == id && p.JobOrderCancelLineId != CancelLineId
                        join t in Context.JobOrderCancelHeader on p.JobOrderCancelHeaderId equals t.JobOrderCancelHeaderId
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
            UpdateStatusQty(JobStatusQtyConstants.CancelQty, qty, qty * UnitConvMul, date, id, ref Context, true);

        }

        public void DeleteJobQtyOnCancelMultipleDB(int id, ref ApplicationDbContext context)
        {

            var temp = (from t in context.JobOrderCancelLine
                        where t.JobOrderCancelHeaderId == id
                        join p in context.JobOrderLine on t.JobOrderLineId equals p.JobOrderLineId
                        group new { t, p } by t.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key,
                            Qty = g.Sum(m => m.t.Qty),
                            UnitConvMul = g.Max(m => m.p.UnitConversionMultiplier)
                        }).ToList();

            int[] IsdA2 = null;
            IsdA2 = temp.Select(m => m.LineId).ToArray();

            var JobOrderLineStatus = (from p in context.JobOrderLineStatus
                                      where IsdA2.Contains(p.JobOrderLineId.Value)
                                      select p
                                        ).ToList();

            foreach (var item in JobOrderLineStatus)
            {
                item.CancelQty = item.CancelQty - (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.CancelDealQty = item.CancelQty * (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.ObjectState = Model.ObjectState.Modified;
                context.JobOrderLineStatus.Add(item);
            }
        }




        //UpdateQty on Receive
        public void UpdateJobQtyReceiveMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context, bool UseDb)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();

            var temp = (from p in Context.JobReceiveLine
                        where (IsdA).Contains(p.JobOrderLineId ?? 0)
                        group p by p.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key,
                            Qty = g.Sum(m => m.Qty),
                        }).ToList();


            var UnitConVMuls = (from p in Context.JobOrderLine
                                where (IsdA).Contains(p.JobOrderLineId)
                                select new
                                {
                                    LineId = p.JobOrderLineId,
                                    UnitConvMul = p.UnitConversionMultiplier
                                }).ToList();

            var LineStatus = (from p in Context.JobOrderLineStatus
                              where IsdA.Contains(p.JobOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.ReceiveQty = Qty[item.JobOrderLineId.Value] + (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.ReceiveDealQty = item.ReceiveQty * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.ReceiveDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                if (!UseDb)
                    Update(item);
                else
                    db.JobOrderLineStatus.Add(item);
            }


        }


        public void UpdateJobQtyOnReceive(int id, int ReceiveLineId, DateTime DocDate, decimal Qty, ref ApplicationDbContext Context)
        {

            var UnitConvMul = (from p in Context.JobOrderLine
                               where p.JobOrderLineId == id
                               select p.UnitConversionMultiplier).FirstOrDefault();

            var temp = (from p in Context.JobReceiveLine
                        where p.JobOrderLineId == id && p.JobReceiveLineId != ReceiveLineId
                        join t in Context.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
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
            UpdateStatusQty(JobStatusQtyConstants.ReceiveQty, qty, qty * UnitConvMul, date, id, ref Context, true);

        }


        public void DeleteJobQtyOnReceiveMultiple(int id, ref ApplicationDbContext Context)
        {

            var temp = (from t in Context.JobReceiveLine
                        where t.JobReceiveHeaderId == id
                        join p in Context.JobOrderLine on t.JobOrderLineId
                         equals p.JobOrderLineId
                        group new { t, p } by t.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key ?? 0,
                            Qty = g.Sum(m => m.t.Qty + m.t.LossQty),
                            UnitConvMul = g.Max(m => m.p.UnitConversionMultiplier)
                        }).ToList();


            int[] IsdA2 = null;
            IsdA2 = temp.Select(m => m.LineId).ToArray();

            var JobOrderLineStatus = (from p in Context.JobOrderLineStatus
                                      where IsdA2.Contains(p.JobOrderLineId.Value)
                                      select p
                                        ).ToList();

            foreach (var item in JobOrderLineStatus)
            {
                item.ReceiveQty = item.ReceiveQty - (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.ReceiveDealQty = item.ReceiveQty * (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobOrderLineStatus.Add(item);
            }



        }



        //UpdateQty on Invoice
        public void UpdateJobQtyInvoiceMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {

            string Ids = null;
            Ids = string.Join(",", Qty.Select(m => m.Key.ToString()));

            int[] IsdA = null;
            IsdA = Ids.Split(",".ToCharArray()).Select(Int32.Parse).ToArray();

            var temp = (from p in Context.JobReceiveLine
                        where (IsdA).Contains(p.JobReceiveLineId)
                        group p by p.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key ?? 0,
                            Qty = g.Sum(m => m.Qty),
                        }).ToList();


            int[] Ids2 = null;
            Ids2 = temp.Select(m => m.LineId).ToArray();

            var UnitConVMuls = (from p in Context.JobOrderLine
                                where (Ids2).Contains(p.JobOrderLineId)
                                select new
                                {
                                    LineId = p.JobOrderLineId,
                                    UnitConvMul = p.UnitConversionMultiplier
                                }).ToList();


            var LineStatus = (from p in Context.JobOrderLineStatus
                              where (Ids2).Contains(p.JobOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.InvoiceQty = (item.InvoiceQty.HasValue ? item.InvoiceQty : 0) + (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.InvoiceDealQty = item.InvoiceQty * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.InvoiceDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                //Context.JobOrderLineStatus.Add(item);
            }
            Context.JobOrderLineStatus.AddRange(LineStatus);

        }


        public void UpdateJobQtyOnInvoice(int id, int InvoiceLineId, DateTime DocDate, decimal Qty, decimal UnitConvMul, ref ApplicationDbContext Context, bool IsDbBased)
        {
            int LineId = (from p in Context.JobReceiveLine
                          where p.JobReceiveLineId == id
                          select p.JobOrderLineId ?? 0).FirstOrDefault();


            var temp = (from p in Context.JobInvoiceLine
                        where p.JobReceiveLine.JobOrderLineId == LineId && p.JobInvoiceLineId != InvoiceLineId
                        join t in Context.JobInvoiceHeader on p.JobInvoiceHeaderId equals t.JobInvoiceHeaderId
                        select new
                        {
                            Qty = p.JobReceiveLine.Qty,
                            Date = t.DocDate,
                            DealQty = p.DealQty,
                        }).ToList();
            decimal qty;
            decimal dealqty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                dealqty = qty * UnitConvMul;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                dealqty = temp.Sum(m => m.DealQty) + Qty * UnitConvMul;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(JobStatusQtyConstants.InvoiceQty, qty, dealqty, date, LineId, ref Context, IsDbBased);

        }

        public void DeleteJobQtyOnInvoiceMultiple(int id, ref ApplicationDbContext Context)
        {

            var temp = (from p in Context.JobInvoiceLine
                        where p.JobInvoiceHeaderId == id
                        join t in Context.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                        group new { t, p } by t.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key ?? 0,
                            Qty = g.Sum(m => m.t.Qty),
                        }).ToList();



            int[] Ids2 = null;
            Ids2 = temp.Select(m => m.LineId).ToArray();


            var UnitConVMuls = (from p in Context.JobOrderLine
                                where (Ids2).Contains(p.JobOrderLineId)
                                select new
                                {
                                    LineId = p.JobOrderLineId,
                                    UnitConvMul = p.UnitConversionMultiplier
                                }).ToList();

            var JobOrderLineStatus = (from p in Context.JobOrderLineStatus
                                      where Ids2.Contains(p.JobOrderLineId.Value)
                                      select p
                                        ).ToList();

            foreach (var item in JobOrderLineStatus)
            {
                item.InvoiceQty = item.InvoiceQty - (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.InvoiceDealQty = item.InvoiceQty * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobOrderLineStatus.Add(item);
                //Update(item);
            }

            //Context.JobOrderLineStatus.AddRange(JobOrderLineStatus);

        }






        //UpdateQty on Return
        public void UpdateJobQtyReturnMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {
            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();

            var temp2 = (from p in Context.JobReceiveLine
                         where (IsdA).Contains(p.JobReceiveLineId)
                         select new
                         {
                             LineId = p.JobOrderLineId ?? 0,
                             RecevLineId = p.JobReceiveLineId,
                         }).ToList();




            var TEST3 = (from p in temp2
                         join t in Qty on p.RecevLineId equals t.Key
                         group new { p, t } by p.LineId into g
                         select new
                         {
                             LineId = g.Key,
                             Qty = g.Sum(m => m.t.Value),
                         }).ToList();

            int[] IsdA2 = null;
            IsdA2 = TEST3.Select(m => m.LineId).ToArray();


            var UnitConVMuls = (from p in Context.JobOrderLine
                                where (IsdA2).Contains(p.JobOrderLineId)
                                select new
                                {
                                    LineId = p.JobOrderLineId,
                                    UnitConvMul = p.UnitConversionMultiplier
                                }).ToList();

            var LineStatus = (from p in Context.JobOrderLineStatus
                              where IsdA2.Contains(p.JobOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.ReturnQty = (item.ReturnQty.HasValue ? item.ReturnQty : 0) + (TEST3.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : TEST3.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.ReturnDealQty = (item.ReturnQty.HasValue ? item.ReturnQty : 0) * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.ReturnDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobOrderLineStatus.Add(item);
            }


        }


        public void UpdateJobQtyOnReturn(int id, int ReturnLineId, DateTime DocDate, decimal Qty, ref ApplicationDbContext Context)
        {
            int LineId = (from p in Context.JobReceiveLine
                          where p.JobReceiveLineId == id
                          select p.JobOrderLineId ?? 0).FirstOrDefault();

            var UnitConvMul = (from p in Context.JobOrderLine
                               where p.JobOrderLineId == LineId
                               select p.UnitConversionMultiplier).FirstOrDefault();

            var temp = (from p in Context.JobReturnLine
                        where p.JobReceiveLine.JobOrderLineId == LineId && p.JobReturnLineId != ReturnLineId
                        join t in Context.JobReturnHeader on p.JobReturnHeaderId equals t.JobReturnHeaderId
                        select new
                        {
                            Qty = p.Qty,
                            Date = t.DocDate,
                            DealQty = p.DealQty,
                        }).ToList();
            decimal qty;
            decimal dealqty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                dealqty = qty * UnitConvMul;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                dealqty = temp.Sum(m => m.DealQty) + Qty * UnitConvMul;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(JobStatusQtyConstants.ReturnQty, qty, dealqty, date, LineId, ref Context, true);

        }

        public void DeleteJobQtyOnReturnMultiple(int id, ref ApplicationDbContext Context)
        {

            var temp = (from p in Context.JobReturnLine
                        where p.JobReturnHeaderId == id
                        join t in Context.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                        group p by t.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key ?? 0,
                            Qty = g.Sum(m => m.Qty),
                        }).ToList();

            int[] IsdA2 = null;
            IsdA2 = temp.Select(m => m.LineId).ToArray();

            var UnitConVMuls = (from p in Context.JobOrderLine
                                where (IsdA2).Contains(p.JobOrderLineId)
                                select new
                                {
                                    LineId = p.JobOrderLineId,
                                    UnitConvMul = p.UnitConversionMultiplier
                                }).ToList();

            var JobOrderLineStatus = (from p in Context.JobOrderLineStatus
                                      where IsdA2.Contains(p.JobOrderLineId.Value)
                                      select p
                                        ).ToList();

            foreach (var item in JobOrderLineStatus)
            {
                item.ReturnQty = item.ReturnQty - (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.ReturnDealQty = item.ReturnQty * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobOrderLineStatus.Add(item);
            }

        }




        //Rate Amendment Functions

        public void UpdateJobRateOnAmendmentMultiple(Dictionary<int, decimal> Rate, DateTime DocDate, ref ApplicationDbContext Context)
        {
            int[] IsdA = null;
            IsdA = Rate.Select(m => m.Key).ToArray();

            var LineStatus = (from p in Context.JobOrderLineStatus
                              where IsdA.Contains(p.JobOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.RateAmendmentRate = Rate[item.JobOrderLineId.Value];
                item.RateAmendmentDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobOrderLineStatus.Add(item);
            }


        }


        public void UpdateJobRateOnAmendment(int id, int AmendmentLineId, DateTime DocDate, decimal Rate, ref ApplicationDbContext Context)
        {

            UpdateStatusQty(JobStatusQtyConstants.AmendmentRate, Rate, 0, DocDate, id, ref Context, true);

        }


        public void DeleteJobRateOnAmendmentMultiple(int id, ref ApplicationDbContext Context)
        {

            var temp = (from t in Context.JobOrderRateAmendmentLine
                        where t.JobOrderAmendmentHeaderId == id
                        join p in Context.JobOrderLine on t.JobOrderLineId equals p.JobOrderLineId
                        group new { t, p } by t.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key,
                        }).ToList();

            int[] IsdA2 = null;
            IsdA2 = temp.Select(m => m.LineId).ToArray();

            var JobOrderLineStatus = (from p in Context.JobOrderLineStatus
                                      where IsdA2.Contains(p.JobOrderLineId.Value)
                                      select p
                                        ).ToList();

            foreach (var item in JobOrderLineStatus)
            {
                item.RateAmendmentRate = 0;
                item.RateAmendmentDate = null;
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobOrderLineStatus.Add(item);
            }
        }



        //UpdateQty on InvoiceReceive
        public void UpdateJobQtyInvoiceReceiveMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {

            string Ids = null;
            Ids = string.Join(",", Qty.Select(m => m.Key.ToString()));

            int[] IsdA = null;
            IsdA = Ids.Split(",".ToCharArray()).Select(Int32.Parse).ToArray();

            var temp = (from p in Context.JobReceiveLine
                        where (IsdA).Contains(p.JobReceiveLineId)
                        group p by p.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key ?? 0,
                            Qty = g.Sum(m => m.Qty),
                        }).ToList();


            int[] Ids2 = null;
            Ids2 = temp.Select(m => m.LineId).ToArray();

            var UnitConVMuls = (from p in Context.JobOrderLine
                                where (Ids2).Contains(p.JobOrderLineId)
                                select new
                                {
                                    LineId = p.JobOrderLineId,
                                    UnitConvMul = p.UnitConversionMultiplier
                                }).ToList();


            var LineStatus = (from p in Context.JobOrderLineStatus
                              where (Ids2).Contains(p.JobOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.InvoiceQty = (item.InvoiceQty.HasValue ? item.InvoiceQty : 0) + (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.InvoiceDealQty = item.InvoiceQty * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.InvoiceDate = DocDate;
                item.ReceiveQty = (item.ReceiveQty.HasValue ? item.ReceiveQty : 0) + (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.InvoiceDealQty = item.ReceiveQty * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.ReceiveDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobOrderLineStatus.Add(item);
            }


        }


        public void UpdateJobQtyOnInvoiceReceive(int id, int InvoiceLineId, DateTime DocDate, decimal Qty, decimal UnitConvMul, ref ApplicationDbContext Context, bool IsDbBased)
        {
            int LineId = id;

            var temp = (from p in Context.JobInvoiceLine
                        where p.JobReceiveLine.JobOrderLineId == LineId && p.JobInvoiceLineId != InvoiceLineId
                        join t in Context.JobInvoiceHeader on p.JobInvoiceHeaderId equals t.JobInvoiceHeaderId
                        select new
                        {
                            Qty = p.JobReceiveLine.Qty,
                            Date = t.DocDate,
                            DealQty = p.DealQty,
                        }).ToList();
            decimal qty;
            decimal dealqty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                dealqty = qty * UnitConvMul;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                dealqty = temp.Sum(m => m.DealQty) + Qty * UnitConvMul;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(JobStatusQtyConstants.InvoiceReceiveQty, qty, dealqty, date, LineId, ref Context, IsDbBased);

        }

        public void DeleteJobQtyOnInvoiceReceiveMultiple(int id, ref ApplicationDbContext Context)
        {

            var temp = (from p in Context.JobInvoiceLine
                        where p.JobInvoiceHeaderId == id
                        join t in Context.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                        group new { t, p } by t.JobOrderLineId into g
                        select new
                        {
                            LineId = g.Key ?? 0,
                            Qty = g.Sum(m => m.t.Qty+m.t.LossQty),
                        }).ToList();



            int[] Ids2 = null;
            Ids2 = temp.Select(m => m.LineId).ToArray();


            var UnitConVMuls = (from p in Context.JobOrderLine
                                where (Ids2).Contains(p.JobOrderLineId)
                                select new
                                {
                                    LineId = p.JobOrderLineId,
                                    UnitConvMul = p.UnitConversionMultiplier
                                }).ToList();

            var JobOrderLineStatus = (from p in Context.JobOrderLineStatus
                                      where Ids2.Contains(p.JobOrderLineId.Value)
                                      select p
                                        ).ToList();

            foreach (var item in JobOrderLineStatus)
            {
                item.InvoiceQty = item.InvoiceQty - (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.InvoiceDealQty = item.InvoiceQty * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.ReceiveQty = item.ReceiveQty - (temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().Qty);
                item.ReceiveDealQty = item.ReceiveQty * (UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().UnitConvMul);
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobOrderLineStatus.Add(item);
                //Update(item);
            }



        }




        public void UpdateStatusQty(string QtyType, decimal Qty, decimal DealQty, DateTime date, int Id, ref ApplicationDbContext Context, bool DbBased)
        {

            JobOrderLineStatus Stat = Find(Id);

            switch (QtyType)
            {
                case JobStatusQtyConstants.CancelQty:
                    {
                        Stat.CancelQty = Qty;
                        Stat.CancelDealQty = DealQty;
                        Stat.CancelDate = date;
                        break;
                    }
                case JobStatusQtyConstants.ReceiveQty:
                    {
                        Stat.ReceiveQty = Qty;
                        Stat.ReceiveDealQty = DealQty;
                        Stat.ReceiveDate = date;
                        break;
                    }
                case JobStatusQtyConstants.InvoiceQty:
                    {
                        Stat.InvoiceQty = Qty;
                        Stat.InvoiceDealQty = DealQty;
                        Stat.InvoiceDate = date;
                        break;
                    }
                case JobStatusQtyConstants.InvoiceReceiveQty:
                    {
                        Stat.InvoiceQty = Qty;
                        Stat.InvoiceDealQty = DealQty;
                        Stat.ReceiveQty = Qty;
                        Stat.ReceiveDealQty = DealQty;
                        Stat.InvoiceDate = date;
                        Stat.ReceiveDate = date;
                        break;
                    }
                case JobStatusQtyConstants.ReturnQty:
                    {
                        Stat.ReturnQty = Qty;
                        Stat.ReturnDealQty = DealQty;
                        Stat.ReturnDate = date;
                        break;
                    }
                case JobStatusQtyConstants.AmendmentRate:
                    {
                        Stat.RateAmendmentRate = Qty;
                        Stat.RateAmendmentDate = date;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            Stat.ObjectState = Model.ObjectState.Modified;

            if (DbBased)
                Context.JobOrderLineStatus.Add(Stat);
            else
                Update(Stat);


        }


        public void Dispose()
        {
        }


        public Task<IEquatable<JobOrderLineStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderLineStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
