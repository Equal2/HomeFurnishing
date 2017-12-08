SaleOrderBalanceDisplay = angular.module('SaleOrderBalanceDisplay', ['ngTouch', 'ui.grid', 'ui.grid.resizeColumns', 'ui.grid.moveColumns',
    'ui.grid.exporter', 'ui.grid.cellNav', 'ui.grid.pinning'])



SaleOrderBalanceDisplay.controller('MainCtrl', ['$scope', '$log', '$http', 'uiGridConstants',



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
          if (rowCol.row.entity.BillToBuyerId != null)
          {
              $("#Buyer").select2('data', { id: rowCol.row.entity.BillToBuyerId, text: rowCol.row.entity.BuyerName });
          }
          if (rowCol.row.entity.ProductCategoryId != null)
          {
              $("#ProductCategory").select2('data', { id: rowCol.row.entity.ProductCategoryId, text: rowCol.row.entity.ProductCategoryName });
          }
          if (rowCol.row.entity.ProductTypeId != null)
          {
              $("#ProductType").select2('data', { id: rowCol.row.entity.ProductTypeId, text: rowCol.row.entity.ProductTypeName });
          }
          if (rowCol.row.entity.ProductNatureId != null)
          {
              $("#ProductNature").select2('data', { id: rowCol.row.entity.ProductNatureId, text: rowCol.row.entity.ProductNatureName });
          }
          //if (rowCol.row.entity.ProcessId != null)
          //{
          //    $("#Process").select2('data', { id: rowCol.row.entity.ProcessId, text: rowCol.row.entity.ProcessName });
          //}
          
          
          
        
           var DocTypeId = parseInt(rowCol.row.entity.DocTypeId);
           var DocId = parseInt(rowCol.row.entity.SaleOrderHeaderId);
           if (rowCol.row.entity.SaleOrderHeaderId != null)
          {
              window.open('/Display_SaleOrderBalance/DocumentMenu/?DocTypeId=' + DocTypeId + '&DocId=' + DocId, '_blank');
              return;
          }
          

          $.ajax({
              async : false,
              cache: false,
              type: "POST",
              url: '/Display_SaleOrderBalance/SaveCurrentSetting',
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
                    url: '/Display_SaleOrderBalance/GetParameterSettingsForLastDisplay',
               success: function (result) {
                   $("#Format").val(result.Format);
                   $("#FromDate").val(result.FromDate);
                   $("#ToDate").val(result.ToDate);
                   $("#Buyer").val(result.Buyer);
                   $("#ProductNature").val(result.ProductNature);
                   $("#ProductType").val(result.ProductType);
                   //$("#Process").val(result.Process);
                   $("#SiteIds").val(result.SiteId);
                   $("#DivisionIds").val(result.DivisionId);
                   $("#TextHidden").val(result.TextHidden);
                   $("#ProductCategory").val(result.ProductCategory);
                   CustomSelectFunction($("#ProductCategory"), '/ComboHelpList/GetProductCategory', '/ComboHelpList/SetSingleProductCategory', ' ', false, 0);
                   CustomSelectFunction($("#ProductNature"), '/ComboHelpList/GetProductNature', '/ComboHelpList/SetSingleProductNature', ' ', false, 0);
                   CustomSelectFunction($("#ProductType"), '/ComboHelpList/GetProductType', '/ComboHelpList/SetSingleProductType', ' ', false, 0);
                   CustomSelectFunction($("#Buyer"), '/ComboHelpList/GetPerson', '/ComboHelpList/SetSinglePerson', ' ', false, 0);
                   CustomSelectFunction($("#DivisionIds"), '/ComboHelpList/GetDivision', '/ComboHelpList/SetSingleDivision', ' ', false, 0);
                   CustomSelectFunction($("#SiteIds"), '/ComboHelpList/GetSite', '/ComboHelpList/SetSingleSite', ' ', false, 0);
                   //CustomSelectFunction($("#Process"), '/ComboHelpList/GetProcess', '/ComboHelpList/SetSingleProcess', ' ', false, 0);
                   if (result.Format != null)
                   {
                       $("#Format").select2('data', { id: result.Format, text: result.Format });
                   }
                   if (result.Buyer == null)
                       $("#Buyer").select2('data', { id: '', text: '' });
                   if (result.ProductType == null)
                       $("#ProductNature").select2('data', { id: '', text: '' });
                   if (result.ProductType == null)
                       $("#ProductType").select2('data', { id: '', text: '' });
                   if (result.SiteId == null)
                       $("#SiteIds").select2('data', { id: '', text: '' });
                   if (result.DivisionId == null)
                       $("#DivisionIds").select2('data', { id: '', text: '' });
                   //if (result.Process == null)
                   //    $("#Process").select2('data', { id: '', text: '' });
                   
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
              url: '/Display_SaleOrderBalance/DisplaySaleOrderBalanceFill/' + $(this).serialize(),
              type: "POST",
              data: $("#registerSubmit").serialize(),
              success: function (result) {
                  Lock = false;
                  if (result.Success == true) {
                      $scope.gridOptions.columnDefs = new Array();

                      if ($("#Format").val() == "Order No Wise" || $("#Format").val() == "" || $("#Format").val()==null)
                      {


                          $scope.gridOptions.columnDefs.push({ field: 'SaleOrderHeaderId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'DocTypeId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'DocNo', width: 100, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'DocDate', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DueDate', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BuyerName', width: 350, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'ProductName', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Dimension1Name', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Dimension2Name', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Dimension3Name', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Dimension4Name', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Specification', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 90, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceQty', width: 105, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'UnitName', width: 60, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 100, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DealUnitName', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'RatePerQty', width: 75, cellClass: 'cell-text ', headerCellClass: 'text-right header-text' });                         
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width: 100, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'CurrencyName', width: 75, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                      }
                      else if ($("#Format").val() == "Buyer Wise Summary")
                      {
                          $scope.gridOptions.columnDefs.push({ field: 'BillToBuyerId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false }); 
                          $scope.gridOptions.columnDefs.push({ field: 'BuyerName', width: 450, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'UnitName', width: 70, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({field: 'BalanceQty', width: 150,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,headerCellClass: 'text-right header-text',footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DealUnitName', width: 90, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width: 205, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'CurrencyName', width: 75, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                      }
                      else if ($("#Format").val() == "Month Wise Summary")
                      {
                          $scope.gridOptions.columnDefs.push({ field: 'FromDate', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ToDate', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Month', width: 200, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'UnitName', width: 70, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({field: 'BalanceQty', width: 150,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,headerCellClass: 'text-right header-text',footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DealUnitName', width: 90, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width: 205, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'CurrencyName', width: 75, cellClass: 'cell-text ', headerCellClass: 'header-text' });

                      }
                      else if ($("#Format").val() == "Product Type Wise Summary")
                      {
                          $scope.gridOptions.columnDefs.push({ field: 'ProductTypeId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ProductTypeName', width: 200, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'UnitName', width: 80, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({field: 'BalanceQty', width:150,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DealUnitName', width: 90, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width: 205, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'CurrencyName', width: 75, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                      }
                      else if ($("#Format").val() == "Product Nature Wise Summary")
                      {
                          $scope.gridOptions.columnDefs.push({ field: 'ProductNatureId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ProductNatureName', width: 200, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'UnitName', width: 80, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DealUnitName', width: 90, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width: 205, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'CurrencyName', width: 75, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                      }
                      else if ($("#Format").val() == "Construction Wise")
                      {
                          $scope.gridOptions.columnDefs.push({ field: 'ProductCategoryId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ProductCategoryName', width: 200, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'UnitName', width: 80, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DealUnitName', width: 90, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width: 205, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'CurrencyName', width: 75, cellClass: 'cell-text ', headerCellClass: 'header-text' });
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

SaleOrderBalanceDisplay.filter('trusted', function ($sce) {
    return function (value) {
        return $sce.trustAsHtml(value);
    }
});




