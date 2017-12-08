/// <reference path="jquery-2.0.3.min.js" />



//$.ajaxSetup({
//    error: function (jqXHR, exception) {
//        var obj = jQuery.parseJSON(jqXHR.responseText);        
//        if (jqXHR.status === 401) {
//            alert('HTTP Error 401 Unauthorized.');
//        } else if(obj.message=="Permission Denied"){
//            window.location = "/Error/PermissionDenied"
//        }
//    }
//});


var StatusContstantsEnum = {
    Drafted: 0,
    Submitted: 1,
    Approved: 2,
    Modified: 3,
    ModificationSubmitted: 4,
    Closed:5,
}

var TransactionTypeConstantsEnum = {
    Issue: "Issue",
    Receive:"Receive",
}



$(document).ready(function () {


    $("img.UserIndexImage,img.UserImage").error(function () {
        $(this).attr('src', '/Images/DefaultUser.png');
    });

    //ProgressBar
    var start = 0;
    var astart = 0;



    NProgress.configure({ showSpinner: false });
    $(document).ready(function () {
        start = 1;
        NProgress.start();
        return;
    });

    jQuery(window).load(function () {
        if (astart != 1) {
            NProgress.done();
        }
        start = 0;
        return;

    });
    $(document).ajaxStart(function () {
        astart = 1;
        NProgress.start();
        return;
    });

    $(document).ajaxStop(function () {
        if (start != 1) {
            NProgress.done();
        }
        astart = 0;
        return;
    });






    //PageRefresh
    $("#RefreshPage").click(function () {
        window.location.reload();
        return false;
    });

    $("a.bookmark").click(function () {

        if (!$(this).hasClass('BookMarked')) {

            saveBookmark($(this).attr('href'));

            UpdateRecord($(this));

            $(this).addClass('BookMarked');
        }
        else {
            deleteBookmark($(this).attr('href'));
            DeleteRecord($(this));
            $(this).removeClass('BookMarked');
        }
        return false;
    });




    function saveBookmark(element) {

        $.ajax({
            cache: false,
            type: 'POST',
            url: "/UserBookMark/AddBookMark/",
            data: { caid: element },
            success: function (data) {

            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to add Bookmark' + thrownError);
            },
        });

        return;

    };



    function deleteBookmark(element) {

        $.ajax({
            cache: false,
            type: 'POST',
            url: "/UserBookMark/RemoveBookMark/",
            data: { caid: element },
            success: function (data) {
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to delete Bookmark' + thrownError);
            },
        });

        return;

    };



    function UpdateRecord(item) {
        var name = item.closest("li").find(".menuname").text();
        var iconname = item.closest('li').find('.menuname span').attr('class');
        $("#bookmarkdd").append(" <li><a href='/Menu/DropDown/" + item.attr("href") + "'> <span class='" + iconname + "'> </span>" + name + "</a></li>");

        return this;
    }



    function DeleteRecord(item) {

        var num = item.attr('href');
        var simnum = new RegExp(num);

        $('#bookmarkdd a').filter(function () { return simnum.test($(this).attr('href')); }).empty();

        return this;
    }




    //Assigning Permissions

    $("a.permission").click(function () {

        if (!$(this).hasClass('Assigned')) {

            savePermission($(this).attr('href'));            

            $(this).addClass('Assigned');
        }
        else {
            deletePermission($(this).attr('href'));            
            $(this).removeClass('Assigned');
        }
        return false;
    });




    function savePermission(element) {

        $.ajax({
            cache: false,
            type: 'POST',
            url: "/UserPermission/AddPermission/",
            data: { caid: element },
            success: function (data) {

            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to create Permission' + thrownError);
            },
        });

        return;

    };



    function deletePermission(element) {

        $.ajax({
            cache: false,
            type: 'POST',
            url: "/UserPermission/RemovePermission/",
            data: { caid: element },
            success: function (data) {
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to remove Permission' + thrownError);
            },
        });

        return;

    };



    //Required Checker
    $(".required").each(function () {

        var text = $(this).val();
        $(this).toggleClass("has-error", (text.length > 0 && text != 0) ? false : true);
        $(this).parent().parent().children('label').toggleClass("has-error", (text.length > 0 && text != 0) ? false : true);
    });

    $(".required").change(function () {

        var text = $(this).val();
        $(this).toggleClass("has-error", (text.length > 0 && text != 0) ? false : true);
        $(this).parent().parent().children('label').toggleClass("has-error", (text.length > 0 && text != 0) ? false : true)
    });


    //$('form').find('input[type=text],select').filter(':visible:first').focus();


    $.fn.callModalOnLoad = function () {

        $.ajaxSetup({ cache: false });
        var url = $(this).attr('href');
        //alert(url);
        //alert(this.attr('href'));
        //alert($(this).attr('href'));
        $('#myModalContent').load(url, function () {

            $('#myModal').modal({
                backdrop: 'static',
                keyboard: true
            }, 'show');

            bindForm(this);
        });

    }

    //Modal MAster


    $(function () {

        $.ajaxSetup({ cache: false });

        $("a[data-modal]").on("click", function (e) {
            // hide dropdown if any
            $(e.target).closest('.btn-group').children('.dropdown-toggle').dropdown('toggle');
            //alert(' Script');
            $('#myModalContent').load(this.href, function () {
                $('#myModal').modal({
                    backdrop: 'static',
                    keyboard: true
                }, 'show');

                bindForm(this);
            });

            return false;
        });
    });



    $(function () {

        $.ajaxSetup({ cache: false });
        $("a[delete-modal]").on("click", function (e) {
            //alert('here');
            $.ajax({
                url: this.href,
                type: 'POST',

                success: function (result) {
                    if (result.success) {
                        $('#myModal').modal('hide');
                        //Refresh
                        location.reload();
                    } else {
                        $('#myModalContent').html(result);
                        //bindForm();
                    }
                }
            });
            return false;
        });
    });


    function bindForm(dialog) {
        //alert('binding Script');
        $('#modform', dialog).submit(function () {
            //alert('inside script');
            //alert(this.action);
            //alert(this.method);
            $.ajax({
                url: this.action,
                type: this.method,
                data: $(this).serialize(),
                success: function (result) {
                    if (result.success) {
                        $('#myModal').modal('hide');
                        //Refresh
                        // alert('this.action');
                        location.reload();
                    } else {
                        $('#myModalContent').html(result);
                        bindForm();
                    }
                }
            });
            return false;
        });



        $('a#AddToExisting ').click(function () {
            //alert('inside script');
            //alert(this.href);
            //alert(this.method);
            $.ajax({
                url: this.href,


                success: function (result) {
                    if (result.success) {
                        $('#myModal').modal('hide');
                        //Refresh
                        location.reload();
                    } else {
                        $('#myModalContent').html(result);
                        bindForm();
                    }
                }
            });
            return false;
        });
    }




    //on closing model




    (function ($) {
        DeleteValidation = function () {

            var $row = $('table.grid-table .grid-row.grid-row-selected');
            var tescon = $row.find('[data-name="IsSystemDefine"]').text();

            if (!$('table.grid-table .grid-row.grid-row-selected').get(0)) {

                $('#flash-messages').flashMessage({ message: 'Please select a record before deleting.', alert: 'Warning' });

                return false;
            }
            if (tescon == "True") {
                $('#flash-messages').flashMessage({ message: 'Cannot delete System defined record.', alert: 'Warning' });

                return false;
            }

            return true;

        };
    })(jQuery);

    (function ($) {
        DeleteValidationWithStatus = function () {

            var $row = $('table.grid-table .grid-row.grid-row-selected');
            var tescon = $row.find('[data-name="Status"]').text();

            if (!$('table.grid-table .grid-row.grid-row-selected').get(0)) {

                $('#flash-messages').flashMessage({ message: 'Please select a record before deleting.', alert: 'Warning' });

                return false;
            }
            if (tescon == 2) {
                $('#flash-messages').flashMessage({ message: 'Cannot delete this record.', alert: 'Warning' });

                return false;
            }

            return true;

        };
    })(jQuery);


})


