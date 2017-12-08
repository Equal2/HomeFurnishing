



var FooterFields = [];
var ProductFields = [];
var ProductCharges = [];
var ChargeTypeEnum = {
    Amount: 5,
    SUBTOTAL: 4,
    ROUNDOFF: 3,
    SALESATAX: 2,
    SALESTAX: 1,
};
var RateTypeEnum = {
    RATE: 1,
    PERCENTAGE: 2,
    NA: 3,
};


var AddDeductEnum = {
    Deduct: 0,
    Add: 1,
    OverRide: 2,
}

function AddCalculationFields(DocHeaderId, DebugMode, CalculationId, HeaderCTable, LineCTable, MaxLinId, DocumentType, SiteId, DivisionId) {

    $.ajax({
        async: false,
        cache: false,
        type: 'POST',
        url: "/TaxCalculation/GetCalculationFieldsProduct/",
        data: { HeaderId: DocHeaderId, CalculationId: CalculationId, LineChargeTable: LineCTable, MaxLineId: MaxLinId, DocTypeId: DocumentType, SiteId: SiteId, DivisionId: DivisionId },
        success: function (data) {
            ProductFields = data;
            DrawProductFields(DebugMode);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrive calculationproduct' + thrownError);
        },
    });
    $.ajax({
        cache: false,
        type: 'POST',
        url: "/TaxCalculation/GetCalculationFieldsFooter/",
        data: { HeaderId: DocHeaderId, CalculationId: CalculationId, HeaderChargeTable: HeaderCTable, LineChargeTable: LineCTable, DocTypeId: DocumentType, SiteId: SiteId, DivisionId: DivisionId },
        success: function (data) {
            FooterFields = data;
            DrawFooterFields(DebugMode);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrive calculation footer' + thrownError);
        },
    });

};



function AddCalculationFieldsEdit(DocHeaderId, DocLineId, DebugMode, HeaderCTable, LineCTable) {

    $.ajax({
        async: false,
        cache: false,
        type: 'POST',
        url: "/TaxCalculation/GetProductCharge/",
        data: { LineId: DocLineId, LineTable: LineCTable },
        success: function (data) {
            ProductFields = data;
            DrawProductFields(DebugMode);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrive calculationproduct' + thrownError);
        },
    });
    $.ajax({
        cache: false,
        type: 'POST',
        url: "/TaxCalculation/GetHeaderCharge/",
        data: { HeaderId: DocHeaderId, HeaderTable: HeaderCTable, LineTable: LineCTable },
        success: function (data) {
            FooterFields = data;
            DrawFooterFields(DebugMode);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrive calculation footer' + thrownError);
        },
    });

};

