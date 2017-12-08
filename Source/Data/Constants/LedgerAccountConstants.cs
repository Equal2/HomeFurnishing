using Jobs.Constants.LedgerAccountGroup;
using Jobs.Constants.Site;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.LedgerAccount
{
    public static class LedgerAccountConstants
    {
        public static class CHARGE
        {
            public const int LedgerAccountId = 10;
            public const string LedgerAccountName = "|CHARGE|";
            public const string LedgerAccountSuffix = "Charge";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.Provisions.LedgerAccountGroupId;
        }
        public static class PARTY
        {
            public const int LedgerAccountId = 20;
            public const string LedgerAccountName = "|PARTY|";
            public const string LedgerAccountSuffix = "Party";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.Provisions.LedgerAccountGroupId;
        }
        public static class BankAc
        {
            public const int LedgerAccountId = 30;
            public const string LedgerAccountName = "Bank A/c";
            public const string LedgerAccountSuffix = "BANK";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.BankAccounts.LedgerAccountGroupId;
        }

        public static class CGSTReverseChargeReserve
        {
            public const int LedgerAccountId = 80;
            public const string LedgerAccountName = "CGST Reverse Charge Reserve";
            public const string LedgerAccountSuffix = "CGST Reverse Charge Reserve";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.ReservesAndSurplus.LedgerAccountGroupId;
        }
        public static class DebitNoteAc
        {
            public const int LedgerAccountId = 90;
            public const string LedgerAccountName = "Debit Note A/c";
            public const string LedgerAccountSuffix = "Dnote";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.SuspenseAc.LedgerAccountGroupId;
        }
        public static class DiscountAc
        {
            public const int LedgerAccountId = 100;
            public const string LedgerAccountName = "Discount A/c";
            public const string LedgerAccountSuffix = "Discount";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DirectExpenses.LedgerAccountGroupId;
        }
        public static class FreightAc
        {
            public const int LedgerAccountId = 110;
            public const string LedgerAccountName = "Freight A/c";
            public const string LedgerAccountSuffix = "Frt";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.IndirectExpenses.LedgerAccountGroupId;
        }

        public static class GSTPurchaseState5
        {
            public const int LedgerAccountId = 190;
            public const string LedgerAccountName = "GST Purchase State 5%";
            public const string LedgerAccountSuffix = "GST Purchase State 5%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.PurchaseAccounts.LedgerAccountGroupId;
        }
        public static class GSTPurchaseState12
        {
            public const int LedgerAccountId = 160;
            public const string LedgerAccountName = "GST Purchase State 12%";
            public const string LedgerAccountSuffix = "GST Purchase State 12%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.PurchaseAccounts.LedgerAccountGroupId;
        }
        public static class GSTPurchaseState18
        {
            public const int LedgerAccountId = 170;
            public const string LedgerAccountName = "GST Purchase State 18%";
            public const string LedgerAccountSuffix = "GST Purchase State 18%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.PurchaseAccounts.LedgerAccountGroupId;
        }
        public static class GSTPurchaseState28
        {
            public const int LedgerAccountId = 180;
            public const string LedgerAccountName = "GST Purchase State 28%";
            public const string LedgerAccountSuffix = "GST Purchase State 28%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.PurchaseAccounts.LedgerAccountGroupId;
        }

        public static class GSTPurchaseExState5
        {
            public const int LedgerAccountId = 150;
            public const string LedgerAccountName = "GST Purchase Ex-State 5%";
            public const string LedgerAccountSuffix = "GST Purchase Ex-State 5%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.PurchaseAccounts.LedgerAccountGroupId;
        }
        public static class GSTPurchaseExState12
        {
            public const int LedgerAccountId = 120;
            public const string LedgerAccountName = "GST Purchase Ex-State 12%";
            public const string LedgerAccountSuffix = "GST Purchase Ex-State 12%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.PurchaseAccounts.LedgerAccountGroupId;
        }
        public static class GSTPurchaseExState18
        {
            public const int LedgerAccountId = 130;
            public const string LedgerAccountName = "GST Purchase Ex-State 18%";
            public const string LedgerAccountSuffix = "GST Purchase Ex-State 18%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.PurchaseAccounts.LedgerAccountGroupId;
        }
        public static class GSTPurchaseExState28
        {
            public const int LedgerAccountId = 140;
            public const string LedgerAccountName = "GST Purchase Ex-State 28%";
            public const string LedgerAccountSuffix = "GST Purchase Ex-State 28%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.PurchaseAccounts.LedgerAccountGroupId;
        }
        

        public static class CGSTInputState2AndHalf
        {
            public const int LedgerAccountId = 50;
            public const string LedgerAccountName = "CGST Input State 2.5%";
            public const string LedgerAccountSuffix = "CGST Input State 2.5%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class CGSTInputState6
        {
            public const int LedgerAccountId = 60;
            public const string LedgerAccountName = "CGST Input State 6%";
            public const string LedgerAccountSuffix = "CGST Input State 6%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class CGSTInputState9
        {
            public const int LedgerAccountId = 70;
            public const string LedgerAccountName = "CGST Input State 9%";
            public const string LedgerAccountSuffix = "CGST Input State 9%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class CGSTInputState14
        {
            public const int LedgerAccountId = 40;
            public const string LedgerAccountName = "CGST Input State 14%";
            public const string LedgerAccountSuffix = "CGST Input State 14%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class SGSTInputState2AndHalf
        {
            public const int LedgerAccountId = 310;
            public const string LedgerAccountName = "SGST Input State 2.5%";
            public const string LedgerAccountSuffix = "SGST Input State 2.5%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class SGSTInputState6
        {
            public const int LedgerAccountId = 320;
            public const string LedgerAccountName = "SGST Input State 6%";
            public const string LedgerAccountSuffix = "SGST Input State 6%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class SGSTInputState9
        {
            public const int LedgerAccountId = 330;
            public const string LedgerAccountName = "SGST Input State 9%";
            public const string LedgerAccountSuffix = "SGST Input State 9%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class SGSTInputState14
        {
            public const int LedgerAccountId = 300;
            public const string LedgerAccountName = "SGST Input State 14%";
            public const string LedgerAccountSuffix = "SGST Input State 14%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class IGSTInputExState5
        {
            public const int LedgerAccountId = 230;
            public const string LedgerAccountName = "IGST Input Ex-State 5%";
            public const string LedgerAccountSuffix = "IGST Input Ex-State 5%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class IGSTInputExState12
        {
            public const int LedgerAccountId = 200;
            public const string LedgerAccountName = "IGST Input Ex-State 12%";
            public const string LedgerAccountSuffix = "IGST Input Ex-State 12%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class IGSTInputExState18
        {
            public const int LedgerAccountId = 210;
            public const string LedgerAccountName = "IGST Input Ex-State 18%";
            public const string LedgerAccountSuffix = "IGST Input Ex-State 18%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class IGSTInputExState28
        {
            public const int LedgerAccountId = 220;
            public const string LedgerAccountName = "IGST Input Ex-State 28%";
            public const string LedgerAccountSuffix = "IGST Input Ex-State 28%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        
        public static class IncentiveAc
        {
            public const int LedgerAccountId = 240;
            public const string LedgerAccountName = "Incentive A/c";
            public const string LedgerAccountSuffix = "Incentive";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.BankAccounts.LedgerAccountGroupId;
        }
        public static class PenaltyAc
        {
            public const int LedgerAccountId = 250;
            public const string LedgerAccountName = "Penalty A/c";
            public const string LedgerAccountSuffix = "Incentive A/c";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.BankAccounts.LedgerAccountGroupId;
        }
        public static class PurchaseAc
        {
            public const int LedgerAccountId = 260;
            public const string LedgerAccountName = "Purchase A/c";
            public const string LedgerAccountSuffix = "Purchase";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.PurchaseAccounts.LedgerAccountGroupId;
        }
        public static class RoundOffAc
        {
            public const int LedgerAccountId = 270;
            public const string LedgerAccountName = "Round Off A/c";
            public const string LedgerAccountSuffix = "Rof";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.Provisions.LedgerAccountGroupId;
        }
        public static class SaleAc
        {
            public const int LedgerAccountId = 280;
            public const string LedgerAccountName = "Sale A/c";
            public const string LedgerAccountSuffix = "Sale";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.BankAccounts.LedgerAccountGroupId;
        }
        public static class SatAc
        {
            public const int LedgerAccountId = 290;
            public const string LedgerAccountName = "Sat A/c";
            public const string LedgerAccountSuffix = "Sat";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }

        public static class SGSTReverseChargeReserveAc
        {
            public const int LedgerAccountId = 340;
            public const string LedgerAccountName = "SGST Reverse Charge Reserve A/c";
            public const string LedgerAccountSuffix = "SGST Reverse Charge Reserve";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.ReservesAndSurplus.LedgerAccountGroupId;
        }
        public static class VatAc
        {
            public const int LedgerAccountId = 350;
            public const string LedgerAccountName = "Vat A/c";
            public const string LedgerAccountSuffix = "Vat";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }





        public static class GSTSaleState5
        {
            public const int LedgerAccountId = 360;
            public const string LedgerAccountName = "GST Sale State 5%";
            public const string LedgerAccountSuffix = "GST Sale State 5%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.SalesAccounts.LedgerAccountGroupId;
        }
        public static class GSTSaleState12
        {
            public const int LedgerAccountId = 370;
            public const string LedgerAccountName = "GST Sale State 12%";
            public const string LedgerAccountSuffix = "GST Sale State 12%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.SalesAccounts.LedgerAccountGroupId;
        }
        public static class GSTSaleState18
        {
            public const int LedgerAccountId = 380;
            public const string LedgerAccountName = "GST Sale State 18%";
            public const string LedgerAccountSuffix = "GST Sale State 18%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.SalesAccounts.LedgerAccountGroupId;
        }
        public static class GSTSaleState28
        {
            public const int LedgerAccountId = 390;
            public const string LedgerAccountName = "GST Sale State 28%";
            public const string LedgerAccountSuffix = "GST Sale State 28%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.SalesAccounts.LedgerAccountGroupId;
        }

        public static class GSTSaleExState5
        {
            public const int LedgerAccountId = 400;
            public const string LedgerAccountName = "GST Sale Ex-State 5%";
            public const string LedgerAccountSuffix = "GST Sale Ex-State 5%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.SalesAccounts.LedgerAccountGroupId;
        }
        public static class GSTSaleExState12
        {
            public const int LedgerAccountId = 410;
            public const string LedgerAccountName = "GST Sale Ex-State 12%";
            public const string LedgerAccountSuffix = "GST Sale Ex-State 12%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.SalesAccounts.LedgerAccountGroupId;
        }
        public static class GSTSaleExState18
        {
            public const int LedgerAccountId = 420;
            public const string LedgerAccountName = "GST Sale Ex-State 18%";
            public const string LedgerAccountSuffix = "GST Sale Ex-State 18%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.SalesAccounts.LedgerAccountGroupId;
        }
        public static class GSTSaleExState28
        {
            public const int LedgerAccountId = 430;
            public const string LedgerAccountName = "GST Sale Ex-State 28%";
            public const string LedgerAccountSuffix = "GST Sale Ex-State 28%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.SalesAccounts.LedgerAccountGroupId;
        }


        public static class CGSTOutputState2AndHalf
        {
            public const int LedgerAccountId = 440;
            public const string LedgerAccountName = "CGST Output State 2.5%";
            public const string LedgerAccountSuffix = "CGST Output State 2.5%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class CGSTOutputState6
        {
            public const int LedgerAccountId = 450;
            public const string LedgerAccountName = "CGST Output State 6%";
            public const string LedgerAccountSuffix = "CGST Output State 6%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class CGSTOutputState9
        {
            public const int LedgerAccountId = 460;
            public const string LedgerAccountName = "CGST Output State 9%";
            public const string LedgerAccountSuffix = "CGST Output State 9%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class CGSTOutputState14
        {
            public const int LedgerAccountId = 470;
            public const string LedgerAccountName = "CGST Output State 14%";
            public const string LedgerAccountSuffix = "CGST Output State 14%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class SGSTOutputState2AndHalf
        {
            public const int LedgerAccountId = 480;
            public const string LedgerAccountName = "SGST Output State 2.5%";
            public const string LedgerAccountSuffix = "SGST Output State 2.5%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class SGSTOutputState6
        {
            public const int LedgerAccountId = 490;
            public const string LedgerAccountName = "SGST Output State 6%";
            public const string LedgerAccountSuffix = "SGST Output State 6%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class SGSTOutputState9
        {
            public const int LedgerAccountId = 500;
            public const string LedgerAccountName = "SGST Output State 9%";
            public const string LedgerAccountSuffix = "SGST Output State 9%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class SGSTOutputState14
        {
            public const int LedgerAccountId = 510;
            public const string LedgerAccountName = "SGST Output State 14%";
            public const string LedgerAccountSuffix = "SGST Output State 14%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class IGSTOutputExState5
        {
            public const int LedgerAccountId = 520;
            public const string LedgerAccountName = "IGST Output Ex-State 5%";
            public const string LedgerAccountSuffix = "IGST Output Ex-State 5%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class IGSTOutputExState12
        {
            public const int LedgerAccountId = 530;
            public const string LedgerAccountName = "IGST Output Ex-State 12%";
            public const string LedgerAccountSuffix = "IGST Output Ex-State 12%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class IGSTOutputExState18
        {
            public const int LedgerAccountId = 540;
            public const string LedgerAccountName = "IGST Output Ex-State 18%";
            public const string LedgerAccountSuffix = "IGST Output Ex-State 18%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }
        public static class IGSTOutputExState28
        {
            public const int LedgerAccountId = 550;
            public const string LedgerAccountName = "IGST Output Ex-State 28%";
            public const string LedgerAccountSuffix = "IGST Output Ex-State 28%";
            public const int LedgerAccountGroupId = LedgerAccountGroupConstants.DutiesAndTaxes.LedgerAccountGroupId;
        }

    }
}