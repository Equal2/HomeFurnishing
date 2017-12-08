JobOrderBalanceDisplay = angular.module('JobOrderBalanceDisplay', ['ngTouch', 'ui.grid', 'ui.grid.resizeColumns', 'ui.grid.moveColumns',
    'ui.grid.exporter', 'ui.grid.cellNav', 'ui.grid.pinning'])



JobOrderBalanceDisplay.controller('MainCtrl', ['$scope', '$log', '$http', 'uiGridConstants', 'uiGridExporterConstants', 'uiGridExporterService',



  function ($scope, $log, $http, uiGridConstants, uiGridExporterConstants, uiGridExporterService, uiGridTreeViewConstants) {
      $scope.gridOptions = {
          enableGridMenu: true,
          enableFiltering: true,
          showColumnFooter: true,
          enableGridMenu: true,

          //exporterCsvFilename: 'myFile.csv',
          //exporterPdfDefaultStyle: { fontSize: 9 },
          //exporterPdfTableStyle: { margin: [30, 30, 30, 30] },
          //exporterPdfTableHeaderStyle: { fontSize: 10, bold: true, italics: true, color: 'red' },
          //exporterPdfHeader: { text: "My Header", style: 'headerStyle' },
          //exporterPdfFooter: function (currentPage, pageCount) {
          //    return { text: currentPage.toString() + ' of ' + pageCount.toString(), style: 'footerStyle' };
          //},
          //exporterPdfCustomFormatter: function (docDefinition) {
          //    docDefinition.styles.headerStyle = { fontSize: 22, bold: true };
          //    docDefinition.styles.footerStyle = { fontSize: 10, bold: true };
          //    return docDefinition;
          //},
          //exporterPdfOrientation: 'portrait',
          //exporterPdfPageSize: 'LETTER',
          //exporterPdfMaxGridWidth: 500,
          //enableHorizontalScrollbar: uiGridConstants.scrollbars.ALWAYS,
          //exporterCsvLinkElement: angular.element(document.querySelectorAll(".custom-csv-link-location")),
          //exporterCsvFilename: 'myFile.csv',


          exporterMenuPdf: false,
          gridMenuShowHideColumns: true,
          exporterCsvLinkElement: angular.element(document.querySelectorAll(".custom-csv-link-location")),
          exporterCsvFilename: $("#ReportType").val() + '.csv',
          exporterMenuCsv: false,
          gridMenuCustomItems: [{
              title: 'Export Data As CSV',
              order: 100,
              action: function ($event) {
                  $scope.gridApi.exporter.csvExport('visible', 'visible');
              }
          },
          {
              title: 'Export Data As PDF',
              order: 100,
              action: function ($event) {
                  $scope.export();
              }
          }],


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
          if (rowCol.row.entity.JobWorkerId != null)
          {
              $("#JobWorker").select2('data', { id: rowCol.row.entity.JobWorkerId, text: rowCol.row.entity.SupplierName });
          }
          if (rowCol.row.entity.ProductTypeId != null)
          {
              $("#ProductType").select2('data', { id: rowCol.row.entity.ProductTypeId, text: rowCol.row.entity.ProductTypeName });
          }
          if (rowCol.row.entity.ProductNatureId != null)
          {
              $("#ProductNature").select2('data', { id: rowCol.row.entity.ProductNatureId, text: rowCol.row.entity.ProductNatureName });
          }
          if (rowCol.row.entity.ProcessId != null)
          {
              $("#Process").select2('data', { id: rowCol.row.entity.ProcessId, text: rowCol.row.entity.ProcessName });
          }
          
          
          
        
           var DocTypeId = parseInt(rowCol.row.entity.DocTypeId);
           var DocId = parseInt(rowCol.row.entity.JobOrderHeaderId);
            if (rowCol.row.entity.JobOrderHeaderId != null)
          {
              window.open('/Display_JobOrderBalance/DocumentMenu/?DocTypeId=' + DocTypeId + '&DocId=' + DocId, '_blank');
              return;
          }
          

          $.ajax({
              async : false,
              cache: false,
              type: "POST",
              url: '/Display_JobOrderBalance/SaveCurrentSetting',
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
                    url: '/Display_JobOrderBalance/GetParameterSettingsForLastDisplay',
               success: function (result) {
                   $("#Format").val(result.Format);
                   $("#FromDate").val(result.FromDate);
                   $("#ToDate").val(result.ToDate);
                   $("#JobWorker").val(result.JobWorker);
                   $("#ProductNature").val(result.ProductNature);
                   $("#ProductType").val(result.ProductType);
                   $("#Process").val(result.Process);
                   $("#SiteIds").val(result.SiteId);
                   $("#DivisionIds").val(result.DivisionId);
                   $("#TextHidden").val(result.TextHidden);

                   CustomSelectFunction($("#ProductNature"), '/ComboHelpList/GetProductNature', '/ComboHelpList/SetSingleProductNature', ' ', false, 0);
                   CustomSelectFunction($("#ProductType"), '/ComboHelpList/GetProductType', '/ComboHelpList/SetSingleProductType', ' ', false, 0);
                   CustomSelectFunction($("#JobWorker"), '/ComboHelpList/GetPerson', '/ComboHelpList/SetSinglePerson', ' ', false, 0);
                   CustomSelectFunction($("#DivisionIds"), '/ComboHelpList/GetDivision', '/ComboHelpList/SetSingleDivision', ' ', false, 0);
                   CustomSelectFunction($("#SiteIds"), '/ComboHelpList/GetSite', '/ComboHelpList/SetSingleSite', ' ', false, 0);
                   CustomSelectFunction($("#Process"), '/ComboHelpList/GetProcess', '/ComboHelpList/SetSingleProcess', ' ', false, 0);

                   if (result.Format != null)
                   {
                       $("#Format").select2('data', { id: result.Format, text: result.Format });
                   }
                   if (result.ProductType == null)
                       $("#JobWorker").select2('data', { id: '', text: '' });
                   if (result.ProductType == null)
                       $("#ProductNature").select2('data', { id: '', text: '' });
                   if (result.ProductType == null)
                       $("#ProductType").select2('data', { id: '', text: '' });
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



      var Dimension1Caption = "Dimension1";
      var Dimension2Caption = "Dimension2";
      var Dimension3Caption = "Dimension3";
      var Dimension4Caption = "Dimension4";
      function SetCaptions()
      {
          $.ajax({
              async: false,
              cache: false,
              type: "POST",
              url: '/Display_JobOrderBalance/GetDivisionSettings',
              success: function (result) {
                  Dimension1Caption = result.Dimension1Caption;
                  Dimension2Caption = result.Dimension2Caption;
                  Dimension3Caption = result.Dimension3Caption;
                  Dimension4Caption = result.Dimension4Caption;
              },
              error: function (xhr, ajaxOptions, thrownError) {
                  alert('Failed to retrieve product details.' + thrownError);
              }
          });
      }



      $scope.export = function () {
          var i = 0;
          var columns = $scope.gridApi.grid.options.showHeader ? uiGridExporterService.getColumnHeaders($scope.gridApi.grid, 'visible') : [];

          var pagewidth = 0;
          $.each(columns, function () {
              pagewidth = pagewidth + columns[i]["width"]
              i = i + 1;
          });



          var PageOrientation = 'p';
          var PageSize = 'A4';
          var ColumnFontSize = 10;
          var Inch = (pagewidth * 1 / 100);

          if (Inch < 8) {
              ColumnFontSize = 10;
          }
          else if ((Inch * 90 / 100) < 8) {
              ColumnFontSize = 9
          }
          else if ((Inch * 80 / 100) < 8) {
              ColumnFontSize = 8
          }
          else {
              PageOrientation = 'l';
              if (Inch < 11) {
                  ColumnFontSize = 10;
              }
              else if ((Inch * 90 / 100) < 11) {
                  ColumnFontSize = 9
              }
              else if ((Inch * 80 / 100) < 11) {
                  ColumnFontSize = 8
              }
              else if ((Inch * 70 / 100) < 11) {
                  ColumnFontSize = 7
              }
              else if ((Inch * 60 / 100) < 11) {
                  ColumnFontSize = 7
              }
              else {
                  PageSize = 'legal';
                  if (Inch < 13.5) {
                      ColumnFontSize = 10;
                  }
                  else if ((Inch * 90 / 100) < 13.5) {
                      ColumnFontSize = 9
                  }
                  else if ((Inch * 80 / 100) < 13.5) {
                      ColumnFontSize = 8
                  }
                  else if ((Inch * 70 / 100) < 13.5) {
                      ColumnFontSize = 7
                  }
                  else {
                      ColumnFontSize = 7
                  }
              }
          }



          var Rows = [];
          $scope.gridApi.core.getVisibleRows($scope.gridApi.grid).some(function (rowItem) {
              Rows[i] = rowItem.entity;
              i++;
          });



          var pdfColumns = new Array();
          var pdfColumnsStyle = {};
          var pdfColumnsHeaderStyle = {};
          i = 0;
          $.each(columns, function () {
              pdfColumns.push({ title: columns[i]["displayName"], dataKey: columns[i]["name"] });
              pdfColumnsStyle[columns[i]["name"]] = { columnWidth: columns[i]["width"] * ((ColumnFontSize / 1000) * 25), fontSize: ColumnFontSize };
              i = i + 1;
          });



          // Only pt supported (not mm or in) 
          var doc = new jsPDF(PageOrientation, 'mm', PageSize);


          doc.autoTable(pdfColumns, Rows, {
              addPageContent: function (data) {
                  // HEADER
                  doc.setFontSize(10);
                  doc.setFontStyle('bold');

                  if ($("#ReportHeaderCompanyDetail_LogoBlob").val() != null && $("#ReportHeaderCompanyDetail_LogoBlob").val() != "") {
                      var imgData = 'data:image/jpeg;base64,' + $("#ReportHeaderCompanyDetail_LogoBlob").val();
                      doc.addImage(imgData, 'JPEG', data.settings.margin.left, 6, 20, 18);
                  }
                  doc.text($("#ReportHeaderCompanyDetail_CompanyName").val(), data.settings.margin.left + 22, 10)
                  doc.setFontSize(9);
                  doc.setFontStyle('normal');
                  doc.text($("#ReportHeaderCompanyDetail_Address").val(), data.settings.margin.left + 22, 14)
                  doc.text($("#ReportHeaderCompanyDetail_CityName").val(), data.settings.margin.left + 22, 18)
                  doc.text($("#ReportHeaderCompanyDetail_Phone").val(), data.settings.margin.left + 22, 22)
              },

              margin: { top: 28 },
              columnStyles: pdfColumnsStyle,
              styles: {
                  overflow: 'linebreak',
                  tableWidth: 'auto',
              },
              headerStyles: {
                  fontSize: ColumnFontSize,
              },
          });
          //doc.save('table.pdf');
          var string = doc.output('datauristring');
          var iframe = "<iframe width='100%' height='100%' src='" + string + "'></iframe>"
          var x = window.open();
          x.document.open();
          x.document.write(iframe);
          x.document.close();

      };



      //$scope.gridApi.rowsRendered($scope, function () {
      //    //var allvisiblerows = $scope.gridApi.core.getVisibleRows($scope.gridApi.grid);
      //    //$scope.visibleRowsCount = allvisiblerows.length;
      //    $scope.removeNoDataColumn();
      //});






      $scope.removeNoDataColumns = function () {
          var removeColumns = new Array();

          for (var i = 0; i <= $scope.gridOptions.columnDefs.length - 1 ; i++) {
              removeColumns.push({ columnname: $scope.gridOptions.columnDefs[i].field })
          }

          for (var i = 0; i <= $scope.gridOptions.columnDefs.length - 1 ; i++) {
              for (var k = 0; k <= $scope.gridOptions.data.length - 1 ; k++) {
                  if ($scope.gridOptions.data[k][$scope.gridOptions.columnDefs[i].field] != 0
                      && $scope.gridOptions.data[k][$scope.gridOptions.columnDefs[i].field] != ""
                      && $scope.gridOptions.data[k][$scope.gridOptions.columnDefs[i].field] != null
                      && $scope.gridOptions.columnDefs[i].field != "DealUnitName"
                      && $scope.gridOptions.columnDefs[i].field != "BalanceDealQty")
                  {
                      for (var j = 0; j <= removeColumns.length - 1 ; j++) {
                          if (removeColumns[j].columnname == $scope.gridOptions.columnDefs[i].field) {
                              removeColumns.splice(j, 1);
                          }
                      }
                  }

                  if ($scope.gridOptions.data[k]["DealUnitName"] != $scope.gridOptions.data[k]["UnitName"])
                  {
                      for (var j = 0; j <= removeColumns.length - 1 ; j++) {
                          if (removeColumns[j].columnname == "DealUnit" || removeColumns[j].columnname == "BalanceDealQty") {
                              removeColumns.splice(j, 1);
                          }
                      }
                  }
              }
          }

          if (removeColumns.length != $scope.gridOptions.columnDefs.length) {
              for (var i = 0; i <= removeColumns.length - 1 ; i++) {
                  for (var j = 0; j <= $scope.gridOptions.columnDefs.length - 1 ; j++) {
                      if ($scope.gridOptions.columnDefs[j].field == removeColumns[i].columnname) {
                          $scope.gridOptions.columnDefs.splice(j, 1);
                      }
                  }
              }
          }
      }


      var i = 0;
      $scope.BindData = function ()
      {
          SetCaptions();
          
          $scope.myData = [];


          $.ajax({
              url: '/Display_JobOrderBalance/DisplayJobOrderBalanceFill/' + $(this).serialize(),
              type: "POST",
              data: $("#registerSubmit").serialize(),
              success: function (result) {
                  Lock = false;
                  if (result.Success == true) {
                      $scope.gridOptions.columnDefs = new Array();

                      if ($("#Format").val() == "Order No Wise" || $("#Format").val() == "" || $("#Format").val()==null)
                      {
                          $scope.gridOptions.columnDefs.push({ field: 'JobOrderHeaderId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'DocTypeId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'DocNo', width: 100, cellClass: 'cell-text', headerCellClass: 'header-text',cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>',});
                          $scope.gridOptions.columnDefs.push({ field: 'DocDate', width:100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DueDate', width:100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'SupplierName', width: 250, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'ProductName', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Dimension1', displayName: Dimension1Caption, width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Dimension2', displayName: Dimension2Caption, width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Dimension3', displayName: Dimension3Caption, width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Dimension4', displayName: Dimension4Caption, width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'ProdOrderNo', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'LotNo', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Specification', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });                          
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 90, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceQty', width:105,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,headerCellClass: 'text-right header-text',footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});
                          $scope.gridOptions.columnDefs.push({ field: 'UnitName', width: 65, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 100, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'DealUnitName', width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'Rate', width: 75, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width:100,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,headerCellClass: 'text-right header-text',footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});
                      }
                    else  if($("#Format").val()=="Job Worker Wise Summary")
                      {
                          $scope.gridOptions.columnDefs.push({ field: 'JobWorkerId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false }); 
                          $scope.gridOptions.columnDefs.push({ field: 'SupplierName', width: 450, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceQty', width: 150,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,headerCellClass: 'text-right header-text',footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width: 205,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,headerCellClass: 'text-right header-text',footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});

                      }
                    else if ($("#Format").val() == "Month Wise Summary") {
                          $scope.gridOptions.columnDefs.push({ field: 'FromDate', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ToDate', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Month', width: 450, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceQty', width: 150,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,headerCellClass: 'text-right header-text',footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width: 205,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,headerCellClass: 'text-right header-text',footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});

                      }
                      else if ($("#Format").val() == "Product Type Wise Summary") {
                          $scope.gridOptions.columnDefs.push({ field: 'ProductTypeId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ProductTypeName', width: 450, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceQty', width:150,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width:205,aggregationType: uiGridConstants.aggregationTypes.sum,aggregationHideLabel: true,headerCellClass: 'text-right header-text',footerCellClass: 'text-right ',cellClass: 'text-right cell-text'});
                      }
                      else if ($("#Format").val() == "Product Nature Wise Summary") {
                          $scope.gridOptions.columnDefs.push({ field: 'ProductNatureId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ProductNatureName', width: 450, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width: 205, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                      }
                      else if ($("#Format").val() == "Process Wise Symmary") {
                          $scope.gridOptions.columnDefs.push({ field: 'ProcessId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'Format', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ProcessName', width: 450, cellClass: 'cell-text', headerCellClass: 'header-text', cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>', });
                          $scope.gridOptions.columnDefs.push({ field: 'OrderQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceDealQty', width: 150, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                          $scope.gridOptions.columnDefs.push({ field: 'BalanceAmount', width: 205, aggregationType: uiGridConstants.aggregationTypes.sum, aggregationHideLabel: true, headerCellClass: 'text-right header-text', footerCellClass: 'text-right ', cellClass: 'text-right cell-text' });
                      }
                      $scope.gridOptions.data = result.Data;
                      $scope.gridApi.grid.refresh();
                      $scope.removeNoDataColumns();
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

JobOrderBalanceDisplay.filter('trusted', function ($sce) {
    return function (value) {
        return $sce.trustAsHtml(value);
    }
});




