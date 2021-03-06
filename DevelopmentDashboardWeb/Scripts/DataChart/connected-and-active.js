﻿var interval = 1;
var groupName = "hour";
var result = [{}];
var zoomedChart = null;
var rangeSelector = null;
var from, to, userNamesList;
var userNames = "";
var usersSelected = [];
var userNamesList;
var additionalData;
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
function LoadAdditionalData() {
    $.ajax({
        type: 'GET',
        url: $("#zoomedChart").attr('data-get-parameters'),
        async: false,
        success: function (data) {
            additionalData = data;
        }
    })
}
var zoomedChartOptions = {
    dataSource: [{}],
    commonSeriesSettings: {
        argumentField: 'Moment'
    },
    series: [
                { valueField: 'ConnectedSessionsCount', name: 'Connected Sessions' },
                { valueField: 'ActiveSessionsCount', name: 'Active Sessions' }
            ],
    argumentAxis: {
        argumentType: 'datetime',
        minorTickInterval: 'minute',
        format: 'dd.MM.yyyy HH:mm'
    },
    scrollBar: {
        visible: true
    },
    scrollingMode: "all",
    zoomingMode: "all",
    legend: {
        visible: true
    },
    useAggregation: false,
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
            return { html: '<div style="text-align:left">' + GetMomentSessions(moment(pointInfo.argument), pointInfo.seriesName) + '</div>' }
        }
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
    },
    loadingIndicator: {
        show: true
    },
    title: {
        text: "Connected and Active users"
    }
};

var rangeSelectorOptions = {
    size: {
        height: 220
    },
    margin: {
        left: 10,
        right: 10
    },
    scale: {
        minorTickCount: 2,
        valueType: 'datetime'
    },
    dataSource: [{}],
    dataSourceField: 'Moment',
    chart: {
        series: [
                { valueField: 'ConnectedSessionsCount', name: 'Connected Sessions' },
                { valueField: 'ActiveSessionsCount', name: 'Active Sessions' }
            ],
        valueAxis: {
            valueType: 'datetime'
        }
    },
    behavior: {
        callSelectedRangeChanged: "onMoving"
    },
    onSelectedRangeChanged: function (e) {
        var zoomedChart = $("#container #zoomedChart").dxChart("instance");
        zoomedChart.zoomArgument(e.startValue, e.endValue);
    },
    loadingIndicator: {
        show: true
    }
};

var GetMomentSessions = function (chunkMoment, seriesName) {
    var chunkString = chunkMoment.format("DD.MM.YYYY HH:mm:ss")
    var tooltipContainer = "";
    var offset = new Date().getTimezoneOffset() / 60;
    $.ajax({
        type: 'POST',
        url: $("#zoomedChart").attr('data-get-moment-sessions'),
        data:
        {
            dateFrom: moment(from).format("DD.MM.YYYY"),
            dateTo: moment(to).format("DD.MM.YYYY"),
            moment: chunkString,
            interval: interval,
            group: groupName,
            sessionsType: seriesName,
            userNames: userNames,
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

function InitControls() {
    var numericBoxOptions = {
        showSpinButtons: true,
        onValueChanged: function (data) {
            interval = data.value;
        },
        value: 1,
        width: 100
    }
    var selectBoxOptions = {
        items: ["minute", "hour", "day"],
        value: "hour",
        onValueChanged: function (data) {
            groupName = data.value;
        },
        width: 100
    }
    $('#numericBox').dxNumberBox(numericBoxOptions).dxNumberBox("instance");
    $('#selectBox').dxSelectBox(selectBoxOptions).dxSelectBox("instance");

    $('#create-chart').dxButton({ text: "Create Chart",
        type: "normal",
        width: 400,
        onClick: LoadChart
    });
    from = new Date(additionalData.PeriodBegin);
    to = new Date(additionalData.PeriodEnd);
    $('#from').dxDateBox({
        format: "datetime",
        formatString: "dd.MM.yyyy HH:mm:ss",
        value: new Date(additionalData.PeriodBegin),
        onValueChanged: function (data) {
            from = new Date(data.value);
        }
    });
    $('#to').dxDateBox({
        format: "datetime",
        formatString: "dd.MM.yyyy HH:mm:ss",
        value: new Date(additionalData.PeriodEnd),
        onValueChanged: function (data) {
            to = new Date(data.value);
        }
    });

    userNamesList = $('#user-names').dxList({
        dataSource: additionalData.UserNamesByGroup,
        height: 200,
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
}

function LoadChart() {
    var chartData;
    zoomedChart = $('#zoomedChart').dxChart(zoomedChartOptions).dxChart("instance");
    rangeSelector = $('#rangeSelector').dxRangeSelector(rangeSelectorOptions).dxRangeSelector("instance");
    /*zoomedChart.showLoadingIndicator();
    zoomedChart.beginUpdate();*/
    /*rangeSelector.showLoadingIndicator();
    rangeSelector.beginUpdate();*/

    $.when($.ajax({
        type: 'POST',
        url: $("#zoomedChart").attr('data-get-source'),
        data: { dateFrom: moment(from).format("DD.MM.YYYY"), dateTo: moment(to).format("DD.MM.YYYY"), interval: interval, groupName: groupName, userNames: userNames },
        success: function (data) {
            result = $.map(data, function (item, index) {
                item['Moment'] = new Date(item['Moment']);
                item['ActiveSessionsCount'] = item['ActiveSessionsCount'] == 0 ? null : item['ActiveSessionsCount'];
                item['ConnectedSessionsCount'] = item['ConnectedSessionsCount'] == 0 ? null : item['ConnectedSessionsCount'];
                return item;
            })
            chartData = data;
        }
    }))
        .then(function () {
            zoomedChart.option("dataSource", chartData);
            //zoomedChart.endUpdate();
            rangeSelector.option("dataSource", chartData);
            //rangeSelector.endUpdate();
        });
}

$(function () {
    LoadAdditionalData();
    InitControls();
    LoadChart();
});