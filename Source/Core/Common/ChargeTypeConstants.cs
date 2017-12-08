using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    public enum TaxTypeConstants
    {
        SalesTax = 1
    }

    public enum ChargeTypesEnum
    {

        Amount=5,
        SubTotal=4,
        RoundOFF=3,
        SalesATax=2,
        SalesTax=1
    }
    public enum AddDeductEnum
    {
        Deduct,
        Add,
        OverRide
    }
    public enum RateTypeEnum
    {
        Rate=1,
        Percentage=2,
        Na=3,
    }
    public class ChargeConstants
    {
        public const string Incentive = "Incentive";
        public const string Penalty = "Penalty";
    }

    public class ChargeTypeConstants
    {
        public const string SalesTaxableAmount = "Sales Taxable Amount";
    }

    public enum DivisionEnum
    {
        KELIM = 1,
        KNOTTED = 2,
        MAIN = 3,
        SAMPLE = 4,
        SHAG = 5,
        TUFTED = 6,
    }

    public enum ProductTypeAttributeTypess
    {
        Binding = 1,
        Gachhai = 2,
        PattiMuraiDurry=3,
    }

    public class ProductTypeAttributeValuess
    {
        public const string NA = "N/A";
        public const string Length = "Length";
        public const string Width = "Width";
        public const string LengthAndWidth = "Length + Width";
    }

    public class LedgerAccounts
    {
        public const string CashAc = "Cash A/c";
        public const string BankAc = "Bank A/c";
    }

}
