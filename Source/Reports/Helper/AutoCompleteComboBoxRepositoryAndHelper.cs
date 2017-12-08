
using Data.Models;
using Model.Models;
using Model.ViewModels;
using Reports.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Service;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System;
using Core.Common;

namespace Reports.Presentation.Helper
{ 
    public class AutoCompleteComboBoxRepositoryAndHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private string _productCacheKeyHint;
        private IComboHelpListService cbl = new ComboHelpListService();
     
        public IQueryable<ComboBoxList> ResultList { get; set; }

        public AutoCompleteComboBoxRepositoryAndHelper()
        {
           
        }

        public AutoCompleteComboBoxRepositoryAndHelper(string productCacheKeyHint)
        {
            // TODO: Complete member initialization
            _productCacheKeyHint = productCacheKeyHint;
            ResultList = GenerateAndCacheData(_productCacheKeyHint);
        }

        public AutoCompleteComboBoxRepositoryAndHelper(IEnumerable<ComboBoxList> HelpList, string CacheKey, Boolean Refresh)
        {
            // TODO: Complete member initialization
            _productCacheKeyHint = CacheKey;

            if (HttpContext.Current.Cache[CacheKey] == null || Refresh == true)
            {
                HttpContext.Current.Cache[CacheKey] = HelpList.ToList().AsQueryable();
            }

            ResultList = (IQueryable<ComboBoxList>)HttpContext.Current.Cache[CacheKey];
        }

        public ComboBoxPagedResult TranslateToComboBoxFormat(List<ComboBoxList> listItems, int itemCount)
        {
            ComboBoxPagedResult jsonComboBoxList = new ComboBoxPagedResult();
            jsonComboBoxList.Results = new List<ComboBoxResult>();

            //Loop through our listItems and translate it into a text value and an id for the select list
            foreach (ComboBoxList a in listItems)
            {

                if (a.PropSecond == null && a.PropThird == null)
                {
                    jsonComboBoxList.Results.Add(new ComboBoxResult { id = a.Id.ToString(), text = a.PropFirst });
                }
                else if (a.PropThird == null)
                {
                    jsonComboBoxList.Results.Add(new ComboBoxResult { id = a.Id.ToString(), text = a.PropFirst + " | " + a.PropSecond });
                }
                else
                    jsonComboBoxList.Results.Add(new ComboBoxResult { id = a.Id.ToString(), text = a.PropFirst + " | " + a.PropSecond + " | " + a.PropThird });
            }
            //Set the total count of the results from the query.
            jsonComboBoxList.Total = itemCount;

            return jsonComboBoxList;
        }
        //Return only the results we want
        public List<ComboBoxList> GetListForComboBox(string searchTerm, int pageSize, int pageNum)
        {
            return GetQuery(searchTerm)              
                .Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();
        }

        //And the total count of records
        public int GetCountForComboBox(string searchTerm, int pageSize, int pageNum)
        {
            return GetQuery(searchTerm)
                .Count();
        }


        //Our search term
        private IQueryable<ComboBoxList> GetQuery(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();

            return ResultList
                .Where(
                    a =>
                    a.PropFirst.Like(searchTerm) ||
                    a.PropSecond.Like(searchTerm)
                );
        }

        //Generate test data
        private IQueryable<ComboBoxList> GenerateAndCacheData(string productCacheKeyHint) 
        {
            //Check cache first before regenerating test data
            string cacheKey = productCacheKeyHint;
            if (HttpContext.Current.Cache[cacheKey] != null)
            {
                return (IQueryable<ComboBoxList>)HttpContext.Current.Cache[cacheKey];
            }
            else
            {
                var cmbList = new List<ComboBoxList>();

                if (productCacheKeyHint == "CacheProducts")
                {
                    FillProductList(cmbList);
                }
                else if (productCacheKeyHint == "CacheVender")
                {
                    FillVenderList(cmbList);
                }

                var result = cmbList.AsQueryable();

                //Cache results
                HttpContext.Current.Cache[cacheKey] = result;
                return result;
            }
        }

        public class ClsAnnonomous
        {
            public int Id { get; set; }
            public string ProductCode { get; set; }
            public string ProductName { get; set; } 
        }

        private void FillProductList(List<ComboBoxList> cmbList)
        {
            List<ClsAnnonomous> prodList = db.Product.Select(i => new ClsAnnonomous {Id = i.ProductId, ProductName = i.ProductName, ProductCode = i.ProductCode}).ToList();

            foreach (ClsAnnonomous product in prodList)
            {
                cmbList.Add(
                                   new ComboBoxList()
                                   {
                                       Id = product.Id,
                                       PropFirst = product.ProductCode,
                                       PropSecond = product.ProductName                                       
                                   }
                                   );
            }
        }

        private void FillVenderList(List<ComboBoxList> cmbList)
        {
            throw new System.NotImplementedException();
        }
    }
}