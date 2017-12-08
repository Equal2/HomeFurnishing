using System.Linq;
using Data.Models;
using Model.Models;
using System.Configuration;
using Mailer;
using Mailer.Model;

namespace EmailContents
{
    public class SaleOrderEmailContents
    {
        public static void SendSaleOrderSubmitEmail(int DocId)
        {
            EmailMessage message = new EmailMessage();
            message.Subject = "Approve new sale order";
            string temp = (ConfigurationManager.AppSettings["SalesManager"]);
            string domain = ConfigurationManager.AppSettings["CurrentDomain"];
            message.To = temp;
            string link = domain + "/SaleOrderHeader/Detail/" + DocId + "?transactionType=approve";

            SaleOrderHeader doc = new SaleOrderHeader();

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                doc = context.SaleOrderHeader.Find(DocId);
                string format = "dd/MMM/yyyy";

                var SaleOrder = (from H in context.SaleOrderHeader
                                 where H.SaleOrderHeaderId == DocId
                                 select new
                                 {
                                     OrderNo = H.DocNo,
                                     OrderDate = H.DocDate,
                                     BuyerOrderNo = H.BuyerOrderNo,
                                     DueDate = H.DueDate,
                                     DocumentTypeName = H.DocType.DocumentTypeName,
                                     BuyerName = H.SaleToBuyer.Name
                                 }).FirstOrDefault();


                message.Body += "<div style='width:100% ;font-family sans-serif;'>"
                             + "<table style='border: 1px solid rgb(79,129,189);border-spacing:0px;width: 100%;font-size:1.4em;'>"
                             + "<tr style='background-color:rgb(79,129,189)'>"
                             + "<td colspan='6'>"
                             + "<table style='width:100%'> <tr> <td style='width:85%'> <h2 style='color:#FFF;margin-top:10px;margin-bottom:10px'> Please approve new sale order : " + SaleOrder.OrderNo + " </h2> </td>"
                             + " <td>"
                             + "<!--[if mso]>"
                             + "<v:roundrect xmlns:v='urn:schemas-microsoft-com:vml' xmlns:w='urn:schemas-microsoft-com:office:word' href='" + link + "' style='height:36px;v-text-anchor:middle;width:150px;' arcsize='5%' strokecolor='black' fillcolor='white'>"
                             + "<w:anchorlock/>"
                             + "<center style='color:black;font-family:Helvetica, Arial,sans-serif;font-size:16px;'>Approve &rarr;</center>"
                             + "</v:roundrect>"
                             + "<![endif]-->"
                             + "<a href='" + link + "' style='background-color: #FFF; border: 1px solid #2980b9; border-radius: 3px; float: right; color: #000; display: inline-block; font-family: sans-serif; font-size: 16px; line-height: 30px; text-align: center; text-decoration: none; width: 100px; -webkit-text-size-adjust: none; mso-hide: all;margin-right:15% '>Approve &rarr;</a>"
                             + "</td> </tr>  </table>"
                             + "<tr style='height: 30px; '>"
                             + "<td style='font-weight:bold;width:18%'> Order Type </td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>"+SaleOrder.DocumentTypeName+ "</td>"
                             + "<td style='font-weight: bold; width: 18%'> Order No</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td> "+SaleOrder.OrderNo+" </td>"
                             + "</tr>"
                             + "<tr style='height: 30px; background-color: rgb(220, 230, 241); '>"
                             + "<td style='font-weight: bold; width: 18%; '> Order Date</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>" + SaleOrder.OrderDate.ToString(format) + " </td>"
                             + "<td style='font-weight: bold; width: 18%'> Buyer Order No</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td> "+SaleOrder.BuyerOrderNo+" </td>"
                             + "</tr>"
                             + "<tr style='height: 30px; '>"
                             + "<td style='font-weight: bold; width: 18%; '> Due Date</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>" + SaleOrder.DueDate.ToString(format) + "</td>"
                             + "<td style='font-weight: bold; width: 18%; '> Buyer</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td> "+SaleOrder.BuyerName+" </td>"
                             + "</tr>"
                             + "</table>"
                             + "</div>";



            }

