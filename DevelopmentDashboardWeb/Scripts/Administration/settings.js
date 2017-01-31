var performCrudRequest = function (type, parameters) {
    var url, sendData;
    if (type == "update-service") {
        url = $("#crud-helpers").attr('data-update-service');
        sendData = { serviceId: parameters.Id, value: parameters.NewValue, description: parameters.Description };
    }

    $.ajax({
        type: 'POST',
        url: url,
        data: sendData,
        async: false,
        success: function (data) {
            load_settings_grid();
        }
    });
}

var load_settings_grid = function () {
    var gridDataSource = [];
    $.ajax({
        type: 'GET',
        async: false,
        url: $("#gridContainer").attr('data-get-source'),
        success: function (data) {
            grid = $("#gridContainer").dxDataGrid(settingsGridOptions).dxDataGrid("instance");
            grid.option('dataSource', data);
        }
    });
    
}

var settingsGridOptions = {
    dataSource: [{}],
    paging: {
        pageSize: 10
    },
    showColumnLines: true,
    showRowLines: true,
    rowAlternationEnabled: true,
    groupPanel: {
        visible: false
    },
    editing: {
        mode: "row",
        allowUpdating: true
    },
    columns: [
        { dataField: "Id", allowEditing: false, width:50 },
        { dataField: "ServiceName", allowEditing: false },
        { dataField: "Name", allowEditing: false },
        { dataField: "Value" },
        { dataField: "Description" }
    ],
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [5, 10, 20],
        showInfo: true
    },
    allowColumnResizing: true,
    columnAutoWidth: true,
    columnChooser: {
        enabled: true,
        height: 380,
        width: 400,
        emptyPanelText: 'A place to hide the columns'
    },
    allowColumnReordering: true,
    noDataText: 'No settings were added',
    onRowUpdating: function (e) {
        performCrudRequest("update-service", { Id: e.key.Id, NewValue: e.newData.Value ? e.newData.Value : e.oldData.Value, Description: e.newData.Description });
    }
}
$(function () {
    load_settings_grid();

})