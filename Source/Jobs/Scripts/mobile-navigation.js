


$(function () {
    var click;
    $('table.grid-table td').click(function (e) {
        $('.popover').popover('destroy');

        var row = $(this).closest("tr");
        var url = row.find('a.RecEditurl:hidden').attr('href');
        var delurl = row.find('a.RecDelurl:hidden').attr('href');
        if (url && click !== this)
            $(this).popover({
                html: true,
                trigger: 'focus',
                container: 'table.grid-table',
                placement: 'top',
                content: "<a class='cbtn btn-circle glyphicon glyphicon-edit' onclick=\"$('.popover').popover('destroy');\" style='padding-top: 6px;padding-bottom: 6px;' href='" + url + "'></a>"
                //content: "<a href='/JobOrderHeader/Index/458?IndexType=All'>Link</a>"
            }).popover('show');
        click = this;
    });

    var lclick;

    $(document).on("click", "#gbody .grid-body .navbody .block,#gbodycharges .grid-body,.grid-content .grid-body", function (e) {
        $('.popover').popover('destroy');

        var row = $(this).closest("#gbody .grid-body");
        var url = row.find('a[edit]').attr('href');
        var delurl = row.find('a[delete]').attr('href');;
        if (url && lclick !== this)
            $(this).popover({
                html: true,
                trigger: 'focus',
                container: $(this).closest('div.block'),
                placement: 'top',
                content: "<a class='cbtn btn-circle glyphicon glyphicon-edit' data-modal='' onclick=\"$('.popover').popover('destroy');\" style='padding-top: 6px;padding-bottom: 6px;' href='" + url + "'></a>"
                //content: "<a href='/JobOrderHeader/Index/458?IndexType=All'>Link</a>"
            }).popover('show');
        lclick = this;
    });

})