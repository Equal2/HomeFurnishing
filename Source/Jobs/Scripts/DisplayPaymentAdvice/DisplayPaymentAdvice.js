DisplayPaymentAdvice = angular.module('DisplayPaymentAdvice', ['ngTouch', 'ui.grid', 'ui.grid.resizeColumns', 'ui.grid.moveColumns',
    'ui.grid.exporter', 'ui.grid.cellNav', 'ui.grid.pinning'])



DisplayPaymentAdvice.controller('MainCtrl', ['$scope', '$log', '$http', 'uiGridConstants',



  function ($scope, $log, $http, uiGridConstants, uiGridTreeViewConstants) {
      $scope.gridOptions = {
          enableGridMenu: true,
          exporterCsvFilename: 'myFile.csv',
          exporterPdfDefaultStyle: { fontSize: 9 },
          exporterPdfTableStyle: { margin: [30, 30, 30, 30] },
          exporterPdfTableHeaderStyle: { fontSize: 10, bold: true, italics: true, color: 'red' },
          exporterPdfHeader: { text: "My Header", style: 'headerStyle' },
          exporterPdfFooter: function (currentPage, pageCount) {
              return { text: currentPage.toString() + ' of ' + pageCount.toString(), style: 'footerStyle' };
          },
          exporterPdfCustomFormatter: function (docDefinition) {
              docDefinition.styles.headerStyle = { fontSize: 22, bold: true };
              docDefinition.styles.footerStyle = { fontSize: 10, bold: true };
              return docDefinition;
          },
          exporterPdfOrientation: 'portrait',
          exporterPdfPageSize: 'LETTER',
          exporterPdfMaxGridWidth: 500,



          enableHorizontalScrollbar: uiGridConstants.scrollbars.ALWAYS,
          showTreeExpandNoChildren : false,
          enableFiltering: true,
          //enableTreeView : true,
          showColumnFooter: true,
          enableGridMenu: true,
          exporterCsvLinkElement: angular.element(document.querySelectorAll(".custom-csv-link-location")),
          exporterCsvFilename: 'myFile.csv',
          onRegisterApi: function (gridApi) {
              $scope.gridApi = gridApi;
          }
      };







      $scope.ShowDetail = function () {
          var rowCol = $scope.gridApi.cellNav.getFocusedCell();
          //alert(rowCol.col.name);

          $("#TextHidden").val(rowCol.col.name);

          $("#Format").val(rowCol.row.entity.Format);
          if (rowCol.row.entity.Format != null)
          {
            $("#Format").select2('data', { id: rowCol.row.entity.Format, text: rowCol.row.entity.Format });
          }
          if (rowCol.row.entity.FromDate != null)
          {
              $("#FromDate").val(rowCol.row.entity.FromDate);
          }
          if (rowCol.row.entity.ToDate != null)
          {
              $("#ToDate").val(rowCol.row.entity.ToDate);
          }
          if (rowCol.row.entity.JobWorkerId != null)
          {
              $("#JobWorker").select2('data', { id: rowCol.row.entity.JobWorkerId, text: rowCol.row.entity.Name });
          }
          if (rowCol.row.entity.ProcessId != null)
          {
              $("#Process").select2('data', { id: rowCol.row.entity.ProcessId, text: rowCol.row.entity.ProcessName });
          }
          
          
          
        
          var DocTypeId = parseInt(rowCol.row.entity.DocTypeId);
           var DocId = parseInt(rowCol.row.entity.JobInvoiceHeaderId);
           if (rowCol.row.entity.JobInvoiceHeaderId != null)
          {
              window.open('/Display_PaymentAdivceWithInvoice/DocumentMenu/?DocTypeId=' + DocTypeId + '&DocId=' + DocId, '_blank');
              return;
          }
          

          $.ajax({
              async : false,
              cache: false,
              type: "POST",
              url: '/Display_PaymentAdivceWithInvoice/SaveCurrentSetting',
              success: function (data) {
              },
              error: function (xhr, ajaxOptions, thrownError) {
                  alert('Failed to retrieve product details.' + thrownError);
              }
          });

          $scope.BindData();

      };
      

      $(document).keyup(function (e) {
          if (e.keyCode == 27) { // escape key maps to keycode `27`
              $.ajax({
                    async: false,
                    cache: false,
                    type: "POST",
                    url: '/Display_PaymentAdivceWithInvoice/GetParameterSettingsForLastDisplay',
               success: function (result) {
                   $("#Format").val(result.Format);
                   $("#FromDate").val(result.FromDate);
                   $("#ToDate").val(result.ToDate);
                   $("#JobWorker").val(result.JobWorker);
                   $("#Process").val(result.Process);
                   $("#SiteIds").val(result.SiteId);
                   $("#DivisionIds").val(result.DivisionId);
                   $("#TextHidden").val(result.TextHidden);
                  
                   CustomSelectFunction($("#Process"), '/ComboHelpList/GetProcessWithChildProcess', '/ComboHelpList/SetSingleProcess', ' ', false, 0);
                   CustomSelectFunction($("#DivisionIds"), '/ComboHelpList/GetDivision', '/ComboHelpList/SetSingleDivision', ' ', false, 0);
                   CustomSelectFunction($("#SiteIds"), '/ComboHelpList/GetSite', '/ComboHelpList/SetSingleSite', ' ', false, 0);
                   CustomSelectFunction($("#JobWorker"), '/ComboHelpList/GetPerson', '/ComboHelpList/SetSinglePerson', ' ', false, 0);
                   if (result.Format != null)
                   {
                       $("#Format").select2('data', { id: result.Format, text: result.Format });
                   }
                   if (result.JobWorker == null)
                       $("#JobWorker").select2('data', { id: '', text: '' });                   
                   if (result.SiteId == null)
                       $("#SiteIds").select2('data', { id: '', text: '' });
                   if (result.DivisionId == null)
                       $("#DivisionIds").select2('data', { id: '', text: '' });
                   if (result.Process == null)
                       $("#Process").select2('data', { id: '', text: '' });
                  
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    alert('Failed to retrieve product details.' + thrownError);
                }
              });

            $scope.BindData();
          }


          if (e.keyCode == 13) {
              // escape key maps to keycode `27`
              if ($scope.gridApi.cellNav.getFocusedCell() != null)
              {
                  $scope.ShowDetail();
              }
          }
      });



      var i = 0;
      $scope.BindData = function ()
      {
          $scope.myData = [];


          $.ajax({
              url: '/Display_PaymentAdivceWithInvoice/DisplayPaymentAdviceFill/' + $(this).serialize(),
              type: "POST",
              data: $("#registerSubmit").serialize(),
              success: function (result) {
                  Lock = false;
                  if (result.Success == true) {
                      $scope.gridOptions.columnDefs = new Array();
                      if ($("#Format").val() == "Order No Wise") {
                            $scope.gridOptions.columnDefs.push({ field: 'JobInvoiceHeaderId', width: 50, visible: false });
                            $scope.gridOptions.columnDefs.push({ field: 'DocTypeId', width: 50, visible: false });
                            $scope.gridOptions.columnDefs.push({ field: 'DocumentTypeName', width:200, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                            $scope.gridOptions.columnDefs.push({ field: 'InvoiceNo', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                            $scope.gridOptions.columnDefs.push({ field: 'InvoiceDate', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                            $scope.gridOptions.columnDefs.push({ field: 'IssueChallanNo', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                            $scope.gridOptions.columnDefs.push({ field: 'IssueDate', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                            $scope.gridOptions.columnDefs.push({ field: 'Design', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                            $scope.gridOptions.columnDefs.push({ field: 'ProductName', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                            $scope.gridOptions.columnDefs.push({ field: 'size', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                            $scope.gridOptions.columnDefs.push({ field: 'SalesTaxProductCodes', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                            $scope.gridOptions.columnDefs.push({ field: 'ChargeGroupPersonName', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                            $scope.gridOptions.columnDefs.push({ field: 'ProcessName', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'Name', width: 300, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                        $scope.gridOptions.columnDefs.push({ field: 'PCS', width: 75, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'Area', width: 100, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'DealUnit', width: 125, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'Amount', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });                        
                        $scope.gridOptions.columnDefs.push({ field: 'IncentiveAmt', width: 140, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'PenaltyAmt', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'GrossAmount', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text', visible: false });
                        $scope.gridOptions.columnDefs.push({ field: 'GSTAmt', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'NetAmt', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'TDSAmount', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'Advance', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'DebitNote', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'CreditNote', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'GSTDebitCredit', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'NetPayable', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                      }
                    else  if($("#Format").val()=="Job Worker Wise Summary")
                      {
                        $scope.gridOptions.columnDefs.push({ field: 'JobWorkerId', width: 50, visible: false });
                        $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                        $scope.gridOptions.columnDefs.push({ field: 'Name', width: 300, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                        $scope.gridOptions.columnDefs.push({ field: 'PCS', width: 75, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'Area', width: 100, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'DealUnit', width: 125, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'Amount', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'IncentiveAmt', width: 140, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'PenaltyAmt', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'GrossAmount', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text', visible: false });
                        $scope.gridOptions.columnDefs.push({ field: 'GSTAmt', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'NetAmt', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'TDSAmount', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'Advance', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'DebitNote', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'CreditNote', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'GSTDebitCredit', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                        $scope.gridOptions.columnDefs.push({ field: 'NetPayable', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                      }
                      else if ($("#Format").val() == "Process Wise Symmary") {
                          $scope.gridOptions.columnDefs.push({ field: 'ProcessId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ProcessName', width:250, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'PCS', width:75, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Area', width: 100, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DealUnit', width: 125, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Amount', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'IncentiveAmt', width: 140, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'PenaltyAmt', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'GrossAmount', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text', visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'GSTAmt', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'NetAmt', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'TDSAmount', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Advance', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DebitNote', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'CreditNote', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'GSTDebitCredit', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'NetPayable', width: 125, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          }
                      $scope.gridOptions.data = result.Data;
                      $scope.gridApi.grid.refresh();

                   }
                  else if (!result.Success) {
                      alert('Something went wrong');
                  }
              },
              error: function () {
                  Lock: false;
                  alert('Something went wrong');
              }
          });
      }
  }
]);

DisplayPaymentAdvice.filter('trusted', function ($sce) {
    return function (value) {
        return $sce.trustAsHtml(value);
    }
});




