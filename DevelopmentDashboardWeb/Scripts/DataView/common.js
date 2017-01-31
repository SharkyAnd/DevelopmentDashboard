var sessionGridOptions = {
    dataSource: [{}],
    paging: {
        pageSize: 10
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
        { dataField: "UserName", allowGrouping: true },
        { dataField: "MachineName", allowGrouping: true },
        {
            dataField: "SessionBegin",
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
            {
                dataField: "SessionEnd",
                dataType: "date",
                format: 'dd.MM.yyyy HH:mm:ss',
                allowGrouping: false,
                filterOperations: ['>=', '<=', 'between'],
                selectedFilterOperation: '<=',
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
            {
                dataField: "LastInputTime",
                dataType: "date",
                format: 'dd.MM.yyyy HH:mm:ss',
                allowGrouping: false,
                filterOperations: ['>=', '<=', 'between'],
                selectedFilterOperation: '<=',
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
                {
                    dataField: "ActiveHours",
                    allowGrouping: false,
                    cellTemplate: function (container, options) {
                        $('<a/>').addClass('dx-link')
                        .text(options.value)
                        .on('dxclick', function () {
                            GetSessionsActivityProfiles(options.key.Id);
                        })
                        .appendTo(container);
                    }
                },
                { dataField: "SessionState", allowGrouping: false },
                { dataField: "UtcOffset", allowGrouping: false },
                { dataField: "ClientName", allowGrouping: false },
                { dataField: "ClientDisplayDetails", allowGrouping: false, visible: false },
                { dataField: "ClientReportedIpAddress", allowGrouping: false, visible: false },
                { dataField: "ClientBuildNumber", allowGrouping: false, visible: false },
                { dataField: "ActiveTime", allowGrouping: false, caption: "Active Time %", format: "percent", precision: 4 },
                { dataField: "LinearHours", allowGrouping: false }
            ],
    summary: {
        groupItems: [{
            column: "Id",
            summaryType: "count",
            displayFormat: "{0} total sessions"
        }, {
            column: "ActiveHours",
            summaryType: "sum",
            valueFormat: "fixedPoint",
            precision: 4,
            alignByColumn: true,
            displayFormat: "{0}"
        }, {
            column: "LinearHours",
            summaryType: "sum",
            valueFormat: "fixedPoint",
            precision: 4,
            alignByColumn: true,
            displayFormat: "{0}"
        }
                ]
    },
    columnChooser: {
        enabled: true,
        height: 380,
        width: 400,
        emptyPanelText: 'A place to hide the columns'
    },
    selection: { mode: 'multiple' },
    onSelectionChanged: function (selectedItems) {
        if (selectedItems.selectedRowsData.length > 0)
            btnGetChart.option("disabled", false);
        else
            btnGetChart.option("disabled", true);
    },
    onEditorPrepared: function (e) {
        if ((e.dataField == 'SessionBegin' || e.dataField == 'SessionEnd') && e.parentType == 'filterRow') {
            e.editorElement.dxDateBox('instance').option('format', 'datetime');
            e.editorElement.dxDateBox('instance').option('onValueChanged', function (options) { e.setValue(options.value); });
        }
    },
    allowColumnReordering: true
};
var popupOptions = null;
var session = {};
var popup = null;
var GetSessionsActivityProfiles = function (data) {
    $.ajax({
        type: 'POST',
        url: $("#btnGetChart").attr('data-get-chart'),
        data: { _sessionsIDsString: data },
        success: function (data) {
            showSessionChart(data);
        }
    });
}
var showSessionChart = function (data) {
    session = data;
    popup && $(".popup").remove();
    $popupContainer = $("<div />")
                            .addClass("popup")
                            .appendTo($("#popup"));
    popup = $popupContainer.dxPopup(popupOptions).dxPopup("instance");
    popup.show();
};
var btnGetChart = null;
var dataGrid = null;
$(document).ready(function () {
    btnGetChart = $("#btnGetChart").dxButton({
        text: 'Compare Selected Sessions',
        onClick: function (info) {
            var sessionIds = "";
            var data = $('#gridContainer').dxDataGrid("instance").getSelectedRowsData();
            for (var i = 0; i < data.length; i++) {
                sessionIds += data[i].Id + ",";
            }
            GetSessionsActivityProfiles(sessionIds);
        },
        disabled: true
    }).dxButton("instance");
});