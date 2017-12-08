/// <reference path="select2.min.js" />






function CustomSelectFunction(ElementId, GetAction, SetAction, placehold, IsMultiple,MinLength,filterid) {
    var geturl = GetAction;
    //The url we will send our get request to
    var attendeeUrl = GetAction;
    var pageSize = 20;
    
    ElementId.select2(
    {
        
        placeholder: placehold,
        //Does the user have to enter any data before sending the ajax request
        minimumInputLength: MinLength,
        allowClear: true,
        multiple: IsMultiple,
        ajax: {
            //How long the user has to pause their typing before sending the next request
            quietMillis: 1000,
            //The url of the json service
            url: attendeeUrl,
            dataType: 'jsonp',
            //Our search term and what page we are on
            data: function (term, page) {
                return {
                    pageSize: pageSize,
                    pageNum: page,
                    searchTerm: term,
                    filter:filterid
                };
            },
            results: function (data, page) {
                //Used to determine whether or not there are more results available,
                //and if requests for more data should be sent in the infinite scrolling
                var more = (page * pageSize) < data.Total;
                return { results: data.Results, more: more };
            }
        },
        initSelection: function (element, callback) {

            var xval = element.val();
            if (xval != 0)
            {
            $.ajax({
                cache: false,
                type: "POST",
                url: SetAction,
                data: { Ids: element.val() },
                success: function (data) {
                    callback(data);
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    alert('Failed to Retrive Qty' + thrownError);
                }
            })
            }
            //callback([{ id: "1", text: "arpit" }, { id: "2", text: "akash" }]);
        }
    });

    //Id.change(function () {
    //    var selectedList = Id.select2("val");
    //    alert(selectedList);
    //    Id.val(selectedList);
    //})


    function SetProducts() {
        //alert("set product called");
        Id.select2("data", [{ id: "1", text: "arpit" }, { id: "2", text: "akash" }]);
    }
}