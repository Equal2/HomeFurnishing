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


namespace Service
{
    public interface ILedgerAccountOpeningService : IDisposable
    {
        LedgerAccountOpeningViewModel GetLedgerAccountOpeningForEdit(int id);
        IEnumerable<LedgerAccountOpeningViewModel> GetLedgerAccountOpeningListForIndex(int LedgerAccountId);

    }

    public class LedgerAccountOpeningService : ILedgerAccountOpeningService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public LedgerAccountOpeningService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public LedgerAccountOpeningViewModel GetLedgerAccountOpeningForEdit(int id)
        {
            LedgerAccountOpeningViewModel vm = (from H in db.LedgerHeader
                                         join L in db.Ledger on H.LedgerHeaderId equals L.LedgerHeaderId into LedgerTable
                                         from LedgerTab in LedgerTable.DefaultIfEmpty()
                                         where H.LedgerHeaderId == id
                                         select new LedgerAccountOpeningViewModel
                                         {
                                             LedgerHeaderId = H.LedgerHeaderId,
                                             DocTypeId = H.DocTypeId,
                                             DocDate = H.DocDate,
                                             DocNo = H.DocNo,
                                             PartyDocNo = H.PartyDocNo,
                                             PartyDocDate = H.PartyDocDate,
                                             Amount = LedgerTab.AmtDr != 0 ? LedgerTab.AmtDr : LedgerTab.AmtCr,
                                             DrCr = LedgerTab.AmtDr != 0 ? NatureConstants.Debit : NatureConstants.Credit,
                                             Narration = H.Narration,
                                             LedgerAccountId = LedgerTab.LedgerAccountId,
                                             DivisionId = H.DivisionId,
                                             SiteId = H.SiteId
                                         }).FirstOrDefault();
            return vm;           
        }
        public IEnumerable<LedgerAccountOpeningViewModel> GetLedgerAccountOpeningListForIndex(int LedgerAccountId)
        {
            int OpeningDocCategoryId = new DocumentCategoryService(_unitOfWork).FindByName(TransactionDocCategoryConstants.Opening).DocumentCategoryId;

            IEnumerable<LedgerAccountOpeningViewModel> vm = (from H in db.LedgerHeader
                                                             join L in db.Ledger on H.LedgerHeaderId equals L.LedgerHeaderId into LedgerTable
                                                             from LedgerTab in LedgerTable.DefaultIfEmpty()
                                                             where LedgerTab.LedgerAccountId == LedgerAccountId && H.DocType.DocumentCategoryId == OpeningDocCategoryId
                                                             select new LedgerAccountOpeningViewModel
                                                             {
                                                                 LedgerHeaderId = H.LedgerHeaderId,
                                                                 DocDate = H.DocDate,
                                                                 DocNo = H.DocNo,
                                                                 PartyDocNo = H.PartyDocNo,
                                                                 PartyDocDate = H.PartyDocDate ?? H.DocDate,
                                                                 Amount = LedgerTab.AmtDr != 0 ? LedgerTab.AmtDr : LedgerTab.AmtCr,
                                                                 DrCr = LedgerTab.AmtDr != 0 ? NatureConstants.Debit : NatureConstants.Credit,
                                                                 SiteName = H.Site.SiteName,
                                                                 DivisionName = H.Division.DivisionName,
                                                                 Narration = H.Narration
                                                             }).ToList();
            return vm;
        }

        public void Dispose()
        {
        }
        



    }
}