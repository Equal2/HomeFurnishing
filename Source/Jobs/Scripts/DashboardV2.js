$(function () {

    'use strict';

    /* ChartJS
     * -------
     * Here we will create a few charts using ChartJS
     */



    //-----------------------
    //- MONTHLY SALES CHART -
    //-----------------------

    // Get context with jQuery - using jQuery's .get() method.
    var salesChartCanvas = $("#salesChart").get(0).getContext("2d");
    // This will get the first returned node in the jQuery collection.
    var salesChart = new Chart(salesChartCanvas);
    var ChartDataArray = null
    GetChartData();

    function GetChartData() {
        $.ajax({
            async: false,
            cache: false,
            type: "POST",
            url: '/DashBoardAuto/GetChartData',
            success: function (result) {
                ChartDataArray = result.Data;
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to retrieve product details.' + thrownError);
            }
        });
    }



    var labels = [], data_Incomes = [], data_Expenses = [];
    ChartDataArray.forEach(function (value) {
        labels.push(value.Month);
        data_Incomes.push(value.Income);
        data_Expenses.push(value.Expense);
    });




    var salesChartData = {
        labels: labels,
        datasets: [
          {
              label: "Incomes",
              fillColor: "rgb(210, 214, 222)",
              strokeColor: "rgb(210, 214, 222)",
              pointColor: "rgb(210, 214, 222)",
              pointStrokeColor: "#c1c7d1",
              pointHighlightFill: "#fff",
              pointHighlightStroke: "rgb(220,220,220)",
              data: data_Expenses
          },
          {
              label: "Expenses",
              fillColor: "rgba(60,141,188,0.9)",
              strokeColor: "rgba(60,141,188,0.8)",
              pointColor: "#3b8bba",
              pointStrokeColor: "rgba(60,141,188,1)",
              pointHighlightFill: "#fff",
              pointHighlightStroke: "rgba(60,141,188,1)",
              data: data_Incomes
          }
        ]
    };



    //var salesChartData = {
    //    labels: ["January", "February", "March", "April", "May", "June", "July"],
    //    datasets: [
    //      {
    //          //label: "Electronics",
    //          //fillColor: "rgb(210, 214, 222)",
    //          //strokeColor: "rgb(210, 214, 222)",
    //          //pointColor: "rgb(210, 214, 222)",
    //          //pointStrokeColor: "#c1c7d1",
    //          //pointHighlightFill: "#fff",
    //          //pointHighlightStroke: "rgb(220,220,220)",
    //          data: [65, 59, 80, 81, 56, 55, 40]
    //      },
    //      //{
    //      //    //label: "Digital Goods",
    //      //    //fillColor: "rgba(60,141,188,0.9)",
    //      //    //strokeColor: "rgba(60,141,188,0.8)",
    //      //    //pointColor: "#3b8bba",
    //      //    //pointStrokeColor: "rgba(60,141,188,1)",
    //      //    //pointHighlightFill: "#fff",
    //      //    //pointHighlightStroke: "rgba(60,141,188,1)",
    //      //    data: [28, 48, 40, 19, 86, 27, 90]
    //      //}
    //    ]
    //};

    var salesChartOptions = {
        //Boolean - If we should show the scale at all
        showScale: true,
        //Boolean - Whether grid lines are shown across the chart
        scaleShowGridLines: false,
        //String - Colour of the grid lines
        scaleGridLineColor: "rgba(0,0,0,.05)",
        //Number - Width of the grid lines
        scaleGridLineWidth: 1,
        //Boolean - Whether to show horizontal lines (except X axis)
        scaleShowHorizontalLines: true,
        //Boolean - Whether to show vertical lines (except Y axis)
        scaleShowVerticalLines: true,
        //Boolean - Whether the line is curved between points
        bezierCurve: true,
        //Number - Tension of the bezier curve between points
        bezierCurveTension: 0.3,
        //Boolean - Whether to show a dot for each point
        pointDot: false,
        //Number - Radius of each point dot in pixels
        pointDotRadius: 4,
        //Number - Pixel width of point dot stroke
        pointDotStrokeWidth: 1,
        //Number - amount extra to add to the radius to cater for hit detection outside the drawn point
        pointHitDetectionRadius: 20,
        //Boolean - Whether to show a stroke for datasets
        datasetStroke: true,
        //Number - Pixel width of dataset stroke
        datasetStrokeWidth: 2,
        //Boolean - Whether to fill the dataset with a color
        datasetFill: true,
        //String - A legend template
        legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].lineColor%>\"></span><%=datasets[i].label%></li><%}%></ul>",
        //Boolean - whether to maintain the starting aspect ratio or not when responsive, if set to false, will take up entire container
        maintainAspectRatio: true,
        //Boolean - whether to make the chart responsive to window resizing
        responsive: true
    };

    //Create the line chart
    salesChart.Line(salesChartData, salesChartOptions);

    //---------------------------
    //- END MONTHLY SALES CHART -
    //---------------------------

    //-------------
    //- PIE CHART -
    //-------------
    // Get context with jQuery - using jQuery's .get() method.
    var pieChartCanvas = $("#pieChart").get(0).getContext("2d");
    var pieChart = new Chart(pieChartCanvas);
    var PieChartDataArray = null
    GetPieChartData();

    function GetPieChartData() {
        $.ajax({
            async: false,
            cache: false,
            type: "POST",
            url: '/DashBoardAuto/GetPieChartData',
            success: function (result) {
                PieChartDataArray = result.Data;
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to retrieve product details.' + thrownError);
            }
        });
    }

    //var PieData = JSON.stringify(PieChartDataArray)

    //var PieData = [
    //  {
    //      value: 700,
    //      color: "#f56954",
    //      highlight: "#f56954",
    //      label: "Chrome"
    //  },
    //  {
    //      value: 500,
    //      color: "#00a65a",
    //      highlight: "#00a65a",
    //      label: "IE"
    //  },
    //  {
    //      value: 400,
    //      color: "#f39c12",
    //      highlight: "#f39c12",
    //      label: "FireFox"
    //  },
    //  {
    //      value: 600,
    //      color: "#00c0ef",
    //      highlight: "#00c0ef",
    //      label: "Safari"
    //  },
    //  {
    //      value: 300,
    //      color: "#3c8dbc",
    //      highlight: "#3c8dbc",
    //      label: "Opera"
    //  },
    //  {
    //      value: 100,
    //      color: "#d2d6de",
    //      highlight: "#d2d6de",
    //      label: "Navigator"
    //  }
    //];


    var PieChartHint = '<ul class="chart-legend clearfix">'
    PieChartDataArray.forEach(function (value) {
        PieChartHint = PieChartHint + '<li><i class="fa fa-circle-o" style="color:' + value.color + '"></i> ' + value.label + '</li>'
    });
    PieChartHint = PieChartHint + '</ul>'

    $('#PieChartHint').html(PieChartHint)
    //var PieChartHint = '<ul class="chart-legend clearfix">' +
    //                        '<li><i class="fa fa-circle-o text-red"></i> Chrome</li>' +
    //                        '<li><i class="fa fa-circle-o text-green"></i> IE</li>' +
    //                        '<li><i class="fa fa-circle-o text-yellow"></i> FireFox</li>' +
    //                        '<li><i class="fa fa-circle-o text-aqua"></i> Safari</li>' +
    //                        '<li><i class="fa fa-circle-o text-light-blue"></i> Opera</li>'  +
    //                        '<li><i class="fa fa-circle-o text-gray"></i> Navigator</li>'  +
    //                    '</ul>'



    var pieOptions = {
        //Boolean - Whether we should show a stroke on each segment
        segmentShowStroke: true,
        //String - The colour of each segment stroke
        segmentStrokeColor: "#fff",
        //Number - The width of each segment stroke
        segmentStrokeWidth: 1,
        //Number - The percentage of the chart that we cut out of the middle
        percentageInnerCutout: 50, // This is 0 for Pie charts
        //Number - Amount of animation steps
        animationSteps: 100,
        //String - Animation easing effect
        animationEasing: "easeOutBounce",
        //Boolean - Whether we animate the rotation of the Doughnut
        animateRotate: true,
        //Boolean - Whether we animate scaling the Doughnut from the centre
        animateScale: false,
        //Boolean - whether to make the chart responsive to window resizing
        responsive: true,
        // Boolean - whether to maintain the starting aspect ratio or not when responsive, if set to false, will take up entire container
        maintainAspectRatio: false,
        //String - A legend template
        legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<segments.length; i++){%><li><span style=\"background-color:<%=segments[i].fillColor%>\"></span><%if(segments[i].label){%><%=segments[i].label%><%}%></li><%}%></ul>",
        //String - A tooltip template
        tooltipTemplate: "<%=value %> <%=label%>"
    };
    //Create pie or douhnut chart
    // You can switch between pie and douhnut using the method below.
    pieChart.Doughnut(PieChartDataArray, pieOptions);
    //-----------------
    //- END PIE CHART -
    //-----------------

    /* jVector Maps
     * ------------
     * Create a world map with markers
     */
    $('#world-map-markers').vectorMap({
        map: 'world_mill_en',
        normalizeFunction: 'polynomial',
        hoverOpacity: 0.7,
        hoverColor: false,
        backgroundColor: 'transparent',
        regionStyle: {
            initial: {
                fill: 'rgba(210, 214, 222, 1)',
                "fill-opacity": 1,
                stroke: 'none',
                "stroke-width": 0,
                "stroke-opacity": 1
            },
            hover: {
                "fill-opacity": 0.7,
                cursor: 'pointer'
            },
            selected: {
                fill: 'yellow'
            },
            selectedHover: {}
        },
        markerStyle: {
            initial: {
                fill: '#00a65a',
                stroke: '#111'
            }
        },
        markers: [
          { latLng: [41.90, 12.45], name: 'Vatican City' },
          { latLng: [43.73, 7.41], name: 'Monaco' },
          { latLng: [-0.52, 166.93], name: 'Nauru' },
          { latLng: [-8.51, 179.21], name: 'Tuvalu' },
          { latLng: [43.93, 12.46], name: 'San Marino' },
          { latLng: [47.14, 9.52], name: 'Liechtenstein' },
          { latLng: [7.11, 171.06], name: 'Marshall Islands' },
          { latLng: [17.3, -62.73], name: 'Saint Kitts and Nevis' },
          { latLng: [3.2, 73.22], name: 'Maldives' },
          { latLng: [35.88, 14.5], name: 'Malta' },
          { latLng: [12.05, -61.75], name: 'Grenada' },
          { latLng: [13.16, -61.23], name: 'Saint Vincent and the Grenadines' },
          { latLng: [13.16, -59.55], name: 'Barbados' },
          { latLng: [17.11, -61.85], name: 'Antigua and Barbuda' },
          { latLng: [-4.61, 55.45], name: 'Seychelles' },
          { latLng: [7.35, 134.46], name: 'Palau' },
          { latLng: [42.5, 1.51], name: 'Andorra' },
          { latLng: [14.01, -60.98], name: 'Saint Lucia' },
          { latLng: [6.91, 158.18], name: 'Federated States of Micronesia' },
          { latLng: [1.3, 103.8], name: 'Singapore' },
          { latLng: [1.46, 173.03], name: 'Kiribati' },
          { latLng: [-21.13, -175.2], name: 'Tonga' },
          { latLng: [15.3, -61.38], name: 'Dominica' },
          { latLng: [-20.2, 57.5], name: 'Mauritius' },
          { latLng: [26.02, 50.55], name: 'Bahrain' },
          { latLng: [0.33, 6.73], name: 'São Tomé and Príncipe' }
        ]
    });


    ///*
    // * BAR CHART
    // * ---------
    // */

    //var bar_data = {
    //    data: [["January", 10], ["February", 8], ["March", 4], ["April", 13], ["May", 17], ["June", 9]],
    //    color: "#3c8dbc"
    //};

    ////var bar_data = {
    ////    data: [["1", 10], ["2", 8], ["3", 4], ["4", 13], ["5", 17], ["6", 9]],
    ////    color: "#3c8dbc"
    ////};


    //var VehicleSaleChartDataArray = null
    //GetVehicleSaleChartData();

    //function GetVehicleSaleChartData() {
    //    $.ajax({
    //        async: false,
    //        cache: false,
    //        type: "POST",
    //        url: '/DashBoardAuto/GetVehicleSaleChartData',
    //        success: function (result) {
    //            VehicleSaleChartDataArray = result.Data;
    //        },
    //        error: function (xhr, ajaxOptions, thrownError) {
    //            alert('Failed to retrieve product details.' + thrownError);
    //        }
    //    });
    //}

    
    //var ParentData = []

    //VehicleSaleChartDataArray.forEach(function (value) {
    //    var arr = [];
    //    for (var prop in value) {
    //        arr.push(value[prop]);
    //    }
    //    ParentData.push(arr);
    //});


    //var barChartData = {
    //    data: ParentData,
    //    color: "#3c8dbc"
    //};

    //$.plot("#bar-chart", [barChartData], {
    //    grid: {
    //        borderWidth: 1,
    //        borderColor: "#f3f3f3",
    //        tickColor: "#f3f3f3"
    //    },
    //    series: {
    //        bars: {
    //            show: true,
    //            barWidth: 0.5,
    //            align: "center"
    //        },
    //    },
    //    xaxis: {
    //        mode: "categories",
    //        tickLength: 0
    //    }
    //});
    ///* END BAR CHART */



    //-------------
    //- BAR CHART -
    //-------------
    var barChartCanvas = $("#barChart").get(0).getContext("2d");
    var barChart = new Chart(barChartCanvas);
    

    var VehicleSaleChartDataArray = null
    GetVehicleSaleChartData();

    function GetVehicleSaleChartData() {
        $.ajax({
            async: false,
            cache: false,
            type: "POST",
            url: '/DashBoardAuto/GetVehicleSaleChartData',
            success: function (result) {
                VehicleSaleChartDataArray = result.Data;
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to retrieve product details.' + thrownError);
            }
        });
    }

    var labels_SalesChart = [], data_SaleChart = []
    VehicleSaleChartDataArray.forEach(function (value) {
        labels_SalesChart.push(value.Month);
        data_SaleChart.push(value.Amount);
    });


    var VehicleSalesChartData = {
        labels: labels_SalesChart,
        datasets: [
          {
              label: "Amount",
              fillColor: "rgb(210, 214, 222)",
              strokeColor: "rgb(210, 214, 222)",
              pointColor: "rgb(210, 214, 222)",
              pointStrokeColor: "#c1c7d1",
              pointHighlightFill: "#fff",
              pointHighlightStroke: "rgb(220,220,220)",
              data: data_SaleChart
          }
        ]
    };

    var barChartData = VehicleSalesChartData;
    barChartData.datasets[0].fillColor = "#00a65a";
    barChartData.datasets[0].strokeColor = "#00a65a";
    barChartData.datasets[0].pointColor = "#00a65a";
    var barChartOptions = {
        //Boolean - Whether the scale should start at zero, or an order of magnitude down from the lowest value
        scaleBeginAtZero: true,
        //Boolean - Whether grid lines are shown across the chart
        scaleShowGridLines: true,
        //String - Colour of the grid lines
        scaleGridLineColor: "rgba(0,0,0,.05)",
        //Number - Width of the grid lines
        scaleGridLineWidth: 1,
        //Boolean - Whether to show horizontal lines (except X axis)
        scaleShowHorizontalLines: true,
        //Boolean - Whether to show vertical lines (except Y axis)
        scaleShowVerticalLines: true,
        //Boolean - If there is a stroke on each bar
        barShowStroke: true,
        //Number - Pixel width of the bar stroke
        barStrokeWidth: 1,
        //Number - Spacing between each of the X value sets
        barValueSpacing: 5,
        //Number - Spacing between data sets within X values
        barDatasetSpacing: 1,
        //String - A legend template
        legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].fillColor%>\"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>",
        //Boolean - whether to make the chart responsive
        responsive: true,
        maintainAspectRatio: true
    };

    barChartOptions.datasetFill = false;
    barChart.Bar(VehicleSalesChartData, barChartOptions);

    /* SPARKLINE CHARTS
     * ----------------
     * Create a inline charts with spark line
     */

    //-----------------
    //- SPARKLINE BAR -
    //-----------------
    $('.sparkbar').each(function () {
        var $this = $(this);
        $this.sparkline('html', {
            type: 'bar',
            height: $this.data('height') ? $this.data('height') : '30',
            barColor: $this.data('color')
        });
    });

    //-----------------
    //- SPARKLINE PIE -
    //-----------------
    $('.sparkpie').each(function () {
        var $this = $(this);
        $this.sparkline('html', {
            type: 'pie',
            height: $this.data('height') ? $this.data('height') : '90',
            sliceColors: $this.data('color')
        });
    });

    //------------------
    //- SPARKLINE LINE -
    //------------------
    $('.sparkline').each(function () {
        var $this = $(this);
        $this.sparkline('html', {
            type: 'line',
            height: $this.data('height') ? $this.data('height') : '90',
            width: '100%',
            lineColor: $this.data('linecolor'),
            fillColor: $this.data('fillcolor'),
            spotColor: $this.data('spotcolor')
        });
    });
});



