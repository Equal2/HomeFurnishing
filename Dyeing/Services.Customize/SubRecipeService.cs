using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity.SqlServer;
using Infrastructure.IO;
using ProjLib.Constants;
using Models.BasicSetup.ViewModels;
using Models.Customize.Models;
using Models.Customize.ViewModels;
using Models.BasicSetup.Models;
using Models.Company.Models;
using Models.Customize.DataBaseViews;
using Components.Logging;
using Services.BasicSetup;
using AutoMapper;
using System.Xml.Linq;
using System.Data;
using ProjLib.DocumentConstants;
using DocumentPrint;

namespace Services.Customize
{
    public interface ISubRecipeService : IDisposable
    {
        SubRecipeViewModel Create(SubRecipeViewModel vmSubRecipe, string UserName);
        Decimal GetRecipeBalanceForSubRecipe(int JobOrderHeaderId);

        IQueryable<ComboBoxResult> GetRecipeHelpList(int filter, string term);
    }
    public class SubRecipeService : ISubRecipeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Unit> _unitRepository;
        private readonly ILogger _logger;
        private readonly IModificationCheck _modificationCheck;
        private readonly IJobOrderHeaderService _JobOrderHeaderService;
        private readonly IJobOrderLineService _JobOrderLineService;
        private readonly IRecipeHeaderService _RecipeHeaderService;

        private ActiivtyLogViewModel logVm = new ActiivtyLogViewModel();

        public SubRecipeService(IUnitOfWork unit,
            IJobOrderHeaderService JobOrderHeaderService,
            IJobOrderLineService JobOrderLineService,
            IRecipeHeaderService RecipeHeaderService,
            ILogger log, IModificationCheck modificationCheck)
        {
            _unitOfWork = unit;
            _logger = log;
            _modificationCheck = modificationCheck;
            _JobOrderHeaderService = JobOrderHeaderService;
            _JobOrderLineService = JobOrderLineService;
            _RecipeHeaderService = RecipeHeaderService;

            //Log Initialization
            logVm.SessionId = 0;
            logVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            logVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            logVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }



        public SubRecipeViewModel Create(SubRecipeViewModel vmSubRecipe, string UserName)
        {

            _RecipeHeaderService.CreateProdOrder(vmSubRecipe.JobOrderHeaderId, UserName, vmSubRecipe.Qty);

            _unitOfWork.Save();

            JobOrderHeader Header = _RecipeHeaderService.Find(vmSubRecipe.JobOrderHeaderId);



            _logger.LogActivityDetail(logVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = Header.DocTypeId,
                DocId = Header.JobOrderHeaderId,
                ActivityType = (int)ActivityTypeContants.Modified,
                DocNo = Header.DocNo,
                DocDate = Header.DocDate,
                DocStatus = Header.Status,
            }));



            return vmSubRecipe;
        }






        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

        public IQueryable<ComboBoxResult> GetRecipeHelpList(int filter, string term)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var list = (from p in _unitOfWork.Repository<ViewRecipeBalanceForSubRecipe>().Instance
                        join pt in _unitOfWork.Repository<Product>().Instance on p.ProductId equals pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join D1 in _unitOfWork.Repository<Dimension1>().Instance on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in _unitOfWork.Repository<Dimension2>().Instance on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where p.BalanceQty > 0
                        && string.IsNullOrEmpty(term) ? 1 == 1 : p.RecipeNo.ToLower().Contains(term.ToLower())
                        && p.SiteId == CurrentSiteId
                        && p.DivisionId == CurrentDivisionId
                        orderby p.DocDate descending, p.RecipeNo descending
                        select new ComboBoxResult
                        {
                            text = p.RecipeNo,
                            id = p.JobOrderHeaderId.ToString(),
                            TextProp1 = "Product: " + ProductTab.ProductName.ToString(),
                            TextProp2 = "Qty: " + p.BalanceQty.ToString(),
                            AProp1 = Dimension1Tab.Dimension1Name,
                            AProp2 = Dimension2Tab.Dimension2Name
                        });

            return list;
        }


        public Decimal GetRecipeBalanceForSubRecipe(int JobOrderHeaderId)
        {
            var RecipeBalance = (from L in _unitOfWork.Repository<ViewRecipeBalanceForSubRecipe>().Instance
                                 where L.JobOrderHeaderId == JobOrderHeaderId
                                 select L).FirstOrDefault();

            if (RecipeBalance != null)
            {
                return RecipeBalance.BalanceQty;
            }
            else
            {
                return 0;
            }
        }
    }


}