function DrawProductFields(DebugMode) {
    var temp = "";
    for (var i = 0; i < ProductFields.length; i++) {
        if (ProductFields[i].RateType == RateTypeEnum.NA) {
            temp += "<div class='col-md-6' style='display:" + (ProductFields[i].IsVisible ? "" : "none") + "'>"
                + "      <div class='form-group'>"
                + "             <label class='control-label col-md-4 col-sm-2'>" + ProductFields[i].ChargeName + "</label> <div class='col-md-7 col-sm-10'><input class='form-control Calculation text-right' id='CALL_" + ProductFields[i].ChargeCode + "' name='linecharges[" + i + "].Amount' type='text' value='" + ProductFields[i].Amount + "'></div> "
                //+ "             <label class='control-label col-xs-4'>" + ProductFields[i].ChargeName + "</label> <div class='col-xs-7'><input class='form-control Calculation text-right' id='CALL_" + ProductFields[i].ChargeCode + "' name='CALL_" + ProductFields[i].ChargeCode + "' type='text' value='" + ProductFields[i].Amount + "'></div> "
                + "     </div>"
                + " </div>"
        }
        else {
            temp += "<div class='col-md-6' style='display:" + (ProductFields[i].IsVisible || DebugMode ? "" : "none") + "'>"
            + "      <div class='form-group'>"
            + "         <label class='control-label col-md-4 col-sm-2'>" + ProductFields[i].ChargeName + "</label> "
            + "                <div class='col-md-7 col-sm-10'>"
            + "                <div class='col-xs-4' style='padding:0px;'>"
            + "                    <input class='form-control cusrightnormal Calculation text-right' id='CALL_" + ProductFields[i].ChargeCode + "RATE' name='linecharges[" + i + "].RATE'   type='text' value='" + ProductFields[i].Rate + "' " + (ProductFields[i].ChargeTypeId == null ? "" : "readonly") + " />"
            //+ "                    <input class='form-control cusrightnormal Calculation text-right' id='CALL_" + ProductFields[i].ChargeCode + "RATE' name='CALL_" + ProductFields[i].ChargeCode + "RATE'   type='text' value='" + ProductFields[i].Rate + "'   />"
            + "                 </div>"
            + "                  <div></div>"
            + "                 <div class='col-xs-8' style='padding:0px'>"
            //+ "                     <input class='form-control cusleftnormal Calculation text-right' id='CALL_" + ProductFields[i].ChargeCode + "' name='CALL_" + ProductFields[i].ChargeCode + "'   type='text' value='" + ProductFields[i].Amount + "'  />"
            + "                     <input class='form-control cusleftnormal Calculation text-right' id='CALL_" + ProductFields[i].ChargeCode + "' name='linecharges[" + i + "].Amount'   type='text' value='" + ProductFields[i].Amount + "' " + (ProductFields[i].ChargeTypeId == null ? "" : "readonly") + "  />"
            + "                 </div>"
            + "          </div>"
            + "     </div>"
            + "     </div>"
        }
        temp += "<div class='col-md-6' style='display:" + (DebugMode ? ("") : ("none")) + "'>"
                + "      <div class='form-group'>"
                + "         <label class='control-label col-xs-4'>X" + ProductFields[i].ChargeName + "</label> <div class='col-xs-7'><input class='form-control Calculation text-right' id='XCALL_" + ProductFields[i].ChargeCode + "' name='XCALL_" + ProductFields[i].ChargeCode + "' type='text' value='" + ProductFields[i].Amount + "' /></div> "
                + "     </div>"
                + " </div>"

        temp += "<div class='col-md-6' style='display:" + (ProductFields[i].IsVisibleLedgerAccountDr ? ("") : ("none")) + "'>"
                + "      <div class='form-group'>"
                + "         <label class='control-label col-xs-4'>" + ProductFields[i].ChargeName + " A/C DR.</label> <div class='col-xs-7'><input class='form-control Calculation' id='CALL_" + ProductFields[i].ChargeCode + "ACDR' name='linecharges[" + i + "].LedgerAccountDrId' type='text' value='" + (ProductFields[i].LedgerAccountDrId == null ? "" : ProductFields[i].LedgerAccountDrId) + "' /></div> "
                + "     </div>"
                + " </div>"

        temp += "<div class='col-md-6' style='display:" + (ProductFields[i].IsVisibleLedgerAccountCr ? ("") : ("none")) + "'>"
                + "      <div class='form-group'>"
                + "         <label class='control-label col-xs-4'>" + ProductFields[i].ChargeName + " A/C CR.</label> <div class='col-xs-7'><input class='form-control Calculation' id='CALL_" + ProductFields[i].ChargeCode + "ACCR' name='linecharges[" + i + "].LedgerAccountCrId' type='text' value='" + (ProductFields[i].LedgerAccountCrId == null ? "" : ProductFields[i].LedgerAccountCrId) + "' /></div> "
                + "     </div>"
                + " </div>"



        temp += "<input type='hidden' value='" + (ProductFields[i].IsVisibleLedgerAccountDr == null ? "" : ProductFields[i].IsVisibleLedgerAccountDr) + "' name='linecharges[" + i + "].IsVisibleLedgerAccountDr' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].IsVisibleLedgerAccountCr == null ? "" : ProductFields[i].IsVisibleLedgerAccountCr) + "' name='linecharges[" + i + "].IsVisibleLedgerAccountCr' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].filterLedgerAccountGroupsDrId == null ? "" : ProductFields[i].filterLedgerAccountGroupsDrId) + "' name='linecharges[" + i + "].filterLedgerAccountGroupsDrId' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].filterLedgerAccountGroupsCrId == null ? "" : ProductFields[i].filterLedgerAccountGroupsCrId) + "' name='linecharges[" + i + "].filterLedgerAccountGroupsCrId' />"
        //temp += "<input type='hidden' id='CALL_" + ProductFields[i].ChargeCode + "ACCR' value='" + (ProductFields[i].LedgerAccountCrId == null ? "" : ProductFields[i].LedgerAccountCrId) + "'  name='linecharges[" + i + "].LedgerAccountCrId' />"
        //temp += "<input type='hidden' id='CALL_" + ProductFields[i].ChargeCode + "ACDR' value='" + (ProductFields[i].LedgerAccountDrId == null ? "" : ProductFields[i].LedgerAccountDrId) + "' name='linecharges[" + i + "].LedgerAccountDrId'  />"

        temp += "<input type='hidden' id='CALL_" + ProductFields[i].ChargeCode + "CLAC' value='" + (ProductFields[i].ContraLedgerAccountId == null ? "" : ProductFields[i].ContraLedgerAccountId) + "' name='linecharges[" + i + "].ContraLedgerAccountId' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].AddDeduct == null ? "" : ProductFields[i].AddDeduct) + "' name='linecharges[" + i + "].AddDeduct' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].AffectCost == null ? "" : ProductFields[i].AffectCost) + "' name='linecharges[" + i + "].AffectCost' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].CalculateOnId == null ? "" : ProductFields[i].CalculateOnId) + "' name='linecharges[" + i + "].CalculateOnId' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].ChargeId == null ? "" : ProductFields[i].ChargeId) + "' name='linecharges[" + i + "].ChargeId' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].ChargeTypeId == null ? "" : ProductFields[i].ChargeTypeId) + "' name='linecharges[" + i + "].ChargeTypeId' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].CostCenterId == null ? "" : ProductFields[i].CostCenterId) + "' name='linecharges[" + i + "].CostCenterId' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].IncludedInBase == null ? "" : ProductFields[i].IncludedInBase) + "' name='linecharges[" + i + "].IncludedInBase' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].IsVisible == null ? "" : ProductFields[i].IsVisible) + "' name='linecharges[" + i + "].IsVisible' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].ParentChargeId == null ? "" : ProductFields[i].ParentChargeId) + "' name='linecharges[" + i + "].ParentChargeId' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].RateType == null ? "" : ProductFields[i].RateType) + "' name='linecharges[" + i + "].RateType' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].Sr == null ? "" : ProductFields[i].Sr) + "' name='linecharges[" + i + "].Sr' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].Id == null ? "" : ProductFields[i].Id) + "' name='linecharges[" + i + "].Id' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].IncludedCharges == null ? "" : ProductFields[i].IncludedCharges) + "' name='linecharges[" + i + "].IncludedCharges' />"
        temp += "<input type='hidden' value='" + (ProductFields[i].IncludedChargesCalculation == null ? "" : ProductFields[i].IncludedChargesCalculation) + "' name='linecharges[" + i + "].IncludedChargesCalculation' />"


    }
    var varXAmount = document.getElementById('Amount').value ? document.getElementById('Amount').value : 0;

    temp += "  <input type='hidden' value='" + varXAmount + "' id='xAmount' class='form-control col-xs-7 required text-right' />  "
    //temp += "<hr/>"
    $(temp).appendTo('.modal-body .row:last');

    for (var i = 0; i < ProductFields.length; i++) {
        CustomSelectFunction($('#CALL_' + ProductFields[i].ChargeCode + 'ACDR'), '/ComboHelpList/GetLedgerAccountForGroup', '/ComboHelpList/SetSingleLedgerAccount', ' ', false, 0, ProductFields[i].filterLedgerAccountGroupsDrId);
        CustomSelectFunction($('#CALL_' + ProductFields[i].ChargeCode + 'ACCR'), '/ComboHelpList/GetLedgerAccountForGroup', '/ComboHelpList/SetSingleLedgerAccount', ' ', false, 0, ProductFields[i].filterLedgerAccountGroupsCrId);
    }
}
//"+(DebugMode?"text":"hidden")+"
function DrawFooterFields(DebugMode) {
    var temp = "<hr style='margin-top: 0px;margin-bottom: 0px;border-top-width: 0px;'/><div class='row'>";
    for (var i = 0; i < FooterFields.length; i++) {
        if (FooterFields[i].RateType == RateTypeEnum.NA) {

            temp += "<div class='col-md-6' style='display:" + (DebugMode ? ("") : ("none")) + "'>"
             + "      <div class='form-group'>"
             //+ "         <label class='control-label col-xs-4'>" + FooterFields[i].ChargeName + "</label> <div class='col-xs-7'><input class='form-control Calculation text-right' id='CALH_" + FooterFields[i].ChargeCode + "' name='CALH_" + FooterFields[i].ChargeCode + "' type='text' value='" + FooterFields[i].Amount + "'></div> "
             + "         <label class='control-label col-md-4 col-sm-2'>" + FooterFields[i].ChargeName + "</label> <div class='col-md-7 col-sm-10'><input class='form-control Calculation text-right' id='CALH_" + FooterFields[i].ChargeCode + "' name='footercharges[" + i + "].Amount' type='text' value='" + FooterFields[i].Amount + "'></div> "
             + "     </div>"
             + " </div>"
            if (FooterFields[i].ProductChargeId != null || FooterFields[i].ChargeTypeId == ChargeTypeEnum.Amount) {
                temp += "<div class='col-md-6' style='display:" + (DebugMode ? ("") : ("none")) + "'>"
                 + "      <div class='form-group'>"
                 + "         <label class='control-label col-md-4 col-sm-2'>X" + FooterFields[i].ChargeName + "</label> <div class='col-md-7 col-sm-10'><input class='form-control Calculation text-right' id='XCALH_" + FooterFields[i].ChargeCode + "' name='XCALH_" + FooterFields[i].ChargeCode + "' type='text' value='" + FooterFields[i].Amount + "'></div> "
                 + "     </div>"
                 + " </div>"
            }
        }
        else {
            temp += "<div class='col-md-6' style='display:" + (DebugMode ? ("") : ("none")) + "'>"
            + "          <div class='form-group'>"
            + "              <label class='control-label col-md-4 col-sm-2'>" + FooterFields[i].ChargeName + "</label> "
            + "                <div class='col-md-7 col-sm-10'>"
            + "                <div class='col-xs-4' style='padding:0px;'>"
            //+ "                    <input class='form-control cusrightnormal Calculation text-right' id='CALH_" + FooterFields[i].ChargeCode + "RATE' name='CALH_" + FooterFields[i].ChargeCode + "RATE'   type='text' value='" + FooterFields[i].Rate + "'   />"
            + "                    <input class='form-control cusrightnormal Calculation text-right' id='CALH_" + FooterFields[i].ChargeCode + "RATE' name='footercharges[" + i + "].Rate'   type='text' value='" + FooterFields[i].Rate + "'   />"
            + "                 </div>"
            + "                  <div></div>"
            + "                 <div class='col-xs-8' style='padding:0px'>"
            //+ "                     <input class='form-control cusleftnormal Calculation text-right' id='CALH_" + FooterFields[i].ChargeCode + "' name='CALH_" + FooterFields[i].ChargeCode + "'   type='text' value='" + FooterFields[i].Amount + "'  />"
            + "                     <input class='form-control cusleftnormal Calculation text-right' id='CALH_" + FooterFields[i].ChargeCode + "' name='footercharges[" + i + "].Amount'   type='text' value='" + FooterFields[i].Amount + "'  />"
            + "                 </div>"
            + "          </div>"
            + "     </div>"
            + "     </div>"
            if (FooterFields[i].ProductChargeId != null || FooterFields[i].ChargeTypeId == ChargeTypeEnum.Amount) {
                temp += "<div class='col-md-6' style='display:" + (DebugMode ? ("") : ("none")) + "'>"
               + "          <div class='form-group'>"
               + "              <label class='control-label col-md-4 col-sm-2'>X" + FooterFields[i].ChargeName + "</label> "
               + "                <div class='col-md-7 col-sm-10'>"
               + "                <div class='col-xs-4' style='padding:0px;'>"
               + "                    <input class='form-control cusrightnormal Calculation text-right' id='XCALH_" + FooterFields[i].ChargeCode + "RATE' name='XCALH_" + FooterFields[i].ChargeCode + "RATE'   type='text' value='" + FooterFields[i].Rate + "'   />"
               + "                 </div>"
               + "                  <div></div>"
               + "                 <div class='col-xs-8' style='padding:0px'>"
               + "                     <input class='form-control cusleftnormal Calculation text-right' id='XCALH_" + FooterFields[i].ChargeCode + "' name='XCALH_" + FooterFields[i].ChargeCode + "'   type='text' value='" + FooterFields[i].Amount + "'  />"
               + "                 </div>"
               + "          </div>"
               + "     </div>"
               + "     </div>"
            }
        }
        temp += "<input type='hidden' id='CALH_" + FooterFields[i].ChargeCode + "ACCR' value='" + (FooterFields[i].LedgerAccountCrId == null ? "" : FooterFields[i].LedgerAccountCrId) + "'   name='footercharges[" + i + "].LedgerAccountCrId' />"
        temp += "<input type='hidden' id='CALH_" + FooterFields[i].ChargeCode + "ACDR' value='" + (FooterFields[i].LedgerAccountDrId == null ? "" : FooterFields[i].LedgerAccountDrId) + "'   name='footercharges[" + i + "].LedgerAccountDrId' />"
        temp += "<input type='hidden' id='CALH_" + FooterFields[i].ChargeCode + "CLAC' value='" + (FooterFields[i].ContraLedgerAccountId == null ? "" : FooterFields[i].ContraLedgerAccountId) + "'   name='footercharges[" + i + "].ContraLedgerAccountId' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].AddDeduct == null ? "" : FooterFields[i].AddDeduct) + "' name='footercharges[" + i + "].AddDeduct' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].AffectCost == null ? "" : FooterFields[i].AffectCost) + "' name='footercharges[" + i + "].AffectCost' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].CalculateOnId == null ? "" : FooterFields[i].CalculateOnId) + "' name='footercharges[" + i + "].CalculateOnId' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].ChargeId == null ? "" : FooterFields[i].ChargeId) + "' name='footercharges[" + i + "].ChargeId' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].ChargeTypeId == null ? "" : FooterFields[i].ChargeTypeId) + "' name='footercharges[" + i + "].ChargeTypeId' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].CostCenterId == null ? "" : FooterFields[i].CostCenterId) + "' name='footercharges[" + i + "].CostCenterId' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].IncludedInBase == null ? "" : FooterFields[i].IncludedInBase) + "' name='footercharges[" + i + "].IncludedInBase' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].IsVisible == null ? "" : FooterFields[i].IsVisible) + "' name='footercharges[" + i + "].IsVisible' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].ParentChargeId == null ? "" : FooterFields[i].ParentChargeId) + "' name='footercharges[" + i + "].ParentChargeId' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].ProductChargeId == null ? "" : FooterFields[i].ProductChargeId) + "' name='footercharges[" + i + "].ProductChargeId' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].RateType == null ? "" : FooterFields[i].RateType) + "' name='footercharges[" + i + "].RateType' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].Sr == null ? "" : FooterFields[i].Sr) + "' name='footercharges[" + i + "].Sr' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].Id == null ? "" : FooterFields[i].Id) + "' name='footercharges[" + i + "].Id' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].IncludedCharges == null ? "" : FooterFields[i].IncludedCharges) + "' name='footercharges[" + i + "].IncludedCharges' />"
        temp += "<input type='hidden' value='" + (FooterFields[i].IncludedChargesCalculation == null ? "" : FooterFields[i].IncludedChargesCalculation) + "' name='footercharges[" + i + "].IncludedChargesCalculation' />"

    }
    temp += "</div>"
    $(temp).appendTo('.modal-body');
}