function GetVehicleSale()
{
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        url: '/DashBoardAuto/GetVehicleSale',
        success: function (result) {
            $('#VehicleSaleAmount').text(result.Data[0].SaleAmount);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrieve product details.' + thrownError);
        }
    });
}

function GetVehicleProfit() {
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        url: '/DashBoardAuto/GetVehicleProfit',
        success: function (result) {
            $('#VehicleProfitAmount').text(result.Data[0].ProfitAmount);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrieve product details.' + thrownError);
        }
    });
}

function GetVehicleOutstanding() {
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        url: '/DashBoardAuto/GetVehicleOutstanding',
        success: function (result) {
            $('#VehicleOutstandingAmount').text(result.Data[0].OutstandingAmount);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrieve product details.' + thrownError);
        }
    });
}

function GetVehicleStock() {
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        url: '/DashBoardAuto/GetVehicleStock',
        success: function (result) {
            $('#VehicleStockAmount').text(result.Data[0].StockAmount);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrieve product details.' + thrownError);
        }
    });
}




function GetSpareSale() {
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        url: '/DashBoardAuto/GetSpareSale',
        success: function (result) {
            $('#SpareSaleAmount').text(result.Data[0].SaleAmount);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrieve product details.' + thrownError);
        }
    });
}

function GetSpareProfit() {
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        url: '/DashBoardAuto/GetSpareProfit',
        success: function (result) {
            $('#SpareProfitAmount').text(result.Data[0].ProfitAmount);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrieve product details.' + thrownError);
        }
    });
}

