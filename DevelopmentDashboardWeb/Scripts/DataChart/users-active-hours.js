var result = [{}];
var chart = null;
var from, to, userNamesList;
var userNames = "";
var usersSelected = [];
var aggregationInterval = "Day"
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
        url: $("#usersActiveHoursChart").attr('data-get-parameters'),
        async: false,
        success: function (data) {
            additionalData = data;
        }
    })
}
var chartOptions = {
    dataSource: [{}],
    commonSeriesSettings: {
        argumentField: "SessionBegin",
        type: "bar",
        hoverMode: "allArgumentPoints",
        selectionMode: "allArgumentPoints",
        label: {
            visible: true,
            format: {
                type: "fixedPoint",
                precision: 0
            }
        },
        valueField: 'ActiveHours'
    },
    series: [
        
    ],
    title: "Users Active Hours",
    legend: {
        verticalAlignment: "bottom",
        horizontalAlignment: "center"
    },
    "export": {
        enabled: true
    },
    onPointClick: function (e) {
        e.target.select();
    }
};

function InitControls() {

    $('#aggregation-interval').dxSelectBox({
        dataSource: ["Day", "Week", "Month"],
        value: "Day",
        onValueChanged: function (data) {
            aggregationInterval = data.value;
        }
    })

    $('#create-chart').dxButton({
        text: "Create Chart",
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
function arrayObjectIndexOf(myArray, searchTerm, property) {
    for (var i = 0, len = myArray.length; i < len; i++) {
        if (myArray[i][property] === searchTerm) return i;
    }
    return -1;
}
function LoadChart() {
    var chartData;
    chart = $('#usersActiveHoursChart').dxChart(chartOptions).dxChart("instance");

    $.when($.ajax({
        type: 'POST',
        url: $("#usersActiveHoursChart").attr('data-get-source'),
        data: { dateFrom: moment(from).format("DD.MM.YYYY"), dateTo: moment(to).format("DD.MM.YYYY"), userNames: userNames, aggregationInterval: aggregationInterval },
        success: function (data) {
            result = $.map(data, function (item, index) {
                if (aggregationInterval == "Day")
                    item['SessionBegin'] = new Date(item['SessionBegin']);
                return item;
            })
            var newData = [{}];
            chartOptions.series.length = 0;
            var userNames = [];
            for (var i = 0; i < data.length; i++) {
                var index = arrayObjectIndexOf(newData, data[i].SessionBegin, 'SessionBegin');
                if (index == -1)
                    newData.push({ SessionBegin: data[i].SessionBegin });
                index = arrayObjectIndexOf(newData, data[i].SessionBegin, 'SessionBegin');
                var obj = newData[index];
                obj[data[i].UserName] = data[i].ActiveHours;

                index = userNames.indexOf(data[i].UserName);
                if (index == -1)
                    userNames.push(data[i].UserName);
            }
            for (var i = 0; i < userNames.length; i++)
            {
                chartOptions.series.push({ valueField: userNames[i], name: userNames[i] });
            }
            chartData = newData;
        }
    }))
        .then(function () {
            chart.option("dataSource", chartData);
        });
}

$(function () {
    LoadAdditionalData();
    InitControls();
    //LoadChart();
});