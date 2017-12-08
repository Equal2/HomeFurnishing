using Jobs.Constants.Calculation;
using Jobs.Constants.Charge;
using Jobs.Constants.ChargeType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.CalculationProduct
{
    public static class CalculationProductConstants
    {
        public static class Calculation_GrossAmount
        {
            public const int CalculationProductId = 1;
            public static readonly int CalculationId = CalculationConstants.Calculation.CalculationId;
            public static readonly int Sr = 10;
            public static readonly int ChargeId = ChargeConstants.GrossAmount.ChargeId;
            public static readonly byte AddDeduct = ((int)AddDeductEnum.Add);
            public static readonly bool AffectCost = true;
            public static readonly int ChargeTypeId = ChargeTypeConstants.Amount.ChargeTypeId;
            public static readonly int CalculateOnId = ChargeConstants.GrossAmount.ChargeId;
            public static readonly byte RateType = ((byte)(RateTypeConstants.NA));
            public static readonly bool IncludedInBase = false;
            public static readonly int? ParentChargeId = null;
            public static readonly bool IsVisible = true;
            public static readonly bool IsActive = true;
            public static readonly string IncludedCharges = null;
            public static readonly string IncludedChargesCalculation = null;
        }
        public static class Calculation_OtherCharges
        {
            public const int CalculationProductId = 3;
            public static readonly int CalculationId = CalculationConstants.Calculation.CalculationId;
            public static readonly int Sr = 30;
            public static readonly int ChargeId = ChargeConstants.OtherCharge.ChargeId;
            public static readonly byte AddDeduct = ((int)AddDeductEnum.Add);
            public static readonly bool AffectCost = true;
            public static readonly int? ChargeTypeId = null;
            public static readonly int CalculateOnId = ChargeConstants.GrossAmount.ChargeId;
            public static readonly byte RateType = ((byte)(RateTypeConstants.Rate));
            public static readonly bool IncludedInBase = false;
            public static readonly int? ParentChargeId = null;
            public static readonly bool IsVisible = true;
            public static readonly bool IsActive = true;
            public static readonly string IncludedCharges = null;
            public static readonly string IncludedChargesCalculation = null;
        }
        public static class Calculation_Discount
        {
            public const int CalculationProductId = 2;
            public static readonly int CalculationId = CalculationConstants.Calculation.CalculationId;
            public static readonly int Sr = 20;
            public static readonly int ChargeId = ChargeConstants.Discount.ChargeId;
            public static readonly byte AddDeduct = ((int)AddDeductEnum.Deduct);
            public static readonly bool AffectCost = true;
            public static readonly int? ChargeTypeId = null;
            public static readonly int CalculateOnId = ChargeConstants.GrossAmount.ChargeId;
            public static readonly byte RateType = ((byte)(RateTypeConstants.Rate));
            public static readonly bool IncludedInBase = false;
            public static readonly int? ParentChargeId = null;
            public static readonly bool IsVisible = true;
            public static readonly bool IsActive = true;
            public static readonly string IncludedCharges = null;
            public static readonly string IncludedChargesCalculation = null;
        }

        public static class Calculation_TaxableAmount
        {
            public const int CalculationProductId = 4;
            public static readonly int CalculationId = CalculationConstants.Calculation.CalculationId;
            public static readonly int Sr = 40;
            public static readonly int ChargeId = ChargeConstants.TaxableAmount.ChargeId;
            public static readonly byte? AddDeduct = null;
            public static readonly bool AffectCost = true;
            public static readonly int ChargeTypeId = ChargeTypeConstants.SubTotal.ChargeTypeId;
            public static readonly int? CalculateOnId = null;
            public static readonly byte RateType = ((byte)(RateTypeConstants.NA));
            public static readonly bool IncludedInBase = false;
            public static readonly int? ParentChargeId = null;
            public static readonly bool IsVisible = true;
            public static readonly bool IsActive = true;
            public static readonly string IncludedCharges = null;
            public static readonly string IncludedChargesCalculation = null;
        }
        public static class Calculation_Tax1
        {
            public const int CalculationProductId = 5;
            public static readonly int CalculationId = CalculationConstants.Calculation.CalculationId;
            public static readonly int Sr = 50;
            public static readonly int ChargeId = ChargeConstants.Tax1.ChargeId;
            public static readonly byte AddDeduct = ((int)AddDeductEnum.Add);
            public static readonly bool AffectCost = true;
            public static readonly int? ChargeTypeId = null;
            public static readonly int CalculateOnId = ChargeConstants.TaxableAmount.ChargeId;
            public static readonly byte RateType = ((byte)(RateTypeConstants.Percentage));
            public static readonly bool IncludedInBase = false;
            public static readonly int? ParentChargeId = null;
            public static readonly bool IsVisible = true;
            public static readonly bool IsActive = true;
            public static readonly string IncludedCharges = null;
            public static readonly string IncludedChargesCalculation = null;
        }
        public static class Calculation_Tax2
        {
            public const int CalculationProductId = 6;
            public static readonly int CalculationId = CalculationConstants.Calculation.CalculationId;
            public static readonly int Sr = 60;
            public static readonly int ChargeId = ChargeConstants.Tax2.ChargeId;
            public static readonly byte AddDeduct = ((int)AddDeductEnum.Add);
            public static readonly bool AffectCost = true;
            public static readonly int? ChargeTypeId = null;
            public static readonly int CalculateOnId = ChargeConstants.TaxableAmount.ChargeId;
            public static readonly byte RateType = ((byte)(RateTypeConstants.Percentage));
            public static readonly bool IncludedInBase = false;
            public static readonly int? ParentChargeId = null;
            public static readonly bool IsVisible = true;
            public static readonly bool IsActive = true;
            public static readonly string IncludedCharges = null;
            public static readonly string IncludedChargesCalculation = null;
        }
        public static class Calculation_Tax3
        {
            public const int CalculationProductId = 7;
            public static readonly int CalculationId = CalculationConstants.Calculation.CalculationId;
            public static readonly int Sr = 70;
            public static readonly int ChargeId = ChargeConstants.Tax3.ChargeId;
            public static readonly byte AddDeduct = ((int)AddDeductEnum.Add);
            public static readonly bool AffectCost = true;
            public static readonly int? ChargeTypeId = null;
            public static readonly int CalculateOnId = ChargeConstants.TaxableAmount.ChargeId;
            public static readonly byte RateType = ((byte)(RateTypeConstants.Percentage));
            public static readonly bool IncludedInBase = false;
            public static readonly int? ParentChargeId = null;
            public static readonly bool IsVisible = true;
            public static readonly bool IsActive = true;
            public static readonly string IncludedCharges = null;
            public static readonly string IncludedChargesCalculation = null;
        }
        public static class Calculation_Tax4
        {
            public const int CalculationProductId = 8;
            public static readonly int CalculationId = CalculationConstants.Calculation.CalculationId;
            public static readonly int Sr = 80;
            public static readonly int ChargeId = ChargeConstants.Tax4.ChargeId;
            public static readonly byte AddDeduct = ((int)AddDeductEnum.Add);
            public static readonly bool AffectCost = true;
            public static readonly int? ChargeTypeId = null;
            public static readonly int CalculateOnId = ChargeConstants.TaxableAmount.ChargeId;
            public static readonly byte RateType = ((byte)(RateTypeConstants.Percentage));
            public static readonly bool IncludedInBase = false;
            public static readonly int? ParentChargeId = null;
            public static readonly bool IsVisible = true;
            public static readonly bool IsActive = true;
            public static readonly string IncludedCharges = null;
            public static readonly string IncludedChargesCalculation = null;
        }
        public static class Calculation_Tax5
        {
            public const int CalculationProductId = 9;
            public static readonly int CalculationId = CalculationConstants.Calculation.CalculationId;
            public static readonly int Sr = 90;
            public static readonly int ChargeId = ChargeConstants.Tax5.ChargeId;
            public static readonly byte AddDeduct = ((int)AddDeductEnum.Add);
            public static readonly bool AffectCost = true;
            public static readonly int? ChargeTypeId = null;
            public static readonly int CalculateOnId = ChargeConstants.TaxableAmount.ChargeId;
            public static readonly byte RateType = ((byte)(RateTypeConstants.Percentage));
            public static readonly bool IncludedInBase = false;
            public static readonly int? ParentChargeId = null;
            public static readonly bool IsVisible = true;
            public static readonly bool IsActive = true;
            public static readonly string IncludedCharges = null;
            public static readonly string IncludedChargesCalculation = null;
        }
        public static class Calculation_NetAmount
        {
            public const int CalculationProductId = 10;
            public static readonly int CalculationId = CalculationConstants.Calculation.CalculationId;
            public static readonly int Sr = 100;
            public static readonly int ChargeId = ChargeConstants.NetAmount.ChargeId;
            public static readonly byte? AddDeduct = null;
            public static readonly bool AffectCost = false;
            public static readonly int ChargeTypeId = ChargeTypeConstants.SubTotal.ChargeTypeId;
            public static readonly int CalculateOnId = ChargeConstants.GrossAmount.ChargeId;
            public static readonly byte RateType = ((byte)(RateTypeConstants.NA));
            public static readonly bool IncludedInBase = false;
            public static readonly int? ParentChargeId = null;
            public static readonly bool IsVisible = true;
            public static readonly bool IsActive = true;
            public static readonly string IncludedCharges = null;
            public static readonly string IncludedChargesCalculation = null;
        }
    }
}