function GetSpareOutstanding() {
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        url: '/DashBoardAuto/GetSpareOutstanding',
        success: function (result) {
            $('#SpareOutstandingAmount').text(result.Data[0].OutstandingAmount);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrieve product details.' + thrownError);
        }
    });
}

function GetSpareStock() {
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        url: '/DashBoardAuto/GetSpareStock',
        success: function (result) {
            $('#SpareStockAmount').text(result.Data[0].StockAmount);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrieve product details.' + thrownError);
        }
    });
}


function GetTotalSale() {
    if ($('#VehicleSaleAmount').text() != '' && $('#VehicleSaleAmount').text() != null && $('#SpareSaleAmount').text() != '' && $('#SpareSaleAmount').text() != null)
    {
        $('#TotalSaleAmount').text(parseFloat($('#VehicleSaleAmount').text()) + parseFloat($('#SpareSaleAmount').text()));
        $('#TotalSaleAmount').text($('#TotalSaleAmount').text() + ' Crore')
    }
}

function GetTotalProfit() {
    if ($('#VehicleProfitAmount').text() != '' && $('#VehicleProfitAmount').text() != null && $('#SpareProfitAmount').text() != '' && $('#SpareProfitAmount').text() != null) {
        $('#TotalProfitAmount').text(parseFloat($('#VehicleProfitAmount').text()) + parseFloat($('#SpareProfitAmount').text()));
        $('#TotalProfitAmount').text($('#TotalProfitAmount').text() + ' Crore')
    }
}

