var users, roles, permissions;
var popupOptions = null;
var popup = null;
var isShowDialog = true;
var dataGrid;
var popupType;
var performCrudRequest = function (type, parameters) {
    var url, sendData;
    if (type == "add-role") {
        url = $("#crud-helpers").attr('data-add-role');
        sendData = parameters;
    }
    else if (type == "update-role") {
        url = $("#crud-helpers").attr('data-update-role');
        sendData = parameters;
    }
    else if (type == "delete-role") {
        url = $("#crud-helpers").attr('data-delete-role');
        sendData = { roleId: parameters };
    }
    else if (type == "add-user-to-role") {
        url = $("#crud-helpers").attr('data-add-user-role');
        sendData = parameters;
    }
    else if (type == "remove-user-from-role") {
        url = $("#crud-helpers").attr('data-remove-user-role');
        sendData = parameters
    }
    else if (type == "add-user") {
        url = $("#crud-helpers").attr('data-add-user');
        sendData = parameters;
    }
    else if (type == "update-user") {
        url = $("#crud-helpers").attr('data-update-user');
        sendData = parameters;
    }
    else if (type == "delete-user") {
        url = $("#crud-helpers").attr('data-delete-user');
        sendData = { idUser: parameters };
    }
    else if (type == "send-confirmation") {
        url = $("#crud-helpers").attr('data-send-confirmation');
        sendData = { UserName: parameters.userName, Email: parameters.email, Password: parameters.pass, AssociatedRoles: parameters["roles"] == undefined ? null : parameters.roles.join(', ') };
    }
    else if (type == "add-rule") {
        url = $("#crud-helpers").attr('data-add-rule');
        sendData = parameters;
    }
    else if (type == "update-rule") {
        url = $("#crud-helpers").attr('data-update-rule');
        sendData = parameters;
    }
    else if (type == "remove-rule") {
        url = $("#crud-helpers").attr('data-delete-rule');
        sendData = { ruleId: parameters };
    }

    $.ajax({
        type: 'POST',
        url: url,
        data: sendData,
        async: false,
        success: function (data) {
            //load_users_grid();
        }
    });
}

var get_additional_data = function () {
    $.ajax({
        type: 'GET',
        url: $('#form_template').attr('data-get-additional'),
        async: false,
        success: function (data) {
            users = data.UserNames;
            roles = data.RolesNames;
            permissions = data.Permissions;
        }
    });
}

var showPopupForm = function () {
    popup && $(".popup").remove();
    $popupContainer = $("<div />")
                            .addClass("popup")
                            .appendTo($("#popup"));
    popup = $popupContainer.dxPopup(popupOptions).dxPopup("instance");
    popup.option('title', popupType);
    popup.show();
};

var UpdateUserRole = function (add, cellInfo) {
    if (add)
        performCrudRequest("add-user-to-role", { Id: cellInfo.data.Id, RoleName: cellInfo.column.caption, UserName: cellInfo.data.UserName });
    else
        performCrudRequest("remove-user-from-role", { RoleName: cellInfo.column.caption, UserName: cellInfo.data.UserName });
}

function arrayObjectIndexOf(myArray, searchTerm, property) {
    for (var i = 0, len = myArray.length; i < len; i++) {
        if (myArray[i][property] === searchTerm) return i;
    }
    return -1;
}

