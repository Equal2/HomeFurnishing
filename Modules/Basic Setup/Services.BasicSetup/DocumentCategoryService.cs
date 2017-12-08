using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.Company.Models;
using Infrastructure.IO;
using Models.BasicSetup.ViewModels;

namespace Services.BasicSetup
{
    public interface IDocumentCategoryService : IDisposable
    {
        DocumentCategory Create(DocumentCategory pt);
        void Delete(int id);
        void Delete(DocumentCategory pt);
        IEnumerable<DocumentCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DocumentCategory pt);
        DocumentCategory Add(DocumentCategory pt);
        IEnumerable<DocumentCategory> GetDocumentCategoryList();
        Task<IEquatable<DocumentCategory>> GetAsync();
        Task<DocumentCategory> FindAsync(int id);
        DocumentCategory Find(int id);
        int NextId(int id);
        int PrevId(int id);
        #region HelpList Getter
        /// <summary>
        /// *General Function*
        /// This function will create the help list for Projects
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum);
        #endregion

        #region HelpList Setters
        /// <summary>
        /// *General Function*
        /// This function will return the object in (Id,Text) format based on the Id
        /// </summary>
        /// <param name="Id">Primarykey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult GetValue(int Id);

        /// <summary>
        /// *General Function*
        /// This function will return list of object in (Id,Text) format based on the Ids
        /// </summary>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>List<ComboBoxResult></returns>
        List<ComboBoxResult> GetListCsv(string Id);
        #endregion
    }

    public class DocumentCategoryService : IDocumentCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<DocumentCategory> _documentCategoryRepository;

        public DocumentCategoryService(IUnitOfWork unitOfWork, IRepository<DocumentCategory> DocCategoryRepo)
        {
            _unitOfWork = unitOfWork;
            _documentCategoryRepository = DocCategoryRepo;
        }
        public DocumentCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _documentCategoryRepository = unitOfWork.Repository<DocumentCategory>();
        }
        public DocumentCategory Create(DocumentCategory pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DocumentCategory>().Insert(pt);
            return pt;
        }

        public DocumentCategory Find(int id)
        {
            return _unitOfWork.Repository<DocumentCategory>().Find(id);
        }

        public DocumentCategory FindByName(string DocumentCategoryName)
        {
            return _unitOfWork.Repository<DocumentCategory>().Query().Get().Where(i => i.DocumentCategoryName == DocumentCategoryName).FirstOrDefault();
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DocumentCategory>().Delete(id);
        }

        public void Delete(DocumentCategory pt)
        {
            _unitOfWork.Repository<DocumentCategory>().Delete(pt);
        }

        public void Update(DocumentCategory pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DocumentCategory>().Update(pt);
        }

        public IEnumerable<DocumentCategory> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DocumentCategory>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocumentCategoryName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<DocumentCategory> GetDocumentCategoryList()
        {
            var pt = _unitOfWork.Repository<DocumentCategory>().Query().Get().OrderBy(m => m.DocumentCategoryName);

            return pt;
        }


        public DocumentCategory Add(DocumentCategory pt)
        {
            _unitOfWork.Repository<DocumentCategory>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _documentCategoryRepository.Instance
                        orderby p.DocumentCategoryName
                        select p.DocumentCategoryId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _documentCategoryRepository.Instance
                        orderby p.DocumentCategoryName
                        select p.DocumentCategoryId).FirstOrDefault();
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

                temp = (from p in _documentCategoryRepository.Instance
                        orderby p.DocumentCategoryName
                        select p.DocumentCategoryId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _documentCategoryRepository.Instance
                        orderby p.DocumentCategoryName
                        select p.DocumentCategoryId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _documentCategoryRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.DocumentCategoryName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.DocumentCategoryName
                        select new ComboBoxResult
                        {
                            text = pr.DocumentCategoryName,
                            id = pr.DocumentCategoryId.ToString()
                        }
              );

            var temp = list
               .Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = list.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return Data;
        }

        public ComboBoxResult GetValue(int Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<DocumentCategory> DocumentCategorys = from pr in _documentCategoryRepository.Instance
                                      where pr.DocumentCategoryId == Id
                                      select pr;

            ProductJson.id = DocumentCategorys.FirstOrDefault().DocumentCategoryId.ToString();
            ProductJson.text = DocumentCategorys.FirstOrDefault().DocumentCategoryName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<DocumentCategory> DocumentCategorys = from pr in _documentCategoryRepository.Instance
                                          where pr.DocumentCategoryId == temp
                                          select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = DocumentCategorys.FirstOrDefault().DocumentCategoryId.ToString(),
                    text = DocumentCategorys.FirstOrDefault().DocumentCategoryName
                });
            }
            return ProductJson;
        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<DocumentCategory>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DocumentCategory> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
