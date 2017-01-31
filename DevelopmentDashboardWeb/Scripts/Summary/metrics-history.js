var viewMode = "Simple";
var groupVal;

var beforeGroup = function (sender) {
    groupVal = this.dataField;
    return sender[this.dataField];
}

var simpleGridStore = new DevExpress.data.CustomStore({
    load: function (options) {
        var deferred = new $.Deferred();

        $.getJSON($('#grid-container').attr('data-get-source-simple'), { options: JSON.stringify(options) }, function (data) {
            $.map(data, function (item, index) {
                item["CalculationMoment"] = item["CalculationMoment"] == 0 ? null : item["CalculationMoment"];
            })
            deferred.resolve(data);
        });
        Loader.hide();
        return deferred.promise();
    },
    totalCount: function (options) {
        var deferred = new $.Deferred();
        $.getJSON($('#grid-container').attr('data-get-count'), null, function (data) {
            deferred.resolve(data);
        });

        return deferred.promise();
    }
});

var metricsHistoryGridOptions = {
    dataSource: { store: simpleGridStore },
    remoteOperations: true,
    paging: {
        pageSize: 10
    }, "export": {
        enabled: true,
        fileName: "MetricsHistory"
    },
    showColumnLines: true,
    showRowLines: true,
    rowAlternationEnabled: true,
    headerFilter: {
        visible: true
    },
    filterRow: {
        visible: true,
        applyFilter: "auto"
    },
    groupPanel: {
        visible: true
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [5, 10, 20],
        showInfo: true
    },
    grouping: {
        autoExpandAll: false
    },
    allowColumnResizing: true,
    columnAutoWidth: true,
    columns: [
        { dataField: "Id", allowGrouping: false },
        { dataField: "CalculationMachine", allowGrouping: true },
        { dataField: "ProjectGroup", allowGrouping: true },
        { dataField: "ProjectName", allowGrouping: true, calculateGroupValue: beforeGroup },
        {
            dataField: "CalculationMoment",
            dataType: "date",
            format: 'dd.MM.yyyy HH:mm:ss',
            allowGrouping: false,
            filterOperations: ['>=', '<=', 'between'],
            selectedFilterOperation: '>=',
            calculateFilterExpression: function (filterValue, selectedFilterOperation) {
                if (filterValue) {
                    var startValue, endValue;
                    if (selectedFilterOperation == 'between' && filterValue.length < 2)
                        return;
                    if (selectedFilterOperation != 'between')
                        startValue = new Date(filterValue);
                    else
                        startValue = new Date(filterValue[0]);
                    startValue.setSeconds(0);
                    startValue.setMilliseconds(0);
                    if (filterValue.length > 1)
                        endValue = new Date(filterValue[1]);
                    if (selectedFilterOperation == '>=')
                        return [[this.dataField, '>=', startValue]];
                    else if (selectedFilterOperation == '<=')
                        return [[this.dataField, '<=', startValue]];
                    else
                        return [[this.dataField, '>=', startValue], 'and', [this.dataField, '<=', endValue]];
                }
            }
        },
            { dataField: "RevisionNumber", allowGrouping: true, calculateGroupValue: beforeGroup },
            { dataField: "RevisionMessage", allowGrouping: false },
            { dataField: "RevisionAuthor", allowGrouping: true },
            { dataField: "MeasurementTool", allowGrouping: true },
            { dataField: "MetricName", allowGrouping: false },
            { dataField: "MetricValue", allowGrouping: false }
            ],
    summary: {
        groupItems: [{
            column: "Id",
            summaryType: "count",
            displayFormat: "{0} total metrics calculation"
        },
        {
            name: 'LinesOfCodeSUM',
            summaryType: "custom",
            displayFormat: "{0}",
            customizeText: function (cellInfo) {
                return "Lines of code: " + cellInfo.value.linesOfCode;
            }
        }
                ],
        calculateCustomSummary: function (options) {
            if (options.name === "LinesOfCodeSUM") {
                if (options.summaryProcess === "start") {
                    options.totalValue = { moment: moment('1900-01-01'), linesOfCode: 0, includedProjects: [] };
                }
                if (options.summaryProcess === "calculate") {

                    if (groupVal == "RevisionNumber") {
                        if (options.value.MetricName == "LinesOfCode")
                            if (options.totalValue.includedProjects.indexOf(options.value.ProjectName) == -1) {
                                options.totalValue.includedProjects.push(options.value.ProjectName);
                                options.totalValue.linesOfCode += options.value.MetricValue
                            }
                    }
                    else {
                        var fMoment = moment(options.totalValue.moment);
                        var sMoment = moment(options.value.CalculationMoment);
                        if (options.value.MetricName == "LinesOfCode" && sMoment.isAfter(fMoment))
                            options.totalValue = { moment: options.value.CalculationMoment, linesOfCode: options.value.MetricValue };
                    }
                }
                if (options.summaryProcess === "finalize") {
                    //
                }
            }
        }
    },
    columnChooser: {
        enabled: true,
        height: 380,
        width: 400,
        emptyPanelText: 'A place to hide the columns'
    },
    onEditorPrepared: function (e) {
        if (e.dataField == 'CalculationMoment' && e.parentType == 'filterRow') {
            e.editorElement.dxDateBox('instance').option('format', 'datetime');
            e.editorElement.dxDateBox('instance').option('onValueChanged', function (options) { e.setValue(options.value); });
        }
    },
    allowColumnReordering: true,
    wordWrapEnabled: true
};

