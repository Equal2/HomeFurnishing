using Jobs.Constants.ChargeGroupPerson;
using Jobs.Constants.ChargeGroupProduct;
using Jobs.Constants.ChargeType;
using Jobs.Constants.LedgerAccount;
using Jobs.Constants.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.ChargeGroupSetting
{
    public static class ChargeGroupSettingConstants
    {
        #region "Purchase State Registered GST 5%"
        public static class PurchaseStateRegisteredGST5PerTaxableAmount 
        {
            public const int ChargeGroupSettingId = 1;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseState5.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST5PerCGST 
        {
            public const int ChargeGroupSettingId = 2;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 2.5000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST5PerSGST 
        {
            public const int ChargeGroupSettingId = 3;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 2.5000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST5PerIGST
        {
            public const int ChargeGroupSettingId = 4;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateRegisteredGST5PerCESS
        {
            public const int ChargeGroupSettingId = 5;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase State Registered GST 12%"
        public static class PurchaseStateRegisteredGST12PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 6;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseState12.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST12PerCGST
        {
            public const int ChargeGroupSettingId = 7;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 6.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState6.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST12PerSGST
        {
            public const int ChargeGroupSettingId = 8;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 6.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState6.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST12PerIGST
        {
            public const int ChargeGroupSettingId = 9;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateRegisteredGST12PerCESS
        {
            public const int ChargeGroupSettingId = 10;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }

        #endregion
        #region "Purchase State Registered GST 18%"
        public static class PurchaseStateRegisteredGST18PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 11;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseState18.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST18PerCGST
        {
            public const int ChargeGroupSettingId = 12;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 9.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState9.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST18PerSGST
        {
            public const int ChargeGroupSettingId = 13;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 9.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState9.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST18PerIGST
        {
            public const int ChargeGroupSettingId = 14;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateRegisteredGST18PerCESS
        {
            public const int ChargeGroupSettingId = 15;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase State Registered GST 28%"
        public static class PurchaseStateRegisteredGST28PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 16;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseState28.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST28PerCGST
        {
            public const int ChargeGroupSettingId = 17;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 14.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState14.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST28PerSGST
        {
            public const int ChargeGroupSettingId = 18;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 14.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState14.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGST28PerIGST
        {
            public const int ChargeGroupSettingId = 19;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateRegisteredGST28PerCESS
        {
            public const int ChargeGroupSettingId = 20;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase State Registered GST Exempt"
        public static class PurchaseStateRegisteredGSTExemptTaxableAmount
        {
            public const int ChargeGroupSettingId = 21;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseState28.LedgerAccountId;
        }
        public static class PurchaseStateRegisteredGSTExemptCGST
        {
            public const int ChargeGroupSettingId = 22;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateRegisteredGSTExemptSGST
        {
            public const int ChargeGroupSettingId = 23;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateRegisteredGSTExemptIGST
        {
            public const int ChargeGroupSettingId = 24;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateRegisteredGSTExemptCESS
        {
            public const int ChargeGroupSettingId = 25;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion

        #region "Purchase State Un-Registered GST 5%"
        public static class PurchaseStateUnRegisteredGST5PerTaxableAmount 
        {
            public const int ChargeGroupSettingId = 26;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseState5.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST5PerCGST 
        {
            public const int ChargeGroupSettingId = 27;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 2.5000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST5PerSGST 
        {
            public const int ChargeGroupSettingId = 28;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 2.5000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST5PerIGST
        {
            public const int ChargeGroupSettingId = 29;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateUnRegisteredGST5PerCESS
        {
            public const int ChargeGroupSettingId = 30;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase State Un-Registered GST 12%"
        public static class PurchaseStateUnRegisteredGST12PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 31;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseState12.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST12PerCGST
        {
            public const int ChargeGroupSettingId = 32;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 6;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState6.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST12PerSGST
        {
            public const int ChargeGroupSettingId = 33;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 6;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState6.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST12PerIGST
        {
            public const int ChargeGroupSettingId = 34;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateUnRegisteredGST12PerCESS
        {
            public const int ChargeGroupSettingId = 35;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase State Un-Registered GST 18%"
        public static class PurchaseStateUnRegisteredGST18PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 36;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseState18.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST18PerCGST
        {
            public const int ChargeGroupSettingId = 37;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 9;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState9.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST18PerSGST
        {
            public const int ChargeGroupSettingId = 38;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 9;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState9.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST18PerIGST
        {
            public const int ChargeGroupSettingId = 39;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateUnRegisteredGST18PerCESS
        {
            public const int ChargeGroupSettingId = 40;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase State Un-Registered GST 28%"
        public static class PurchaseStateUnRegisteredGST28PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 41;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseState28.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST28PerCGST
        {
            public const int ChargeGroupSettingId = 42;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 14;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST28PerSGST
        {
            public const int ChargeGroupSettingId = 43;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 14;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGST28PerIGST
        {
            public const int ChargeGroupSettingId = 44;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseStateUnRegisteredGST28PerCESS
        {
            public const int ChargeGroupSettingId = 45;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase State Un-Registered GST Exempt"
        public static class PurchaseStateUnRegisteredGSTExemptTaxableAmount
        {
            public const int ChargeGroupSettingId = 46;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseState28.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGSTExemptCGST
        {
            public const int ChargeGroupSettingId = 47;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGSTExemptSGST
        {
            public const int ChargeGroupSettingId = 48;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class PurchaseStateUnRegisteredGSTExemptIGST
        {
            public const int ChargeGroupSettingId = 49;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }

        public static class PurchaseStateUnRegisteredGSTExemptCESS
        {
            public const int ChargeGroupSettingId = 50;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }

        #endregion

        #region "Purchase ExState-Registered GST 5%"
        public static class PurchaseExStateRegisteredGST5PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 51;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseExState5.LedgerAccountId;
        }
        public static class PurchaseExStateUnRegisteredGST5PerCGST
        {
            public const int ChargeGroupSettingId = 52;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnRegisteredGST5PerSGST
        {
            public const int ChargeGroupSettingId = 53;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateRegisteredGST5PerIGST
        {
            public const int ChargeGroupSettingId = 54;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 5.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState5.LedgerAccountId;
        }
        public static class PurchaseExStateRegisteredGST5PerCESS
        {
            public const int ChargeGroupSettingId = 55;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase ExState-Registered GST 12%"
        public static class PurchaseExStateRegisteredGST12PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 56;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseExState12.LedgerAccountId;
        }
        public static class PurchaseExStateUnRegisteredGST12PerCGST
        {
            public const int ChargeGroupSettingId = 57;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnRegisteredGST12PerSGST
        {
            public const int ChargeGroupSettingId = 58;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateRegisteredGST12PerIGST
        {
            public const int ChargeGroupSettingId = 59;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 12.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState12.LedgerAccountId;
        }
        public static class PurchaseExStateRegisteredGST12PerCESS
        {
            public const int ChargeGroupSettingId = 60;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase ExState-Registered GST 18%"
        public static class PurchaseExStateRegisteredGST18PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 61;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseExState18.LedgerAccountId;
        }
        public static class PurchaseExStateUnRegisteredGST18PerCGST
        {
            public const int ChargeGroupSettingId = 62;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnRegisteredGST18PerSGST
        {
            public const int ChargeGroupSettingId = 63;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateRegisteredGST18PerIGST
        {
            public const int ChargeGroupSettingId = 64;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 18.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState18.LedgerAccountId;
        }
        public static class PurchaseExStateRegisteredGST18PerCESS
        {
            public const int ChargeGroupSettingId = 65;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase ExState-Registered GST 28%"
        public static class PurchaseExStateRegisteredGST28PerTaxableAmount 
        {
            public const int ChargeGroupSettingId = 66;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseExState28.LedgerAccountId;
        }
        public static class PurchaseExStateUnRegisteredGST28PerCGST
        {
            public const int ChargeGroupSettingId = 67;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnRegisteredGST28PerSGST
        {
            public const int ChargeGroupSettingId = 68;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateRegisteredGST28PerIGST 
        {
            public const int ChargeGroupSettingId = 69;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 28.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState28.LedgerAccountId;
        }
        public static class PurchaseExStateRegisteredGST28PerCESS
        {
            public const int ChargeGroupSettingId = 70;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase ExState-Registered GST Exempt"
        public static class PurchaseExStateRegisteredGSTExemptTaxableAmount
        {
            public const int ChargeGroupSettingId = 71;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseExState28.LedgerAccountId;
        }
        public static class PurchaseExStateUnRegisteredGSTExemptCGST
        {
            public const int ChargeGroupSettingId = 72;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnRegisteredGSTExemptSGST
        {
            public const int ChargeGroupSettingId = 73;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateRegisteredGSTExemptIGST
        {
            public const int ChargeGroupSettingId = 74;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 28.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState28.LedgerAccountId;
        }
        public static class PurchaseExStateRegisteredGSTExemptCESS
        {
            public const int ChargeGroupSettingId = 75;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion

        #region "Purchase ExState-UnRegistered GST 5%"
        public static class PurchaseExStateUnRegisteredGST5PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 76;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseExState5.LedgerAccountId;
        }
        public static class PurchaseExStateUnUnRegisteredGST5PerCGST
        {
            public const int ChargeGroupSettingId = 77;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnUnRegisteredGST5PerSGST
        {
            public const int ChargeGroupSettingId = 78;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnRegisteredGST5PerIGST
        {
            public const int ChargeGroupSettingId = 79;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 5.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState5.LedgerAccountId;
        }
        public static class PurchaseExStateUnRegisteredGST5PerCESS
        {
            public const int ChargeGroupSettingId = 80;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase ExState-UnRegistered GST 12%"
        public static class PurchaseExStateUnRegisteredGST12PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 81;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseExState12.LedgerAccountId;
        }
        public static class PurchaseExStateUnUnRegisteredGST12PerCGST
        {
            public const int ChargeGroupSettingId = 82;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnUnRegisteredGST12PerSGST
        {
            public const int ChargeGroupSettingId = 83;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnRegisteredGST12PerIGST
        {
            public const int ChargeGroupSettingId = 84;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 12.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState12.LedgerAccountId;
        }
        public static class PurchaseExStateUnRegisteredGST12PerCESS
        {
            public const int ChargeGroupSettingId = 85;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase ExState-UnRegistered GST 18%"
        public static class PurchaseExStateUnRegisteredGST18PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 86;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseExState18.LedgerAccountId;
        }
        public static class PurchaseExStateUnUnRegisteredGST18PerCGST
        {
            public const int ChargeGroupSettingId = 87;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnUnRegisteredGST18PerSGST
        {
            public const int ChargeGroupSettingId = 88;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnRegisteredGST18PerIGST
        {
            public const int ChargeGroupSettingId = 89;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 18.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState18.LedgerAccountId;
        }
        public static class PurchaseExStateUnRegisteredGST18PerCESS
        {
            public const int ChargeGroupSettingId = 90;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase ExState-UnRegistered GST 28%"
        public static class PurchaseExStateUnRegisteredGST28PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 91;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseExState28.LedgerAccountId;
        }
        public static class PurchaseExStateUnUnRegisteredGST28PerCGST
        {
            public const int ChargeGroupSettingId = 92;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnUnRegisteredGST28PerSGST
        {
            public const int ChargeGroupSettingId = 93;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnRegisteredGST28PerIGST
        {
            public const int ChargeGroupSettingId = 94;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 28.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState28.LedgerAccountId;
        }
        public static class PurchaseExStateUnRegisteredGST28PerCESS
        {
            public const int ChargeGroupSettingId = 95;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Purchase ExState-UnRegistered GST Exempt"
        public static class PurchaseExStateUnRegisteredGSTExemptTaxableAmount
        {
            public const int ChargeGroupSettingId = 96;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTPurchaseExState28.LedgerAccountId;
        }
        public static class PurchaseExStateUnUnRegisteredGSTExemptCGST
        {
            public const int ChargeGroupSettingId = 97;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnUnRegisteredGSTExemptSGST
        {
            public const int ChargeGroupSettingId = 98;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class PurchaseExStateUnRegisteredGSTExemptIGST
        {
            public const int ChargeGroupSettingId = 99;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 28.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState28.LedgerAccountId;
        }
        public static class PurchaseExStateUnRegisteredGSTExemptCESS
        {
            public const int ChargeGroupSettingId = 100;
            public const int ProcessId = ProcessConstants.Purchase.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion





        #region "Sale State Registered GST 5%"
        public static class SaleStateRegisteredGST5PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 101;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleState5.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST5PerCGST
        {
            public const int ChargeGroupSettingId = 102;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 2.5000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST5PerSGST
        {
            public const int ChargeGroupSettingId = 103;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 2.5000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST5PerIGST
        {
            public const int ChargeGroupSettingId = 104;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateRegisteredGST5PerCESS
        {
            public const int ChargeGroupSettingId = 105;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale State Registered GST 12%"
        public static class SaleStateRegisteredGST12PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 106;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleState12.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST12PerCGST
        {
            public const int ChargeGroupSettingId = 107;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 6.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState6.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST12PerSGST
        {
            public const int ChargeGroupSettingId = 108;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 6.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState6.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST12PerIGST
        {
            public const int ChargeGroupSettingId = 109;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateRegisteredGST12PerCESS
        {
            public const int ChargeGroupSettingId = 110;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }

        #endregion
        #region "Sale State Registered GST 18%"
        public static class SaleStateRegisteredGST18PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 111;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleState18.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST18PerCGST
        {
            public const int ChargeGroupSettingId = 112;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 9.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState9.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST18PerSGST
        {
            public const int ChargeGroupSettingId = 113;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 9.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState9.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST18PerIGST
        {
            public const int ChargeGroupSettingId = 114;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateRegisteredGST18PerCESS
        {
            public const int ChargeGroupSettingId = 115;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale State Registered GST 28%"
        public static class SaleStateRegisteredGST28PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 116;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleState28.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST28PerCGST
        {
            public const int ChargeGroupSettingId = 117;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 14.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState14.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST28PerSGST
        {
            public const int ChargeGroupSettingId = 118;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 14.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState14.LedgerAccountId;
        }
        public static class SaleStateRegisteredGST28PerIGST
        {
            public const int ChargeGroupSettingId = 119;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateRegisteredGST28PerCESS
        {
            public const int ChargeGroupSettingId = 120;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale State Registered GST Exempt"
        public static class SaleStateRegisteredGSTExemptTaxableAmount
        {
            public const int ChargeGroupSettingId = 121;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleState28.LedgerAccountId;
        }
        public static class SaleStateRegisteredGSTExemptCGST
        {
            public const int ChargeGroupSettingId = 122;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateRegisteredGSTExemptSGST
        {
            public const int ChargeGroupSettingId = 123;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateRegisteredGSTExemptIGST
        {
            public const int ChargeGroupSettingId = 124;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateRegisteredGSTExemptCESS
        {
            public const int ChargeGroupSettingId = 125;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion

        #region "Sale State Un-Registered GST 5%"
        public static class SaleStateUnRegisteredGST5PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 126;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleState5.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST5PerCGST
        {
            public const int ChargeGroupSettingId = 127;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 2.5000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST5PerSGST
        {
            public const int ChargeGroupSettingId = 128;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 2.5000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST5PerIGST
        {
            public const int ChargeGroupSettingId = 129;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateUnRegisteredGST5PerCESS
        {
            public const int ChargeGroupSettingId = 130;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale State Un-Registered GST 12%"
        public static class SaleStateUnRegisteredGST12PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 131;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleState12.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST12PerCGST
        {
            public const int ChargeGroupSettingId = 132;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 6;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState6.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST12PerSGST
        {
            public const int ChargeGroupSettingId = 133;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 6;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState6.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST12PerIGST
        {
            public const int ChargeGroupSettingId = 134;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateUnRegisteredGST12PerCESS
        {
            public const int ChargeGroupSettingId = 135;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale State Un-Registered GST 18%"
        public static class SaleStateUnRegisteredGST18PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 136;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleState18.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST18PerCGST
        {
            public const int ChargeGroupSettingId = 137;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 9;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState9.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST18PerSGST
        {
            public const int ChargeGroupSettingId = 138;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 9;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState9.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST18PerIGST
        {
            public const int ChargeGroupSettingId = 139;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateUnRegisteredGST18PerCESS
        {
            public const int ChargeGroupSettingId = 140;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale State Un-Registered GST 28%"
        public static class SaleStateUnRegisteredGST28PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 141;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleState28.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST28PerCGST
        {
            public const int ChargeGroupSettingId = 142;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 14;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST28PerSGST
        {
            public const int ChargeGroupSettingId = 143;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 14;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGST28PerIGST
        {
            public const int ChargeGroupSettingId = 144;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleStateUnRegisteredGST28PerCESS
        {
            public const int ChargeGroupSettingId = 145;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale State Un-Registered GST Exempt"
        public static class SaleStateUnRegisteredGSTExemptTaxableAmount
        {
            public const int ChargeGroupSettingId = 146;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleState28.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGSTExemptCGST
        {
            public const int ChargeGroupSettingId = 147;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.CGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGSTExemptSGST
        {
            public const int ChargeGroupSettingId = 148;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.SGSTInputState2AndHalf.LedgerAccountId;
        }
        public static class SaleStateUnRegisteredGSTExemptIGST
        {
            public const int ChargeGroupSettingId = 149;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }

        public static class SaleStateUnRegisteredGSTExemptCESS
        {
            public const int ChargeGroupSettingId = 150;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }

        #endregion

        #region "Sale ExState-Registered GST 5%"
        public static class SaleExStateRegisteredGST5PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 151;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleExState5.LedgerAccountId;
        }
        public static class SaleExStateUnRegisteredGST5PerCGST
        {
            public const int ChargeGroupSettingId = 152;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnRegisteredGST5PerSGST
        {
            public const int ChargeGroupSettingId = 153;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateRegisteredGST5PerIGST
        {
            public const int ChargeGroupSettingId = 154;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 5.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState5.LedgerAccountId;
        }
        public static class SaleExStateRegisteredGST5PerCESS
        {
            public const int ChargeGroupSettingId = 155;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale ExState-Registered GST 12%"
        public static class SaleExStateRegisteredGST12PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 156;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleExState12.LedgerAccountId;
        }
        public static class SaleExStateUnRegisteredGST12PerCGST
        {
            public const int ChargeGroupSettingId = 157;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnRegisteredGST12PerSGST
        {
            public const int ChargeGroupSettingId = 158;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateRegisteredGST12PerIGST
        {
            public const int ChargeGroupSettingId = 159;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 12.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState12.LedgerAccountId;
        }
        public static class SaleExStateRegisteredGST12PerCESS
        {
            public const int ChargeGroupSettingId = 160;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale ExState-Registered GST 18%"
        public static class SaleExStateRegisteredGST18PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 161;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleExState18.LedgerAccountId;
        }
        public static class SaleExStateUnRegisteredGST18PerCGST
        {
            public const int ChargeGroupSettingId = 162;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnRegisteredGST18PerSGST
        {
            public const int ChargeGroupSettingId = 163;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateRegisteredGST18PerIGST
        {
            public const int ChargeGroupSettingId = 164;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 18.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState18.LedgerAccountId;
        }
        public static class SaleExStateRegisteredGST18PerCESS
        {
            public const int ChargeGroupSettingId = 165;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale ExState-Registered GST 28%"
        public static class SaleExStateRegisteredGST28PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 166;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleExState28.LedgerAccountId;
        }
        public static class SaleExStateUnRegisteredGST28PerCGST
        {
            public const int ChargeGroupSettingId = 167;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnRegisteredGST28PerSGST
        {
            public const int ChargeGroupSettingId = 168;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateRegisteredGST28PerIGST
        {
            public const int ChargeGroupSettingId = 169;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 28.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState28.LedgerAccountId;
        }
        public static class SaleExStateRegisteredGST28PerCESS
        {
            public const int ChargeGroupSettingId = 170;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale ExState-Registered GST Exempt"
        public static class SaleExStateRegisteredGSTExemptTaxableAmount
        {
            public const int ChargeGroupSettingId = 171;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleExState28.LedgerAccountId;
        }
        public static class SaleExStateUnRegisteredGSTExemptCGST
        {
            public const int ChargeGroupSettingId = 172;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnRegisteredGSTExemptSGST
        {
            public const int ChargeGroupSettingId = 173;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateRegisteredGSTExemptIGST
        {
            public const int ChargeGroupSettingId = 174;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 28.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState28.LedgerAccountId;
        }
        public static class SaleExStateRegisteredGSTExemptCESS
        {
            public const int ChargeGroupSettingId = 175;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion

        #region "Sale ExState-UnRegistered GST 5%"
        public static class SaleExStateUnRegisteredGST5PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 176;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleExState5.LedgerAccountId;
        }
        public static class SaleExStateUnUnRegisteredGST5PerCGST
        {
            public const int ChargeGroupSettingId = 177;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnUnRegisteredGST5PerSGST
        {
            public const int ChargeGroupSettingId = 178;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnRegisteredGST5PerIGST
        {
            public const int ChargeGroupSettingId = 179;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 5.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState5.LedgerAccountId;
        }
        public static class SaleExStateUnRegisteredGST5PerCESS
        {
            public const int ChargeGroupSettingId = 180;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST5Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale ExState-UnRegistered GST 12%"
        public static class SaleExStateUnRegisteredGST12PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 181;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleExState12.LedgerAccountId;
        }
        public static class SaleExStateUnUnRegisteredGST12PerCGST
        {
            public const int ChargeGroupSettingId = 182;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnUnRegisteredGST12PerSGST
        {
            public const int ChargeGroupSettingId = 183;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnRegisteredGST12PerIGST
        {
            public const int ChargeGroupSettingId = 184;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 12.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState12.LedgerAccountId;
        }
        public static class SaleExStateUnRegisteredGST12PerCESS
        {
            public const int ChargeGroupSettingId = 185;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST12Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale ExState-UnRegistered GST 18%"
        public static class SaleExStateUnRegisteredGST18PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 186;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleExState18.LedgerAccountId;
        }
        public static class SaleExStateUnUnRegisteredGST18PerCGST
        {
            public const int ChargeGroupSettingId = 187;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnUnRegisteredGST18PerSGST
        {
            public const int ChargeGroupSettingId = 188;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnRegisteredGST18PerIGST
        {
            public const int ChargeGroupSettingId = 189;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 18.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState18.LedgerAccountId;
        }
        public static class SaleExStateUnRegisteredGST18PerCESS
        {
            public const int ChargeGroupSettingId = 190;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST18Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale ExState-UnRegistered GST 28%"
        public static class SaleExStateUnRegisteredGST28PerTaxableAmount
        {
            public const int ChargeGroupSettingId = 191;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleExState28.LedgerAccountId;
        }
        public static class SaleExStateUnUnRegisteredGST28PerCGST
        {
            public const int ChargeGroupSettingId = 192;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnUnRegisteredGST28PerSGST
        {
            public const int ChargeGroupSettingId = 193;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnRegisteredGST28PerIGST
        {
            public const int ChargeGroupSettingId = 194;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 28.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState28.LedgerAccountId;
        }
        public static class SaleExStateUnRegisteredGST28PerCESS
        {
            public const int ChargeGroupSettingId = 195;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GST28Per.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
        #region "Sale ExState-UnRegistered GST Exempt"
        public static class SaleExStateUnRegisteredGSTExemptTaxableAmount
        {
            public const int ChargeGroupSettingId = 196;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.TaxableAmount.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 100.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.GSTSaleExState28.LedgerAccountId;
        }
        public static class SaleExStateUnUnRegisteredGSTExemptCGST
        {
            public const int ChargeGroupSettingId = 197;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnUnRegisteredGSTExemptSGST
        {
            public const int ChargeGroupSettingId = 198;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.SGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        public static class SaleExStateUnRegisteredGSTExemptIGST
        {
            public const int ChargeGroupSettingId = 199;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.IGST.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 28.0000;
            public static readonly int? ChargeLedgerAccountId = LedgerAccountConstants.IGSTInputExState28.LedgerAccountId;
        }
        public static class SaleExStateUnRegisteredGSTExemptCESS
        {
            public const int ChargeGroupSettingId = 200;
            public const int ProcessId = ProcessConstants.Sale.ProcessId;
            public const int ChargeTypeId = ChargeTypeConstants.CESS.ChargeTypeId;
            public const int ChargeGroupPersonId = ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonId;
            public const int ChargeGroupProductId = ChargeGroupProductConstants.GSTExempt.ChargeGroupProductId;
            public const double ChargePer = 0;
            public static readonly int? ChargeLedgerAccountId = null;
        }
        #endregion
    }
}