using Core.Common;
using Data.Models;
using Model.Models;
using Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Controllers
{
    public static class DocumentValidation
    {
        public static bool ValidateDocument(DocumentUniqueId Args, string Type, string UserName, out string Msg, out bool Continue)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            Continue = UserRoles.Contains("Admin");
            Msg = "";


            using (ApplicationDbContext Con = new ApplicationDbContext())
            {
                DateTime Today = DateTime.Now.Date;
                Decimal MaxAllowedDays = 0;
                Decimal MaxBackDays = 0;

                DocumentTypeTimeExtension TimeExtension = (from p in Con.DocumentTypeTimeExtension
                                                           where p.DocTypeId == Args.DocTypeId
                                                           && p.SiteId == Args.SiteId && p.DivisionId == Args.DivisionId
                                                           && p.Type == Type && p.ExpiryDate >= Today && p.UserName.ToUpper() == UserName.ToUpper() && Args.DocDate.Date == p.DocDate
                                                           select p).FirstOrDefault();


                DocumentTypeTimePlan TimePlan = (from p in Con.DocumentTypeTimePlan
                                                 where p.DocTypeId == Args.DocTypeId
                                                 && p.SiteId == Args.SiteId && p.DivisionId == Args.DivisionId
                                                 && p.Type == Type
                                                 select p).FirstOrDefault();

                if (TimePlan != null)
                    MaxAllowedDays = TimePlan.Days;


                DocumentTypeTimePlan TimePlanBackDays = (from p in Con.DocumentTypeTimePlan
                                                         where p.DocTypeId == Args.DocTypeId
                                                         && p.SiteId == Args.SiteId && p.DivisionId == Args.DivisionId
                                                         && p.Type == DocumentTimePlanTypeConstants.Create
                                                         select p).FirstOrDefault();

                DocumentTypeTimeExtension BackDaysTimeExtension = (from p in Con.DocumentTypeTimeExtension
                                                                   where p.DocTypeId == Args.DocTypeId
                                                                   && p.SiteId == Args.SiteId && p.DivisionId == Args.DivisionId
                                                                   && p.Type == DocumentTimePlanTypeConstants.Create && p.UserName.ToUpper() == UserName.ToUpper() && Args.DocDate.Date == p.DocDate
                                                                   select p).FirstOrDefault();

                if (TimePlanBackDays != null)
                    MaxBackDays = TimePlanBackDays.Days;



                switch (Type)
                {
                    case DocumentTimePlanTypeConstants.Create:
                        {
                            if (TimePlan != null && (((DateTime.Now.Date - Args.DocDate.Date.Date).Days) > MaxAllowedDays) && TimeExtension == null)
                            {
                                Msg = MaxAllowedDays != 0 ? "You cannot Create record older than " + string.Format("{0:0}", MaxAllowedDays) + " days" : "You cannot Create record older than today. <br />";
                            }
                            else
                            {
                                Msg = "";
                            }
                            break;

                        }
                    case DocumentTimePlanTypeConstants.Modify:
                        {
                            if (TimePlan != null && (((DateTime.Now.Date - (Args.CreatedDate == null ? DateTime.Now.Date : Args.CreatedDate.Date)).Days) > MaxAllowedDays) && TimeExtension == null && Args.Status == (int)StatusConstants.Submitted)
                            {
                                Msg = MaxAllowedDays > 0 ? "You cannot Modify record older than " + string.Format("{0:0}", MaxAllowedDays) + " days" : "You cannot Modify record older than today. <br />";
                            }
                            else
                            {

                                if ((TimePlanBackDays != null && ((((Args.CreatedDate == null ? DateTime.Now.Date : Args.CreatedDate.Date) - Args.DocDate.Date).Days) > MaxBackDays)) && Args.Status == (int)StatusConstants.Submitted && BackDaysTimeExtension == null)
                                {
                                    Msg = MaxAllowedDays > 0 ? "You cannot Create record older than " + string.Format("{0:0}", MaxBackDays) + " days" : "You cannot Create record older than today. <br />";
                                }
                                else
                                {
                                    Msg = "";
                                }
                            }


                            if ((Args.Status == (int)StatusConstants.Drafted || Args.Status == (int)StatusConstants.Modified) && !string.IsNullOrEmpty(Args.ModifiedBy) && Args.ModifiedBy.ToUpper() != UserName.ToUpper())
                            {
                                Msg += "Record must be submitted before modification. <br />";
                            }

                            if (Args.GatePassHeaderId.HasValue && Args.GatePassHeaderId.Value > 0)
                            {
                                Continue = false;
                                Msg += "Cannot modify record whose gatepass is generated. <br />";
                            }

                            if (!string.IsNullOrEmpty(Args.LockReason))
                            {
                                Continue = false;
                                Msg += "Record Locked: " + Args.LockReason + ". <br />";
                            }

                            break;

                        }
                    case DocumentTimePlanTypeConstants.Submit:
                        {
                            if (TimePlan != null && (((DateTime.Now.Date - (Args.CreatedDate == null ? DateTime.Now.Date : (Args.Status == (int)StatusConstants.Modified ? Args.ModifiedDate.Date : Args.CreatedDate.Date))).Days) > (Args.Status == (int)StatusConstants.Modified ? 1 : MaxAllowedDays)) && TimeExtension == null)
                            {
                                Msg = Args.Status == (int)StatusConstants.Modified ? "You cannot Submit modified record older than one day. <br />" : (MaxAllowedDays != 0 ? "You cannot Submit record older than " + string.Format("{0:0}", MaxAllowedDays) + " days." : "You cannot Submit record older than today. <br />");
                            }
                            else
                            {
                                Msg = "";
                            }
                            break;
                        }
                    case DocumentTimePlanTypeConstants.Delete:
                        {
                            if (TimePlan != null && (((DateTime.Now.Date - (Args.CreatedDate == null ? DateTime.Now.Date : Args.CreatedDate.Date)).Days) > MaxAllowedDays) && TimeExtension == null)
                            {
                                Msg = MaxAllowedDays > 0 ? "You cannot Delete record older than " + string.Format("{0:0}", MaxAllowedDays) + " days." : "You cannot Delete record older than today. <br />";
                            }
                            else
                            {
                                Msg = "";
                            }

                            if (!string.IsNullOrEmpty(Args.LockReason))
                            {
                                Continue = false;
                                Msg += "Record Locked: " + Args.LockReason + ". <br />";
                            }

                            break;
                        }
                    case DocumentTimePlanTypeConstants.GatePassCreate:
                        {
                            if (TimePlan != null && (((DateTime.Now.Date - ( Args.ModifiedDate.Date)).Days) >  MaxAllowedDays) && TimeExtension == null)
                            {
                                Msg = Args.DocNo +"<br />"+(MaxAllowedDays != 0 ? "You cannot Generate gatepass older than " + string.Format("{0:0}", MaxAllowedDays) + " days." : "You cannot generate gatepass older than today. <br />");
                            }
                            else
                            {
                                Msg = "";
                            }
                            break;
                        }
                    case DocumentTimePlanTypeConstants.GatePassCancel:
                        {
                            var GatePass = Con.GatePassHeader.Find(Args.GatePassHeaderId);


                            if (GatePass.Status != (int)StatusConstants.Drafted)
                                Msg = "Cannot cancel a submitted gatepass.";
                            else if (TimePlan != null && (((DateTime.Now.Date -  GatePass.CreatedDate ).Days) > (MaxAllowedDays)) && TimeExtension == null)
                            {
                                Msg = Args.DocNo + "<br />" + (MaxAllowedDays != 0 ? "You cannot Cancel gatepass older than " + string.Format("{0:0}", MaxAllowedDays) + " days." : "You cannot Cancel gatepass older than today. <br />");
                            }
                            else
                            {
                                Msg = "";
                            }
                            break;
                        }
                }

            }

            return (string.IsNullOrEmpty(Msg));

        }





        public static bool ValidateDocumentLine(DocumentUniqueId Args, string UserName, out string Msg, out bool Continue)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            Continue = UserRoles.Contains("Admin");
            Msg = "";

            if (!string.IsNullOrEmpty(Args.LockReason))
            {
                Continue = false;
                Msg += "Record Locked: " + Args.LockReason + ". \n";
            }


            return (string.IsNullOrEmpty(Msg));
        }
    }
}