var expandedGridOptions = {
    dataSource: [{}],
    paging: {
        pageSize: 10
    }, "export": {
        enabled: true,
        fileName: "MetricsHistory"
    },
    showColumnLines: true,
    showRowLines: true,
    rowAlternationEnabled: true,
    headerFilter: {
        visible: true
    },
    filterRow: {
        visible: true,
        applyFilter: "auto"
    },
    groupPanel: {
        visible: true
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [5, 10, 20],
        showInfo: true
    },
    grouping: {
        autoExpandAll: false
    },
    allowColumnResizing: true,
    columnAutoWidth: true,
    columns: [
        { dataField: "RevisionNumber", allowGrouping: true, caption: "Revision", calculateGroupValue: beforeGroup },
        { dataField: "RevisionMessage", allowGrouping: false },
        {
            dataField: "CalculationMoment",
            dataType: "date",
            format: 'dd.MM.yyyy HH:mm:ss',
            allowGrouping: false,
            filterOperations: ['>=', '<=', 'between'],
            selectedFilterOperation: '>=',
            calculateFilterExpression: function (filterValue, selectedFilterOperation) {
                if (filterValue) {
                    var startValue, endValue;
                    if (selectedFilterOperation == 'between' && filterValue.length < 2)
                        return;
                    if (selectedFilterOperation != 'between')
                        startValue = new Date(filterValue);
                    else
                        startValue = new Date(filterValue[0]);
                    startValue.setSeconds(0);
                    startValue.setMilliseconds(0);
                    if (filterValue.length > 1)
                        endValue = new Date(filterValue[1]);
                    if (selectedFilterOperation == '>=')
                        return [[this.dataField, '>=', startValue]];
                    else if (selectedFilterOperation == '<=')
                        return [[this.dataField, '<=', startValue]];
                    else
                        return [[this.dataField, '>=', startValue], 'and', [this.dataField, '<=', endValue]];
                }
            }
        },
            { dataField: "ProjectName", allowGrouping: true, caption: "Project", calculateGroupValue: beforeGroup },
            { dataField: "UserName", allowGrouping: true, caption: "Revision Author" },
            ],
    summary: {
        groupItems: [{
            column: "Id",
            summaryType: "count",
            displayFormat: "{0} total metrics calculation"
        }
                ],
        calculateCustomSummary: function (options) {
            if (options.name === "LinesOfCodeSUM") {
                if (options.summaryProcess === "start") {
                    options.totalValue = { moment: moment('1900-01-01'), linesOfCode: 0, includedProjects: [] };
                }
                if (options.summaryProcess === "calculate") {

                    if (groupVal == "RevisionNumber") {
                        options.totalValue.moment = options.value.CalculationMoment;
                        if (options.totalValue.includedProjects.indexOf(options.value.ProjectName) == -1) {
                            options.totalValue.linesOfCode += options.value.LinesOfCode;
                            options.totalValue.includedProjects.push(options.value.ProjectName);
                        }
                    }
                    else {
                        var fMoment = moment(options.totalValue.moment);
                        var sMoment = moment(options.value.CalculationMoment);
                        if (sMoment.isAfter(fMoment))
                            options.totalValue = { moment: options.value.CalculationMoment, linesOfCode: options.value.LinesOfCode };
                    }
                }
                if (options.summaryProcess === "finalize") {
                    //
                }
            }
        }
    },
    columnChooser: {
        enabled: true,
        height: 380,
        width: 400,
        emptyPanelText: 'A place to hide the columns'
    },
    onEditorPrepared: function (e) {
        if (e.dataField == 'CalculationMoment' && e.parentType == 'filterRow') {
            e.editorElement.dxDateBox('instance').option('format', 'datetime');
            e.editorElement.dxDateBox('instance').option('onValueChanged', function (options) { e.setValue(options.value); });
        }
    },
    allowColumnReordering: true,
    wordWrapEnabled: true
}

