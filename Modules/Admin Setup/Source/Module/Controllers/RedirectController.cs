using System.Web.Mvc;
using Service;

namespace Presentation.Controllers
{
    [Authorize]
    public class RedirectController : Controller
    {
        private IRedirectService _RedirectService;
        public RedirectController(IRedirectService RedServ)
        {
            _RedirectService = RedServ;
        }

        public ActionResult RedirectToDocument(int DocTypeId, int DocId, int? DocLineId)
        {

            if (DocTypeId == 0 || DocId == 0)
            {
                return View("Error");
            }

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var DocumentType = _RedirectService.GetDocumentType(DocTypeId);


            if (!string.IsNullOrEmpty(DocumentType.ControllerName) && !string.IsNullOrEmpty(DocumentType.ActionName))
            {
                if (!string.IsNullOrEmpty(DocumentType.DomainName) && !DocLineId.HasValue)
                {
                    return Redirect(System.Configuration.ConfigurationManager.AppSettings[DocumentType.DomainName] + "/" + DocumentType.ControllerName + "/" + DocumentType.ActionName + "/" + DocId);
                }
                else if (!string.IsNullOrEmpty(DocumentType.DomainName) && DocLineId.HasValue && DocLineId.Value > 0)
                {
                    return Redirect(System.Configuration.ConfigurationManager.AppSettings[DocumentType.DomainName] + "/" + DocumentType.ControllerName + "/" + DocumentType.ActionName + "?Id=" + DocId + "&DocLineId=" + DocLineId);
                }
                else
                {
                    return RedirectToAction(DocumentType.ActionName, DocumentType.ControllerName, new { id = DocId });
                }
            }

            ViewBag.Message = "Settings not configured";
            return View("~/Views/Shared/UnderImplementation.cshtml");

        }

        public ActionResult BarCodeDetail(string BarCode)
        {

            if (string.IsNullOrEmpty(BarCode))
            {
                return View("Error");
            }

            return Redirect(System.Configuration.ConfigurationManager.AppSettings["StoreDomain"] + "/BarCodeHistory/Index?ProductUid=" + BarCode);
        }
    }
}