$(document).on('change', '.Calculation', ChargeCalculation);


function GetChargeRates(CalculationId, DocumentType, SiteId, DivisionId, ProcessId, ChargeGroupPersonId, ChargeGroupProductId, ProductId)
{
    $.ajax({
        async: false,
        cache: false,
        type: 'POST',
        url: "/TaxCalculation/GetChargeRates/",
        data: { CalculationId: CalculationId, DocTypeId: DocumentType, SiteId: SiteId, DivisionId: DivisionId, ProcessId: ProcessId, ChargeGroupPersonId: ChargeGroupPersonId, ChargeGroupProductId: ChargeGroupProductId, ProductId: ProductId },
        success: function (data) {
            ProductCharges = data;
            AssignValuesToChargeRates();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrive calculationproduct' + thrownError);
        },
    });
}




function ChargeCalculation() {

    
    var SubTotalProduct = 0;
    var SubTotalFooter = 0;
    for (var i = 0; i < ProductFields.length; i++) {
        var selector = "#CALL_" + ProductFields[i].ChargeCode;
        var selectorRate = "#CALL_" + ProductFields[i].ChargeCode + "RATE";


        if (ProductFields[i].ChargeTypeId == ChargeTypeEnum.SUBTOTAL) {
            $(selector).val(SubTotalProduct.toFixed(2));
        }

        else if (ProductFields[i].ChargeTypeId == ChargeTypeEnum.Amount) {
            var selector = "#CALL_" + ProductFields[i].ChargeCode;
            $(selector).val(parseFloat($('#Amount').val()).toFixed(2));
        }
        else if (ProductFields[i].RateType == RateTypeEnum.NA && ProductFields[i].CalculateOnId != null) {
            var calculateon = "#CALL_" + ProductFields[i].CalculateOnCode;
            $(selector).val(parseFloat($(calculateon).val()).toFixed(2));
        }
        else if (ProductFields[i].RateType == RateTypeEnum.PERCENTAGE) {
            var calculateOn = "#CALL_" + ProductFields[i].CalculateOnCode;
            if ($.isNumeric($(selectorRate).val()) && $(selectorRate).val() != 0)
                $(selector).val(parseFloat(parseFloat($(calculateOn).val()) * parseFloat($(selectorRate).val()) / 100).toFixed(2));
        }
        else if (ProductFields[i].RateType == RateTypeEnum.RATE) {
            var calculateOn = "#CALL_" + ProductFields[i].CalculateOnCode;

            if ($.isNumeric($(selectorRate).val()) && $(selectorRate).val() != 0) {
                //Changed Logic To Calculate On DealQty
                //$(selector).val(parseFloat(parseFloat($(calculateOn).val()) * parseFloat($(selectorRate).val())).toFixed(2));
                $(selector).val(parseFloat(parseFloat($('#DealQty').val()) * parseFloat($(selectorRate).val())).toFixed(2));
            }
        }


        if (ProductFields[i].IncludedChargesCalculation) {
            var Rate = EvalExpression(ProductFields[i].IncludedCharges, ProductFields[i].IncludedChargesCalculation, "#CALL_");
            if ($.isNumeric(Rate)) {//SubTotalProduct = (SubTotalProduct * 100) / (100 + Rate);
                var Amt = (($(selector).val() * 100) / (100 + Rate));
                $(selector).val(Amt.toFixed(2));
            }
        }

        if (ProductFields[i].AddDeduct == AddDeductEnum.Add) {
            SubTotalProduct = (SubTotalProduct + parseFloat($(selector).val()));
        }
        else if (ProductFields[i].AddDeduct == AddDeductEnum.Deduct) {
            SubTotalProduct = (SubTotalProduct - parseFloat($(selector).val()));
        }
        else if (ProductFields[i].AddDeduct == AddDeductEnum.OverRide) {
            SubTotalProduct = parseFloat($(selector).val());
        }


    }

    function EvalExpression(IncludedCharges, ChargeCalculation, selector) {
        var Rate;

        if (IncludedCharges) {

            var Params = IncludedCharges.split(',');

            var ParamDict = [];

            $.each(Params, function (i, val) {

                var Elem = selector + val;

                ParamDict.push({ id: val, text: $(Elem).val() });
            })

            var CalExpr = ChargeCalculation;

            $.each(ParamDict, function (i, val) {

                var replace = '/' + val.id + '/gi';

                CalExpr = CalExpr.replace(new RegExp(val.id, 'ig'), val.text);

            })

            Rate = parseFloat(eval(CalExpr));

        }
        else {
            Rate = parseFloat(eval(ChargeCalculation));
        }

        return Rate;
    }


    for (i = 0; i < FooterFields.length; i++) {
        var selector = "#CALH_" + FooterFields[i].ChargeCode;
        var selectorRate = "#CALH_" + FooterFields[i].ChargeCode + "RATE";
        var xselector = "#XCALH_" + FooterFields[i].ChargeCode;
        var xselectorRate = "#XCALH_" + FooterFields[i].ChargeCode + "RATE";


        if (FooterFields[i].ChargeTypeId == ChargeTypeEnum.SUBTOTAL) {

            $(selector).val(Number(SubTotalFooter).toFixed(2));
        }
        else if (FooterFields[i].ChargeTypeId == ChargeTypeEnum.Amount) {
            if ($.isNumeric($('#Amount').val())) {
                $(selector).val((parseFloat($(xselector).val()) - parseFloat($('#xAmount').val()) + parseFloat($('#Amount').val())).toFixed(2))
            }
        }

        else if (FooterFields[i].ProductChargeId != null) {
            var ProductChargeElement = "#CALL_" + FooterFields[i].ProductChargeCode;
            var xProductChargeElement = "#XCALL_" + FooterFields[i].ProductChargeCode;
            if ($.isNumeric($(ProductChargeElement).val())) {
                //alert(xselector);
                //alert($(xselector).val());
                //alert(xProductChargeElement);
                //alert($(xProductChargeElement).val());
                //alert(ProductChargeElement);
                //alert($(ProductChargeElement).val());
                $(selector).val((parseFloat($(xselector).val()) - parseFloat($(xProductChargeElement).val()) + parseFloat($(ProductChargeElement).val())).toFixed(2))
            }
        }

        else if (FooterFields[i].ChargeTypeId == ChargeTypeEnum.ROUNDOFF) {

            var calculateon = "#CALH_" + FooterFields[i].CalculateOnCode
            var value = (Math.round(($(calculateon).val())) - (($(calculateon).val()))).toFixed(2);
            $(selector).val(value);
        }

        else if (FooterFields[i].ChargeTypeId == ChargeTypeEnum.Amount) {

            $(selector).val($('#Amount').val());
        }
        else if (FooterFields[i].RateType == RateTypeEnum.NA && FooterFields[i].CalculateOnId != null) {

            var calculateon = "#CALH_" + FooterFields[i].CalculateOnCode;


            $(selector).val(parseFloat($(calculateon).val()).toFixed(2));
        }
        else if (FooterFields[i].RateType == RateTypeEnum.PERCENTAGE) {

            var calculateOn = "#CALH_" + FooterFields[i].CalculateOnCode;

            if ($.isNumeric($(selectorRate).val()) && $(selectorRate).val() != 0)
                $(selector).val(parseFloat(parseFloat($(calculateOn).val()) * parseFloat($(selectorRate).val()) / 100).toFixed(2));
        }
        else if (FooterFields[i].RateType == RateTypeEnum.RATE) {

            var calculateOn = "#CALH_" + FooterFields[i].CalculateOnCode;

            if ($.isNumeric($(selectorRate).val()) && $(selectorRate).val() != 0)
                $(selector).val(parseFloat(parseFloat($(calculateOn).val()) * parseFloat($(selectorRate).val())).toFixed(2));
        }


        if (FooterFields[i].IncludedChargesCalculation) {
            var Rate = EvalExpression(FooterFields[i].IncludedCharges, FooterFields[i].IncludedChargesCalculation, "#CALH_");
            if ($.isNumeric(Rate)) {//SubTotalProduct = (SubTotalProduct * 100) / (100 + Rate);
                var Amt = (($(selector).val() * 100) / (100 + Rate));
                $(selector).val(Amt.toFixed(2));
            }

        }

        if (FooterFields[i].AddDeduct == AddDeductEnum.Add) {

            if ($.isNumeric($(selector).val()))
                SubTotalFooter = (SubTotalFooter + parseFloat($(selector).val()));
        }
        else if (FooterFields[i].AddDeduct == AddDeductEnum.Deduct) {


            if ($.isNumeric($(selector).val()))
                SubTotalFooter = (SubTotalFooter - parseFloat($(selector).val()));


        }
        else if (FooterFields[i].AddDeduct == AddDeductEnum.OverRide) {
            if ($.isNumeric($(selector).val()))
                SubTotalFooter = parseFloat($(selector).val());


        }

    }


};

