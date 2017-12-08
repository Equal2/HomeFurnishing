using Jobs.Constants.LedgerAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.Process
{
    public static class ProcessConstants
    {
        public static class Purchase
        {
            public const int ProcessId = 1;
            public const string ProcessCode = "Purchase";
            public const string ProcessName = "Purchase";
            public const int AccountId = LedgerAccountConstants.PurchaseAc.LedgerAccountId;
        }
        public static class Sale
        {
            public const int ProcessId = 2;
            public const string ProcessCode = "Sale";
            public const string ProcessName = "Sale";
            public const int AccountId = LedgerAccountConstants.SaleAc.LedgerAccountId;
        }
    }
}