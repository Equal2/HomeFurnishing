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
using System.Web.Mvc;
using Model.ViewModels;

namespace Service
{
    public interface ICalculationService : IDisposable
    {
        Calculation Create(Calculation pt);
        void Delete(int id);
        void Delete(Calculation pt);
        Calculation Find(string Name);
        Calculation Find(int id);
        IEnumerable<Calculation> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Calculation pt);
        Calculation Add(Calculation pt);
        IEnumerable<Calculation> GetCalculationList();
        Task<IEquatable<Calculation>> GetAsync();
        Task<Calculation> FindAsync(int id);
        Calculation GetCalculationByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        PurchaseOrderLineCharge GetLineFromFormCollection(FormCollection collection);
    }

    public class CalculationService : ICalculationService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Calculation> _CalculationRepository;
        RepositoryQuery<Calculation> CalculationRepository;

        public CalculationService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CalculationRepository = new Repository<Calculation>(db);
            CalculationRepository = new RepositoryQuery<Calculation>(_CalculationRepository);
        }
        public PurchaseOrderLineCharge GetLineFromFormCollection(FormCollection collection)
        {
            PurchaseOrderLineCharge temp = new PurchaseOrderLineCharge();

            return temp;
        }
        public Calculation GetCalculationByName(string terms)
        {
            return (from p in db.Calculation
                    where p.CalculationName == terms
                    select p).FirstOrDefault();
        }

        public Calculation Find(string Name)
        {
            return CalculationRepository.Get().Where(i => i.CalculationName == Name).FirstOrDefault();
        }
        //public IEnumerable<ChargeViewModel> GetCalculationFields(int CalculationId)
        //{
        //    List<ChargeViewModel> CalculationFields = new List<ChargeViewModel>();

        //    var temp1 = (from p in db.CalculationFooter
        //                 join t in db.Charge on p.ChargeId equals t.ChargeId into table
        //                 from tab in table.DefaultIfEmpty()
        //                 where p.CalculationId == CalculationId
        //                 select new ChargeViewModel
        //                 {
        //                     ChargeCode = tab.ChargeCode,
        //                     ChargeId = tab.ChargeId,
        //                     ChargeName = tab.ChargeName,
        //                 }).ToList();
        //    foreach (var item in temp1)
        //    {
        //        CalculationFields.Add(new ChargeViewModel
        //        {
        //            ChargeCode = item.ChargeCode,
        //            ChargeId = item.ChargeId,
        //            ChargeName = item.ChargeName,
        //            IsFooter=true
        //        });
        //    };

        //    var temp2 = (from p in db.CalculationProduct
        //                 join t in db.Charge on p.ChargeId equals t.ChargeId into table
        //                 from tab in table.DefaultIfEmpty()
        //                 where p.CalculationId == CalculationId
        //                 select new ChargeViewModel
        //                 {
        //                     ChargeCode = tab.ChargeCode,
        //                     ChargeId = tab.ChargeId,
        //                     ChargeName = tab.ChargeName,
        //                 }).ToList();

        //    foreach (var item in temp1)
        //    {
        //        CalculationFields.Add(new ChargeViewModel
        //        {
        //            ChargeCode = item.ChargeCode,
        //            ChargeId = item.ChargeId,
        //            ChargeName = item.ChargeName,
        //            IsFooter=false,
        //        });
        //    };

        //    return (CalculationFields);
        //}


        public Calculation Find(int id)
        {
            return _unitOfWork.Repository<Calculation>().Find(id);
        }

        public Calculation Create(Calculation pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Calculation>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Calculation>().Delete(id);
        }

        public void Delete(Calculation pt)
        {
            _unitOfWork.Repository<Calculation>().Delete(pt);
        }

        public void Update(Calculation pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Calculation>().Update(pt);
        }

        public IEnumerable<Calculation> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Calculation>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CalculationName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Calculation> GetCalculationList()
        {
            var pt = _unitOfWork.Repository<Calculation>().Query().Get().OrderBy(m=>m.CalculationName);
            return pt;
        }

        public Calculation Add(Calculation pt)
        {
            _unitOfWork.Repository<Calculation>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Calculation
                        orderby p.CalculationName
                        select p.CalculationId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Calculation
                        orderby p.CalculationName
                        select p.CalculationId).FirstOrDefault();
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
                temp = (from p in db.Calculation
                        orderby p.CalculationName
                        select p.CalculationId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Calculation
                        orderby p.CalculationName
                        select p.CalculationId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Calculation>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Calculation> FindAsync(int id)
        {
            throw new NotImplementedException();
        }


        public void LedgerPosting(ref LedgerHeaderViewModel LedgerHeaderViewModel, IEnumerable<CalculationHeaderCharge> HeaderTable, IEnumerable<CalculationLineCharge> LineTable)
        {
            int PersonAccountId = 6612;
            int LedgerHeaderId = 0;

            if (LedgerHeaderViewModel.LedgerHeaderId == 0)
            {
                LedgerHeader LedgerHeader = new LedgerHeader();

                LedgerHeader.DocHeaderId = LedgerHeaderViewModel.DocHeaderId;
                LedgerHeader.DocTypeId = LedgerHeaderViewModel.DocTypeId;
                LedgerHeader.ProcessId  = LedgerHeaderViewModel.ProcessId;
                LedgerHeader.DocDate = LedgerHeaderViewModel.DocDate;
                LedgerHeader.DocNo = LedgerHeaderViewModel.DocNo;
                LedgerHeader.DivisionId = LedgerHeaderViewModel.DivisionId;
                LedgerHeader.SiteId = LedgerHeaderViewModel.SiteId;
                LedgerHeader.Narration = LedgerHeaderViewModel.Narration;
                LedgerHeader.Remark = LedgerHeaderViewModel.Remark;
                LedgerHeader.CreatedBy = LedgerHeaderViewModel.CreatedBy;
                LedgerHeader.CreatedDate = DateTime.Now.Date;
                LedgerHeader.ModifiedBy = LedgerHeaderViewModel.ModifiedBy;
                LedgerHeader.ModifiedDate = DateTime.Now.Date;

                new LedgerHeaderService(_unitOfWork).Create(LedgerHeader);
            }
            else
            {
                LedgerHeader LedgerHeader = new LedgerHeaderService(_unitOfWork).Find((int)LedgerHeaderViewModel.LedgerHeaderId);

                LedgerHeader.DocHeaderId = LedgerHeaderViewModel.DocHeaderId;
                LedgerHeader.DocTypeId = LedgerHeaderViewModel.DocTypeId;
                LedgerHeader.ProcessId = LedgerHeaderViewModel.ProcessId;
                LedgerHeader.DocDate = LedgerHeaderViewModel.DocDate;
                LedgerHeader.DocNo = LedgerHeaderViewModel.DocNo;
                LedgerHeader.DivisionId = LedgerHeaderViewModel.DivisionId;
                LedgerHeader.SiteId = LedgerHeaderViewModel.SiteId;
                LedgerHeader.Narration = LedgerHeaderViewModel.Narration;
                LedgerHeader.Remark = LedgerHeaderViewModel.Remark;
                LedgerHeader.ModifiedBy = LedgerHeaderViewModel.ModifiedBy;
                LedgerHeader.ModifiedDate = DateTime.Now.Date;

                new LedgerHeaderService(_unitOfWork).Update(LedgerHeader);

                IEnumerable<Ledger> LedgerList = new LedgerService(_unitOfWork).FindForLedgerHeader(LedgerHeader.LedgerHeaderId);

                var LedgerAdjDrList = (from H in db.LedgerHeader
                                     join L in db.Ledger on H.LedgerHeaderId equals L.LedgerHeaderId into LedgerTable
                                     from LedgerTab in LedgerTable.DefaultIfEmpty()
                                     join La in db.LedgerAdj on LedgerTab.LedgerId equals La.DrLedgerId into LedgerAdjTable
                                     from LedgerAdjTab in LedgerAdjTable.DefaultIfEmpty()
                                       where H.LedgerHeaderId == LedgerHeader.LedgerHeaderId && LedgerAdjTab.LedgerAdjId != null
                                     select LedgerAdjTab).ToList();

                foreach (LedgerAdj item in LedgerAdjDrList)
                {
                    new LedgerAdjService(_unitOfWork).Delete(item.LedgerAdjId);
                }

                var LedgerAdjCrList = (from H in db.LedgerHeader
                                       join L in db.Ledger on H.LedgerHeaderId equals L.LedgerHeaderId into LedgerTable
                                       from LedgerTab in LedgerTable.DefaultIfEmpty()
                                       join La in db.LedgerAdj on LedgerTab.LedgerId equals La.CrLedgerId into LedgerAdjTable
                                       from LedgerAdjTab in LedgerAdjTable.DefaultIfEmpty()
                                       where H.LedgerHeaderId == LedgerHeader.LedgerHeaderId && LedgerAdjTab.LedgerAdjId != null
                                       select LedgerAdjTab).ToList();

                foreach (LedgerAdj item in LedgerAdjCrList)
                {
                    new LedgerAdjService(_unitOfWork).Delete(item.LedgerAdjId);
                }


                foreach (Ledger item in LedgerList)
                {
                    new LedgerService(_unitOfWork).Delete(item);
                }

                LedgerHeaderId = LedgerHeader.LedgerHeaderId;
            }


            IEnumerable<LedgerPostingViewModel> LedgerHeaderAmtDr = from H in HeaderTable
                                                                    join A in db.LedgerAccount on H.PersonID equals A.PersonId into LedgerAccountTable
                                                                    from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                                                                    where H.LedgerAccountDrId != null && H.Amount != 0 && H.Amount != null
                                                                    select new LedgerPostingViewModel
                                                                    {
                                                                        LedgerAccountId = (int)(H.LedgerAccountDrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : H.LedgerAccountDrId),
                                                                        ContraLedgerAccountId = (H.LedgerAccountCrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : H.LedgerAccountCrId),
                                                                        CostCenterId = H.CostCenterId,
                                                                        AmtDr = Math.Abs((H.Amount > 0 ? H.Amount : 0) ?? 0),
                                                                        AmtCr = Math.Abs((H.Amount < 0 ? H.Amount : 0) ?? 0)
                                                                    };

            IEnumerable<LedgerPostingViewModel> LedgerHeaderAmtCr = from H in HeaderTable
                                                                    join A in db.LedgerAccount on H.PersonID equals A.PersonId into LedgerAccountTable
                                                                    from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                                                                    where H.LedgerAccountCrId != null && H.Amount != 0 && H.Amount != null
                                                                    select new LedgerPostingViewModel
                                                                    {
                                                                        LedgerAccountId = (int)(H.LedgerAccountCrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : H.LedgerAccountCrId),
                                                                        ContraLedgerAccountId = (H.LedgerAccountDrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : H.LedgerAccountDrId),
                                                                        CostCenterId = H.CostCenterId,
                                                                        AmtCr = Math.Abs((H.Amount > 0 ? H.Amount : 0) ?? 0),
                                                                        AmtDr = Math.Abs((H.Amount < 0 ? H.Amount : 0) ?? 0)
                                                                    };

            IEnumerable<LedgerPostingViewModel> LedgerLineAmtDr = from L in LineTable
                                                                  join A in db.LedgerAccount on L.PersonID equals A.PersonId into LedgerAccountTable
                                                                  from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                                                                  where L.LedgerAccountDrId != null && L.Amount != 0 && L.Amount != null
                                                                  select new LedgerPostingViewModel
                                                                  {
                                                                      LedgerAccountId = (int)(L.LedgerAccountDrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : L.LedgerAccountDrId),
                                                                      ContraLedgerAccountId = (L.LedgerAccountCrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : L.LedgerAccountCrId),
                                                                      CostCenterId = L.CostCenterId,
                                                                      AmtDr = Math.Abs((L.Amount > 0 ? L.Amount : 0) ?? 0),
                                                                      AmtCr = Math.Abs((L.Amount < 0 ? L.Amount : 0) ?? 0)
                                                                  };

            IEnumerable<LedgerPostingViewModel> LedgerLineAmtCr = from L in LineTable
                                                                  join A in db.LedgerAccount on L.PersonID equals A.PersonId into LedgerAccountTable
                                                                  from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                                                                  where L.LedgerAccountCrId != null && L.Amount != 0 && L.Amount != null
                                                                  select new LedgerPostingViewModel
                                                                  {
                                                                      LedgerAccountId = (int)(L.LedgerAccountCrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : L.LedgerAccountCrId),
                                                                      ContraLedgerAccountId = (L.LedgerAccountDrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : L.LedgerAccountDrId),
                                                                      CostCenterId = L.CostCenterId,
                                                                      AmtCr = Math.Abs((L.Amount > 0 ? L.Amount : 0) ?? 0),
                                                                      AmtDr = Math.Abs((L.Amount < 0 ? L.Amount : 0) ?? 0)
                                                                  };


            IEnumerable<LedgerPostingViewModel> LedgerCombind = LedgerHeaderAmtDr.Union(LedgerHeaderAmtCr).Union(LedgerLineAmtDr).Union(LedgerLineAmtCr);

            //IEnumerable<LedgerPostingViewModel> LedgerPost = from L in LedgerCombind
            //                                                 group new { L } by new { L.LedgerAccountId, L.ContraLedgerAccountId, L.CostCenterId } into Result
            //                                                 select new LedgerPostingViewModel
            //                                                 {
            //                                                     LedgerAccountId = Result.Key.LedgerAccountId,
            //                                                     ContraLedgerAccountId = Result.Key.ContraLedgerAccountId,
            //                                                     CostCenterId = Result.Key.CostCenterId,
            //                                                     AmtDr = Result.Sum(i => i.L.AmtDr),
            //                                                     AmtCr = Result.Sum(i => i.L.AmtCr)
            //                                                 };


            IEnumerable<LedgerPostingViewModel> LedgerPost1 = from L in LedgerCombind
                                                             join Ca in db.LedgerAccount on L.ContraLedgerAccountId equals Ca.LedgerAccountId into ContraLedgerAccountTable
                                                             from ContraLedgerAccountTab in ContraLedgerAccountTable.DefaultIfEmpty()
                                                              join Cag in db.LedgerAccountGroup on ContraLedgerAccountTab.LedgerAccountGroupId equals Cag.LedgerAccountGroupId into ContraLedgerAccountGroupTable
                                                              from ContraLedgerAccountGroupTab in ContraLedgerAccountGroupTable.DefaultIfEmpty()
                                                              group new { L, ContraLedgerAccountGroupTab } by new { L.LedgerAccountId, L.ContraLedgerAccountId, L.CostCenterId } into Result
                                                             select new LedgerPostingViewModel
                                                             {
                                                                 LedgerAccountId = Result.Key.LedgerAccountId,
                                                                 ContraLedgerAccountId = Result.Key.ContraLedgerAccountId,
                                                                 ContraLedgerAccountWeightage = Result.Max(i => i.ContraLedgerAccountGroupTab.Weightage ?? 0),
                                                                 CostCenterId = Result.Key.CostCenterId,
                                                                 AmtDr = Result.Sum(i => i.L.AmtDr),
                                                                 AmtCr = Result.Sum(i => i.L.AmtCr)
                                                             };


            IEnumerable<LedgerPostingViewModel> LedgerPost2 = from L in LedgerPost1
                                                              group new { L } by new { L.LedgerAccountId, L.CostCenterId } into Result
                                                              select new LedgerPostingViewModel
                                                              {
                                                                  LedgerAccountId = Result.Key.LedgerAccountId,
                                                                  ContraLedgerAccountWeightage = Result.Max(i => i.L.ContraLedgerAccountWeightage ?? 0),
                                                                  CostCenterId = Result.Key.CostCenterId,
                                                                  AmtDr = Result.Sum(i => i.L.AmtDr) - Result.Sum(i => i.L.AmtCr) > 0 ? Result.Sum(i => i.L.AmtDr) - Result.Sum(i => i.L.AmtCr) : 0,
                                                                  AmtCr = Result.Sum(i => i.L.AmtCr) - Result.Sum(i => i.L.AmtDr) > 0 ? Result.Sum(i => i.L.AmtCr) - Result.Sum(i => i.L.AmtDr) : 0,
                                                              };

            IEnumerable<LedgerPostingViewModel> LedgerPost = from L2 in LedgerPost2
                                                             join L1 in LedgerPost1 on new { A1 = L2.LedgerAccountId, A2 = L2.CostCenterId, A3 = L2.ContraLedgerAccountWeightage } equals new { A1 = L1.LedgerAccountId, A2 = L1.CostCenterId, A3 = L1.ContraLedgerAccountWeightage } into LedgerPost1Table
                                                             from LedgerPost1Tab in LedgerPost1Table.DefaultIfEmpty()
                                                             group new { LedgerPost1Tab, L2 } by new { L2.LedgerAccountId, L2.CostCenterId } into Result
                                                             select new LedgerPostingViewModel
                                                             {
                                                                 LedgerAccountId = Result.Key.LedgerAccountId,
                                                                 ContraLedgerAccountId = Result.Max(i => i.LedgerPost1Tab.ContraLedgerAccountId),
                                                                 CostCenterId = Result.Key.CostCenterId,
                                                                 AmtDr = Result.Max(i => i.L2.AmtDr),
                                                                 AmtCr = Result.Max(i => i.L2.AmtCr)

                                                             };


            var temp = (from L in LedgerPost
                        group L by 1 into Result
                        select new
                        {
                            AmtDr = Result.Sum(i => i.AmtDr),
                            AmtCr = Result.Sum(i => i.AmtCr)
                        }).FirstOrDefault();


            if (temp != null)
            { 
                if (temp.AmtDr != temp.AmtCr)
                {
                    throw new Exception("Debit amount and credit amount is not equal.Check account posting.");
                }
            }



            foreach (LedgerPostingViewModel item in LedgerPost)
            {
                Ledger Ledger = new Ledger();

                if (LedgerHeaderId != 0)
                {
                    Ledger.LedgerHeaderId = LedgerHeaderId;
                }
                Ledger.LedgerAccountId = item.LedgerAccountId;
                Ledger.ContraLedgerAccountId = item.ContraLedgerAccountId;


                var TempCostCenter = (from C in db.CostCenter
                                      where C.CostCenterId == item.CostCenterId && C.LedgerAccountId == item.LedgerAccountId
                                      select new { CostCenterId = C.CostCenterId }).FirstOrDefault();

                if (TempCostCenter != null)
                {
                    Ledger.CostCenterId = item.CostCenterId;
                }




                
                Ledger.AmtDr = item.AmtDr * (LedgerHeaderViewModel.ExchangeRate ?? 1);
                Ledger.AmtCr = item.AmtCr * (LedgerHeaderViewModel.ExchangeRate ?? 1);
                Ledger.Narration = "";

                new LedgerService(_unitOfWork).Create(Ledger);
            }
        }


        public void LedgerPostingDB(ref LedgerHeaderViewModel LedgerHeaderViewModel, IEnumerable<CalculationHeaderCharge> HeaderTable, IEnumerable<CalculationLineCharge> LineTable,ref ApplicationDbContext Context)
        {
            int PersonAccountId = 6612;
            int LedgerHeaderId = 0;

            if (LedgerHeaderViewModel.LedgerHeaderId == 0)
            {
                LedgerHeader LedgerHeader = new LedgerHeader();

                LedgerHeader.DocHeaderId = LedgerHeaderViewModel.DocHeaderId;
                LedgerHeader.DocTypeId = LedgerHeaderViewModel.DocTypeId;
                LedgerHeader.DocDate = LedgerHeaderViewModel.DocDate;
                LedgerHeader.DocNo = LedgerHeaderViewModel.DocNo;
                LedgerHeader.DivisionId = LedgerHeaderViewModel.DivisionId;
                LedgerHeader.SiteId = LedgerHeaderViewModel.SiteId;
                LedgerHeader.PartyDocNo = LedgerHeaderViewModel.PartyDocNo;
                LedgerHeader.PartyDocDate = LedgerHeaderViewModel.PartyDocDate;
                LedgerHeader.Narration = LedgerHeaderViewModel.Narration;
                LedgerHeader.Remark = LedgerHeaderViewModel.Remark;
                LedgerHeader.CreatedBy = LedgerHeaderViewModel.CreatedBy;
                LedgerHeader.ProcessId = LedgerHeaderViewModel.ProcessId;
                LedgerHeader.CreatedDate = DateTime.Now.Date;
                LedgerHeader.ModifiedBy = LedgerHeaderViewModel.ModifiedBy;
                LedgerHeader.ModifiedDate = DateTime.Now.Date;
                LedgerHeader.ObjectState = Model.ObjectState.Added;
                Context.LedgerHeader.Add(LedgerHeader);
                //new LedgerHeaderService(_unitOfWork).Create(LedgerHeader);
            }
            else
            {

                int LedHeadId = LedgerHeaderViewModel.LedgerHeaderId;
                LedgerHeader LedgerHeader = (from p in Context.LedgerHeader
                                             where p.LedgerHeaderId == LedHeadId
                                             select p).FirstOrDefault();

                LedgerHeader.DocHeaderId = LedgerHeaderViewModel.DocHeaderId;
                LedgerHeader.DocTypeId = LedgerHeaderViewModel.DocTypeId;
                LedgerHeader.DocDate = LedgerHeaderViewModel.DocDate;
                LedgerHeader.DocNo = LedgerHeaderViewModel.DocNo;
                LedgerHeader.DivisionId = LedgerHeaderViewModel.DivisionId;
                LedgerHeader.ProcessId = LedgerHeaderViewModel.ProcessId;
                LedgerHeader.SiteId = LedgerHeaderViewModel.SiteId;
                LedgerHeader.PartyDocNo = LedgerHeaderViewModel.PartyDocNo;
                LedgerHeader.PartyDocDate = LedgerHeaderViewModel.PartyDocDate;
                LedgerHeader.Narration = LedgerHeaderViewModel.Narration;
                LedgerHeader.Remark = LedgerHeaderViewModel.Remark;
                LedgerHeader.ModifiedBy = LedgerHeaderViewModel.ModifiedBy;
                LedgerHeader.ModifiedDate = DateTime.Now.Date;
                LedgerHeader.ObjectState = Model.ObjectState.Modified;
                Context.LedgerHeader.Add(LedgerHeader);
                //new LedgerHeaderService(_unitOfWork).Update(LedgerHeader);

                IEnumerable<Ledger> LedgerList = (from p in Context.Ledger
                                                  where p.LedgerHeaderId == LedHeadId
                                                  select p).ToList();
                foreach (Ledger item in LedgerList)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    Context.Ledger.Remove(item);
                    //new LedgerService(_unitOfWork).Delete(item);
                }

                LedgerHeaderId = LedgerHeader.LedgerHeaderId;
            }


            IEnumerable<LedgerPostingViewModel> LedgerHeaderAmtDr = (from H in HeaderTable
                                                                    join A in Context.LedgerAccount on H.PersonID equals A.PersonId into LedgerAccountTable
                                                                    from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                                                                    where H.LedgerAccountDrId != null && H.Amount != 0 && H.Amount != null
                                                                    select new LedgerPostingViewModel
                                                                    {
                                                                        LedgerAccountId = (int)(H.LedgerAccountDrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : H.LedgerAccountDrId),
                                                                        ContraLedgerAccountId = (H.LedgerAccountCrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : H.LedgerAccountCrId),
                                                                        CostCenterId = H.CostCenterId,
                                                                        AmtDr = Math.Abs((H.Amount > 0 ? H.Amount : 0) ?? 0),
                                                                        AmtCr = Math.Abs((H.Amount < 0 ? H.Amount : 0) ?? 0)
                                                                    }).ToList();

            IEnumerable<LedgerPostingViewModel> LedgerHeaderAmtCr = (from H in HeaderTable
                                                                    join A in Context.LedgerAccount on H.PersonID equals A.PersonId into LedgerAccountTable
                                                                    from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                                                                    where H.LedgerAccountCrId != null && H.Amount != 0 && H.Amount != null
                                                                    select new LedgerPostingViewModel
                                                                    {
                                                                        LedgerAccountId = (int)(H.LedgerAccountCrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : H.LedgerAccountCrId),
                                                                        ContraLedgerAccountId = (H.LedgerAccountDrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : H.LedgerAccountDrId),
                                                                        CostCenterId = H.CostCenterId,
                                                                        AmtCr = Math.Abs((H.Amount > 0 ? H.Amount : 0) ?? 0),
                                                                        AmtDr = Math.Abs((H.Amount < 0 ? H.Amount : 0) ?? 0)
                                                                    }).ToList();

            IEnumerable<LedgerPostingViewModel> LedgerLineAmtDr = (from L in LineTable
                                                                  join A in Context.LedgerAccount on L.PersonID equals A.PersonId into LedgerAccountTable
                                                                  from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                                                                  where L.LedgerAccountDrId != null && L.Amount != 0 && L.Amount != null
                                                                  select new LedgerPostingViewModel
                                                                  {
                                                                      LedgerAccountId = (int)(L.LedgerAccountDrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : L.LedgerAccountDrId),
                                                                      ContraLedgerAccountId = (L.LedgerAccountCrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : L.LedgerAccountCrId),
                                                                      CostCenterId = L.CostCenterId,
                                                                      AmtDr = Math.Abs((L.Amount > 0 ? L.Amount : 0) ?? 0),
                                                                      AmtCr = Math.Abs((L.Amount < 0 ? L.Amount : 0) ?? 0)
                                                                  }).ToList();

            IEnumerable<LedgerPostingViewModel> LedgerLineAmtCr = (from L in LineTable
                                                                  join A in Context.LedgerAccount on L.PersonID equals A.PersonId into LedgerAccountTable
                                                                  from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                                                                  where L.LedgerAccountCrId != null && L.Amount != 0 && L.Amount != null
                                                                  select new LedgerPostingViewModel
                                                                  {
                                                                      LedgerAccountId = (int)(L.LedgerAccountCrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : L.LedgerAccountCrId),
                                                                      ContraLedgerAccountId = (L.LedgerAccountDrId == PersonAccountId ? LedgerAccountTab.LedgerAccountId : L.LedgerAccountDrId),
                                                                      CostCenterId = L.CostCenterId,
                                                                      AmtCr = Math.Abs((L.Amount > 0 ? L.Amount : 0) ?? 0),
                                                                      AmtDr = Math.Abs((L.Amount < 0 ? L.Amount : 0) ?? 0)
                                                                  }).ToList();


            IEnumerable<LedgerPostingViewModel> LedgerCombind = LedgerHeaderAmtDr.Union(LedgerHeaderAmtCr).Union(LedgerLineAmtDr).Union(LedgerLineAmtCr).ToList();

            //IEnumerable<LedgerPostingViewModel> LedgerPost = (from L in LedgerCombind
            //                                                 group new { L } by new { L.LedgerAccountId, L.ContraLedgerAccountId, L.CostCenterId } into Result
            //                                                 select new LedgerPostingViewModel
            //                                                 {
            //                                                     LedgerAccountId = Result.Key.LedgerAccountId,
            //                                                     ContraLedgerAccountId = Result.Key.ContraLedgerAccountId,
            //                                                     CostCenterId = Result.Key.CostCenterId,
            //                                                     AmtDr = Result.Sum(i => i.L.AmtDr),
            //                                                     AmtCr = Result.Sum(i => i.L.AmtCr)
            //                                                 }).ToList();



            IEnumerable<LedgerPostingViewModel> LedgerPost1 = from L in LedgerCombind
                                                              join Ca in db.LedgerAccount on L.ContraLedgerAccountId equals Ca.LedgerAccountId into ContraLedgerAccountTable
                                                              from ContraLedgerAccountTab in ContraLedgerAccountTable.DefaultIfEmpty()
                                                              join Cag in db.LedgerAccountGroup on ContraLedgerAccountTab.LedgerAccountGroupId equals Cag.LedgerAccountGroupId into ContraLedgerAccountGroupTable
                                                              from ContraLedgerAccountGroupTab in ContraLedgerAccountGroupTable.DefaultIfEmpty()
                                                              group new { L, ContraLedgerAccountGroupTab } by new { L.LedgerAccountId, L.ContraLedgerAccountId, L.CostCenterId } into Result
                                                              select new LedgerPostingViewModel
                                                              {
                                                                  LedgerAccountId = Result.Key.LedgerAccountId,
                                                                  ContraLedgerAccountId = Result.Key.ContraLedgerAccountId,
                                                                  ContraLedgerAccountWeightage = Result.Max(i => i.ContraLedgerAccountGroupTab.Weightage ?? 0),
                                                                  CostCenterId = Result.Key.CostCenterId,
                                                                  AmtDr = Result.Sum(i => i.L.AmtDr),
                                                                  AmtCr = Result.Sum(i => i.L.AmtCr)
                                                              };


            IEnumerable<LedgerPostingViewModel> LedgerPost2 = from L in LedgerPost1
                                                              group new { L } by new { L.LedgerAccountId, L.CostCenterId } into Result
                                                              select new LedgerPostingViewModel
                                                              {
                                                                  LedgerAccountId = Result.Key.LedgerAccountId,
                                                                  ContraLedgerAccountWeightage = Result.Max(i => i.L.ContraLedgerAccountWeightage ?? 0),
                                                                  CostCenterId = Result.Key.CostCenterId,
                                                                  AmtDr = Result.Sum(i => i.L.AmtDr) - Result.Sum(i => i.L.AmtCr) > 0 ? Result.Sum(i => i.L.AmtDr) - Result.Sum(i => i.L.AmtCr) : 0,
                                                                  AmtCr = Result.Sum(i => i.L.AmtCr) - Result.Sum(i => i.L.AmtDr) > 0 ? Result.Sum(i => i.L.AmtCr) - Result.Sum(i => i.L.AmtDr) : 0,
                                                              };

            IEnumerable<LedgerPostingViewModel> LedgerPost = from L2 in LedgerPost2
                                                             join L1 in LedgerPost1 on new { A1 = L2.LedgerAccountId, A2 = L2.CostCenterId, A3 = L2.ContraLedgerAccountWeightage } equals new { A1 = L1.LedgerAccountId, A2 = L1.CostCenterId, A3 = L1.ContraLedgerAccountWeightage } into LedgerPost1Table
                                                             from LedgerPost1Tab in LedgerPost1Table.DefaultIfEmpty()
                                                             group new { LedgerPost1Tab, L2 } by new { L2.LedgerAccountId, L2.CostCenterId } into Result
                                                             select new LedgerPostingViewModel
                                                             {
                                                                 LedgerAccountId = Result.Key.LedgerAccountId,
                                                                 ContraLedgerAccountId = Result.Max(i => i.LedgerPost1Tab.ContraLedgerAccountId),
                                                                 CostCenterId = Result.Key.CostCenterId,
                                                                 AmtDr = Result.Max(i => i.L2.AmtDr),
                                                                 AmtCr = Result.Max(i => i.L2.AmtCr)

                                                             };




            var temp = (from L in LedgerPost
                        group L by 1 into Result
                        select new
                        {
                            AmtDr = Result.Sum(i => i.AmtDr),
                            AmtCr = Result.Sum(i => i.AmtCr)
                        }).FirstOrDefault();


            if (temp != null)
            {
                if (temp.AmtDr != temp.AmtCr)
                {
                    throw new Exception("Debit amount and credit amount is not equal.Check account posting.");
                }
            }



            foreach (LedgerPostingViewModel item in LedgerPost)
            {
                Ledger Ledger = new Ledger();

                if (LedgerHeaderId != 0)
                {
                    Ledger.LedgerHeaderId = LedgerHeaderId;
                }
                Ledger.LedgerAccountId = item.LedgerAccountId;
                Ledger.ContraLedgerAccountId = item.ContraLedgerAccountId;


                var TempCostCenter = (from C in Context.CostCenter
                                      where C.CostCenterId == item.CostCenterId && C.LedgerAccountId == item.LedgerAccountId
                                      select new { CostCenterId = C.CostCenterId }).FirstOrDefault();

                if (TempCostCenter != null)
                {
                    Ledger.CostCenterId = item.CostCenterId;
                }





                Ledger.AmtDr = item.AmtDr * (LedgerHeaderViewModel.ExchangeRate ?? 1);
                Ledger.AmtCr = item.AmtCr * (LedgerHeaderViewModel.ExchangeRate ?? 1);
                Ledger.Narration = "";

                //new LedgerService(_unitOfWork).Create(Ledger);
                Ledger.ObjectState = Model.ObjectState.Added;
                Context.Ledger.Add(Ledger);
            }
        }













        

    }
}