function formatDate(format, date) {

    var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

    format = format.replace(/Y/g, (date.getFullYear()));
    format = format.replace(/m/g, (months[date.getMonth()]));
    format = format.replace(/d/g, date.getDate());
    var tem = format.replace(/\//g, '-');
    return tem;
}

function formatDateWithTime(format, date) {

    var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

    format = format.replace(/Y/g, (date.getFullYear()));
    format = format.replace(/m/g, (months[date.getMonth()]));
    format = format.replace(/d/g, date.getDate());
    format = format.replace(/Hr/g, date.getHours());
    format = format.replace(/Mn/g, date.getMinutes());
    format = format.replace(/Se/g, date.getSeconds());
    var tem = format.replace(/\//g, '-');
    return tem;
}



//For Adding Commas to numbers
//Call this function after selecting the wrapper set which have the numbers to be comma added


addCommas = function (input) {
    // If the regex doesn't match, `replace` returns the string unmodified
    return (input.toString()).replace(
      // Each parentheses group (or 'capture') in this regex becomes an argument 
      // to the function; in this case, every argument after 'match'
      /^([-+]?)(0?)(\d+)(.?)(\d+)$/g, function (match, sign, zeros, before, decimal, after) {

          // Less obtrusive than adding 'reverse' method on all strings
          var reverseString = function (string) { return string.split('').reverse().join(''); };

          // Insert commas every three characters from the right
          var insertCommas = function (string) {

              // Reverse, because it's easier to do things from the left
              var reversed = reverseString(string);

              // Add commas every three characters
              var reversedWithCommas = reversed.match(/.{1,3}/g).join(',');

              // Reverse again (back to normal)
              return reverseString(reversedWithCommas);
          };

          // If there was no decimal, the last capture grabs the final digit, so
          // we have to put it back together with the 'before' substring
          return sign + (decimal ? insertCommas(before) + decimal + after : insertCommas(before + after));
      }
    );
};

$.fn.addCommas = function () {
    $(this).each(function () {
        //alert($(this).text());
        //alert(addCommas($(this).text()));
        $(this).text(addCommas($(this).text()));
    });
};


function AddFields() {
    $('form:last').append($("<input type='hidden' name='UserRemark'></input>"))
}




function DisablePage() {

    //Disabling input fields
    $(':input').attr('disabled', 'disabled');

    //Removing all the events from the newly created lines
    $('#gbody').unbind();

    //Removing Add New Row ActionLink
    $('a').on("click", function (event) {

        event.stopImmediatePropagation();
        return false;

    });

}












function InitializePopover(element, ProdUid, IsPostStock, GodwnId,Type) {

    $(element).popover('destroy');

    var DataArray;
    var status;
    if(Type=="Issue")
        var url = "/ProductUid/GetProductUidValidation";
    else if (Type == "Receive")
        var url = "/ProductUid/GetProductUidReceiveValidation"
    $.ajax({
        async: false,
        url: url,
        data: { ProductUID: ProdUid, PostedInStock: IsPostStock, GodownId: GodwnId },
        success: function (data) {
            DataArray = data;
        }
    })

    if (DataArray.ErrorType == "InvalidID" || DataArray.ErrorType == "InvalidGodown") {
        $(element)
         .popover({
             trigger: 'manual',
             container: '.modal-body',
             'delay': { "hide": "1000" },
             html: true,
             content: "<ul class='list-group'>  <li class='list-group-item active'> Validation Detail </li>    <li class='list-group-item'>Message:" + DataArray.ErrorMessage + "</li>   </ul>"
         });
        ResetFields();
        status = false;
    }

    else if (DataArray.ErrorType == "GodownNull") {
        $(element)
          .popover({
              trigger: 'manual',
              container: '.modal-body',
              'delay': { "hide": "1000" },
              html: true,
              content: "<ul class='list-group'>  <li class='list-group-item active'> Validation Detail </li>    <li class='list-group-item'>" + DataArray.ErrorMessage + "  </li>    <li class='list-group-item'> DocType:" + (DataArray.GenDocTypeName == null ? "" : DataArray.GenDocTypeName) + " <br /> DocNo:" + (DataArray.GenDocNo == null ? "" : DataArray.GenDocNo) + " <br /> DocDate:" + (DataArray.GenDocDate == null ? "" : formatDate('d/m/Y', new Date(parseInt(DataArray.GenDocDate.substr(6))))) + " <br /> Process:" + (DataArray.CurrentProcessName == null ? "" : DataArray.CurrentProcessName) + " <br /> Person:" + (DataArray.LastTransactionPersonName == null ? "" : DataArray.LastTransactionPersonName) + " </li>   </ul>"
          });
        ResetFields();
        status = false;
    }
    else if (DataArray.ErrorType == "Success") {
        var $page = $(element).closest('.modal-body').get(0);
        $($page).find('#ProductId').select2("data", { id: DataArray.ProductId, text: DataArray.ProductName }).attr('readonly', 'true').trigger('change');
        $($page).find('#Qty').val(1).attr('readonly', 'true');
        $($page).find('#ProductUidId').val(DataArray.ProductUIDId);

        if (DataArray.Dimension1Id)
            $($page).find('#Dimension1Id').select2("data", { id: DataArray.Dimension1Id, text: DataArray.Dimension1Name }).attr('readonly', 'true');
        else
            $($page).find('#Dimension1Id').attr('readonly', 'true');

        if (DataArray.Dimension2Id)
            $($page).find('#Dimension2Id').select2("data", { id: DataArray.Dimension2Id, text: DataArray.Dimension2Name }).attr('readonly', 'true');
        else
            $($page).find('#Dimension2Id').attr('readonly', 'true');

        if (DataArray.CurrenctProcessId)
            $($page).find('#FromProcessId').select2("data", { id: DataArray.CurrenctProcessId, text: DataArray.CurrentProcessName }).attr('readonly', 'true');
        else
            $($page).find('#FromProcessId').attr('readonly', 'true');

        $($page).find('#LotNo').val(DataArray.LotNo);
        status = true;

    }





    function ResetFields() {

        var $page = $(element).closest('.modal-body').get(0);
        $($page).find('#ProductId').select2('val','').removeAttr('readonly');
        $($page).find('#Qty').val('').removeAttr('readonly');
        $($page).find('#Dimension1Id').select2('val', '').removeAttr('readonly');
        $($page).find('#Dimension2Id').select2('val', '').removeAttr('readonly');
        $($page).find('#FromProcessId').select2('val','').removeAttr('readonly');
        $($page).find('#LotNo').val('').removeAttr('readonly');
        $($page).find('#ProductUidId').val(0);


    }


    function Result() {
        var self = this;
        self.status = status;
        self.data = DataArray;

    }

    var temp = new Result();   

    return temp;


}


function InitializePermissionsPopover(element, MenId) {

    $(element).popover('destroy');

    var DataArray;
    var status;
    var url = "/AdminSetup/GetActionsForMenu";
    $.ajax({
        async: false,
        url: url,
        data: { MenuId: MenId },
        success: function (data) {
            DataArray = data;
        }
    })
    var row = "";
    row+="<ul class='list-group'>  <li class='list-group-item active'> Validation Detail </li>  ";
    $.each(DataArray,function(index,item){
        row += " <li class='list-group-item'> DocType:" + (item.ActionName == null ? "" : item.ActionName) + "<input type='hidden' class='CAID' value='" + item.ControllerActionId + "'></input> <br />  </li>"
    }) 
    row+=   "</ul>";
        $(element)
          .popover({
              animation:true,
              trigger: 'manual',
              container: 'body',
              'delay': { "hide": "1000" },
              html: true,
              content: row,
          });
        
        status = false;
    

    return status;


}
$(document).on("focus", "input,textarea", function () {
    $(this).select();
})

$(document).on("keypress", "input.number", function (e) {
    var KeyCode = (e.keyCode || e.which);
    // Allow: backspace, delete, tab, escape, enter and .
    if (
        $.inArray(KeyCode, [46, 40, 41, 43, 42, 47, 45]) !== -1 ||
        // Allow: Ctrl+A
        (KeyCode == 97 && e.ctrlKey === true) ||
        // Allow: Ctrl+C
        (KeyCode == 99 && e.ctrlKey === true) ||
        // Allow: Ctrl+X
        (KeyCode == 120 && e.ctrlKey === true) ||
        // Allow: Ctrl+Y
        (KeyCode == 118 && e.ctrlKey === true)
        // Allow: home, end, left, right
        //(KeyCode >= 35 && KeyCode <= 39)
        ) {
        // let it happen, don't do anything
        return;
    }
    // Ensure that it is a number and stop the keypress
    if ((e.shiftKey || (KeyCode < 48 || KeyCode > 57))) {
        e.preventDefault();
    }
});





var AlertTypeConstants = new Array();
AlertTypeConstants["Warning"] = "warning";
AlertTypeConstants["Success"] = "success";
AlertTypeConstants["Danger"] = "danger";
AlertTypeConstants["Info"] = "info";

var AlertIconconstants = new Array();
AlertIconconstants["success"] = "glyphicon glyphicon-ok";
AlertIconconstants["warning"] = "glyphicon glyphicon-warning-sign";
AlertIconconstants["info"] = "glyphicon glyphicon-info-sign";
AlertIconconstants["danger"] = "glyphicon glyphicon-remove";






//$.notify.defaults({ className: "success", position: "bottom right", clickToHide: true, autoHide: false, style: 'bootstrap' });


$.notifyDefaults({
    type: 'success',
    allow_dismiss: true,
    target: '_blank',
    placement: {
        from: "bottom",
        align: "right"
    },
    newest_on_top: true,
});




$.fn.CustomNotify = function (options) {
    var target = this;
    options = $.extend({ timeout: 5000, alert: 'info' }, options);

    if (!options.message) {
        setMessageFromCookie(options);
    }

    if (options.message) {

        //$.notify(options.message, options.alert);

    } else {
        return;
    }

    return this;

    // Get the first alert message read from the cookie
    function setMessageFromCookie() {
        $.each(new Array('Success', 'Danger', 'Warning', 'Info'), function (i, alert) {
            var cookie = $.Cuscookie("Flash." + alert);

            if (cookie != "null") {
                var Length = cookie.length;

                for (var i = 0; i < Length; i++) {

                    options.message = cookie[i].Text;

                    options.alert = AlertTypeConstants[alert];

                    CookieNotify(options);

                    deleteMessageCookie(cookie[i].Id);
                }

                //return;
            }
        });
    }

    // Delete the named flash cookie
    function deleteMessageCookie(alert) {
        $.cookie(alert, null, { path: '/', expires: -1 });
        $.removeCookie(alert);
    }
};


function CookieNotify(cookie) {
    if (cookie.message) {
        if (cookie.alert === AlertTypeConstants["Warning"] || cookie.alert === AlertTypeConstants["Danger"]) {
            $.notify({
                icon: AlertIconconstants[cookie.alert],
                message: cookie.message,
            }, {
                type: cookie.alert,
                delay: 0,
            });
        }
        else {
            $.notify({
                icon: AlertIconconstants[cookie.alert],
                message: cookie.message,
            }, {
                type: cookie.alert,
            });
        }
    }
}

function NavigateToLineRecord(id) {
    var Id = id;
    var NavOffset = $('.navbar-collapse.collapse').height();
    $('html, body').animate({
        scrollTop: $(Id).offset().top - ($(window).height() / 2),
    }, 0);
    $(Id).addClass('SelectedLine');
    setTimeout(function () {
        $(Id).removeClass('SelectedLine');
    }, 2000)
}