var from, to;
var machineNames = "", userNames = "";
var usersSelected = [];
var userNamesList, machineNameList;
var pivotGrid;
var aggregationInterval = "Day";

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

$(document).ready(function () {

    $('.accordion-section-title').click(function (e) {
        // Grab current anchor value
        var currentAttrValue = $(this).attr('href');

        if ($(e.target).is('.active')) {
            close_accordion_section();
        } else {
            $(this).addClass('active');
            // Open up the hidden content panel
            $('.accordion ' + currentAttrValue).slideDown(300).addClass('open');
        }

        e.preventDefault();
    });


    function update_settings_string() {
        var settingsString = "FROM: " + moment(from).format("DD.MM.YYYY") + ". TO: " + moment(to).format("DD.MM.YYYY") + ". Aggregation Interval: " + aggregationInterval;
        $('#settings-string').text(settingsString);
    }

    var pivotGridOptions = {
        allowSortingBySummary: true,
        allowFiltering: true,
        allowSorting: true,
        allowExpandAll: true,
        fieldChooser: {
            enabled: true
        },
        "export": {
            enabled: true,
            fileName: "Sessions"
        },
        dataSource: {
            fields: [{
                groupName: "Date",
                dataField: "Moment",
                dataType: "date",
                area: "row",
                format: "dd.MM.yyyy",
                allowSorting: true
            }, {
                caption: "Month",
                area: "row",
                groupName: "Date",
                groupIndex: 1,
                groupInterval: "month",
                selector: function (data) {
                    var currentDate = moment(data.Moment);
                    return currentDate.format("MMMM YYYY");
                },
                visible: false,
                sortingMethod: function (a, b) {
                    var firstVal = moment(a.value, "DD.MM.YYYY");
                    var secondVal = moment(b.value, "DD.MM.YYYY");
                    if (firstVal.isBefore(secondVal))
                        return 1;
                    else
                        return -1;
                }
            }, {
                caption: "Week",
                area: "row",
                groupName: "Date",
                groupIndex: 2,
                groupInterval: "day",
                visible: false,
                selector: function (data) {
                    var currentDate = moment(data.Moment);
                    return currentDate.startOf("week").format("DD.MM.YYYY") + " - " + currentDate.endOf("week").format("DD.MM.YYYY");
                },
                sortingMethod: function (a, b) {
                    var firstVal = moment(a.value, "DD.MM.YYYY");
                    var secondVal = moment(b.value, "DD.MM.YYYY");
                    if (firstVal.isBefore(secondVal))
                        return 1;
                    else
                        return -1;
                }
            }, {
                caption: "Day",
                allowSorting: true,
                area: "row",
                groupName: "Date",
                groupIndex: 3,
                groupInterval: "day",
                visible: true,
                selector: function (data) {
                    var currentDate = moment(data.Moment);
                    return currentDate.format("DD.MM.YYYY dddd");
                },
                sortingMethod: function (a, b) {
                    var firstVal = moment(a.value, "DD.MM.YYYY");
                    var secondVal = moment(b.value, "DD.MM.YYYY");
                    if (firstVal.isBefore(secondVal))
                        return 1;
                    else
                        return -1;
                }
            }, {
                caption: "User Name",
                area: "column",
                dataField: "UserName"
            }, {
                caption: "Sessions Count",
                area: "data",
                dataField: "SessionsCount",
                summaryType: "sum",
                dataType: "number",
                visible: true,
                isMeasure: true,
                customizeText: function (cellInfo) {
                    return (cellInfo.value == 0) ? "" : cellInfo.valueText;
                }
            }, {
                caption: "Commits Count",
                area: "data",
                dataField: "CommitsCount",
                summaryType: "sum",
                dataType: "number",
                visible: true,
                isMeasure: true,
                customizeText: function (cellInfo) {
                    return (cellInfo.value == 0) ? "" : cellInfo.valueText;
                }
            }, {
                caption: "Publications Count",
                area: "data",
                dataField: "PublicationsCount",
                summaryType: "sum",
                dataType: "number",
                visible: true,
                isMeasure: true,
                customizeText: function (cellInfo) {
                    return (cellInfo.value == 0) ? "" : cellInfo.valueText;
                }
            }, {
                caption: "Local builds count",
                area: "data",
                dataField: "LocalBuildsCount",
                summaryType: "sum",
                dataType: "number",
                visible: true,
                isMeasure: true,
                customizeText: function (cellInfo) {
                    return (cellInfo.value == 0) ? "" : cellInfo.valueText;
                }
            }, {
                caption: "Linear Hours",
                area: "data",
                dataField: "LinearHours",
                summaryType: "sum",
                format: "fixedPoint",
                precision: 4,
                visible: true,
                isMeasure: true,
                customizeText: function (cellInfo) {
                    return (cellInfo.value == 0) ? "" : cellInfo.valueText;
                }
            }, {
                caption: "Active Hours",
                area: "data",
                dataField: "ActiveHours",
                summaryType: "sum",
                format: "fixedPoint",
                precision: 4,
                visible: true,
                isMeasure: true,
                customizeText: function (cellInfo) {
                    return (cellInfo.value == 0) ? "" : cellInfo.valueText;
                }
            }],
            store: [{}]
        }
    };



    $('#metrics').dxSelectBox({
        dataSource: ["Linear Hours", "Active Hours", "Sessions Count"],
        value: "Day",
        onValueChanged: function (data) {
            aggregationInterval = data.value;
        }
    });

    $('#aggregation-interval').dxSelectBox({
        dataSource: ["Day", "Week", "Month"],
        value: "Day",
        onValueChanged: function (data) {
            aggregationInterval = data.value;
            var pivotGridDS = pivotGrid.getDataSource();
            update_settings_string();
            if (aggregationInterval == "Day") {
                pivotGridDS.field("Month", { visible: false });
                pivotGridDS.field("Week", { visible: false });
                pivotGridDS.field("Day", { visible: true });
            }
            else if (aggregationInterval == "Week") {
                pivotGridDS.field("Month", { visible: false });
                pivotGridDS.field("Week", { visible: true });
                pivotGridDS.field("Day", { visible: true });
            }
            else if (aggregationInterval == "Month") {
                pivotGridDS.field("Month", { visible: true });
                pivotGridDS.field("Week", { visible: true });
                pivotGridDS.field("Day", { visible: true });
            }
            pivotGridDS.load();
        },
        disabled:true
    });

    $('#create-grid').dxButton({ text: "Create Pivot Grid",
        type: "normal",
        width: 400,
        onClick: function (data) {
            update_settings_string();
            $.ajax({
                type: 'POST',
                url: $("#activity-summary-grid").attr('data-get-source'),
                data: { dateFrom: moment(from).format("DD.MM.YYYY"), dateTo: moment(to).format("DD.MM.YYYY"), userNames: userNames },
                success: function (data) {
                    close_accordion_section();
                    var newResult = $.map(data, function (item, index) {
                        item['Moment'] = item['Moment'] == 0?null: new Date(item['Moment']);
                        return item;
                    });

                    if (pivotGrid == null)
                        pivotGrid = $('#activity-summary-grid').dxPivotGrid(pivotGridOptions).dxPivotGrid("instance");
                    pivotGrid.option("dataSource.store", data);

                    var selectBox = $('#aggregation-interval').dxSelectBox("instance");
                    selectBox.option("disabled",false);                             
                }
            });
        }
    });

    $.ajax({
        type: 'GET',
        url: $("#gridSettings").attr('data-get-parameters'),
        success: function (data) {
            from = new Date(data.PeriodBegin);
            to = new Date(data.PeriodEnd);
            $('#from').dxDateBox({
                format: "date",
                formatString: "dd.MM.yyyy",
                value: new Date(data.PeriodBegin),
                onValueChanged: function (data) {
                    from = new Date(data.value);
                }
            });
            $('#to').dxDateBox({
                format: "date",
                formatString: "dd.MM.yyyy",
                value: new Date(data.PeriodEnd),
                onValueChanged: function (data) {
                    to = new Date(data.value);
                }
            });
            userNamesList = $('#user-names').dxList({
                dataSource: data.UserNamesByGroup,
                height: 300,
                showSelectionControls: true,
                selectionMode: "multi",
                grouped: true,
                collapsibleGroups: false,
                onSelectionChanged: function (data) {
                    debugger;
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
    });
});