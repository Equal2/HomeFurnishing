var app = angular.module('app', ['ngTouch', 'ui.grid', 'ui.grid.resizeColumns', 'ui.grid.grouping', 'ui.grid.moveColumns',
    'ui.grid.selection', 'ui.grid.exporter', 'ui.grid.cellNav', 'ui.grid.pinning']);

app.controller('MainCtrl', ['$scope', '$log', '$http', 'uiGridConstants', 'uiGridExporterConstants', 'uiGridExporterService',

  function ($scope, $log, $http, uiGridConstants, uiGridExporterConstants, uiGridExporterService) {

      
      //$scope.gridOptions = {};


      $scope.gridOptions = {
          enableHorizontalScrollbar: uiGridConstants.scrollbars.ALWAYS,
          enableFiltering: true,
          showColumnFooter: true,
          enableGridMenu: true,
          enableSelectAll: true,


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
          //exporterCsvLinkElement: angular.element(document.querySelectorAll(".custom-csv-link-location")),

          enableGridMenu: true,
          exporterMenuPdf: false,
          gridMenuShowHideColumns : true,
          exporterCsvLinkElement: angular.element(document.querySelectorAll(".custom-csv-link-location")),
          exporterCsvFilename: (($("#ReportTitle").val() != null && $("#ReportTitle").val()) != "" ? $("#ReportTitle").val() : $("#ReportHeader_ReportName").val()) + '.csv',
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
          }
      };


      $(document).keyup(function (e) {
          if (e.keyCode == 13) {
              if ($scope.gridApi.cellNav.getFocusedCell() != null) {
                  $scope.ShowDetail();
              }
          }
      });
      

      $scope.ShowDetail = function () {
          var rowCol = $scope.gridApi.cellNav.getFocusedCell();
          if (rowCol.row.entity.DocTypeId != null && rowCol.row.entity.DocId != null)
          {
              var DocTypeId = parseInt(rowCol.row.entity.DocTypeId);
              var DocId = parseInt(rowCol.row.entity.DocId);
              var Url = "/Redirect/RedirectToDocument?DocTypeId=" + DocTypeId + "&DocId=" + DocId + "&DocLineId=";
              window.open(Url, '_blank');
              return;
          }
          //if (rowCol.row.entity.ChildRepName != null) {
          //    $("#ReportHeader_ReportName").val(rowCol.row.entity.ChildRepName);
          //    $scope.BindData()
          //}
      };


      function GetColumnWidth(results,j) {
          var ColWidth = 130;
          if (results.Data[0][j]["Value"] != null) {
              if ((results.Data[0][j]["Value"].length * 10).toString() != "NaN") {
                  ColWidth = results.Data[0][j]["Value"].length * 10;
              }
              else {
                  ColWidth = results.Data[0][j]["Key"].length * 10
              }
          }
          else {
              ColWidth = results.Data[0][j]["Key"].length * 10
          }

          if (ColWidth < 90)
              ColWidth = 90;

          if (ColWidth > 300)
              ColWidth = 300;

          return ColWidth;
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

          if (Inch < 8)
          {
              ColumnFontSize = 10;
          }
          else if ((Inch * 90 / 100) < 8)
          {
              ColumnFontSize = 9
          }
          else if ((Inch * 80 / 100) < 8)
          {
              ColumnFontSize = 8
          }
          else
          {
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
              else
              {
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
                  else  {
                      ColumnFontSize = 7
                  }
              }
          }


          var i = 0;
          //var Rows = [];          
          //$scope.gridApi.core.getVisibleRows($scope.gridApi.grid).some(function (rowItem) {              
          //    Rows[i] = rowItem.entity;
          //    i++;
          //});
          var Rows = [];
          $scope.gridApi.core.getVisibleRows($scope.gridApi.grid).some(function (rowItem) {
              var myPdfRow = jQuery.extend({}, rowItem.entity);

              for (var key in myPdfRow) {
                  if (myPdfRow[key] == null) {
                       myPdfRow[key] = '';
                  }
              }
              Rows[i] = myPdfRow;
              i++;
          });

          
          var RowsAggregation = {};
          for (var j = 0; j <= $scope.gridOptions.columnDefs.length - 1; j++)
          {
              RowsAggregation[$scope.gridApi.grid.columns[j].name] = $scope.gridApi.grid.columns[j].getAggregationValue();
          }
          Rows.push(RowsAggregation);




          

          var pdfColumns = new Array();
          var pdfColumnsStyle = {};
          var headerStyles = new Array();
          i = 0;
          $.each(columns, function()
          {
              //console.log(columns[i]["width"] * PdfColumnAspectRatio);
              pdfColumns.push({ title: columns[i]["displayName"], dataKey : columns[i]["name"] });
              pdfColumnsStyle[columns[i]["name"]] = { columnWidth: columns[i]["width"] * ((ColumnFontSize / 1000) * 25), fontSize: ColumnFontSize, halign: (isNaN(Rows[0][columns[i]["name"]]) ? '' : 'right') };
              //headerStyles: { fontSize: ColumnFontSize, halign: 'right' },
              headerStyles.push({ fontSize: ColumnFontSize, halign : 'right' });
              i = i + 1;
          });

          

          // Only pt supported (not mm or in) 
          var doc = new jsPDF(PageOrientation, 'mm', PageSize);


          doc.autoTable(pdfColumns, Rows, {
              addPageContent: function (data) {
                  // HEADER
                  doc.setFontSize(10);
                  doc.setFontStyle('bold');

                  if ($("#ReportHeaderCompanyDetail_LogoBlob").val() != null && $("#ReportHeaderCompanyDetail_LogoBlob").val() != "")
                  {
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

                  var ReportTitle = ($("#ReportTitle").val() != null && $("#ReportTitle").val()) ? $("#ReportTitle").val() : $("#ReportHeader_ReportName").val();
                  xOffset = (doc.internal.pageSize.width / 2) - (doc.getStringUnitWidth(ReportTitle) * doc.internal.getFontSize() / 2);


                  var fontSize = doc.internal.getFontSize();
                  var pageWidth = doc.internal.pageSize.width;
                  txtWidth = doc.getStringUnitWidth(ReportTitle) * fontSize / doc.internal.scaleFactor;
                  xOffset = (pageWidth - txtWidth) / 2;
                  doc.text(ReportTitle, xOffset, 28);
              },

              margin: {top: 32},
              columnStyles: pdfColumnsStyle,
              styles: {
                  overflow: 'linebreak',
                  tableWidth: 'auto',
              },
              //headerStyles: {
              //    fontSize: ColumnFontSize,
              //},
              //headerStyles: { fontSize: ColumnFontSize, halign: 'right' },
              createdHeaderCell: function (cell, data) {
                  cell.styles.fontSize = ColumnFontSize;
                  cell.styles.halign = (isNaN(Rows[0][cell.raw.dataKey]) ? '' : 'right')
              },
              drawCell: function (cell, data) {
                  var rows = data.table.rows;
                  if (data.row.index == rows.length - 1) {
                      doc.setFillColor(200, 200, 255);
                      doc.setFontSize(10);
                      doc.setFontStyle('bold');
                  }
              }
          });
          //doc.save('table.pdf');
          var string = doc.output('datauristring');
          var iframe = "<iframe width='100%' height='100%'  src='" + string + "'></iframe>"
          var x = window.open();
          x.document.open();
          x.document.write(iframe);
          x.document.close();
      };



      $scope.BindData = function ()
      {
          $.ajax({
              url: '/GridReport/GridReportFill/' + $(this).serialize(),
              type: "POST",
              data: $("#registerSubmit").serialize(),
              success: function (result) {
                  Lock = false;
                  if (result.Success == true) {
                      var results = result;
                      if (results.Data.length > 0) {
                          var columnsIn = results.Data[results.Data.length - 1];
                          var j = 0;
                          var ColumnCount = 0;

                          $scope.gridOptions.columnDefs = new Array();

                          $.each(columnsIn, function (key, value) {
                              if (columnsIn[j]["Key"] != "SysParamType") {
                                  var ColWidth = GetColumnWidth(results, j);

                                  $scope.gridOptions.columnDefs.push({
                                      field: columnsIn[j]["Key"], aggregationType: columnsIn[j]["Value"],
                                      cellClass: (columnsIn[j]["Value"] == null ? 'cell-text' : 'text-right cell-text'),
                                      aggregationHideLabel: true,
                                      headerCellClass: (columnsIn[j]["Value"] == null ? 'header-text' : 'text-right header-text'),
                                      footerCellClass: (columnsIn[j]["Value"] == null ? '' : 'text-right '),
                                      width: ColWidth,
                                      enablePinning: true,
                                      visible: (columnsIn[j]["Key"] == "DocId" || columnsIn[j]["Key"] == "DocTypeId" ? false : true)
                                  });
                                }
                              ColumnCount++;
                              j++;
                              
                              
                          });


                          var rowDataSet = [];
                          var i = 0;
                          $.each(results.Data, function (key, value) {
                                var rowData = [];
                                var j = 0   
                                var columnsIn = results.Data[i];
                                if (columnsIn[ColumnCount - 1]["Value"] == null)
                                {
                                    $.each(columnsIn, function (key, value) {
                                        rowData[columnsIn[j]["Key"]] = columnsIn[j]["Value"];
                                        j++;
                                    });
                                }
                                rowDataSet[i] = rowData;
                                i++;
                          });

                          $scope.gridOptions.data = rowDataSet;

                          $scope.gridApi.grid.refresh();

                      }
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






