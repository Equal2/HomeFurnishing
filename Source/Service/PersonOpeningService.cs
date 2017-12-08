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
    public interface IPersonOpeningService : IDisposable
    {
        PersonOpeningViewModel GetPersonOpeningForEdit(int id);
        IEnumerable<PersonOpeningViewModel> GetPersonOpeningListForIndex(int personId);

    }

    public class PersonOpeningService : IPersonOpeningService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public PersonOpeningService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PersonOpeningViewModel GetPersonOpeningForEdit(int id)
        {
            PersonOpeningViewModel vm = (from H in db.LedgerHeader
                                         join L in db.Ledger on H.LedgerHeaderId equals L.LedgerHeaderId into LedgerTable
                                         from LedgerTab in LedgerTable.DefaultIfEmpty()
                                         where H.LedgerHeaderId == id
                                         select new PersonOpeningViewModel
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
                                             PersonId = LedgerTab.LedgerAccount.PersonId ?? 0,
                                             DivisionId = H.DivisionId,
                                             SiteId = H.SiteId
                                         }).FirstOrDefault();
            return vm;           
        }
        public IEnumerable<PersonOpeningViewModel> GetPersonOpeningListForIndex(int personId)
        {
            int OpeningDocCategoryId = new DocumentCategoryService(_unitOfWork).FindByName(TransactionDocCategoryConstants.Opening).DocumentCategoryId;

            IEnumerable<PersonOpeningViewModel> vm = (from H in db.LedgerHeader
                                                      join L in db.Ledger on H.LedgerHeaderId equals L.LedgerHeaderId into LedgerTable
                                                      from LedgerTab in LedgerTable.DefaultIfEmpty()
                                                      where LedgerTab.LedgerAccount.PersonId == personId && H.DocType.DocumentCategoryId == OpeningDocCategoryId
                                                      select new PersonOpeningViewModel
                                                      {
                                                          LedgerHeaderId = H.LedgerHeaderId,
                                                          DocDate = H.DocDate,
                                                          DocNo = H.DocNo,
                                                          PartyDocNo = H.PartyDocNo,
                                                          PartyDocDate = H.PartyDocDate ?? H.DocDate,
                                                          Amount = LedgerTab.AmtDr != 0 ? LedgerTab.AmtDr : LedgerTab.AmtCr ,
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