angular.module('SaleEnquiryProductMappingWithGrid', ['ui.grid', 'ui.grid.resizeColumns', 'ui.grid.cellNav', 'ui.grid.edit', 'ui.select'])
  .controller('MainCtrl', MainCtrl)
  .directive('uiSelectWrap', uiSelectWrap);

MainCtrl.$inject = ['$scope', '$http', 'uiGridConstants'];
function MainCtrl($scope, $http, uiGridConstants) {
    var ProductList = [];
    var ProductList_New = null;

    SetCaptions();
    GetProductList();


    $scope.gridOptions = {
        
        onRegisterApi: function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.edit.on.afterCellEdit($scope, function (rowEntity, colDef, newValue, oldValue) {
                //alert(newValue)
                if (newValue != oldValue) {
                    //alert(newValue)
                    $scope.Post(rowEntity.SaleEnquiryLineId, newValue);
                }
            });
        },

        enableHorizontalScrollbar: uiGridConstants.scrollbars.ALWAYS,
        //rowHeight: 38,
        enableFiltering: true,
        columnDefs: [
          { name: 'SaleEnquiryLineId', width: 50, visible: false },
          { name: 'SaleEnquiryHeaderDocNo', displayName: "Enquiry No", width: 100, cellClass: 'cell-text ', headerCellClass: 'header-text', enableCellEdit : false },
          { name: 'SaleEnquiryHeaderDocDate', displayName: "Enquiry Date", width: 120, cellClass: 'cell-text ', headerCellClass: 'header-text', enableCellEdit: false },
          { name: 'SaleToBuyerName', displayName: "Buyer", width: 150, cellClass: 'cell-text ', headerCellClass: 'header-text', enableCellEdit: false },
          { name: 'BuyerSpecification', displayName: BuyerSpecificationCaption, width: 120, cellClass: 'cell-text ', headerCellClass: 'header-text', enableCellEdit: false },
          { name: 'BuyerSpecification1', displayName: BuyerSpecification1Caption, width: 120, cellClass: 'cell-text ', headerCellClass: 'header-text', enableCellEdit: false },
          { name: 'BuyerSpecification2', displayName: BuyerSpecification2Caption, width: 120, cellClass: 'cell-text ', headerCellClass: 'header-text', enableCellEdit: false },
          { name: 'BuyerSpecification3', displayName: BuyerSpecification3Caption, width: 120, cellClass: 'cell-text ', headerCellClass: 'header-text', enableCellEdit: false },
          {
              name: 'Product', cellClass: 'cell-text ', headerCellClass: 'header-text',
              editableCellTemplate: 'uiSelect',
              //editDropdownOptionsArray: [
              //     { projectorebody_id: '1', orebodyName: 'Residential Plot' },
              //      { projectorebody_id: '2', orebodyName: 'Commercial Plot' },
              //      { projectorebody_id: '3', orebodyName: 'Apartment/Flat' },
              //      { projectorebody_id: '4', orebodyName: 'Townhouse' },
              //      { projectorebody_id: '5', orebodyName: 'Single Family House' },
              //      { projectorebody_id: '6', orebodyName: 'Commercial Property' }
              //],
              //propertyTypes : [
              //      { value: '1', name: 'Residential Plot' },
              //      { value: '2', name: 'Commercial Plot' },
              //      { value: '3', name: 'Apartment/Flat' },
              //      { value: '4', name: 'Townhouse' },
              //      { value: '5', name: 'Single Family House' },
              //      { value: '6', name: 'Commercial Property' },
              //        { value: '7', name: 'Commercial Property' },
              //      { value: '8', name: 'Commercial Property' }
              //],
              editDropdownOptionsArray: ProductList_New
          }
        ]
    };


    $scope.Post = function (SaleEnquiryLineId, ProductName) {
        $.ajax({
            cache: false,
            url: "/SaleEnquiryProductMappingWithGrid/Post/",
            data: { SaleEnquiryLineId: SaleEnquiryLineId, ProductName: ProductName },
            success: function (data) {
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to retrive calculation footer' + thrownError);
            },
        });
    };

    angular.element(document).ready(function () {
        $scope.BindData();
    });


    var BuyerSpecificationCaption = "Buyer Specification";
    var BuyerSpecification1Caption = "Buyer Specification1";
    var BuyerSpecification2Caption = "Buyer Specification2";
    var BuyerSpecification3Caption = "Buyer Specification3";
    function SetCaptions() {
        $.ajax({
            async: false,
            cache: false,
            type: "POST",
            url: '/SaleEnquiryProductMappingWithGrid/GetProductBuyerSettings',
            success: function (result) {
                BuyerSpecificationCaption = result.BuyerSpecificationCaption;
                BuyerSpecification1Caption = result.BuyerSpecification1Caption;
                BuyerSpecification2Caption = result.BuyerSpecification2Caption;
                BuyerSpecification3Caption = result.BuyerSpecification3Caption;
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to retrieve product details.' + thrownError);
            }
        });
    }

    $scope.BindData = function () {
        $.ajax({
            async: false,
            cache: false,
            type: "POST",
            url: '/SaleEnquiryProductMappingWithGrid/PendingMappingFill',
            success: function (result) {
                $scope.gridOptions.data = result.Data;
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to retrieve product details.' + thrownError);
            }
        });
    }


    
    function GetProductList() {
        $.ajax({
            async: false,
            cache: false,
            type: "POST",
            url: '/SaleEnquiryProductMappingWithGrid/GetCustomProductHelpList',
            success: function (result) {
                ProductList_New = result.Data;
                var i = 0;
                for (var elem in result.Data) {
                    if (i <= 10)
                        ProductList.push(result.Data[elem].text);
                    i = i + 1;
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to retrieve product details.' + thrownError);
            }
        });
    }
}

uiSelectWrap.$inject = ['$document', 'uiGridEditConstants'];
function uiSelectWrap($document, uiGridEditConstants) {
    return function link($scope, $elm, $attr) {
        $document.on('click', docClick);
        $document.on('keydown', docClick);

        function docClick(evt) {
            if ($(evt.target).closest('.ui-select-container').size() === 0) {
                $scope.$emit(uiGridEditConstants.events.END_CELL_EDIT);
                $document.off('click', docClick);
            }
        }
    };
}

