using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;

namespace System.Web.Mvc.Html
{
    public static class CustomHelpers
    {
        public static MvcHtmlString ValidationLog(this HtmlHelper helper, string Log)
        {
            if (!string.IsNullOrEmpty(Log))
                return new MvcHtmlString("<div class='alert alert-danger' role='alert' style='margin:0px;text-align:center' id='ExcAlert'> " + Log + " </div>");
            else
                return new MvcHtmlString("");
        }

        public static MvcHtmlString NumericBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {

            var htmlAttr = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            htmlAttr.Add("autocomplete", "off");
            bool roundOff = false;
            object dp;
            roundOff = htmlAttr.TryGetValue("roundOff", out dp);
            //htmlAttr["class"] = htmlAttr["class"] + " number";
            int iRoundOff = 2;
            if (htmlAttr.ContainsKey("roundOff") && roundOff)
            {
                htmlAttr.Add("onchange", "if($(this).val())$(this).val(parseFloat(eval($(this).val())).toFixed(" + Convert.ToInt32(dp) + "))");
                htmlAttr.Remove("roundOff");
                iRoundOff = Convert.ToInt32(dp);
            }
            else
                htmlAttr.Add("onchange", "if($(this).val())$(this).val(parseFloat(eval($(this).val())).toFixed(2))");

            //if (htmlAttr.ContainsKey("number"))
            //{
            //    htmlAttr["type"] = "number";
            //    htmlAttr.Remove("number");
            //}

            return htmlHelper.TextBoxFor(expression, "{0:N" + iRoundOff + "}", htmlAttr);
        }

        //public static MvcHtmlString FormatXmlString(this HtmlHelper helper, string XmlString, int ActivityType)
        //{
        //    if (!string.IsNullOrEmpty(XmlString))
        //    {
        //        XmlDocument doc = new XmlDocument();
        //        doc.LoadXml(XmlString);

        //        String Temp = "";
        //        if (ActivityType == (int)ActivityTypeContants.Modified)
        //        {
        //            Temp += "<table class='table-bordered table-striped'><thead><th>Field Name</th><th>Old Value</th><th>New Value</th></thead> <tbody>";

        //            var Parent = doc.FirstChild;

        //            for (int i = 0; i < Parent.ChildNodes.Count; i++)
        //            {
        //                var ParentTable = Parent.ChildNodes[i];
        //                Temp += "<tr><td  colspan='3' align='center'>"+ParentTable.Name+"</td></tr>";
        //                for (int j = 0; j < ParentTable.ChildNodes.Count; j++)
        //                {
        //                    var ParentField = ParentTable.ChildNodes[j];

        //                    Temp += "<tr> <td>" + ParentField.Name + "</td>" + "<td>" + ParentField["Old"].InnerText + "</td>" + "<td>" + ParentField["New"].InnerText + "</td> </tr>";
        //                }
        //            }

        //            Temp += "</table>";
        //        }
        //        else if (ActivityType == (int)ActivityTypeContants.Deleted)
        //        {
        //            Temp += "<table class='table-bordered table-striped'><thead><th>Field Name</th><th>Value</th></thead> <tbody>";

        //            var Parent = doc.FirstChild;

        //            for (int i = 0; i < Parent.ChildNodes.Count; i++)
        //            {
        //                var ParentTable = Parent.ChildNodes[i];
        //                Temp += "<tr><td  colspan='3' align='center'>" + ParentTable.Name + "</td></tr>";
        //                for (int j = 0; j < ParentTable.ChildNodes.Count; j++)
        //                {
        //                    var ParentField = ParentTable.ChildNodes[j];

        //                    Temp += "<tr> <td>" + ParentField.Name + "</td>" + "<td>" + ParentField.InnerText + "</td>" + "</tr>";
        //                }
        //            }
        //            Temp += "</table>";
        //        }
        //        else
        //        {
        //            Temp += "<textarea disabled='disabled' style='width:100%; resize:none;' rows='5'>";
        //            Temp += XmlString;
        //            Temp += "</textarea>";
        //        }
        //        return new MvcHtmlString(Temp);
        //    }
        //    else
        //        return new MvcHtmlString("");
        //}

    }
}