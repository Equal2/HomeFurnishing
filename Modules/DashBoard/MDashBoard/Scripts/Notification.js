









//$(function () {

//    var ticker = $.connection.Notify; // the generated client-side hub proxy                

//    function init() {

//        ticker.server.pendingMessages().done(function (data) {
//            AddUpdate(data);
//            //$.each(stocks, function () {
//            //    var stock = formatStock(this);
//            //    $stockTableBody.append(rowTemplate.supplant(stock));
//            //});
//        });

//        //$.ajax({
//        //    cache: false,
//        //    type: 'POST',
//        //    url: "/UserBookMark/AddBookMark/",
//        //    data: { caid: element },
//        //    success: function (data) {

//        //    },
//        //    error: function (xhr, ajaxOptions, thrownError) {
//        //        alert('Failed to add Bookmark' + thrownError);
//        //    },
//        //});


//    }



//    $('#UNotificationIcon').click(function () {
//        if ($('#UNotimessage').hasClass('active')) {
//            $('#UNotimessage').text('').removeClass('active');
//            ticker.server.setNotificationSeen();
//        }
//    })







//    // Add a client-side hub method that the server will call
//    ticker.client.sendUpdate = function (data) {
//        $('#UNotimessage').text(data);
//        //var displayStock = formatStock(stock),
//        //    $row = $(rowTemplate.supplant(displayStock));

//        //$stockTableBody.find('tr[data-symbol=' + stock.Symbol + ']')
//        //    .replaceWith($row);

//    }
//    ticker.client.narrationUpdate = function (data) {


//        AddUpdate(data);

//        //var displayStock = formatStock(stock),
//        //    $row = $(rowTemplate.supplant(displayStock));

//        //$stockTableBody.find('tr[data-symbol=' + stock.Symbol + ']')
//        //    .replaceWith($row);

//    }

//    // Start the connection
//    $.connection.hub.start().done(init);

//});

//function AddUpdate(data) {
//    if (data.length) {
//        var count = 0;

//        $('#NotificMen').html('')
//        for (var i = 0; i < data.length; i++) {
//            $('#NotificMen').append(GetNotificationRow(data[i]));

//            if (!data[i].SeenDate) {
//                count++
//            }
//        }
//        if (count) {
//            $('#UNotimessage').text(count).addClass('active');
//            $('li.Notification:first').text('You have ' + count + ' new Notifications');
//        }
//    }
//}

//function GetNotificationRow(obj) {
//    var RowH = " <li><a href='/Notification/NotificationRequest/" + obj.NotificationId + "'><div class='pull-left'> <span class='Icon " + obj.IconName + "'></span></div><h4 >" + obj.NotificationSubjectName + "<small><i class='glyphicon glyphicon-time'></i>" + TimeAgo(obj.CreatedDate) + "</small></h4><p>" + obj.NotificationText + "</p></a></li>";
//    return RowH;
//}


//function TimeAgo(date) {
//    var templates = {
//        prefix: "",
//        suffix: " ago",
//        seconds: "< a min",
//        minute: "a min",
//        minutes: "%d mins",
//        hour: "an hr",
//        hours: "%d hrs",
//        day: "a day",
//        days: "%d days",
//        month: "a month",
//        months: "%d months",
//        year: "a year",
//        years: "%d years"
//    };
//    var template = function (t, n) {
//        return templates[t] && templates[t].replace(/%d/i, Math.abs(Math.round(n)));
//    };



//    if (!date)
//        return;
//    date = date.replace(/\.\d+/, ""); // remove milliseconds
//    date = date.replace(/-/, "/").replace(/-/, "/");
//    date = date.replace(/T/, " ").replace(/Z/, " UTC");
//    date = date.replace(/([\+\-]\d\d)\:?(\d\d)/, " $1$2"); // -04:00 -> -0400
//    date = new Date(date * 1000 || date);




//    var now = new Date();

//    var seconds = ((now.getTime() - date) * .001) >> 0;
//    var minutes = seconds / 60;
//    var hours = minutes / 60;
//    var days = hours / 24;
//    var years = days / 365;

//    return templates.prefix + (
//            seconds < 45 && template('seconds', seconds) ||
//            seconds < 90 && template('minute', 1) ||
//            minutes < 45 && template('minutes', minutes) ||
//            minutes < 90 && template('hour', 1) ||
//            hours < 24 && template('hours', hours) ||
//            hours < 42 && template('day', 1) ||
//            days < 30 && template('days', days) ||
//            days < 45 && template('month', 1) ||
//            days < 365 && template('months', days / 30) ||
//            years < 1.5 && template('year', 1) ||
//            template('years', years)
//            ) + templates.suffix;


//};