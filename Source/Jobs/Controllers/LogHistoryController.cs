using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Data.Models;
using Service;
using Model.ViewModel;
using System.Xml;

namespace Jobs.Controllers
{
    public class LogHistoryController : System.Web.Mvc.Controller
    {
        private LogApplicationDbContext db = new LogApplicationDbContext();
        IExceptionHandlingService _exception;
        public LogHistoryController()
        {
            _exception = new ExceptionHandlingService();
        }

        [Authorize]
        public ActionResult GetHistory(string Ids, int DocTypeId)
        {

            using (ApplicationDbContext con = new ApplicationDbContext())
            {
                List<ActiivtyLogViewModel> temp = new List<ActiivtyLogViewModel>();
                string[] IdsList = null;

                if (!string.IsNullOrEmpty(Ids))
                {
                    IdsList = Ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    temp = (from p in db.ActivityLog
                            join t in db.ActivityType on p.ActivityType equals t.ActivityTypeId
                            where IdsList.Contains(p.DocId.ToString()) && p.DocTypeId == DocTypeId
                            orderby p.CreatedDate
                            select new ActiivtyLogViewModel
                            {
                                DocDate = p.DocDate,
                                DocNo = p.DocNo,
                                DocId = p.DocId,
                                ActivityLogId = p.ActivityLogId,
                                ActivityType = p.ActivityType,
                                CreatedBy = p.CreatedBy,
                                CreatedDate = p.CreatedDate,
                                UserRemark = p.UserRemark,
                                Modifications = p.Modifications,
                                ActivityTypeName = t.ActivityTypeName,
                                Narration = p.Narration,
                                DocLineId = p.DocLineId,
                            }).ToList();




                    foreach (var item in temp.Where(m => !string.IsNullOrEmpty(m.Modifications)))
                    {
                        XmlDocument tem = new XmlDocument();
                        tem.LoadXml(item.Modifications);
                        item.XmlModifications = tem;
                    }
                }

                return PartialView("_LogHistory", temp);

            }
        }
    }
}