function DeletingProductCharges() {

    var SubTotalFooter = 0;
    var FSel;
    $('#Rate').val(0);
    $('#Rate').trigger('keyup');


    $('#Amount').val(0);
    $('#Amount').trigger('keyup');

    for (i = 0; i < ProductFields.length; i++) {
        var selector = "#CALL_" + ProductFields[i].ChargeCode;
        var selectorRate = "#CALL_" + ProductFields[i].ChargeCode + "RATE";
        if (i == 0)
            FSel = selector;


        $(selector).val(0);
        $(selectorRate).val(0);

    }

    $(FSel).trigger('change');


}


function AssignValuesToChargeRates() {
    for (var i = 0; i < ProductFields.length; i++) {
        if (ProductFields[i].ChargeTypeId != null)
        {
            ProductFields[i].Rate = ProductCharges[i].Rate;
            ProductFields[i].LedgerAccountCrId = ProductCharges[i].LedgerAccountCrId;
            ProductFields[i].LedgerAccountDrId = ProductCharges[i].LedgerAccountDrId;

            var selector = "#CALL_" + ProductFields[i].ChargeCode;
            var selectorRate = "#CALL_" + ProductFields[i].ChargeCode + "RATE";
            var LedgerAccCr = "#CALL_" + ProductFields[i].ChargeCode + "ACCR";
            var LedgerAccDr = "#CALL_" + ProductFields[i].ChargeCode + "ACDR";

            $(selectorRate).val(ProductFields[i].Rate);
            if (ProductFields[i].Rate == null || ProductFields[i].Rate == 0)
                $(selector).val(0);
            $(LedgerAccCr).val(ProductFields[i].LedgerAccountCrId);
            $(LedgerAccDr).val(ProductFields[i].LedgerAccountDrId);
        }
    }
}


