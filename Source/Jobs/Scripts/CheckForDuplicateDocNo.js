

//TODO:::
//To use it in a view add this function to a event and go to the duplicate document check service and add a switch case for the corresponding table name:

(function ($) {

    // jQuery plugin definition
    $.fn.DuplicateCheckForCreate = function (options) {

        var settings = $.extend({}, options);
        //alert(settings.name + " , " + settings.value);

        var tab = settings.name;
        var doc = settings.value;
        var doctype = settings.doctype;
        var stat;
        this.each(function () {

            var $t = $(this);
            $.ajax({
                async:false,
                cache: false,
                type: "POST",
                url: '/DuplicateDocumentCheck/DuplicateCheckForCreate',
                dataType: 'json',
                data: { table: tab, docno: doc,doctypeId:doctype },
                success: function (data) {
                    stat = data.returnvalue;
                    if (data.returnvalue) {
                        if (!$t.hasClass('Alert')) {                           
                            $t.addClass("Alert");
                            $t.parent().append($("<span class='text-danger' id='error'></span>").text('Already Taken'));                            
                        }
                    }
                    else {
                        if ($t.hasClass('Alert')) {
                            $t.removeClass("Alert");
                            //$t.addClass("Success");
                            //$t.nextAll('span:first').text(" ");
                            $(document).find('span#error')[0].remove();
                            //setTimeout(function () { $t.removeClass("Success") }, 800);                            
                        }
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    alert('Failed to Check Validation.' + thrownError);
                }
            });

        });
        return stat;
    };
})(jQuery);




//(function ($) {

//    // jQuery plugin definition
//    $.fn.DuplicateCheckForCreate = function (options) {

//        var settings = $.extend({}, options);
//        //alert(settings.name + " , " + settings.value);

//        var tab = settings.name;
//        var doc = settings.value;
//        var doctype = settings.doctype;
//        var stat;

//            var $t = $(this);
//            $.ajax({
//                async: false,
//                cache: false,
//                type: "POST",
//                url: '/DuplicateDocumentCheck/DuplicateCheckForCreate',
//                dataType: 'json',
//                data: { table: tab, docno: doc, doctypeId: doctype },
//                success: function (data) {
//                    stat = data.returnvalue;
//                },
//                error: function (xhr, ajaxOptions, thrownError) {
//                    alert('Failed to Check Validation.' + thrownError);
//                }
//            });
//            return stat;
//    };
//})(jQuery);







(function ($) {
   //Should pass another parameter with the name headerId to exclude the current document while checking for duplicates:
    $.fn.DuplicateCheckForEdit = function (options) {

        var settings = $.extend({}, options);
        //alert(settings.name + " , " + settings.value);

        var tab = settings.name;
        var doc = settings.value;
        var doctype = settings.doctype;
        var headerId = settings.headerId;
        var stat;
        this.each(function () {

            var $t = $(this);
            $.ajax({
                async:false,
                cache: false,
                type: "POST",
                url: '/DuplicateDocumentCheck/DuplicateCheckForEdit',
                dataType: 'json',
                data: { table: tab, docno: doc, doctypeId: doctype, headerid: headerId },
                success: function (data) {
                    stat = data.returnvalue;
                    if (data.returnvalue) {
                        if (!$t.hasClass('Alert')) {
                            $t.addClass("Alert");
                            $t.parent().append($("<span class='text-danger' id='error'></span>").text(' Already Taken'));
                            
                            //$t.parent().parent().nextAll("span:first").text("Already Taken");
                        }

                    }
                    else {
                        if ($t.hasClass('Alert')) {
                            $t.removeClass("Alert");
                            // $t.addClass("Success");
                            //$t.nextAll('span:first').text(" ");
                            $(document).find('span#error')[0].remove();
                            //setTimeout(function () { $t.removeClass("Success") }, 800);
                        }

                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    alert('Failed to Check Validation.' + thrownError);
                }
            })


        });

        return stat;
    };

    })(jQuery);



(function ($) {

    // jQuery plugin definition
    $.fn.CheckMandatory = function () {

        // merge default and user parameters
        // params = $.extend({ minlength: 0, maxlength: 99999 }, params);

        // traverse all nodes
        this.each(function () {

            // express a single node as a jQuery object
            var $t = $(this);

            // find text
            var origText = $t.text();

            // text length within defined limits?
            if (origText.length >= 0) {

                $t.removeClass('has-error');

            }
            else
                $t.addClass('has-error');

        });

        // allow jQuery chaining
        return this;
    };

})(jQuery);