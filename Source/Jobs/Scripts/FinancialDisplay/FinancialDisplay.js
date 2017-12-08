//FinancialDisplay = angular.module('FinancialDisplay', ['ngTouch', 'ui.grid', 'ui.grid.resizeColumns', 'ui.grid.grouping', 'ui.grid.moveColumns',
//    'ui.grid.selection', 'ui.grid.exporter', 'ui.grid.cellNav', 'ui.grid.pinning', 'ui.grid.treeView', 'ui.grid.saveState']);

FinancialDisplay = angular.module('FinancialDisplay', ['ngTouch', 'ui.grid', 
    'ui.grid.exporter', 'ui.grid.cellNav', 'ui.grid.treeView'])



FinancialDisplay.controller('MainCtrl', ['$scope', '$log', '$http', 'uiGridConstants', 'uiGridExporterConstants', 'uiGridExporterService', '$timeout',



  function ($scope, $log, $http, uiGridConstants, uiGridExporterConstants, uiGridExporterService, $timeout) {
      $scope.gridOptions = {
          enableHorizontalScrollbar: uiGridConstants.scrollbars.ALWAYS,
          showTreeExpandNoChildren : false,
          enableFiltering: true,
          enableColumnMenus: false,
          //enableTreeView : true,
          showColumnFooter: true,
          enableGridMenu: true,
          exporterMenuPdf: false,
          gridMenuShowHideColumns : false,
          exporterCsvLinkElement: angular.element(document.querySelectorAll(".custom-csv-link-location")),
          exporterCsvFilename: $("#ReportType").val() + '.csv',

        exporterMenuCsv: false,
        gridMenuCustomItems: [{
            title: 'Export Data As CSV',
            order: 100,
            action: function ($event){
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

              gridApi.core.on.rowsRendered($scope, function () {
                  $scope.$on('$viewContentLoaded', function (event) {
                      alert("go");
                  });
              });



          }
      };






      $scope.GetColumnIndexFromName = function (ColumnName) {
          var colindex = 0;
          for(var i =0 ;i <= $scope.gridApi.grid.columns.length - 1; i ++)
          {
              if ($scope.gridApi.grid.columns[i].name == ColumnName)
                  colindex = i;
          }
          return colindex;
      };

      
      var LedgerAccountGroupFilterArr = []
      var LedgerAccountFilterArr = []
      var FocusCellArr = []

      $scope.ShowDetail = function () {
          var rowCol = $scope.gridApi.cellNav.getFocusedCell();
          $("#ReportType").val(rowCol.row.entity.OpenReportType);
          $("#LedgerAccountGroup").val(rowCol.row.entity.LedgerAccountGroupId);
          $("#LedgerAccount").val(rowCol.row.entity.LedgerAccountId);

          if (rowCol != null)
              FocusCellArr.push(rowCol);

          if (rowCol.row.entity.LedgerAccountGroupName != null)
              LedgerAccountGroupFilterArr.push($scope.gridApi.grid.columns[$scope.GetColumnIndexFromName("LedgerAccountGroupName")].filters[0].term);

          if (rowCol.row.entity.LedgerAccountName != null)
              LedgerAccountFilterArr.push($scope.gridApi.grid.columns[$scope.GetColumnIndexFromName("LedgerAccountName")].filters[0].term);



          $("#IsShowDetail").prop("checked", false);
          $("#IsFullHierarchy").prop("checked", false);
          //$("#IsShowDetail").is(":checked")

          var DocTypeId = parseInt(rowCol.row.entity.DocTypeId);
          var DocId = parseInt(rowCol.row.entity.DocHeaderId);
          if ($("#ReportType").val() == null || $("#ReportType").val() == "")
          {
              window.open('/FinancialDisplay/DocumentMenu/?DocTypeId=' + DocTypeId + '&DocId=' + DocId, '_blank');
              return;
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


      var IsEscapeButtonPressed = false;

      $(document).keyup(function (e) {
          if (e.keyCode == 27) { // escape key maps to keycode `27`
              $.ajax({
                    async: false,
                    cache: false,
                    type: "POST",
                    url: '/FinancialDisplay/GetParameterSettingsForLastDisplay',
                success: function (result) {
                    if (result.ReportType != null)
                    {
                        $("#ReportType").val(result.ReportType);
                        $("#FromDate").val(result.FromDate);
                        $("#ToDate").val(result.ToDate);
                        $("#LedgerAccountGroup").val(result.LedgerAccountGroup);
                        $("#LedgerAccount").val(result.LedgerAccount);
                        if (result.IsShowDetail == "True")
                            $("#IsShowDetail").prop("checked", true);
                        else
                            $("#IsShowDetail").prop("checked", false);

                        if (result.IsFullHierarchy == "True")
                            $("#IsFullHierarchy").prop("checked", true);
                        else
                            $("#IsFullHierarchy").prop("checked", false);
                        
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    alert('Failed to retrieve product details.' + thrownError);
                }

              });

              IsEscapeButtonPressed = true;
              $scope.BindData();
              IsEscapeButtonPressed = false;

              var timeoutPeriod = 500;


              $timeout(function () {
                  if (LedgerAccountGroupFilterArr.length > 0)
                  {
                      $scope.gridApi.grid.columns[$scope.GetColumnIndexFromName("LedgerAccountGroupName")].filters[0].term = LedgerAccountGroupFilterArr[LedgerAccountGroupFilterArr.length - 1];
                      LedgerAccountGroupFilterArr.pop();
                  }

                  if (LedgerAccountFilterArr.length > 0) {
                      $scope.gridApi.grid.columns[$scope.GetColumnIndexFromName("LedgerAccountName")].filters[0].term = LedgerAccountFilterArr[LedgerAccountFilterArr.length - 1];
                      LedgerAccountFilterArr.pop();
                  }

                  if (FocusCellArr.length > 0) {
                      var row = FocusCellArr[FocusCellArr.length - 1].row;
                      var col = FocusCellArr[FocusCellArr.length - 1].col;

                      var RowIndex = 0;
                      for (var i = 0; i <= $scope.gridOptions.data.length - 1; i++)
                      {
                          if ($scope.gridOptions.data[i].LedgerAccountGroupId != null)
                              if ($scope.gridOptions.data[i].LedgerAccountGroupId == row.entity.LedgerAccountGroupId)
                                  RowIndex = i;

                          if ($scope.gridOptions.data[i].LedgerAccountId != null)
                              if ($scope.gridOptions.data[i].LedgerAccountId == row.entity.LedgerAccountId)
                                  RowIndex = i;
                      }
                      
                      if ($scope.gridOptions.data[RowIndex].LedgerAccountGroupId != null)
                          $scope.gridApi.cellNav.scrollToFocus($scope.gridOptions.data[RowIndex], $scope.gridApi.grid.columns[$scope.GetColumnIndexFromName("LedgerAccountGroupName")]);
                      else if ($scope.gridOptions.data[RowIndex].LedgerAccountId != null)
                          $scope.gridApi.cellNav.scrollToFocus($scope.gridOptions.data[RowIndex], $scope.gridApi.grid.columns[$scope.GetColumnIndexFromName("LedgerAccountName")]);

                      FocusCellArr.pop();
                  }
              }, timeoutPeriod)
          }


          if (e.keyCode == 13) { // escape key maps to keycode `27`
              if ($scope.gridApi.cellNav.getFocusedCell() != null)
              {
                  $scope.ShowDetail();
              }
          }
      });

      


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

              //rowItem.entity.LedgerAccountGroupName = rowItem.entity.LedgerAccountGroupName.toString().replace('<Strong>', '').replace('</Strong>', '')

              //console.log(rowItem.entity);
              //var myPdfRow = rowItem.entity;
              var myPdfRow = jQuery.extend({}, rowItem.entity);

              for (var key in myPdfRow) {
                  if (myPdfRow[key] != null) {
                      if (myPdfRow[key].toString().includes('<Strong>'))
                          myPdfRow[key] = myPdfRow[key].toString().replace('<Strong>', '');
                      if (myPdfRow[key].toString().includes('</Strong>'))
                          myPdfRow[key] = myPdfRow[key].toString().replace('</Strong>', '');
                      if (myPdfRow[key].toString().includes('</br>'))
                          myPdfRow[key] = myPdfRow[key].toString().replace('</br>', ' ');
                      if (myPdfRow[key].toString().includes('&nbsp;'))
                          myPdfRow[key] = myPdfRow[key].toString().replace('&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;', '      ');
                  }
                  else {
                      myPdfRow[key] = '';
                  }
              }


              //Rows[i] = rowItem.entity;
              Rows[i] = myPdfRow;
              i++;
          });

          var pdfColumns = new Array();
          var pdfColumnsStyle = {};
          var pdfColumnsHeaderStyle = {};
          i = 0;
          $.each(columns, function () {
              //console.log(columns[i]["width"] * PdfColumnAspectRatio);
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

                  doc.setFontSize(15);
                  doc.setFontStyle('bold');
                  doc.text($("#ReportType").val(), data.settings.margin.left + 102, 28)
              },

              margin: { top: 30 },
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


      var i = 0;
      $scope.BindData = function ()
      {
          $scope.myData = [];


          $.ajax({
              url: '/FinancialDisplay/FinancialDisplayFill/' + $(this).serialize(),
              async : (IsEscapeButtonPressed == true ? false : true),
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
                              if ($("#IsShowDetail").is(":checked") == true)
                              {
                                  var TotalAmtDr = 0;
                                  var TotalAmtCr = 0;
                                  if (result.Data != null) {
                                      TotalAmtDr = result.Data[0]["TotalAmtDr"];
                                      TotalAmtCr = result.Data[0]["TotalAmtCr"];
                                  }

                                  $scope.gridOptions.columnDefs.push({ field: 'LedgerAccountGroupId', width: 50, visible: false });
                                  $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'LedgerAccountGroupName', width: 720, cellClass: 'cell-text', headerCellClass: 'header-text', enableSorting: false, 
                                      cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>',
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'AmtDr',
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtDr + '</div>',
                                      width: ($("#DrCr").val() == "Debit" ? 400 : 200),
                                      aggregationType: uiGridConstants.aggregationTypes.sum,
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      cellClass: 'text-right cell-text',
                                      enableSorting: false,
                                      visible: ($("#DrCr").val() == "Credit" ? false : true)
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'AmtCr',
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtCr + '</div>',
                                      width: ($("#DrCr").val() == "Credit" ? 400 : 200),
                                      aggregationType: uiGridConstants.aggregationTypes.sum,
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      cellClass: 'text-right cell-text',
                                      enableSorting: false,
                                      visible: ($("#DrCr").val() == "Debit" ? false : true)
                                  });
                              }
                              else
                              {
                                  $scope.gridOptions.columnDefs.push({ field: 'LedgerAccountGroupId', width: 50, visible: false});
                                  $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'LedgerAccountGroupName', width: 720, cellClass: 'cell-text', headerCellClass: 'header-text',
                                      //cellTemplate: '<div style="height: 100%;" ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted" class="style:display: inline-block;">  </div>',
                                      cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>',
                                      //cellTemplate: '<div ng-bind-html="COL_FIELD | trusted"></div>',
                                      //cellTemplate: '<div ng-if="row.entity.LedgerAccountId == null">' + row.entity.LedgerAccountId + '</div>',
                                      //cellTemplate: "<div ng-show='row.entity.LedgerAccountId == null'><Strong>{{row.entity.LedgerAccountGroupName}}</Strong></div>",
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'AmtDr',
                                      width: ($("#DrCr").val() == "Debit" ? 400 : 200),
                                      aggregationType: uiGridConstants.aggregationTypes.sum,
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      cellClass: 'text-right cell-text',
                                      visible: ($("#DrCr").val() == "Credit" ? false : true)
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'AmtCr',
                                      width: ($("#DrCr").val() == "Credit" ? 400 : 200),
                                      aggregationType: uiGridConstants.aggregationTypes.sum,
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      cellClass: 'text-right cell-text',
                                      visible: ($("#DrCr").val() == "Debit" ? false : true)
                                  });
                              }
                              
                          }
                          else if ($("#DisplayType").val() == "Summary")
                          {
                              var TotalAmtDr = 0;
                              var TotalAmtCr = 0;
                              var TotalOpening = 0;
                              var TotalOpeningDrCr = "";
                              var TotalBalance = 0;
                              var TotalBalanceDrCr = "";
                              if (result.Data != null) {
                                  TotalAmtDr = result.Data[0]["TotalAmtDr"];
                                  TotalAmtCr = result.Data[0]["TotalAmtCr"];
                                  TotalOpening = result.Data[0]["TotalOpening"];
                                  TotalOpeningDrCr = result.Data[0]["TotalOpeningDrCr"];
                                  TotalBalance = result.Data[0]["TotalBalance"];
                                  TotalBalanceDrCr = result.Data[0]["TotalBalanceDrCr"];
                              }


                              if ($("#IsShowDetail").is(":checked") == true)
                              {
                                  //var TotalAmtDr = 0;
                                  //var TotalAmtCr = 0;
                                  //if (result.Data != null) {
                                  //    TotalAmtDr = result.Data[0]["TotalAmtDr"];
                                  //    TotalAmtCr = result.Data[0]["TotalAmtCr"];
                                  //}

                                  $scope.gridOptions.columnDefs.push({ field: 'LedgerAccountGroupId', width: 50, visible: false });
                                  $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'LedgerAccountGroupName', width: 360, cellClass: 'cell-text', headerCellClass: 'header-text', enableSorting: false,
                                      cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>',
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'Opening', width: 175,
                                      //aggregationType: uiGridConstants.aggregationTypes.sum,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalOpening + '</div>',
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      enableSorting: false,
                                      cellClass: 'text-right cell-text'
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'OpeningDrCr', name: '', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text', enableFiltering: false, enableSorting: false,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalOpeningDrCr + '</div>',
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'AmtDr', width: 175,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtDr + '</div>',
                                      aggregationType: uiGridConstants.aggregationTypes.sum,
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      enableSorting: false,
                                      cellClass: 'text-right cell-text'
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'AmtCr', width: 175,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtCr + '</div>',
                                      aggregationType: uiGridConstants.aggregationTypes.sum,
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      enableSorting: false,
                                      cellClass: 'text-right cell-text'
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'Balance', width: 175,
                                      //aggregationType: uiGridConstants.aggregationTypes.sum,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalBalance + '</div>',
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      enableSorting: false,
                                      cellClass: 'text-right cell-text'
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'BalanceDrCr', displayName: '', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text', enableFiltering: false, enableSorting: false,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalBalanceDrCr + '</div>',
                                  });
                              }
                              else
                              {
                                  $scope.gridOptions.columnDefs.push({ field: 'LedgerAccountGroupId', width: 50, visible: false });
                                  $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'LedgerAccountGroupName', width: 360, cellClass: 'cell-text', headerCellClass: 'header-text',
                                      cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>',
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'Opening', width: 175,
                                      //aggregationType: uiGridConstants.aggregationTypes.sum,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalOpening + '</div>',
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      cellClass: 'text-right cell-text'
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'OpeningDrCr', displayName: '', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text', enableFiltering: false,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalOpeningDrCr + '</div>',
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'AmtDr', width: 175,
                                      //aggregationType: uiGridConstants.aggregationTypes.sum,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtDr + '</div>',
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      cellClass: 'text-right cell-text'
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'AmtCr', width: 175,
                                      //aggregationType: uiGridConstants.aggregationTypes.sum,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtCr + '</div>',
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      cellClass: 'text-right cell-text'
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'Balance', width: 175,
                                      //aggregationType: uiGridConstants.aggregationTypes.sum,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalBalance + '</div>',
                                      aggregationHideLabel: true,
                                      headerCellClass: 'text-right header-text',
                                      footerCellClass: 'text-right ',
                                      cellClass: 'text-right cell-text'
                                  });
                                  $scope.gridOptions.columnDefs.push({
                                      field: 'BalanceDrCr', displayName: '', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text', enableFiltering: false,
                                      footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalBalanceDrCr + '</div>',
                                  });
                              }
                          }
                      }



                      else if ($("#ReportType").val() == "Trial Balance As Per Detail") {

                          //$scope.gridOptions.showTreeRowHeader = true;
                          
                          //$scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.OPTIONS);

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
                                  field: 'LedgerAccountGroupName', width: 720, cellClass: 'cell-text', headerCellClass: 'header-text',
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
                          }
                          else if ($("#DisplayType").val() == "Summary")
                          {
                              var TotalAmtDr = 0;
                              var TotalAmtCr = 0;
                              var TotalOpening = 0;
                              var TotalOpeningDrCr = "";
                              var TotalBalance = 0;
                              var TotalBalanceDrCr = "";
                              if (result.Data != null) {
                                  TotalAmtDr = result.Data[0]["TotalAmtDr"];
                                  TotalAmtCr = result.Data[0]["TotalAmtCr"];
                                  TotalOpening = result.Data[0]["TotalOpening"];
                                  TotalOpeningDrCr = result.Data[0]["TotalOpeningDrCr"];
                                  TotalBalance = result.Data[0]["TotalBalance"];
                                  TotalBalanceDrCr = result.Data[0]["TotalBalanceDrCr"];
                              }

                              $scope.gridOptions.columnDefs.push({ field: 'LedgerAccountGroupId', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'LedgerAccountName', width: 360, cellClass: 'cell-text', headerCellClass: 'header-text',
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'Opening', width: 175,
                                  //aggregationType: uiGridConstants.aggregationTypes.sum,
                                  footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalOpening + '</div>',
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'OpeningDrCr', name: '', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text', enableFiltering: false,
                                  footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalOpeningDrCr + '</div>',
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtDr', width: 175,
                                  //aggregationType: uiGridConstants.aggregationTypes.sum,
                                  footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtDr + '</div>',
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'AmtCr', width: 175,
                                  //aggregationType: uiGridConstants.aggregationTypes.sum,
                                  footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtCr + '</div>',
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'Balance', width: 175,
                                  //aggregationType: uiGridConstants.aggregationTypes.sum,
                                  footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalBalance + '</div>',
                                  aggregationHideLabel: true,
                                  headerCellClass: 'text-right header-text',
                                  footerCellClass: 'text-right ',
                                  cellClass: 'text-right cell-text'
                              });
                              $scope.gridOptions.columnDefs.push({
                                  field: 'BalanceDrCr', displayName: '', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text', enableFiltering: false,
                                  footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalBalanceDrCr + '</div>',
                              });
                          }
                      }
                      else if ($("#ReportType").val() == "Ledger") {


                          var TotalAmtDr = 0;
                          var TotalAmtCr = 0;
                          if (result.Data != null) {
                              TotalAmtDr = result.Data[0]["TotalAmtDr"];
                              TotalAmtCr = result.Data[0]["TotalAmtCr"];
                          }

                          var LedgerBalance = 0;
                          var FinalBalanceDrCr = "";
                          if (result.Data != null) {
                              LedgerBalance = result.Data[result.Data.length - 1]["Balance"];
                              FinalBalanceDrCr = result.Data[result.Data.length - 1]["BalanceDrCr"];
                          }

                          $scope.gridOptions.columnDefs.push({ field: 'LedgerHeaderId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'ReportType', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'DocTypeId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'DocHeaderId', width: 50, visible: false });
                          $scope.gridOptions.columnDefs.push({ field: 'DocDate', width: 90, cellClass: 'cell-text ', headerCellClass: 'header-text', enableSorting : false });
                          $scope.gridOptions.columnDefs.push({ field: 'DocNo', width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text', enableSorting: false });
                          $scope.gridOptions.columnDefs.push({
                              field: 'Narration', width: 430, cellClass: 'cell-text ', headerCellClass: 'header-text', enableSorting: false,
                              //cellTemplate: '<div ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted"></div>',
                              cellTemplate: '<div class="ui-grid-cell-contents my-cell ng-binding ng-scope " ng-dblclick="grid.appScope.ShowDetail()"  ng-bind-html="COL_FIELD | trusted">  </div>',
                          });
                          $scope.gridOptions.columnDefs.push({
                              field: 'AmtDr', width: 140,
                              //aggregationType: uiGridConstants.aggregationTypes.sum,
                              footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtDr + '</div>',
                              aggregationHideLabel: true,
                              headerCellClass: 'text-right header-text',
                              footerCellClass: 'text-right ',
                              cellClass: 'text-right cell-text ',
                              enableSorting: false
                          });
                          $scope.gridOptions.columnDefs.push({
                              field: 'AmtCr', width: 140,
                              //aggregationType: uiGridConstants.aggregationTypes.sum,
                              footerCellTemplate: '<div class="ui-grid-cell-contents" >' + TotalAmtCr + '</div>',
                              aggregationHideLabel: true,
                              headerCellClass: 'text-right header-text',
                              footerCellClass: 'text-right ',
                              cellClass: 'text-right cell-text ',
                              enableSorting: false
                          });
                          $scope.gridOptions.columnDefs.push({
                              field: 'Balance', width: 140,
                              footerCellTemplate: '<div class="ui-grid-cell-contents" >' + LedgerBalance + '</div>',
                              //aggregationType: uiGridConstants.aggregationTypes.sum,
                              //aggregationHideLabel: true,
                              headerCellClass: 'text-right header-text',
                              footerCellClass: 'text-right ',
                              cellClass: 'text-right cell-text ',
                              enableSorting: false
                          });
                          $scope.gridOptions.columnDefs.push({
                              field: 'BalanceDrCr', displayName: '', width: 30, cellClass: 'cell-text', headerCellClass: 'header-text', enableFiltering: false,
                              footerCellTemplate: '<div class="ui-grid-cell-contents" >' + FinalBalanceDrCr + '</div>',
                          });
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
          //$scope.gridApi.grid.columns[2].filters[0].term = "12";
      }
  }
]);

FinancialDisplay.filter('trusted', function ($sce) {
    return function (value) {
        return $sce.trustAsHtml(value);
    }
});




