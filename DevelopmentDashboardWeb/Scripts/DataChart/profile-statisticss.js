var from, to;
var machineNames = "", userNames = "";
var usersSelected = [], machinesSelected = [];
var aggregationMode = "Daily", aggregationInterval = "minute", aggregationOperation = "Average";
var chart;
var userNamesList, machineNameList;
var additionalParameters;

var series = [
                { valueField: 'ConnectedSessionsCount', name: 'Connected Sessions' },
                { valueField: 'ActiveSessionsCount', name: 'Active Sessions' }
            ];

var chartOptions = {
    dataSource: [{}],
    commonSeriesSettings: {
        argumentField: 'Moment'
    },
    argumentAxis: {
        label: { format: '' },
        argumentType: 'datetime',
        tickInterval: { minutes: 0, hours: 0 }
    },
    series: series,
    scrollBar: {
        visible: true
    },
    scrollingMode: "all",
    zoomingMode: "all",
    legend: {
        visible: true
    },
    useAggregation: false,
    valueAxis: {
        label: {
            format: 'fixedPoint',
            precision: 1
        }
    },
    crosshair: {
        enabled: true,
        color: "#949494",
        width: 3,
        dashStyle: "dot",
        label: {
            visible: true,
            backgroundColor: "#949494",
            font: {
                color: "#fff",
                size: 12
            }
        }
    },
    tooltip: {
        enabled: true,
        customizeTooltip: function (pointInfo) {
            return { html: '<div style="text-align:left">' + GetMomentSessionsProfiles(moment(pointInfo.argument), pointInfo.seriesName) + '</div>' }
        }
    },
    title: {
        text: "Session profile statistics"
    },
    onLegendClick: function (e) {
        var series = e.component.option('series');
        for (var i = 0; i < series.length; i++) {
            var s = series[i];
            if (s.name == e.target.name) {
                s.visible = !e.target.isVisible();
                break;
            }
        }
        e.component.option('series', series);
    }
};

var GetMomentSessionsProfiles = function (chunkInterval, seriesType) {
    var offset = new Date().getTimezoneOffset() / 60;
    if (aggregationMode == 'Daily')
        chunkInterval = chunkInterval.format("DD.MM.YYYY HH:mm:ss");
    else
        chunkInterval = chunkInterval._i;
    var tooltipContainer = "";
    $.ajax({
        type: 'POST',
        url: $("#profile-stat-chart").attr('data-get-moment-sessions'),
        data:
                {
                    dateFrom: moment(from).format("DD.MM.YYYY"),
                    dateTo: moment(to).format("DD.MM.YYYY"),
                    userNames: userNames,
                    machineNames: machineNames,
                    aggregationMode: aggregationMode,
                    aggregationOperation: aggregationOperation,
                    aggregationInterval: aggregationInterval,
                    chunkInterval: chunkInterval,
                    returnActive: seriesType == "Active Sessions" ? true : false,
                    clientOffset: offset
                },
        async: false,
        success: function (data) {
            for (var i = 0; i < data.length; i++) {
                tooltipContainer += '<div style="text-align:left">' + data[i] + "</div>";
            }
        }
    });
    return tooltipContainer;
}

var select_group_items = function (e) {
    var groups = userNamesList.option("items");
    for (var i = 0; i < groups.length; i++) {
        if (groups[i].key == e.component.option("text")) {
            for (var j = 0; j < groups[i].items.length; j++) {
                if (e.value)
                    userNamesList.selectItem({ group: i, item: j });
                else
                    userNamesList.unselectItem({ group: i, item: j });
            }
            break;
        }
    }
}

function GetAdditionalParameters() {
    $.ajax({
        type: 'GET',
        url: $("#chartSettings").attr('data-get-parameters'),
        async: false,
        success: function (data) {
            additionalParameters = data;
        }
    });
}