function AssignValuesToCharges() {

    for (var i = 0; i < ProductFields.length; i++) {
        var selector = "#CALL_" + ProductFields[i].ChargeCode;
        var selectorRate = "#CALL_" + ProductFields[i].ChargeCode + "RATE";
        var LedgerAccCr = "#CALL_" + ProductFields[i].ChargeCode + "ACCR";
        var LedgerAccDr = "#CALL_" + ProductFields[i].ChargeCode + "ACDR";
        var ContraLedgerAcc = "#CALL_" + ProductFields[i].ChargeCode + "CLAC";

        ProductFields[i].Rate = $(selectorRate).val();
        ProductFields[i].Amount = parseFloat($(selector).val()).toFixed(2);
        ProductFields[i].LedgerAccountCrId = parseInt($(LedgerAccCr).val());
        ProductFields[i].LedgerAccountDrId = parseInt($(LedgerAccDr).val());
        ProductFields[i].ContraLedgerAccountId = parseInt($(ContraLedgerAcc).val());
        ProductFields[i].DealQty = parseFloat($('#DealQty', '#modform').val());

    }

    return ProductFields;
}

function AssignValuesToFooter() {
    for (var i = 0; i < FooterFields.length; i++) {
        var selector = "#CALH_" + FooterFields[i].ChargeCode;
        var selectorRate = "#CALH_" + FooterFields[i].ChargeCode + "RATE";
        var LedgerAccCr = "#CALH_" + FooterFields[i].ChargeCode + "ACCR";
        var LedgerAccDr = "#CALH_" + FooterFields[i].ChargeCode + "ACDR";
        var ContraLedgerAcc = "#CALH_" + FooterFields[i].ChargeCode + "CLAC";

        FooterFields[i].Rate = $(selectorRate).val();
        FooterFields[i].Amount = parseFloat($(selector).val()).toFixed(2);
        FooterFields[i].LedgerAccountCrId = parseInt($(LedgerAccCr).val());
        FooterFields[i].LedgerAccountDrId = parseInt($(LedgerAccDr).val());
        FooterFields[i].ContraLedgerAccountId = parseInt($(ContraLedgerAcc).val());


    }
    return FooterFields;
}



