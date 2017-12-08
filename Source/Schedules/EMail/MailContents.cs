using System;
using System.Collections.Generic;
using System.Diagnostics;
using Service;
using Data.Models;
using System.Configuration;
using Mailer;
using Core.Common;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Mailer.Model;
using Data.Infrastructure;
using Model.Models;
using System.Linq;

namespace EmailContents
{
    public class MailContents
    {
        ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;
        public MailContents(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public void PurchaseOrder_OnSubmit(int Id)
        {
            EmailMessage message = new EmailMessage();
            message.Subject = "Submit new Purchase Order";
            string temp = (ConfigurationManager.AppSettings["PurchaseManager"]);
            string domain = ConfigurationManager.AppSettings["PurchaseDomain"];
            message.To = temp;
            string link = domain + "/PurchaseOrderHeader/Detail/" + Id + "?transactionType=approve";

            PurchaseOrderHeader doc = new PurchaseOrderHeader();

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                doc = context.PurchaseOrderHeader.Find(Id);
                string format = "dd/MMM/yyyy";


                var PurchaseOrder = (from H in db.PurchaseOrderHeader 
                                 where H.PurchaseOrderHeaderId == Id
                                 select new
                                 {
                                     OrderNo = H.DocNo,
                                     OrderDate = H.DocDate,
                                     DueDate = H.DueDate,
                                     DocumentTypeName = H.DocType.DocumentTypeName,
                                     SupplierName = H.Supplier.Name
                                 }).FirstOrDefault();


                message.Body += "<div style='width:100% ;font-family sans-serif;'>"
                             + "<table style='border: 1px solid rgb(79,129,189);border-spacing:0px;width: 100%;font-size:1.4em;'>"
                             + "<tr style='background-color:rgb(79,129,189)'>"
                             + "<td colspan='6'>"
                             + "<table style='width:100%'> <tr> <td style='width:85%'> <h2 style='color:#FFF;margin-top:10px;margin-bottom:10px'> Please approve new purchase order : " + PurchaseOrder.OrderNo + " </h2> </td>"
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
                             + "<td style='width:26%'>" + PurchaseOrder.DocumentTypeName + "</td>"
                             + "<td style='font-weight: bold; width: 18%'> Order No</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td> " + PurchaseOrder.OrderNo + " </td>"
                             + "</tr>"
                             + "<tr style='height: 30px; background-color: rgb(220, 230, 241); '>"
                             + "<td style='font-weight: bold; width: 18%; '> Order Date</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>" + PurchaseOrder.OrderDate.ToString(format) + " </td>"
                             + "<td style='font-weight: bold; width: 18%'> Supplier</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td> " + PurchaseOrder.SupplierName + " </td>"
                             + "</tr>"
                             + "<tr style='height: 30px; '>"
                             + "<td style='font-weight: bold; width: 18%; '> Due Date</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>" + PurchaseOrder.DueDate.ToString(format) + "</td>"
                             + "</tr>"
                             + "</table>"
                             + "</div>";

            }


            SendEmail.SendEmailMsg(message);
        }

        public void PurchaseOrder_OnApprove(int Id)
        {
            EmailMessage message = new EmailMessage();
            message.Subject = "Submit new Purchase Order";
            string temp = (ConfigurationManager.AppSettings["PurchaseManager"]);
            string domain = ConfigurationManager.AppSettings["PurchaseDomain"];
            message.To = temp;
            string link = domain + "/PurchaseOrderHeader/Detail/" + Id + "?transactionType=detail";

            PurchaseOrderHeader doc = new PurchaseOrderHeader();

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                doc = context.PurchaseOrderHeader.Find(Id);
                string format = "dd/MMM/yyyy";


                var PurchaseOrder = (from H in db.PurchaseOrderHeader
                                     where H.PurchaseOrderHeaderId == Id
                                     select new
                                     {
                                         OrderNo = H.DocNo,
                                         OrderDate = H.DocDate,
                                         DueDate = H.DueDate,
                                         DocumentTypeName = H.DocType.DocumentTypeName,
                                         SupplierName = H.Supplier.Name
                                     }).FirstOrDefault();


                message.Body += "<div style='width:100% ;font-family sans-serif;'>"
                             + "<table style='border: 1px solid rgb(79,129,189);border-spacing:0px;width: 100%;font-size:1.4em;'>"
                             + "<tr style='background-color:rgb(79,129,189)'>"
                             + "<td colspan='6'>"
                             + "<table style='width:100%'> <tr> <td style='width:85%'> <h2 style='color:#FFF;margin-top:10px;margin-bottom:10px'> Please check out new purchase order : " + PurchaseOrder.OrderNo + " </h2> </td>"
                             + " <td>"
                             + "<!--[if mso]>"
                             + "<v:roundrect xmlns:v='urn:schemas-microsoft-com:vml' xmlns:w='urn:schemas-microsoft-com:office:word' href='" + link + "' style='height:36px;v-text-anchor:middle;width:150px;' arcsize='5%' strokecolor='black' fillcolor='white'>"
                             + "<w:anchorlock/>"
                             + "<center style='color:black;font-family:Helvetica, Arial,sans-serif;font-size:16px;'>Show Purchase Order &rarr;</center>"
                             + "</v:roundrect>"
                             + "<![endif]-->"
                             + "<a href='" + link + "' style='background-color: #FFF; border: 1px solid #2980b9; border-radius: 3px; float: right; color: #000; display: inline-block; font-family: sans-serif; font-size: 16px; line-height: 30px; text-align: center; text-decoration: none; width: 100px; -webkit-text-size-adjust: none; mso-hide: all;margin-right:15% '>Show Purchase Order &rarr;</a>"
                             + "</td> </tr>  </table>"
                             + "<tr style='height: 30px; '>"
                             + "<td style='font-weight:bold;width:18%'> Order Type </td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>" + PurchaseOrder.DocumentTypeName + "</td>"
                             + "<td style='font-weight: bold; width: 18%'> Order No</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td> " + PurchaseOrder.OrderNo + " </td>"
                             + "</tr>"
                             + "<tr style='height: 30px; background-color: rgb(220, 230, 241); '>"
                             + "<td style='font-weight: bold; width: 18%; '> Order Date</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>" + PurchaseOrder.OrderDate.ToString(format) + " </td>"
                             + "<td style='font-weight: bold; width: 18%'> Supplier</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td> " + PurchaseOrder.SupplierName + " </td>"
                             + "</tr>"
                             + "<tr style='height: 30px; '>"
                             + "<td style='font-weight: bold; width: 18%; '> Due Date</td>"
                             + "<td style='width:3%'>:</td>"
                             + "<td style='width:26%'>" + PurchaseOrder.DueDate.ToString(format) + "</td>"
                             + "</tr>"
                             + "</table>"
                             + "</div>";

            }


            SendEmail.SendEmailMsg(message);
        }
    }
}
