using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjLib.Constants
{
    public enum StatusConstants
    {
        Drafted = 0,
        Submitted = 1,
        Approved = 2,
        Modified = 3,
        ModificationSubmitted = 4,
        Closed = 5,
        NocOk = 6,
        Complete = 7,
        Import = 8,
        Locked = 9,
        Cancel = 10,
        System = 11,
    }
    public enum PriorityConstants
    {
        Low = -10,
        Normal = 0,
        High = 10
    }

    public class SessionNameConstants
    {
        public const string CompanyName = "CompanyName";
        public const string SiteAddress = "SiteAddress";
        public const string SiteCityName = "SiteCityName";
        public const string SiteName = "SiteName";
        public const string DivisionName = "DivisionName";
        public const string SiteShortName = "SiteShortName";
        public const string LoginSiteId = "LoginSiteId";
        public const string LoginDivisionId = "LoginDivisionId";
        public const string UserNotificationCount = "uNotifiCount";
        public const string LoginDomain = "_loginDomain";
        public const string MenuDomain = "_menuDomain";
    }

    public enum NotificationSubjectConstants
    {
        SaleOrderSubmitted = 1,
        SaleOrderApproved = 2,

        PurchaseOrderSubmitted = 3,
        PurchaseOrderApproved = 4,

        PurchaseOrderCancelSubmitted = 5,
        PurchaseOrderCancelApproved = 6,

        PurchaseGoodsReceiptSubmitted = 7,
        PurchaseGoodsReceiptApproved = 8,

        TaskCreated = 9,
        PendingToSubmit = 10,
        PermissionAssigned = 11,
        UserRegistered = 12,
    }

    public class ProductNatureConstants
    {
        public const string Bom = "Bom";
        public const string Rawmaterial = "Raw Material";
        public const string OtherMaterial = "Other Material";
        public const string FinishedMaterial = "Finished Material";
        public const string Machine = "Machine";
    }

    public class ProductUidStatusConstants
    {
        public const string Issue = "Issue";
        public const string Receive = "Receive";
        public const string Ship = "Ship";
        public const string Transfer = "Transfer";
        public const string Cancel = "Cancel";
        public const string Return = "Return";
        public const string Pack = "Pack";
        public const string Dispatch = "Dispatch";
        public const string Generated = "Gen";
    }

    public class ProductTypeConstants
    {
        public const string Rug = "Rug";
        public const string FinishedMaterial = "Finished Material";
        public const string Bom = "Bom";
        public const string Stencil = "Stencil";
        public const string Trace = "Trace";
        public const string Map = "Map";
        public const string Machine = "Machine";
        public const string OtherMaterial = "Other Material";
    }

    public class PrevNextConstants
    {
        public const string Prev = "Previous";
        public const string Next = "Next";
    }

    public class ProcessConstants
    {
        public const string Sales = "Sale";
        public const string Weaving = "Weaving";
        public const string Silai = "Silai";
        public const string Packing = "Packing";
        public const string Purchase = "Purchase";
        public const string FullFinishing = "Full Finishing";
        public const string ThirdBacking = "Third Backing";
        public const string Dyeing = "Dyeing";
    }
}
