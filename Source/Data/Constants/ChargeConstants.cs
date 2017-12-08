using Jobs.Constants.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.Charge
{
    public static class ChargeConstants
    {
        public static class GrossAmount
        {
            public const int ChargeId = 1;
            public const string ChargeCode = "Gross Amount";
            public const string ChargeName = "Gross Amount";
        }
        public static class Discount
        {
            public const int ChargeId = 2;
            public const string ChargeCode = "Discount";
            public const string ChargeName = "Discount";
        }
        public static class OtherCharge
        {
            public const int ChargeId = 3;
            public const string ChargeCode = "Other Charge";
            public const string ChargeName = "Other Charge";
        }
        public static class TaxableAmount
        {
            public const int ChargeId = 4;
            public const string ChargeCode = "Taxable Amount";
            public const string ChargeName = "Taxable Amount";
        }
        public static class Tax1
        {
            public const int ChargeId = 5;
            public const string ChargeCode = "Tax1";
            public const string ChargeName = "Tax1";
        }
        public static class Tax2
        {
            public const int ChargeId = 6;
            public const string ChargeCode = "Tax2";
            public const string ChargeName = "Tax2";
        }
        public static class Tax3
        {
            public const int ChargeId = 7;
            public const string ChargeCode = "Tax3";
            public const string ChargeName = "Tax3";
        }
        public static class Tax4
        {
            public const int ChargeId = 8;
            public const string ChargeCode = "Tax4";
            public const string ChargeName = "Tax4";
        }
        public static class Tax5
        {
            public const int ChargeId = 9;
            public const string ChargeCode = "Tax5";
            public const string ChargeName = "Tax5";
        }
        public static class SubTotal
        {
            public const int ChargeId = 10;
            public const string ChargeCode = "Sub Total";
            public const string ChargeName = "Sub Total";
        }
        public static class OtherAdditions
        {
            public const int ChargeId = 11;
            public const string ChargeCode = "Other Additions";
            public const string ChargeName = "Other Additions";
        }
        public static class OtherDeductions
        {
            public const int ChargeId = 12;
            public const string ChargeCode = "Other Deductions";
            public const string ChargeName = "Other Deductions";
        }
        public static class SubTotal2
        {
            public const int ChargeId = 13;
            public const string ChargeCode = "Sub Total2";
            public const string ChargeName = "Sub Total2";
        }
        public static class RoundOff
        {
            public const int ChargeId = 14;
            public const string ChargeCode = "Round Off";
            public const string ChargeName = "Round Off";
        }
        public static class NetAmount
        {
            public const int ChargeId = 15;
            public const string ChargeCode = "Net Amount";
            public const string ChargeName = "Net Amount";
        }
    }
    public enum AddDeductEnum
    {
        Deduct,
        Add,
        OverRide
    }
    public enum RateTypeConstants
    {
        Rate = 1,
        Percentage = 2,
        NA = 3,
    }
}