function InitControls() {
    $('#create-chart').dxButton({ text: "Create chart",
        type: "normal",
        width: 400,
        onClick: LoadChart
    });

    $('#aggregation-mode').dxSelectBox({
        dataSource: ["Daily", "Weekly"],
        value: "Daily",
        onValueChanged: function (data) {
            aggregationMode = data.value;
        }
    })

    $('#aggregation-operation').dxSelectBox({
        dataSource: ["Average", "Sum"],
        value: "Average",
        onValueChanged: function (data) {
            aggregationOperation = data.value;
        }
    })

    $('#aggregation-interval').dxSelectBox({
        dataSource: ["minute", "15 min", "hour"],
        value: "minute",
        onValueChanged: function (data) {
            aggregationInterval = data.value;
        }
    });

    $('#from').dxDateBox({
        format: "datetime",
        formatString: "dd.MM.yyyy HH:mm:ss",
        value: new Date(additionalParameters.PeriodBegin),
        onValueChanged: function (data) {
            from = new Date(data.value);
        }
    });
    $('#to').dxDateBox({
        format: "datetime",
        formatString: "dd.MM.yyyy HH:mm:ss",
        value: new Date(additionalParameters.PeriodEnd),
        onValueChanged: function (data) {
            to = new Date(data.value);
        }
    });
    userNamesList = $('#user-names').dxList({
        dataSource: additionalParameters.UserNamesByGroup,
        height: 300,
        showSelectionControls: true,
        selectionMode: "multi",
        grouped: true,
        collapsibleGroups: false,
        onSelectionChanged: function (data) {
            usersSelected = $.map(usersSelected, function (item) {
                if (data.removedItems.length > 0) {
                    if (data.removedItems[0].items[0].indexOf(item) == -1)
                        return item;
                }
                else
                    return item;

            });
            if (data.addedItems.length > 0)
                usersSelected = usersSelected.concat(data.addedItems[0].items[0]);
            userNames = usersSelected.join(",");
        },
        showScrollbar: 'always',
        groupTemplate: function (data) {
            return $("<div style='font-weight:bold'/>").dxCheckBox({
                text: data.key,
                value: true,
                onValueChanged: function (e) {
                    select_group_items(e);
                }
            });

        }
    }).dxList("instance");

    for (var i = 0; i < userNamesList.option("items").length; i++) {
        var group = userNamesList.option("items")[i];
        for (var j = 0; j < group.items.length; j++) {
            userNamesList.selectItem({ group: i, item: j });
        }
    }
    machineNameList = $('#machine-names').dxList({
        dataSource: additionalParameters.MachineNames,
        height: 300,
        showSelectionControls: true,
        selectionMode: "multi",
        onSelectionChanged: function (data) {
            machinesSelected = $.map(machinesSelected, function (item) {
                if (data.removedItems.indexOf(item) == -1)
                    return item;
            });
            machinesSelected = machinesSelected.concat(data.addedItems);
            machineNames = machinesSelected.join(",");

        },
        onItemDeleted: function (data) {
            machinesSelected = $.map(machinesSelected, function (item) {
                if (data.itemData !== item)
                    return item;
            });
            machineNames = machinesSelected.join(",");
        },
        showScrollbar: 'always'
    }).dxList("instance");


    for (var i = 0; i < machineNameList.option("items").length; i++) {
        machineNameList.selectItem(machineNameList.option("items")[i]);
    }

    from = $('#from').dxDateBox("instance").option("value");
     to = $('#to').dxDateBox("instance").option("value");
}

function LoadChart() {
    UpdateSettingsString();
    CloseAccordionSection();
    var chart = $('#profile-stat-chart').dxChart(chartOptions).dxChart("instance");
    var chartData;
    chart.showLoadingIndicator();
    chart.beginUpdate();
    $.when($.ajax({
        type: 'POST',
        url: $("#profile-stat-chart").attr('data-get-source'),
        data:
                {
                    dateFrom: moment(from).format("DD.MM.YYYY"),
                    dateTo: moment(to).format("DD.MM.YYYY"),
                    userNames: userNames,
                    machineNames: machineNames,
                    aggregationMode: aggregationMode,
                    aggregationOperation: aggregationOperation,
                    aggregationInterval: aggregationInterval
                },
        success: function (data) {
            if (aggregationMode != "Weekly") {
                result = $.map(data, function (item, index) {
                    item['Moment'] = new Date(item['Moment']);
                    return item;
                });
            }
            chartData = data;
        }
    }))
    .then(function () {
        chart.option("dataSource", chartData);

        if (aggregationMode == "Daily") {
            chart.option("argumentAxis.argumentType", "datetime");
            chart.option("argumentAxis.label.format", "shortTime");
        }
        else
            chart.option("argumentAxis.argumentType", "string");

        if (aggregationOperation == "Average") {
            chart.option("valueAxis.label.format", "fixedPoint");
            chart.option("tooltip.format", "fixedPoint");
            chart.option("tooltip.precision", 4);
        }
        else {
            chart.option("valueAxis.label.format", "");
            chart.option("tooltip.format", "");
            chart.option("tooltip.precision", 0);
        }

        if (aggregationInterval == "minute")
            chart.option("argumentAxis.tickInterval.minutes", 1);
        else if (aggregationInterval == "15 min")
            chart.option("argumentAxis.tickInterval.minutes", 15);
        else if (aggregationInterval == "hour")
            chart.option("argumentAxis.tickInterval.hours", 1);
        chart.endUpdate();
    });
}

function UpdateSettingsString() {
    var settingsString = "FROM: " + moment(from).format("DD.MM.YYYY") + ". TO: " + moment(to).format("DD.MM.YYYY") + ". Aggregation Mode: " + aggregationMode + ". Aggregation Operation: " + aggregationOperation + ". Aggregation Interval: " + aggregationInterval;
    $('#settings-string').text(settingsString);
}

function CloseAccordionSection() {
    $('.accordion .accordion-section-title').removeClass('active');
    $('.accordion .accordion-section-content').slideUp(300).removeClass('open');
}

$(document).ready(function () {
    $('.accordion-section-title').click(function (e) {
        // Grab current anchor value
        var currentAttrValue = $('.accordion-section-title').attr('href');

        if ($('.accordion-section-title').is('.active')) {
            close_accordion_section();
        } else {
            $('.accordion-section-title').addClass('active');
            // Open up the hidden content panel
            $('.accordion ' + currentAttrValue).slideDown(300).addClass('open');
        }

        e.preventDefault();
    });
    GetAdditionalParameters();
    InitControls();
});