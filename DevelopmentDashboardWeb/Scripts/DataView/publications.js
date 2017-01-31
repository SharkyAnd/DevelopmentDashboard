var publicationsGridOptions = {
    dataSource: [{}],
    paging: {
        pageSize: 10
    }, "export": {
        enabled: true,
        fileName: "Publications"
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
        { dataField: "SourceMachine", allowGrouping: true },
        { dataField: "TargetMachine", allowGrouping: true },
        {
            dataField: "Moment",
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
            { dataField: "RevisionNumber", allowGrouping: false },
            { dataField: "RevisionAuthor", allowGrouping: false },
            { dataField: "RevisionMessage", allowGrouping: false },
            { dataField: "Project", allowGrouping: false },
            { dataField: "ProjectFullPath", allowGrouping: false },
            {
                dataField: "Url",
                allowGrouping: false,
                cellTemplate: function (container, options) {
                    $('<a href='+options.value+'/>').addClass('dx-link')
                        .text(options.value)                       
                        .appendTo(container);
                }
            },
            { dataField: "Comment", allowGrouping: false }
            ],
    summary: {
        groupItems: [{
            column: "Id",
            summaryType: "count",
            displayFormat: "{0} total publications"
        }
                ]
    },
    columnChooser: {
        enabled: true,
        height: 380,
        width: 400,
        emptyPanelText: 'A place to hide the columns'
    },
    onEditorPrepared: function (e) {
        if (e.dataField == 'Moment' && e.parentType == 'filterRow') {
            e.editorElement.dxDateBox('instance').option('format', 'datetime');
            e.editorElement.dxDateBox('instance').option('onValueChanged', function (options) { e.setValue(options.value); });
        }
    },
    allowColumnReordering: true,
    wordWrapEnabled: true
};
var dataGrid;
$(document).ready(function () {
    $.ajax({
        type: 'GET',
        url: $("#gridContainer").attr('data-get-source'),
        success: function (data) {
            var offset = new Date().getTimezoneOffset() / 60;
            var newResult = $.map(data, function (item, index) {
                item['Moment'] = new Date(item['Moment']);
                item['Moment'] = moment(item["Moment"]).add(offset,'h');
                return item;
            });
            dataGrid = $('#gridContainer').dxDataGrid(publicationsGridOptions).dxDataGrid("instance").option("dataSource", data);
        }
    });
});