var get_users_roles_matrix = function () {
    var gridDataSource = [];
    $.ajax({
        type: 'GET',
        async: false,
        url: $("#usersRolesGridContainer").attr('data-get-source'),
        success: function (data) {
            var length = matrixGridOptions.columns.length;
            for (var i = length - 1; i > -1; i--) {
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
                for (var j = 0; j < data.Roles.length; j++) {
                    var dataField = "";
                    var dataFieldParts = data.Roles[j].split(' ');
                    for (var k = 0; k < dataFieldParts.length; k++) {
                        dataField += dataFieldParts[k];
                    }
                    userManagement[dataField] = currentData.UserInRoles[j];
                }
                gridDataSource.push(userManagement);
            }
            for (var i = 0; i < data.Roles.length; i++) {
                var dataField = "";
                var dataFieldParts = data.Roles[i].split(' ');
                for (var j = 0; j < dataFieldParts.length; j++) {
                    dataField += dataFieldParts[j];
                }
                var column = {
                    dataField: dataField,
                    caption: data.Roles[i],
                    cellTemplate: function (container, options) {
                        $('<div/>').dxCheckBox({
                            value: options.value,
                            onValueChanged: function (data) {
                                UpdateUserRole(data.value, options);
                                load_users_roles_grid();
                                load_roles_grid();
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
    noDataText: 'No roles were added',
    wordWrapEnabled: true,
    height: 500
}

var load_users_roles_grid = function () {
    var gridDataSource = get_users_roles_matrix();
    grid = $("#usersRolesGridContainer").dxDataGrid(matrixGridOptions).dxDataGrid("instance");
    grid.option('dataSource', gridDataSource);
}

var load_inner_grid = function () {
    //managementData = GetManagementData();

    $('#saveBtn').dxButton({
        text: 'Save',
        onClick: function (info) {
            if (popupType == "Add new user") {
                var userInfo = get_user_info();

                performCrudRequest('add-user', {
                    UserName: userInfo.userName,
                    Email: userInfo.email,
                    Password: userInfo.pass,
                    AssociatedRoles: userInfo.roles.join(', ')
                });
                for (i = 0; i < userInfo.roles.length; i++) {
                    performCrudRequest("add-user-to-role", { Id: 0, RoleName: userInfo.roles[i], UserName: userInfo.userName });
                }
            }
            else if (popupType == "Add new role") {
                var roleInfo = get_role_info();
                var Permissions = [];
                for (var i = 0; i < roleInfo.Permissions.length; i++) {
                    var perm = roleInfo.Permissions[i];
                    for (var j = 0; j < perm.items.length; j++) {
                        Permissions.push({ Controller: perm.key, Action: perm.items[j] })
                    }
                }
                performCrudRequest('add-role', { role: JSON.stringify({
                    Id: 0, Name: roleInfo.RoleName, Permissions: Permissions
                })
                });

                for (i = 0; i < roleInfo.AssociatedUsers.length; i++) {
                    performCrudRequest("add-user-to-role", { Id: 0, RoleName: roleInfo.RoleName, UserName: roleInfo.AssociatedUsers[i] });
                }
            }
            else if (popupType == "Edit user") {
                var userInfo = get_user_info();

                performCrudRequest('update-user', {
                    Id: currentUser.Id,
                    UserName: userInfo.userName,
                    Email: userInfo.email,
                    Password: userInfo.pass
                });
            }
            else if (popupType == "Edit role") {
                var roleInfo = get_role_info();
                var Permissions = [];
                for (var i = 0; i < roleInfo.Permissions.length; i++) {
                    var perm = roleInfo.Permissions[i];
                    for (var j = 0; j < perm.items.length; j++) {
                        Permissions.push({ Controller: perm.key, Action: perm.items[j] })
                    }
                }

                performCrudRequest('update-role', { role: JSON.stringify({
                    Id: currentRole.Id, Name: roleInfo.RoleName, Permissions: Permissions
                })
                });
            }
            else if (popupType == "Add new rule") {
                var ruleInfo = get_rule_info();
                var Permissions = [];
                for (var i = 0; i < ruleInfo.Permissions.length; i++) {
                    var perm = ruleInfo.Permissions[i];
                    for (var j = 0; j < perm.items.length; j++) {
                        Permissions.push({ Controller: perm.key, Action: perm.items[j] })
                    }
                }
                performCrudRequest('add-rule', { rule: JSON.stringify({
                    Id: 0, Mask: ruleInfo.RuleMask, Permissions: Permissions
                })
                });
            }
            else if (popupType == "Edit rule") {
                var ruleInfo = get_rule_info();
                var Permissions = [];
                for (var i = 0; i < ruleInfo.Permissions.length; i++) {
                    var perm = ruleInfo.Permissions[i];
                    for (var j = 0; j < perm.items.length; j++) {
                        Permissions.push({ Controller: perm.key, Action: perm.items[j] })
                    }
                }

                performCrudRequest('update-rule', { rule: JSON.stringify({
                    Id: currentRule.Id, Mask: ruleInfo.RuleMask, Permissions: Permissions
                })
                });
            }
            popup.hide();
        },
        width: "200px",
        height: "40px",
        disabled: true
    });

    $('#cancelBtn').dxButton({
        text: 'Cancel',
        onClick: function (info) {
            popup.hide();
        },
        width: "200px",
        height: "40px"
    });
    if (popupType == "Add new user") {
        var form = $("#formInPopup").dxForm(user_form_options).dxForm("instance");
        form.itemOption("actions", "visible", false);
        form.itemOption("confirmed", "visible", false);
        form.itemOption("password", "visible", false);
        form.itemOption("roles", "visible", true);
    }
    else if (popupType == "Edit user") {
        var form = $("#formInPopup").dxForm(user_form_options).dxForm("instance");
        form.itemOption("actions", "visible", true);
        form.itemOption("confirmed", "visible", true);
        form.itemOption("password", "visible", true);
        form.itemOption("roles", "visible", false);
        load_user_info();
    }
    else if (popupType == "Add new role") {
        var form = $("#formInPopup").dxForm(role_form_options).dxForm("instance");
        form.itemOption("associatedUsers", "visible", true);
    }
    else if (popupType == "Edit role") {
        var form = $("#formInPopup").dxForm(role_form_options).dxForm("instance");
        form.itemOption("associatedUsers", "visible", false);
        $('#form-permissions').dxList("instance").option('height', 400);
        load_role_info();
    }
    else if (popupType == "Add new rule") {
        $("#formInPopup").dxForm(rule_form_options).dxForm("instance");
    }
    else if (popupType == "Edit rule") {
        $("#formInPopup").dxForm(rule_form_options).dxForm("instance");
        load_rule_info();
    }
}
var updatePage = function () {
    load_users_roles_grid();

    load_users_grid();

    load_roles_grid();

    load_rules_grid();

    get_additional_data();
}
$(document).ready(function () {
    updatePage();
    $("#addNewUser").dxButton({
        text: 'Add new user',
        onClick: function (info) {
            popupType = "Add new user";
            showPopupForm();
        }
    })

    $("#addNewRole").dxButton({
        text: 'Add new role',
        onClick: function (info) {
            popupType = "Add new role";
            showPopupForm();
        }
    })

    $("#addNewRule").dxButton({
        text: 'Add new rule',
        onClick: function (info) {
            popupType = "Add new rule";
            showPopupForm();
        }
    })

    popupOptions = {
        width: 500,
        height: 700,
        contentTemplate: $('#form_template'),
        onShown: function () {
            load_inner_grid();
        },
        onHidden: function () {
            updatePage();
        },
        showTitle: true,
        title: '',
        visible: false,
        dragEnabled: false,
        closeOnOutsideClick: true
    };
});