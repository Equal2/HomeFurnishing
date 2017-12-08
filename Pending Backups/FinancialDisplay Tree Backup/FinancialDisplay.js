//FinancialDisplay = angular.module('FinancialDisplay', ['ngTouch', 'ui.grid', 'ui.grid.resizeColumns', 'ui.grid.grouping', 'ui.grid.moveColumns',
//    'ui.grid.selection', 'ui.grid.exporter', 'ui.grid.cellNav', 'ui.grid.pinning', 'ui.grid.treeView', 'ui.grid.saveState']);

FinancialDisplay = angular.module('FinancialDisplay', ['ngTouch', 'ui.grid', 
    'ui.grid.exporter', 'ui.grid.cellNav', 'ui.grid.treeView'])



FinancialDisplay.controller('MainCtrl', ['$scope', '$log', '$http', 'uiGridConstants', 'uiGridTreeViewConstants',



  function ($scope, $log, $http, uiGridConstants, uiGridTreeViewConstants) {
      $scope.gridOptions = {
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
          if (rowCol.row.entity.ReportType == "Trial Balance As Per Detail" || rowCol.row.entity.ReportType == null)
          {
              $("#ReportType").val("Sub Trial Balance")
              $("#LedgerAccountGroup").val(rowCol.row.entity.LedgerAccountGroupId)
          }
          else if (rowCol.row.entity.ReportType == "Sub Trial Balance") {
              $("#ReportType").val("Ledger")
              $("#LedgerAccount").val(rowCol.row.entity.LedgerAccountId)
          }
          

          $.ajax({
              async : false,
              cache: false,
              type: "POST",
              url: '/FinancialDisplay/SaveCurrentSetting',
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
                    url: '/FinancialDisplay/GetParameterSettingsForLastDisplay',
               success: function (result) {
                   $("#ReportType").val(result.ReportType);
                   $("#FromDate").val(result.FromDate);
                   $("#ToDate").val(result.ToDate);
                   $("#LedgerAccountGroup").val(result.LedgerAccountGroup);
                   $("#LedgerAccount").val(result.LedgerAccount);
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    alert('Failed to retrieve product details.' + thrownError);
                }
            });
            $scope.BindData();
          }
      });


      var writeoutNode = function (childArray, currentLevel, dataArray) {
          if (childArray != null)
          {
              childArray.forEach(function (childNode) {
                  //if (childNode.ChildTrialBalanceAsPerDetailViewModel != null) {
                      childNode.$$treeLevel = currentLevel;
                  //}
                  dataArray.push(childNode);
                  writeoutNode(childNode.ChildTrialBalanceAsPerDetailViewModel, currentLevel + 1, dataArray);
              });
          }
      };







      var i = 0;
      $scope.BindData = function ()
      {
          $scope.myData = [];


          $.ajax({
              url: '/FinancialDisplay/FinancialDisplayFill/' + $(this).serialize(),
              type: "POST",
              data: $("#registerSubmit").serialize(),
              success: function (result) {
                  Lock = false;
                  if (result.Success == true) {
                      $scope.gridOptions.columnDefs = new Array();

                      if ($("#ReportType").val() == "Trial Balance")
                      {
                          if ($("#DisplayType").val() == "Balance")
                          {
                              $scope.gridOptions.columnDefs.push({ field: 'LedgerAccountGroupId', width: 50, visible: false});
                              $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'LedgerAccountGroupName', width: 720, cellClass: 'cell-text', headerCellClass: 'header-text', 
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtDr', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtCr', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                          }
                          else if ($("#DisplayType").val() == "Summary")
                          {
                              $scope.gridOptions.columnDefs.push({ field: 'LedgerAccountGroupId', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'LedgerAccountGroupName', width: 260, cellClass: 'cell-text', headerCellClass: 'header-text',
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'Opening', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'OpeningType', name : '', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text',
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtDr', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtCr', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'Balance', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'BalanceType', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text',
                              });
                          }

                          $scope.gridOptions.data = result.Data;

                      }



                      else if ($("#ReportType").val() == "Trial Balance As Per Detail") {

                          $scope.gridOptions.showTreeRowHeader = true;
                          
                          $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.OPTIONS);

                          var TotalAmtDr = 0;
                          var TotalAmtCr = 0;
                          if (result.Data != null)
                          {
                              TotalAmtDr = result.Data[0]["TotalAmtDr"];
                              TotalAmtCr = result.Data[0]["TotalAmtCr"];
                          }


                          if ($("#DisplayType").val() == "Balance") {
                              $scope.gridOptions.columnDefs.push({ field: 'LedgerAccountGroupId', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'LedgerAccountGroupName', width: 660, cellClass: 'cell-text', headerCellClass: 'header-text',
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtDr', width: 200,
                                  footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtDr + '</div>',
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtCr', width: 200,
                                  footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtCr + '</div>',
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                          }
                          writeoutNode(result.Data, 0, $scope.gridOptions.data);

                          // Access outside scope functions from row template

                      }



                      else if ($("#ReportType").val() == "Sub Trial Balance") {
                          $scope.gridOptions.showTreeRowHeader = false;
                          $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.OPTIONS);
                          
                          if ($("#DisplayType").val() == "Balance")
                          {
                              $scope.gridOptions.columnDefs.push({ field: 'LedgerAccountId', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'LedgerAccountName', width: 720, cellClass: 'cell-text', headerCellClass: 'header-text',
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtDr', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtCr', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              writeoutNode(result.Data, 0, $scope.gridOptions.data);
                          }
                          else if ($("#DisplayType").val() == "Summary")
                          {
                              $scope.gridOptions.columnDefs.push({ field: 'LedgerAccountGroupId', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'LedgerAccountName', width: 260, cellClass: 'cell-text', headerCellClass: 'header-text',
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'Opening', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'OpeningType', name: '', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text',
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtDr', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtCr', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'Balance', width: 200,
                                  aggregationType: uiGridConstants.aggregationTypes.sum,
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'BalanceType', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text',
                              });
                          }
                          $scope.gridOptions.data = result.Data;
                      }
                      else if ($("#ReportType").val() == "Ledger") {

                          //$scope.gridApi.grid = {
                          //    "color": "red",
                          //    "background-color": "coral",
                          //    "font-size": "60px",
                          //    "padding": "50px"
                          //}
                          $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.OPTIONS);

                          //$scope.gridOptions.rowTemplate = '<div style="height: auto !important;display: flex;align-content: stretch;"></div>',

                          //$scope.gridApi.grid.style.cssText = ".ui-grid-viewport .ui-grid-cell-contents {word-wrap: break-word;white-space: normal !important;}.ui-grid-row, .ui-grid-cell {height: auto !important;}.ui-grid-row div[role=row] {display: flex;align-content: stretch;}"

                          $scope.gridOptions.columnDefs.push({ field: 'LedgerHeaderId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'DocDate', width: 90, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DocNo', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({
                              field: 'Narration', width: 430, cellClass: 'cell-text ', headerCellClass: 'header-text',
                              cellTemplate: '<div ng-bind-html="COL_FIELD | trusted"></div>',
                              //cellTemplate: '<div style="word-wrap: break-word;white-space: normal !important;" ng-bind-html="COL_FIELD | trusted"></div>',


                          });
                          $scope.gridOptions.columnDefs.push({
                              field: 'AmtDr', width: 150,
                              aggregationType: uiGridConstants.aggregationTypes.sum,
                              aggregationHideLabel: true,
                              headerCellClass: 'text-right header-text',
                              footerCellClass: 'text-right ',
                              cellClass: 'text-right cell-text '
                          });
                          $scope.gridOptions.columnDefs.push({
                              field: 'AmtCr', width: 150,
                              aggregationType: uiGridConstants.aggregationTypes.sum,
                              aggregationHideLabel: true,
                              headerCellClass: 'text-right header-text',
                              footerCellClass: 'text-right ',
                              cellClass: 'text-right cell-text '
                          });
                          $scope.gridOptions.columnDefs.push({
                              field: 'Balance', width: 150,
                              aggregationType: uiGridConstants.aggregationTypes.sum,
                              aggregationHideLabel: true,
                              headerCellClass: 'text-right header-text',
                              footerCellClass: 'text-right ',
                              cellClass: 'text-right cell-text '
                          });
                          $scope.gridOptions.data = result.Data;
                      }

                      
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

FinancialDisplay.filter('trusted', function ($sce) {
    return function (value) {
        return $sce.trustAsHtml(value);
    }
});




