var currentRole;

var load_roles_grid = function () {
    $.ajax({
        type: 'GET',
        async: false,
        url: $("#rolesGridContainer").attr('data-get-source'),
        success: function (data) {
            $('#rolesGridContainer').dxDataGrid({
                dataSource: data,
                paging: {
                    pageSize: 10
                },
                pager: {
                    showPageSizeSelector: true,
                    allowedPageSizes: [5, 10, 20],
                    showInfo: true
                },
                showColumnLines: true,
                showRowLines: true,
                rowAlternationEnabled: true,
                headerFilter: {
                    visible: true
                },
                editing: {
                    mode: "form",
                    allowDeleting: true,
                    texts: {
                        confirmDeleteMessage: ''
                    }
                },
                groupPanel: {
                    visible: false
                },
                allowColumnResizing: true,
                columns: [
                    { dataField: "Id", allowGrouping: false, width: 50, allowEditing: false },
                    { dataField: "Name", allowGrouping: false, allowEditing: true },
                    { dataField: "Permissions", allowGrouping: false, allowEditing: true,
                        cellTemplate: function (cellElement, cellInfo) {
                            var permissionsString = cellInfo.value[0].Controller + '-' + cellInfo.value[0].Action;
                            for (var i = 1; i < cellInfo.value.length; i++) {
                                permissionsString += ', ' + cellInfo.value[i].Controller + '-' + cellInfo.value[i].Action;
                            }
                            $('<div/>').html(permissionsString).appendTo(cellElement);
                        }
                    },
                    { dataField: "UsersInRole", allowGrouping: false, allowEditing: true },
                    {
                        width: 100,
                        alignment: 'center',
                        cellTemplate: function (container, options) {
                            $('<a/>').addClass('dx-link')
                            .text('Edit')
                            .on('dxclick', function () {
                                popupType = "Edit role";
                                currentRole = options.data;
                                showPopupForm();
                            })
                            .appendTo(container);
                        }
                    }
                ],
                onRowRemoving: function (info) {
                    dataGrid = $("#rolesGridContainer").dxDataGrid("instance");
                    if (isShowDialog) {
                        var result = DevExpress.ui.dialog.confirm("There are " + info.data.UsersInRole + " users associated with the '" + info.data.Name + "' role. " +
                    "Deleting the role will not delete users associated with it but information about their membership to this role will be lost. Are you sure you want to delete this role?", "Confirm delete");
                        info.cancel = true;

                        result.done(function (dialogResult) {
                            isShowDialog = false;
                            dialogResult && dataGrid.deleteRow(dataGrid.getRowIndexByKey(info.key));
                            isShowDialog = true;
                            dialogResult && performCrudRequest("delete-role", info.key.Id);
                            updatePage();
                        });

                    }
                },
                wordWrapEnabled: true
            });
        }
    });
}

var role_form_options = {
    formData: [],
    labelLocation: 'top',
    items: [
            {
                itemType: 'simple',
                label: {
                    text: 'Role name'
                },
                editorType: 'dxTextBox',
                template: function (container, options) {
                    return $('<div id="form-rolename"/>').dxTextBox({
                        value: ''
                    }).dxValidator({ validationRules: [
                            { type: 'required', message: 'Name is required' }
                        ],
                        onValidated: function (data) {
                            if (data.isValid)
                                $('#saveBtn').dxButton("instance").option("disabled", false);
                            else
                                $('#saveBtn').dxButton("instance").option("disabled", true);
                        }
                    });
                },
                isRequired: true
            },
            {
                itemType: 'simple',
                label: {
                    text: 'Permissions'
                },
                template: function (container, options) {
                    return $('<div id="form-permissions"/>').dxList({
                        dataSource: permissions,
                        showSelectionControls: true,
                        height: 200,
                        selectionMode: "multi",
                        grouped: true,
                        collapsibleGroups: false
                    });
                },
                name: 'permissions'
            },
            {
                itemType: 'simple',
                label: {
                    text: 'Associate users'
                },
                template: function (container, options) {
                    return $('<div id="form-users"/>').dxList({
                        dataSource: users,
                        showSelectionControls: true,
                        selectionMode: "multi",
                        height: "300px"
                    });
                },
                name: 'associatedUsers',
                visible: true
            }
            ]
}

var get_role_info = function () {
    var roleInfo = {
        RoleName: $('#form-rolename').dxTextBox("instance").option("value"),
        Permissions: $("#form-permissions").dxList("instance").option("selectedItems")
    }
    if (popupType == "Add new role")
        roleInfo["AssociatedUsers"] = $("#form-users").dxList("instance").option("selectedItems");
    return roleInfo;
}

var load_role_info = function () {
    $('#form-rolename').dxTextBox("instance").option("value", currentRole.Name)
    var form_permissions = $("#form-permissions").dxList("instance")
    for (var i = 0; i < form_permissions.option("items").length; i++) {
        var group = form_permissions.option("items")[i];

        for (var j = 0; j < group.items.length; j++) {
            for (var k = 0; k < currentRole.Permissions.length; k++) {
                var permission = currentRole.Permissions[k];
                if (permission.Controller == group.key && permission.Action == group.items[j]) {
                    form_permissions.selectItem({ group: i, item: j });
                    break;
                }
            }
        }
    }
}