$('#view-mode').dxSelectBox({
    dataSource: ["Simple", "Expanded"],
    value: "Simple",
    onValueChanged: function (data) {
        update_metrics_history(data.value);
    }
});
function arrayObjectIndexOf(myArray, searchTerm, property) {
    for (var i = 0, len = myArray.length; i < len; i++) {
        if (myArray[i][property] === searchTerm) return i;
    }
    return -1;
}
var FormatMetricsHistoryValue = function (data) {
    var gridDataSource = [];
    for (var i = 0; i < data.MetricsHistoryData.length; i++) {
        currentData = data.MetricsHistoryData[i];
        var metricHistory = {
            CalculationMoment: currentData.CalculationMoment,
            RevisionNumber: currentData.RevisionNumber,
            ProjectName: currentData.ProjectName,
            UserName: currentData.UserName,
            RevisionMessage: currentData.RevisionMessage
        };
        for (var j = 0; j < data.MetricNames.length; j++) {
            metricHistory[data.MetricNames[j]] = currentData.MetricValues[j];
        }
        gridDataSource.push(metricHistory);
    }

    for (var j = 0; j < data.MetricNames.length; j++) {
        var column = {
            dataField: data.MetricNames[j], allowGrouping: false
        }
        var groupItem = null;
        if (column.dataField == "LinesOfCode") {
            groupItem = {
                name: 'LinesOfCodeSUM',
                summaryType: "custom",
                displayFormat: "{0}",
                customizeText: function (cellInfo) {
                    return "Lines of code: " + cellInfo.value.linesOfCode;
                }
            }
        }
        else {
            groupItem = {
                column: data.MetricNames[j],
                summaryType: "sum",
                alignByColumn: true,
                displayFormat: "{0}"
            }
        }
        if (arrayObjectIndexOf(expandedGridOptions.columns, column.dataField, 'dataField') == -1)
            expandedGridOptions.columns.push(column);
        if (arrayObjectIndexOf(expandedGridOptions.summary.groupItems, groupItem.column, 'column') == -1)
            expandedGridOptions.summary.groupItems.push(groupItem);
    }

    return gridDataSource;
}

var update_metrics_history = function (type) {
    var url;
    var pivotGrid;

    $('#grid-container').empty();
    var div = $('<div id="metrics-grid" />');
    $('#grid-container').append(div);
    Loader.show();
    if (type == "Simple") {
        pivotGrid = $('#metrics-grid').dxDataGrid(metricsHistoryGridOptions).dxDataGrid("instance");
        return;
    }
    else {        
        url = $("#grid-container").attr('data-get-source-expanded')
        pivotGrid = $('#metrics-grid').dxDataGrid(expandedGridOptions).dxDataGrid("instance");
    }
    var gridData;
    $.when(
    $.ajax({
        type: 'GET',
        url: url,
        success: function (data) {
            var pivotGrid = null;

            var newResult = $.map(data.MetricsHistoryData, function (item, index) {
                item['CalculationMoment'] = new Date(item['CalculationMoment']);
                return item;
            });

            gridData = data;
        }
    }))
    .then(function () {
        var newData = FormatMetricsHistoryValue(gridData);
        pivotGrid.option("dataSource", newData);
        Loader.hide();
    });
}

$(document).ready(function () {
    Loader.init('#grid-container');
    update_metrics_history("Simple");
});