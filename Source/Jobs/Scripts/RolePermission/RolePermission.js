RolePermission = angular.module('RolePermission', ['ngTouch', 'ui.grid',
    'ui.grid.exporter', 'ui.grid.cellNav', 'ui.grid.pinning'])



RolePermission.controller('MainCtrl', ['$rootScope', '$scope', '$log', '$http', 'modal', 'uiGridConstants', 'uiGridExporterConstants', 'uiGridExporterService',



  function ($rootScope, $scope, $log, $http, modal, uiGridConstants, uiGridExporterConstants, uiGridExporterService, uiGridTreeViewConstants) {
      $scope.gridOptions = {
          enableFiltering: true,
          enableGridMenu: false,
          enableColumnMenus: false,
          onRegisterApi: function (gridApi) {
              $scope.gridApi = gridApi;
          }
      };


      $scope.SavePermission = function (row) {
          $.ajax({
              cache: false,
              url: "/RolePermission/SavePermission/",
              data: {
                  RoleId: $('#Id').val(), DocumentTypeId: row.entity.DocumentTypeId, ControllerName: row.entity.ControllerName,
                  AddActionName: row.entity.AddActionName, Add: row.entity.Add,
                  EditActionName: row.entity.EditActionName, Edit: row.entity.Edit,
                  DeleteActionName: row.entity.DeleteActionName, Delete: row.entity.Delete,
                  PrintActionName: row.entity.PrintActionName, Print: row.entity.Print,
                  SubmitActionName: row.entity.SubmitActionName, Submit: row.entity.Submit,
              },
              success: function (data) {
              },
              error: function (xhr, ajaxOptions, thrownError) {
                  alert('Failed to retrive calculation footer' + thrownError);
              },
          });
      };


      $rootScope.SaveProcessPermission = function (row) {
          $.ajax({
              cache: false,
              url: "/RolePermission/SaveProcessPermission/",
              data: {
                  RoleId: $('#Id').val(), DocumentTypeId: row.entity.DocumentTypeId, ControllerName: row.entity.ControllerName,
                  ProcessId: row.entity.ProcessId, IsActive: row.entity.IsActive,
              },
              success: function (data) {
              },
              error: function (xhr, ajaxOptions, thrownError) {
                  alert('Failed to retrive calculation footer' + thrownError);
              },
          });
      };

      $scope.ProcessWisePermission = function (row) {
          //alert("ProcessWisePermission");

          var myModal = new modal();

          $scope.hideGrid = true;

          $rootScope.gridOptions = {
              onRegisterApi: function (gridApi) {
                  $rootScope.gridApi = gridApi;
              }
          };

          var DocTypeId = row.entity.DocumentTypeId;
          var RoleId = $('#Id').val();


          $.ajax({
              url: '/RolePermission/RoleProcessPermissionFill/?RoleId=' + RoleId + '&DocTypeId=' + DocTypeId,
              async : false,
              type: "POST",
              data: $("#registerSubmit").serialize(),
              success: function (result) {
                  if (result.Success == true) {
                      $rootScope.gridOptions.columnDefs = new Array();
                      $rootScope.gridOptions.columnDefs.push({ field: 'DocumentTypeId', width: 50, visible: false });
                      $rootScope.gridOptions.columnDefs.push({ field: 'ProcessId', width: 50, visible: false });
                      $rootScope.gridOptions.columnDefs.push({ field: 'ProcessName', width: 400, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                      $rootScope.gridOptions.columnDefs.push({ field: 'IsActive', width: 145, cellClass: 'cell-text ', headerCellClass: 'header-text', type: 'boolean', cellTemplate: '<input type="checkbox" ng-model="row.entity.IsActive" ng-click="grid.appScope.SaveProcessPermission(row);" >' });
                      $rootScope.gridOptions.data = result.Data;
                  }
                  else if (!result.Success) {
                      alert('Something went wrong');
                  }

              },
              error: function () {
                  alert('Something went wrong');
              }
          });

          myModal.open();
      };


      angular.element(document).ready(function () {
          if ($('#Id').val() != "" && $('#Id').val() != null)
              $scope.BindData();
      });




      var i = 0;
      $scope.BindData = function () {
          $scope.myData = [];

          $.ajax({
              url: '/RolePermission/RolePermissionFill/' + $('#Id').val(),
              type: "POST",
              data: $("#registerSubmit").serialize(),
              success: function (result) {
                  Lock = false;
                  if (result.Success == true) {
                      $scope.gridOptions.columnDefs = new Array();
                      $scope.gridOptions.columnDefs.push({ field: 'DocumentTypeId', width: 50, visible: false });
                      $scope.gridOptions.columnDefs.push({ field: 'DocumentTypeName', width: 580, cellClass: 'cell-text ', headerCellClass: 'header-text' });
                      $scope.gridOptions.columnDefs.push({
                          field: 'Process', name: '', enableFiltering: false, enableSorting: false, width: 40,
                          cellTemplate: '<div ng-if="row.entity.DocumentTypeName != \'Binding\'"><button class="btn primary" ng-click="grid.appScope.ProcessWisePermission(row)">...</button></div>'
                      });
                      $scope.gridOptions.columnDefs.push({ field: 'ControllerName', width: 50, visible: false });
                      $scope.gridOptions.columnDefs.push({ field: 'AddActionName', width: 50, visible: false });
                      $scope.gridOptions.columnDefs.push({ field: 'Add', width: 100, cellClass: 'cell-text-center ', headerCellClass: 'header-text-center', type: 'boolean', cellTemplate: '<input type="checkbox" ng-model="row.entity.Add" ng-click="grid.appScope.SavePermission(row);" >' });
                      $scope.gridOptions.columnDefs.push({ field: 'EditActionName', width: 50, visible: false });
                      $scope.gridOptions.columnDefs.push({ field: 'Edit', width: 100, cellClass: 'cell-text-center ', headerCellClass: 'header-text-center', type: 'boolean', cellTemplate: '<input type="checkbox" ng-model="row.entity.Edit" ng-click="grid.appScope.SavePermission(row);" >' });
                      $scope.gridOptions.columnDefs.push({ field: 'DeleteActionName', width: 50, visible: false });
                      $scope.gridOptions.columnDefs.push({ field: 'Delete', width: 100, cellClass: 'cell-text-center ', headerCellClass: 'header-text-center', type: 'boolean', cellTemplate: '<input type="checkbox" ng-model="row.entity.Delete" ng-click="grid.appScope.SavePermission(row);" >' });
                      $scope.gridOptions.columnDefs.push({ field: 'PrintActionName', width: 50, visible: false });
                      $scope.gridOptions.columnDefs.push({ field: 'Print', width: 100, cellClass: 'cell-text-center ', headerCellClass: 'header-text-center', type: 'boolean', cellTemplate: '<input type="checkbox" ng-model="row.entity.Print" ng-click="grid.appScope.SavePermission(row);" >' });
                      $scope.gridOptions.columnDefs.push({ field: 'SubmitActionName', width: 50, visible: false });
                      $scope.gridOptions.columnDefs.push({ field: 'Submit', width: 100, cellClass: 'cell-text-center ', headerCellClass: 'header-text-center', type: 'boolean', cellTemplate: '<input type="checkbox" ng-model="row.entity.Submit" ng-click="grid.appScope.SavePermission(row);" >' });
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


RolePermission.factory('modal', ['$compile', '$rootScope', function ($compile, $rootScope) {
    return function() {
        var elm;
        var modal = {
            open: function() {
 
                var html = '<div class="modal" ng-style="modalStyle">{{modalStyle}}<div class="modal-dialog"><div class="modal-content"><div class="modal-header"><b>Applicable Processes</b></div><div class="modal-body"><div id="grid1" ui-grid="gridOptions" class="grid"></div></div><div class="modal-footer"><button id="buttonClose" class="btn btn-primary" ng-click="close()">Close</button></div></div></div></div>';
                elm = angular.element(html);
                angular.element(document.body).prepend(elm);
 
                $rootScope.close = function() {
                    modal.close();
                };
 
                $rootScope.modalStyle = {"display": "block"};
 
                $compile(elm)($rootScope);
            },
            close: function() {
                if (elm) {
                    elm.remove();
                }
            }
        };
 
        return modal;
    };
}]);