            SendEmail.SendEmailMsg(message);

        }

        public static void SendSaleOrderModifiedEmail(int DocId,string Reason)
        {
            EmailMessage message = new EmailMessage();
            message.Subject = "Approve modified sale order";
            message.To = ConfigurationManager.AppSettings["SalesManager"];
            string path = ConfigurationManager.AppSettings["CurrentDomain"];
            string link = path + "/SaleOrderHeader/Detail/" + DocId + "?transactionType=approve";
            SaleOrderHeader doc = new SaleOrderHeader();

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                doc = context.SaleOrderHeader.Find(DocId);
                string format = "dd/MMM/yyyy";
                var SaleOrder = (from H in context.SaleOrderHeader
                                 where H.SaleOrderHeaderId == DocId
                                 select new
                                 {
                                     OrderNo = H.DocNo,
                                     OrderDate = H.DocDate,
                                     BuyerOrderNo = H.BuyerOrderNo,
                                     DueDate = H.DueDate,
                                     DocumentTypeName = H.DocType.DocumentTypeName,
                                     BuyerName = H.SaleToBuyer.Name
                                 }).FirstOrDefault();


                message.Body += "<div style='width:100% ;font-family sans-serif;'>"
                             + "<table style='border: 1px solid rgb(79,129,189);border-spacing:0px;width: 100%;font-size:1.4em;'>"
                             + "<tr style='background-color:rgb(79,129,189)'>"
                             + "<td colspan='6'>"
                             + "<table style='width:100%'> <tr> <td style='width:85%'> <h2 style='color:#FFF;margin-top:10px;margin-bottom:10px'> Please approve modified sale order : " + SaleOrder.OrderNo + " </h2> </td>"
                             + " <td>"
                             + "<!--[if mso]>"
                             + "<v:roundrect xmlns:v='urn:schemas-microsoft-com:vml' xmlns:w='urn:schemas-microsoft-com:office:word' href='" + link + "' style='height:36px;v-text-anchor:middle;width:150px;' arcsize='5%' strokecolor='black' fillcolor='white'>"
                             + "<w:anchorlock/>"
                             + "<center style='color:black;font-family:Helvetica, Arial,sans-serif;font-size:16px;'>Approve &rarr;</center>"
                             + "</v:roundrect>"
                             + "<![endif]-->"
                             + "<a href='" + link + "' style='background-color: #FFF; border: 1px solid #2980b9; border-radius: 3px; float: right; color: #000; display: inline-block; font-family: sans-serif; font-size: 16px; line-height: 30px; text-align: center; text-decoration: none; width: 100px; -webkit-text-size-adjust: none; mso-hide: all;margin-right:15% '>Approve &rarr;</a>"
                             + "</td> </tr>  </table>"
                             + "<tr style='height: 30px; '>"
                             + "<td style='font-weight:bold;width:18%'> Order Type </td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>" + SaleOrder.DocumentTypeName + "</td>"
                             + "<td style='font-weight: bold; width: 18%'> Order No</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td> " + SaleOrder.OrderNo + " </td>"
                             + "</tr>"
                             + "<tr style='height: 30px; background-color: rgb(220, 230, 241); '>"
                             + "<td style='font-weight: bold; width: 18%; '> Order Date</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>" + SaleOrder.OrderDate.ToString(format) + " </td>"
                             + "<td style='font-weight: bold; width: 18%'> Buyer Order No</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td> " + SaleOrder.BuyerOrderNo + " </td>"
                             + "</tr>"
                             + "<tr style='height: 30px; '>"
                             + "<td style='font-weight: bold; width: 18%; '> Due Date</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>" + SaleOrder.DueDate.ToString(format) + "</td>"
                             + "<td style='font-weight: bold; width: 18%; '> Buyer</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td> " + SaleOrder.BuyerName + " </td>"
                             + "</tr>"
                             + "</table>"
                             + "<h3 style='color:#C70505'> User Remark:" + Reason + "</h3>"
                             + "</div>";



            }

            SendEmail.SendEmailMsg(message);

        }


    }
}