function GetTotalOutstanding() {
    if ($('#VehicleOutstandingAmount').text() != '' && $('#VehicleOutstandingAmount').text() != null && $('#SpareOutstandingAmount').text() != '' && $('#SpareOutstandingAmount').text() != null) {
        $('#TotalOutstandingAmount').text(parseFloat($('#VehicleOutstandingAmount').text()) + parseFloat($('#SpareOutstandingAmount').text()));
        $('#TotalOutstandingAmount').text($('#TotalOutstandingAmount').text() + ' Crore')
    }
}

function GetTotalStock() {
    if ($('#VehicleStockAmount').text() != '' && $('#VehicleStockAmount').text() != null && $('#SpareStockAmount').text() != '' && $('#SpareStockAmount').text() != null) {
        $('#TotalStockAmount').text(parseFloat($('#VehicleStockAmount').text()) + parseFloat($('#SpareStockAmount').text()));
        $('#TotalStockAmount').text($('#TotalStockAmount').text() + ' Crore')
    }
}

$(document).ready(function () {
    GetVehicleSale();
    GetVehicleProfit();
    GetVehicleOutstanding();
    GetVehicleStock();

    GetSpareSale();
    GetSpareProfit();
    GetSpareOutstanding();
    GetSpareStock();

    GetTotalSale();
    GetTotalProfit();
    GetTotalOutstanding();
    GetTotalStock();
});

