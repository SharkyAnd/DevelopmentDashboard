var currentPage;
var grid;
var managementData = {};
var popupOptions = null;
var popup = null;
var isShowDialog = true;
var dataGrid;
var performCrudRequest = function (type, parameters) {
    var url, sendData;
    if (type == "add-group") {
        url = $("#crud-helpers").attr('data-add-group');
        sendData = { groupName: parameters };
    }
    else if (type == "update-group") {
        url = $("#crud-helpers").attr('data-update-group');
        sendData = { groupId: parameters.GroupId, groupName: parameters.NewValue };
    }
    else if (type == "delete-group") {
        url = $("#crud-helpers").attr('data-delete-group');
        sendData = { groupId: parameters };
    }
    else if (type == "add-user-to-group") {
        url = $("#crud-helpers").attr('data-add-user-group');
        sendData = { groupName: parameters.GroupName, userName: parameters.UserName };
    }
    else if (type == "remove-user-from-group") {
        url = $("#crud-helpers").attr('data-remove-user-group');
        sendData = { groupName: parameters.GroupName, userName: parameters.UserName };
    }

    $.ajax({
        type: 'POST',
        url: url,
        data: sendData,
        async: false,
        success: function (data) {
            load_inner_grid();
        }
    });
}

var showPopupGrid = function () {
    popup && $(".popup").remove();
    $popupContainer = $("<div />")
                            .addClass("popup")
                            .appendTo($("#popup"));
    popup = $popupContainer.dxPopup(popupOptions).dxPopup("instance");
    popup.show();
};

var UpdateUserGroup = function (add, cellInfo) {
    if (add)
        performCrudRequest("add-user-to-group", { GroupName: cellInfo.column.caption, UserName: cellInfo.data.UserName });
    else
        performCrudRequest("remove-user-from-group", { GroupName: cellInfo.column.caption, UserName: cellInfo.data.UserName });
}

function arrayObjectIndexOf(myArray, searchTerm, property) {
    for (var i = 0, len = myArray.length; i < len; i++) {
        if (myArray[i][property] === searchTerm) return i;
    }
    return -1;
}

var load_users_groups = function () {
    var gridDataSource = [];
    $.ajax({
        type: 'GET',
        async: false,
        url: $("#gridContainer").attr('data-get-source'),
        success: function (data) {
            var length = matrixGridOptions.columns.length;
            for (var i = length-1; i > -1; i--) {
                if (matrixGridOptions.columns[i].dataField != "UserName") {
                    var index = arrayObjectIndexOf(matrixGridOptions.columns, matrixGridOptions.columns[i].dataField, 'dataField');
                    if (index != -1)
                        matrixGridOptions.columns.splice(index, 1);
                }
            }

            for (var i = 0; i < data.MatrixData.length; i++) {
                currentData = data.MatrixData[i];
                var userManagement = {
                    UserName: currentData.UserName
                };
                for (var j = 0; j < data.Groups.length; j++) {
                    var dataField = "";
                    var dataFieldParts = data.Groups[j].split(' ');
                    for (var k = 0; k < dataFieldParts.length; k++) {
                        dataField += dataFieldParts[k];
                    }
                    userManagement[dataField] = currentData.UserInGroups[j];
                }
                gridDataSource.push(userManagement);
            }
            for (var i = 0; i < data.Groups.length; i++) {
                var dataField = "";
                var dataFieldParts = data.Groups[i].split(' ');
                for (var j = 0; j < dataFieldParts.length; j++) {
                    dataField += dataFieldParts[j];
                }
                var column = {
                    dataField: dataField,
                    caption: data.Groups[i],
                    cellTemplate: function (container, options) {
                        $('<div/>').dxCheckBox({
                            value: options.value,
                            onValueChanged: function (data) {
                                UpdateUserGroup(data.value, options);
                            }
                        })
                        .appendTo(container);
                    }
                }
                matrixGridOptions.columns.push(column);
            }
        }
    });
    return gridDataSource;
}

var matrixGridOptions = {
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
    columns: [{ dataField: "UserName", caption: "User Name"}],
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
    noDataText: 'No groups were added'
}

var load_grid = function () {
    var gridDataSource = load_users_groups();
    grid = $("#gridContainer").dxDataGrid(matrixGridOptions).dxDataGrid("instance");
    grid.option('dataSource', gridDataSource);
}

var GetManagementData = function () {
    var manageData;
    $.ajax({
        type: 'POST',
        url: $("#content_template").attr('data-get-source'),
        async: false,
        success: function (data) {
            manageData = data;
        }
    });
    return manageData;
}

var load_inner_grid = function () {
    managementData = GetManagementData();
    $("#gridInPopup").dxDataGrid({
        dataSource: managementData,
        paging: {
            enabled: false
        },
        showColumnLines: true,
        showRowLines: true,
        rowAlternationEnabled: true,
        headerFilter: {
            visible: true
        },
        editing: {
            mode: "row",
            allowUpdating: true,
            allowDeleting: true,
            allowAdding: true,
            texts: {
                confirmDeleteMessage: ''
            }
        },
        filterRow: {
            visible: true,
            applyFilter: "auto"
        },
        groupPanel: {
            visible: false
        },
        allowColumnResizing: true,
        columnAutoWidth: true,
        columns: [
                    { dataField: "Id", allowGrouping: false, width: 50, allowEditing: false },
                    {
                        dataField: "Name",
                        caption: "Group name",
                        allowGrouping: false
                    },
                    { dataField: "UsersInGroup", allowGrouping: false, allowEditing: false }
                ],
        onRowInserted: function (e) {
            performCrudRequest("add-group", e.data.Name);
        },
        onRowUpdating: function (e) {
            performCrudRequest("update-group", { GroupId: e.key.Id, NewValue: e.newData.Name });
        },
        onRowRemoving: function (info) {
            dataGrid = $("#gridInPopup").dxDataGrid("instance");
            if (isShowDialog) {
                var result = DevExpress.ui.dialog.confirm("There are " + info.data.UsersInGroup + " user in '" + info.data.Name + "' group. " +
                "Deleting the group will not delete its users but information about their membership to this group will be lost. Are you sure you want to delete this group?", "Confirm delete");
                info.cancel = true;

                result.done(function (dialogResult) {
                    isShowDialog = false;
                    dialogResult && dataGrid.deleteRow(dataGrid.getRowIndexByKey(info.key));
                    isShowDialog = true;
                    dialogResult && performCrudRequest("delete-group", info.key.Id);
                });
            }
        },
        noDataText: 'Add groups using the "+" button'
    });
}

$(document).ready(function () {
    currentPage = $(".active a").text();
    load_grid();
    $(".page").click(function () {
        currentPage = $(this).children().text();
        $(".page").removeClass("active");
        $(this).addClass("active");
        load_grid();
    });

    $("#btnGetGroups").dxButton({
        text: 'View groups',
        onClick: function (info) {
            showPopupGrid();
        }
    })

    popupOptions = {
        width: 1200,
        height: 700,
        contentTemplate: $('#content_template'),
        onShown: function () {
            load_inner_grid();
        },
        onHidden: function () {
            load_grid();
        },
        showTitle: true,
        title: "Groups",
        visible: false,
        dragEnabled: false,
        closeOnOutsideClick: true
    };
});