using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifier.Core
{
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
        PermissionAssigned=11,
        UserRegistered=12,
    }
}
