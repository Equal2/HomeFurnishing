using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Reports.Presentation.Helper
{
    public static class HtmlModelBindingHelperExtensions
    {
        private const string IdsToReuseKey = "__HtmlModelBindingHelperExtensions_IdsToReuse_";

        public static IDisposable BeginCollectionItem(this HtmlHelper html, string collectionName)
        {
            var idsToReuse = GetIdsToReuse(html.ViewContext.HttpContext, collectionName);
            string itemIndex = idsToReuse.Count > 0 ? idsToReuse.Dequeue() : Guid.NewGuid().ToString();

            var previousPrefix = html.ViewData.TemplateInfo.HtmlFieldPrefix;
            // autocomplete="off" is needed to work around a very annoying Chrome behaviour whereby it reuses old values after the user clicks "Back", which causes the xyz.index and xyz[...] values to get out of sync.
            html.ViewContext.Writer.WriteLine(string.Format("<input type=\"hidden\" name=\"{0}.index\" autocomplete=\"off\" value=\"{1}\" />", 
                !string.IsNullOrEmpty(previousPrefix) ? string.Concat(previousPrefix.Trim('.'), ".", collectionName) : collectionName, html.Encode(itemIndex)));

            return BeginHtmlFieldPrefix(html, string.Format("{0}[{1}]", collectionName, itemIndex));
        }

        public static IDisposable BeginHtmlFieldPrefix(this HtmlHelper html, string htmlFieldPrefix)
        {
            return new HtmlFieldPrefix(html.ViewData.TemplateInfo, htmlFieldPrefix);
        }

        private static Queue<string> GetIdsToReuse(HttpContextBase httpContext, string collectionName)
        {
            // We need to use the same sequence of IDs following a server-side validation failure,  
            // otherwise the framework won't render the validation error messages next to each item.
            string key =  IdsToReuseKey + collectionName;
            var queue = (Queue<string>)httpContext.Items[key];
            if (queue == null)
            {
                httpContext.Items[key] = queue = new Queue<string>();
                var previouslyUsedIds = httpContext.Request[collectionName + ".index"];
                if (!string.IsNullOrEmpty(previouslyUsedIds))
                    foreach (string previouslyUsedId in previouslyUsedIds.Split(','))
                        queue.Enqueue(previouslyUsedId);
            }
            return queue;
        }

        private class HtmlFieldPrefix : IDisposable
        {
            private readonly TemplateInfo _templateInfo;
            private readonly string _previousHtmlFieldPrefix;

            public HtmlFieldPrefix(TemplateInfo templateInfo, string htmlFieldPrefix)
            {
                this._templateInfo = templateInfo;

                _previousHtmlFieldPrefix = templateInfo.HtmlFieldPrefix;
                if (!string.IsNullOrEmpty(htmlFieldPrefix))
                {
                    templateInfo.HtmlFieldPrefix = string.Format("{0}.{1}", templateInfo.HtmlFieldPrefix, htmlFieldPrefix);
                }
            }

            public void Dispose()
            {
                _templateInfo.HtmlFieldPrefix = _previousHtmlFieldPrefix;
            }
        }
    }
}