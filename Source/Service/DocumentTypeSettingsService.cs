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

namespace Service
{
    public interface IDocumentTypeSettingsService : IDisposable
    {
        DocumentTypeSettings Create(DocumentTypeSettings pt);
        void Delete(int id);
        void Delete(DocumentTypeSettings pt);
        DocumentTypeSettings Find(int id);
        IEnumerable<DocumentTypeSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DocumentTypeSettings pt);
        DocumentTypeSettings Add(DocumentTypeSettings pt);
        DocumentTypeSettingsViewModel GetDocumentTypeSettingsForDocument(int DocTypeId);
        IEnumerable<DocumentTypeSettingsViewModel> GetDocumentTypeSettingsList();
        Task<IEquatable<DocumentTypeSettings>> GetAsync();
        Task<DocumentTypeSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
    }

    public class DocumentTypeSettingsService : IDocumentTypeSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<DocumentTypeSettings> _DocumentTypeSettingsRepository;
        RepositoryQuery<DocumentTypeSettings> DocumentTypeSettingsRepository;
        public DocumentTypeSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DocumentTypeSettingsRepository = new Repository<DocumentTypeSettings>(db);
            DocumentTypeSettingsRepository = new RepositoryQuery<DocumentTypeSettings>(_DocumentTypeSettingsRepository);
        }

        public DocumentTypeSettings Find(int id)
        {
            return _unitOfWork.Repository<DocumentTypeSettings>().Find(id);
        }

        public DocumentTypeSettingsViewModel GetDocumentTypeSettingsForDocument(int DocTypeId)
        {
            return (from D in db.DocumentType
                    join p in db.DocumentTypeSettings on D.DocumentTypeId equals p.DocumentTypeId into DocumentTypeSettingsTable
                    from DocumentTypeSettingsTab in DocumentTypeSettingsTable.DefaultIfEmpty()
                    where D.DocumentTypeId == DocTypeId
                    select new DocumentTypeSettingsViewModel
                    {
                        DocumentTypeId = D.DocumentTypeId,
                        PartyCaption = DocumentTypeSettingsTab.PartyCaption,
                        ProductUidCaption = DocumentTypeSettingsTab.ProductUidCaption,
                        ProductCaption = DocumentTypeSettingsTab.ProductCaption,
                        ProductGroupCaption = DocumentTypeSettingsTab.ProductGroupCaption,
                        ProductCategoryCaption = DocumentTypeSettingsTab.ProductCategoryCaption,
                        Dimension1Caption = DocumentTypeSettingsTab.Dimension1Caption,
                        Dimension2Caption = DocumentTypeSettingsTab.Dimension2Caption,
                        Dimension3Caption = DocumentTypeSettingsTab.Dimension3Caption,
                        Dimension4Caption = DocumentTypeSettingsTab.Dimension4Caption,
                        ContraDocTypeCaption = DocumentTypeSettingsTab.ContraDocTypeCaption,
                        DealQtyCaption = DocumentTypeSettingsTab.DealQtyCaption,
                        WeightCaption = DocumentTypeSettingsTab.WeightCaption,
                        CostCenterCaption = DocumentTypeSettingsTab.CostCenterCaption,
                        SpecificationCaption = DocumentTypeSettingsTab.SpecificationCaption,
                        ReferenceDocTypeCaption = DocumentTypeSettingsTab.ReferenceDocTypeCaption,
                        ReferenceDocIdCaption = DocumentTypeSettingsTab.ReferenceDocIdCaption
                    }).FirstOrDefault();
        }
        public DocumentTypeSettings Create(DocumentTypeSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DocumentTypeSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DocumentTypeSettings>().Delete(id);
        }

        public void Delete(DocumentTypeSettings pt)
        {
            _unitOfWork.Repository<DocumentTypeSettings>().Delete(pt);
        }

        public void Update(DocumentTypeSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DocumentTypeSettings>().Update(pt);
        }

        public IEnumerable<DocumentTypeSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DocumentTypeSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocumentTypeSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<DocumentTypeSettingsViewModel> GetDocumentTypeSettingsList()
        {
            var pt = (from D in db.DocumentType
                      join p in db.DocumentTypeSettings on D.DocumentTypeId equals p.DocumentTypeId into DocumentTypeSettingsTable
                      from DocumentTypeSettingsTab in DocumentTypeSettingsTable.DefaultIfEmpty()
                      orderby DocumentTypeSettingsTab.DocumentTypeSettingsId
                      select new DocumentTypeSettingsViewModel
                      {
                          DocumentTypeId = D.DocumentTypeId,
                          DocumentTypeSettingsId = DocumentTypeSettingsTab.DocumentTypeSettingsId,
                          ProductCaption = DocumentTypeSettingsTab.ProductCaption,
                          ProductGroupCaption = DocumentTypeSettingsTab.ProductGroupCaption,
                          ProductCategoryCaption = DocumentTypeSettingsTab.ProductCategoryCaption,
                          Dimension1Caption = DocumentTypeSettingsTab.Dimension1Caption,
                          Dimension2Caption = DocumentTypeSettingsTab.Dimension2Caption,
                          Dimension3Caption = DocumentTypeSettingsTab.Dimension3Caption,
                          Dimension4Caption = DocumentTypeSettingsTab.Dimension4Caption,
                      }).ToList();

            return pt;
        }

        public DocumentTypeSettings Add(DocumentTypeSettings pt)
        {
            _unitOfWork.Repository<DocumentTypeSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.DocumentTypeSettings
                        orderby p.DocumentTypeSettingsId
                        select p.DocumentTypeSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DocumentTypeSettings
                        orderby p.DocumentTypeSettingsId
                        select p.DocumentTypeSettingsId).FirstOrDefault();
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

                temp = (from p in db.DocumentTypeSettings
                        orderby p.DocumentTypeSettingsId
                        select p.DocumentTypeSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DocumentTypeSettings
                        orderby p.DocumentTypeSettingsId
                        select p.DocumentTypeSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }



        public void Dispose()
        {
        }


        public Task<IEquatable<DocumentTypeSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DocumentTypeSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