function LoadCalculationFooter(DocHeaderId, HeaderTable, LineTable) {

    $.ajax({
        cache: false,
        type: 'POST',
        url: "/TaxCalculation/GetHeaderCharge/",
        data: { HeaderId: DocHeaderId, HeaderTable: HeaderTable, LineTable: LineTable },
        success: function (data) {
            FooterFields = data;
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrive calculation footer' + thrownError);
        },
    });

};



//style='display:"+item.IsVisible?"":"none"+"'


function LoadCharges(DocHeaderId, HeaderTable, LineTable, EditUrl) {

    var link = '/TaxCalculation/GetHeaderCharge';
    var AmountCount = 0;
    $.ajax({
        url: link,
        type: 'GET',
        data: { HeaderId: DocHeaderId, HeaderTable: HeaderTable, LineTable: LineTable },
        success: function (data) {
            var row = "";
            var srno = 1;
            if (data.length > 0) {
                $.each(data, function (index, item) {
                    row += "<div class='row crow grid-body' style='margin-left:0px;margin-right:0px'>"
                        + "      <div class='col-xs-1 row-index'>" + srno + "<input type='hidden' class='id' value='" + item.HeaderTableId + "' />"
                        + "      </div>"
                        + "      <div class='col-xs-11'>"
                        + "          <div class='row' >"
                        + "              <div class='col-sm-4'>" + (item.ChargeName == null ? " " : "<strong>" + item.ChargeName + "</strong>") + "</div>"
                        + "              <div class='col-sm-3'>" + (item.CalculateOnName == null ? " " : item.CalculateOnName) + "</div>"
                        + "              <div class='col-sm-2'>" + item.AddDeductName + "</div>"
                        + "              <div class='col-sm-1 text-right'>" + (item.Rate == null ? " " : item.Rate.toFixed(2)) + "</div>"
                        + "              <div class='col-sm-2 text-right'>" + (item.Amount == null ? " " : item.Amount.toFixed(2)) + "</div>"
                        + "          </div>"
                        + "      </div>"
                        + "      <div>"
                        //+ "          <a class='' delete='' href='/TaxCalculation/GetHeaderChargeForEdit/" + item.HeaderTableId + "'></a>"
                        + "          <a  edit='' href='" + EditUrl + "?Id=" + item.HeaderTableId + "&HeaderTable=" + HeaderTable + "&LineTable=" + LineTable + "'></a>"
                        + "      </div>"
                        + "</div>"
                        + "<hr style='margin-top:0px;margin-bottom:0px'/>";
                    srno++;
                });
                row += "<div class='row tfooter' style='padding-top:10px;'>"
                    + "  <div class='col-xs-1'>"
                    + "  </div>"
                    + "  <div class='col-xs-10'>"
                    + "      <div class='row'> "
                    + "          <div class='col-sm-3'>"
                    + "          </div>"
                    + "          <div class='col-sm-3 text-right'>"
                    + "          </div>"
                    + "          <div class='col-sm-3 text-right'>"
                    + "          </div>"
                    + "          <div class='col-sm-3 text-right'>"
                    + "          </div>"
                    + "      </div>"
                    + "  </div>"
                    + "  <div class='col-xs-1'></div>"
                    + "</div>"

            }
            else {
                ;
            }
            $("#gbodycharges").html(row);
        }

    })

}



function LoadNewCharges(DocHeaderId, HeaderTable, LineTable, EditUrl) {

    var link = '/TaxCalculation/_GetHeaderCharge';
    var AmountCount = 0;
    $.ajax({
        url: link,
        type: 'GET',
        data: { HeaderId: DocHeaderId, HeaderTable: HeaderTable, LineTable: LineTable, EditUrl: EditUrl },
        success: function (data) {
            $("#gridcharges").html(data);
        }

    })

}