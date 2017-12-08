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
using Model.ViewModels;
using AutoMapper;

namespace Service
{
    public interface ILedgerService : IDisposable
    {
        Ledger Create(Ledger pt);
        void Delete(int id);
        void Delete(Ledger m);
        void Update(Ledger pt);
        Ledger Find(int id);

        IEnumerable<Ledger> FindForLedgerHeader(int LedgerHeaderId);

        void DeleteLedgerForLedgerHeader(int LedgerHeaderId);
        string LedgerPost(IEnumerable<LedgerViewModel> LedgerViewModel);
        void DeleteLedgerForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId);

        IEnumerable<LedgersViewModel> GetLineListForReceiptVoucher(int id);//LedgerHeaderId
        LedgersViewModel GetLedgerVm(int id);
        decimal GetTotalAmtForCreate(int HeaderId, int ? HeaderAccountId, decimal AmtCr, decimal AmtDr);//
        decimal GetTotalAmtForEdit(int HeaderId, int? HeaderAccountId,int LedgerId, decimal AmtCr, decimal AmtDr);//
        Ledger GetContraLedgerAccount(int? contraledgeraccountid, int headerId);
        string LedgerPost(LedgerViewModel LedgerViewModel_New, LedgerViewModel LedgerViewModel_Old);
    }


    public class LedgerService : ILedgerService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Ledger> _LedgerRepository;
        RepositoryQuery<Ledger> LedgerRepository;

        public LedgerService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _LedgerRepository = new Repository<Ledger>(db);
            LedgerRepository = new RepositoryQuery<Ledger>(_LedgerRepository);
        }

        public decimal GetTotalAmtForCreate(int HeaderId, int ? HeaderAccountId, decimal AmtCr, decimal AmtDr)
        {
            decimal ? total = 0;
            
                total = (from p in db.Ledger
                         where p.LedgerHeaderId == HeaderId && p.LedgerAccountId != HeaderAccountId
                         select p).Sum(t => (decimal?)(t.AmtDr - t.AmtCr)) ??0;
                total += (AmtDr-AmtCr);
            
         
            return total??0;
        }
        public decimal GetTotalAmtForEdit(int HeaderId, int? HeaderAccountId,int LedgerId, decimal AmtCr, decimal AmtDr)
        {
            decimal? total = 0;

            total = (from p in db.Ledger
                     where p.LedgerHeaderId == HeaderId && p.LedgerAccountId != HeaderAccountId && p.LedgerId != LedgerId
                     select p).Sum(t => (decimal?)(t.AmtDr - t.AmtCr)) ?? 0;
            total += (AmtDr - AmtCr);


            return total ?? 0;
        }


        public Ledger GetLedger(int pt)
        {
            return _unitOfWork.Repository<Ledger>().Find(pt);
        }
        public Ledger Find(int id)
        {
            return _unitOfWork.Repository<Ledger>().Find(id);
        }


        public IEnumerable<Ledger> FindForLedgerHeader(int LedgerHeaderId)
        {
            return _unitOfWork.Repository<Ledger>().Query().Get().Where(i => i.LedgerHeaderId == LedgerHeaderId);
        }





        public Ledger Find(int LedgerHeaderId, int LedgerAccountId, int? ContraLedgerAccountId, int? CostCenterId)
        {
            Ledger ledger = (from L in db.Ledger
                           where L.LedgerHeaderId == LedgerHeaderId && L.LedgerAccountId == LedgerAccountId && L.ContraLedgerAccountId == ContraLedgerAccountId &&
                                 L.CostCenterId == CostCenterId
                           select L).FirstOrDefault();
            return ledger;
        }


        public Ledger GetContraLedgerAccount(int? id, int headerId)
        {
            return (from p in db.Ledger where p.LedgerAccountId == id && p.LedgerHeaderId == headerId select p).FirstOrDefault();
        }
        public Ledger Create(Ledger pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Ledger>().Insert(pt);
            return pt;
        }
        public LedgersViewModel GetLedgerVm(int id)
        {
            //return (from p in db.Ledger
            //        join t in db.LedgerHeader on p.LedgerHeaderId equals t.LedgerHeaderId into table from tab in table.DefaultIfEmpty()
            //        join L in db.LedgerLine on p.LedgerLineId equals L.LedgerLineId into LedgerLineTable from LedgerLineTab in LedgerLineTable.DefaultIfEmpty()
            //        join Led in db.Ledger on LedgerLineTab.ReferenceId equals Led.LedgerId into LedgerReferenceTable from LedgerReferenceTab in LedgerReferenceTable.DefaultIfEmpty()
            //        join LedH in db.LedgerHeader on LedgerReferenceTab.LedgerHeaderId equals LedH.LedgerHeaderId into LedgerReferenceHeaderTable
            //        from LedgerReferenceHeaderTab in LedgerReferenceHeaderTable.DefaultIfEmpty()
            //        join t2 in db.DocumentType on tab.DocTypeId equals t2.DocumentTypeId into table2 from tab2 in table2.DefaultIfEmpty()                    
            //        where p.LedgerId == id
            //        select new LedgersViewModel
            //        {
            //            AmtDr = p.AmtDr,
            //            AmtCr = p.AmtCr,
            //            ContraLedgerAccountId = p.ContraLedgerAccountId,
            //            ContraText = p.ContraText,
            //            CostCenterId = p.CostCenterId,
            //            DueDate = p.DueDate,
            //            ChqNo = p.ChqNo,
            //            LedgerAccountId = p.LedgerAccountId,
            //            LedgerHeaderId = p.LedgerHeaderId,
            //            LedgerId = p.LedgerId,
            //            Narration = p.Narration,
            //            DocumentCategoryId=tab2.DocumentCategoryId,
            //            LedgerLineId = p.LedgerLineId,
            //            ReferenceId = LedgerLineTab.ReferenceId,
            //            ReferenceDocNo = LedgerReferenceHeaderTab.DocNo
            //        }
            //            ).FirstOrDefault();

            return (from p in db.LedgerLine
                    join Led in db.Ledger on p.ReferenceId equals Led.LedgerId into LedgerReferenceTable
                    from LedgerReferenceTab in LedgerReferenceTable.DefaultIfEmpty()
                    join t2 in db.LedgerHeader on LedgerReferenceTab.LedgerHeaderId equals t2.LedgerHeaderId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    where p.LedgerLineId == id
                    select new LedgersViewModel
                    {
                        Amount = p.Amount,
                        LedgerLineId = p.LedgerLineId,
                        LedgerHeaderId = p.LedgerHeaderId,
                        LedgerAccountId = p.LedgerAccountId,
                        ReferenceId = p.ReferenceId,
                        BaseValue = p.BaseValue,
                        BaseRate = p.BaseRate,
                        ReferenceDocNo = tab2.DocNo,
                        ReferenceDocTypeId = p.ReferenceDocTypeId,
                        ReferenceDocId = p.ReferenceDocId,
                        ChqNo = p.ChqNo,
                        DueDate = p.ChqDate,
                        CostCenterId = p.CostCenterId,
                        Remark = p.Remark,
                        ProductUidId = p.ProductUidId,
                        ProductUidName = p.ProductUid.ProductUidName,
                        LockReason = p.LockReason,
                        DrCr = p.DrCr
                    }).FirstOrDefault();
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Ledger>().Delete(id);
        }
        public IEnumerable<LedgersViewModel> GetLineListForReceiptVoucher(int id)
        {
            //var ledgeraccountId = new LedgerHeaderService(_unitOfWork).Find(id).LedgerAccountId;

            //return (from p in db.Ledger
            //        join t in db.CostCenter on p.CostCenterId equals t.CostCenterId into table
            //        from tab in table.DefaultIfEmpty()
            //        join t1 in db.LedgerAccount on p.LedgerAccountId equals t1.LedgerAccountId into table1
            //        from tab1 in table1.DefaultIfEmpty()
            //        join L in db.LedgerLine on p.LedgerLineId equals L.LedgerLineId into LedgerLineTable
            //        from LedgerLineTab in LedgerLineTable.DefaultIfEmpty()
            //        join Led in db.Ledger on LedgerLineTab.ReferenceId equals Led.LedgerId into LedgerReferenceTable
            //        from LedgerReferenceTab in LedgerReferenceTable.DefaultIfEmpty()
            //        join LedH in db.LedgerHeader on LedgerReferenceTab.LedgerHeaderId equals LedH.LedgerHeaderId into LedgerReferenceHeaderTable
            //        from LedgerReferenceHeaderTab in LedgerReferenceHeaderTable.DefaultIfEmpty()
            //        where p.LedgerHeaderId == id && p.LedgerAccountId != ledgeraccountId
            //        orderby p.LedgerId
            //        select new LedgersViewModel
            //        {
            //            LedgerId = p.LedgerId,
            //            //AmtCr = p.AmtCr,
            //            CostCenterName = tab.CostCenterName,
            //            LedgerAccountName = tab1.LedgerAccountName,
            //            Narration = p.Narration,
            //           // AmtDr=p.AmtDr,
            //            ChqNo = p.ChqNo,
            //            DueDate = p.DueDate,
            //            LedgerLineId = p.LedgerLineId,
            //            ReferenceDocNo = LedgerReferenceHeaderTab.DocNo
            //        }
            //            );


            return from p in db.LedgerLine
                   join t in db.Ledger on p.ReferenceId equals t.LedgerId into table
                   from tab in table.DefaultIfEmpty()
                   join t2 in db.LedgerHeader on tab.LedgerHeaderId equals t2.LedgerHeaderId into table2
                   from tab2 in table2.DefaultIfEmpty()
                   where p.LedgerHeaderId == id
                   select new LedgersViewModel
                   {
                       LedgerLineId = p.LedgerLineId,
                       LedgerHeaderId = p.LedgerHeaderId,
                       LedgerAccountId = p.LedgerAccountId,
                       LedgerAccountName = p.LedgerAccount.LedgerAccountName + ", " + (p.LedgerAccount.PersonId != null ? p.LedgerAccount.Person.Suffix + " [" + p.LedgerAccount.Person.Code + "]" : p.LedgerAccount.LedgerAccountSuffix),
                       ReferenceId = p.ReferenceId,
                       ReferenceDocNo = tab2.DocNo,
                       ChqNo = p.ChqNo,
                       DueDate = p.ChqDate,
                       CostCenterId = p.CostCenterId,
                       Amount = p.Amount,
                       DrCr = p.DrCr,
                       CostCenterName=p.CostCenter.CostCenterName,
                   };


        }
     
        public void Delete(Ledger pt)
        {
            _unitOfWork.Repository<Ledger>().Delete(pt);
        }

        public void Update(Ledger pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Ledger>().Update(pt);
        }

        public IEnumerable<Ledger> GetLedgerList()
        {
            var pt = _unitOfWork.Repository<Ledger>().Query().Get();

            return pt;
        }

        public Ledger Add(Ledger pt)
        {
            _unitOfWork.Repository<Ledger>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Ledger>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Ledger> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void DeleteLedgerForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId)
        {
            IEnumerable<Ledger> LedgerList = (from L in db.Ledger
                                              join H in db.LedgerHeader on L.LedgerHeaderId equals H.LedgerHeaderId into LedgerHeaderTable
                                              from LedgerHeaderTab in LedgerHeaderTable.DefaultIfEmpty()
                                              where LedgerHeaderTab.DocHeaderId == DocHeaderId && LedgerHeaderTab.DocTypeId == DocTypeId && LedgerHeaderTab.SiteId == SiteId && LedgerHeaderTab.DivisionId == DivisionId
                                              select L).ToList();

            if (LedgerList != null   )
            { if (LedgerList.Count() != 0)
                {
                    foreach (Ledger item in LedgerList)
                    {
                        Delete(item);
                    }
                    new LedgerHeaderService(_unitOfWork).Delete(LedgerList.FirstOrDefault().LedgerHeaderId);
                }
            }
        }


        public void DeleteLedgerForLedgerHeader(int LedgerHeaderId)
        {
            IEnumerable<Ledger> LedgerList = (from L in db.Ledger
                                              where L.LedgerHeaderId == LedgerHeaderId
                                              select L).ToList();

            foreach (Ledger item in LedgerList)
            {
                Delete(item);
            }
        }

        public string LedgerPost(IEnumerable<LedgerViewModel> LedgerViewModel)
        {
            string ErrorText = "";

            var ledgertemp = (from L in LedgerViewModel
                              group new { L } by new { L.DocHeaderId } into Result
                              select new
                              {
                                  DocHeadewrId = Result.Key.DocHeaderId,
                                  TotalDrAmt = Result.Sum(i => i.L.AmtDr),
                                  TotalCrAmt = Result.Sum(i => i.L.AmtCr)
                              }).FirstOrDefault();

            if (ledgertemp.TotalCrAmt != ledgertemp.TotalDrAmt)
            {
                ErrorText = "Debit and credit side amounts are not equal.";
                return ErrorText;
            }

            //Get Data from Ledger VIew Model and post in ledger header model for insertion or updation
            LedgerHeader LedgerHeader = (from L in LedgerViewModel
                                         select new LedgerHeader
                                         {
                                             DocHeaderId = L.DocHeaderId,
                                             DocTypeId = L.DocTypeId,
                                             DocDate = L.DocDate,
                                             DocNo = L.DocNo,
                                             DivisionId = L.DivisionId,
                                             SiteId = L.SiteId,
                                             LedgerAccountId = L.HeaderLedgerAccountId,
                                             CreditDays = L.CreditDays,
                                             Narration = L.HeaderNarration,
                                             Remark = L.Remark,
                                             Status = L.Status,
                                             CreatedBy = L.CreatedBy,
                                             CreatedDate = L.CreatedDate,
                                             ModifiedBy = L.ModifiedBy,
                                             ModifiedDate = L.ModifiedDate
                                         }).FirstOrDefault();

            //For Checking that this Doc Header Id already exist in Ledger Header
            var temp = new LedgerHeaderService(_unitOfWork).FindByDocHeader(LedgerHeader.DocHeaderId, LedgerHeader.DocTypeId, LedgerHeader.SiteId, LedgerHeader.DivisionId);

            //if record is found in Ledger Header Table then it will edit it if data is not found in ledger header table than it will add a new record.
            if (temp == null)
            {
                new LedgerHeaderService(_unitOfWork).Create(LedgerHeader);

                LedgerHeader H = new LedgerHeader();

                H.DocHeaderId = LedgerHeader.DocHeaderId;
                H.DocTypeId = LedgerHeader.DocTypeId;
                H.DocDate = LedgerHeader.DocDate;
                H.DocNo = LedgerHeader.DocNo;
                H.DivisionId = LedgerHeader.DivisionId;
                H.SiteId = LedgerHeader.SiteId;
                H.LedgerAccountId = LedgerHeader.LedgerAccountId;
                H.CreditDays = LedgerHeader.CreditDays;
                H.Narration = LedgerHeader.Narration;
                H.Remark = LedgerHeader.Remark;
                H.Status = LedgerHeader.Status;
                H.CreatedBy = LedgerHeader.CreatedBy;
                H.CreatedDate = LedgerHeader.CreatedDate;
                H.ModifiedBy = LedgerHeader.ModifiedBy;
                H.ModifiedDate = LedgerHeader.ModifiedDate;
            }
            else
            {
                DeleteLedgerForLedgerHeader(LedgerHeader.LedgerHeaderId);

                temp.DocHeaderId = LedgerHeader.DocHeaderId;
                temp.DocTypeId = LedgerHeader.DocTypeId;
                temp.DocDate = LedgerHeader.DocDate;
                temp.DocNo = LedgerHeader.DocNo;
                temp.DivisionId = LedgerHeader.DivisionId;
                temp.SiteId = LedgerHeader.SiteId;
                temp.LedgerAccountId = LedgerHeader.LedgerAccountId;
                temp.CreditDays = LedgerHeader.CreditDays;
                temp.Narration = LedgerHeader.Narration;
                temp.Remark = LedgerHeader.Remark;
                temp.Status = LedgerHeader.Status;
                temp.CreatedBy = LedgerHeader.CreatedBy;
                temp.CreatedDate = LedgerHeader.CreatedDate;
                temp.ModifiedBy = LedgerHeader.ModifiedBy;
                temp.ModifiedDate = LedgerHeader.ModifiedDate;

                new LedgerHeaderService(_unitOfWork).Update(temp);
            }

            IEnumerable<Ledger> LedgerList = (from L in LedgerViewModel
                                              group new { L } by new { L.LedgerAccountId, L.ContraLedgerAccountId, L.CostCenterId } into Result
                                              select new Ledger
                                              {
                                                  LedgerAccountId = Result.Key.LedgerAccountId,
                                                  ContraLedgerAccountId = Result.Key.ContraLedgerAccountId,
                                                  CostCenterId = Result.Key.CostCenterId,
                                                  AmtDr = Result.Sum(i => i.L.AmtDr),
                                                  AmtCr = Result.Sum(i => i.L.AmtCr),
                                                  Narration = Result.Max(i => i.L.Narration)
                                              }).ToList();

            foreach (Ledger item in LedgerList)
            {
                if (temp != null)
                {
                    item.LedgerHeaderId = temp.LedgerHeaderId;
                }

                Create(item);
            }

            return ErrorText;
        }














        public string LedgerPost(LedgerViewModel LedgerViewModel_New, LedgerViewModel LedgerViewModel_Old)
        {
            string ErrorText = "";
            LedgerHeader LedgerHeader;


            if (LedgerViewModel_New != null)
            {
                LedgerHeader = new LedgerHeaderService(_unitOfWork).FindByDocHeader(LedgerViewModel_New.DocHeaderId, LedgerViewModel_New.DocTypeId, LedgerViewModel_New.SiteId, LedgerViewModel_New.DivisionId);

                if (LedgerHeader == null)
                {
                    LedgerHeader H = new LedgerHeader();

                    H.DocHeaderId = LedgerHeader.DocHeaderId;
                    H.DocTypeId = LedgerHeader.DocTypeId;
                    H.DocDate = LedgerHeader.DocDate;
                    H.DocNo = LedgerHeader.DocNo;
                    H.DivisionId = LedgerHeader.DivisionId;
                    H.SiteId = LedgerHeader.SiteId;
                    H.LedgerAccountId = LedgerHeader.LedgerAccountId;
                    H.CreditDays = LedgerHeader.CreditDays;
                    H.Narration = LedgerHeader.Narration;
                    H.Remark = LedgerHeader.Remark;
                    H.Status = LedgerHeader.Status;
                    H.CreatedBy = LedgerHeader.CreatedBy;
                    H.CreatedDate = LedgerHeader.CreatedDate;
                    H.ModifiedBy = LedgerHeader.ModifiedBy;
                    H.ModifiedDate = LedgerHeader.ModifiedDate;

                    new LedgerHeaderService(_unitOfWork).Create(H);
                }
                else
                {
                    LedgerHeader.DocHeaderId = LedgerViewModel_New.DocHeaderId;
                    LedgerHeader.DocTypeId = LedgerViewModel_New.DocTypeId;
                    LedgerHeader.DocDate = LedgerViewModel_New.DocDate;
                    LedgerHeader.DocNo = LedgerViewModel_New.DocNo;
                    LedgerHeader.DivisionId = LedgerViewModel_New.DivisionId;
                    LedgerHeader.SiteId = LedgerViewModel_New.SiteId;
                    LedgerHeader.Remark = LedgerViewModel_New.Remark;
                    LedgerHeader.Status = LedgerViewModel_New.Status;
                    LedgerHeader.CreatedBy = LedgerViewModel_New.CreatedBy;
                    LedgerHeader.CreatedDate = LedgerViewModel_New.CreatedDate;
                    LedgerHeader.ModifiedBy = LedgerViewModel_New.ModifiedBy;
                    LedgerHeader.ModifiedDate = LedgerViewModel_New.ModifiedDate;

                    new LedgerHeaderService(_unitOfWork).Update(LedgerHeader);
                }
            }
            else
            {
                LedgerHeader = new LedgerHeaderService(_unitOfWork).FindByDocHeader(LedgerViewModel_Old.DocHeaderId, LedgerViewModel_Old.DocTypeId, LedgerViewModel_Old.SiteId, LedgerViewModel_Old.DivisionId);
            }

            if (LedgerViewModel_Old != null)
            {
                Ledger Ledger_Old = Find(LedgerHeader.LedgerHeaderId, LedgerViewModel_Old.LedgerAccountId, LedgerViewModel_Old.ContraLedgerAccountId, LedgerViewModel_Old.CostCenterId);

                Ledger_Old.AmtCr = Ledger_Old.AmtCr - LedgerViewModel_Old.AmtCr;
                Ledger_Old.AmtDr = Ledger_Old.AmtDr - LedgerViewModel_Old.AmtDr;

                if (Ledger_Old.AmtCr == 0 && Ledger_Old.AmtDr == 0) { Delete(Ledger_Old); }
                else { Update(Ledger_Old); }
            }

            if (LedgerViewModel_New != null)
            {
                Ledger Ledger_New;

                if (LedgerHeader != null)
                {
                    Ledger_New = Find(LedgerHeader.LedgerHeaderId, LedgerViewModel_New.LedgerAccountId, LedgerViewModel_New.ContraLedgerAccountId, LedgerViewModel_New.CostCenterId);
                }
                else
                {
                    Ledger_New = null;
                }

                if (Ledger_New == null)
                {
                    Ledger L = new Ledger();

                    L.LedgerAccountId = LedgerViewModel_New.LedgerAccountId;
                    L.ContraLedgerAccountId = LedgerViewModel_New.ContraLedgerAccountId;
                    L.CostCenterId = LedgerViewModel_New.CostCenterId;
                    L.AmtDr = LedgerViewModel_New.AmtDr;
                    L.AmtCr = LedgerViewModel_New.AmtCr;
                    L.Narration = LedgerViewModel_New.Narration;
                    L.ContraText = LedgerViewModel_New.ContraText;

                    if (LedgerHeader != null)
                    {
                        L.LedgerHeaderId = LedgerHeader.LedgerHeaderId;
                    }

                    Create(L);
                }
                else
                {
                    Ledger_New.AmtDr = Ledger_New.AmtDr + LedgerViewModel_New.AmtDr;
                    Ledger_New.AmtCr = Ledger_New.AmtCr + LedgerViewModel_New.AmtCr;

                    Update(Ledger_New);
                }
            }

            return ErrorText;
        }


    }
}
