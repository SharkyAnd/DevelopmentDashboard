var currentUser;

var load_users_grid = function () {
    $.ajax({
        type: 'GET',
        async: false,
        url: $("#usersGridContainer").attr('data-get-source'),
        success: function (data) {

            $.map(data, function (item, index) {
                item['LastVisitDate'] = item['LastVisitDate'] == 0 ? '' : moment(item['LastVisitDate']);
                item['AddedDate'] = item['AddedDate'] == 0 ? '' : moment(item['AddedDate']);
                return item;
            });
            $('#usersGridContainer').dxDataGrid({
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
                    { dataField: "UserName", allowGrouping: false, allowEditing: true },
                    { dataField: "Email", allowGrouping: false, allowEditing: true },
                    { dataField: "Confirmed", caption: "Account Confirmed", allowGrouping: false, allowEditing: false,
                        cellTemplate: function (container, options) {
                            $('<div/>').dxCheckBox({
                                value: options.value,
                                disabled: true
                            }).appendTo(container);
                        }
                    },
                    { dataField: "LastVisitDate", allowGrouping: false, allowEditing: false, dataType: "date", format: 'dd.MM.yyyy HH:mm:ss' },
                    { dataField: "AddedDate", allowGrouping: false, allowEditing: false, dataType: "date", format: 'dd.MM.yyyy HH:mm:ss' },
                    {
                        width: 100,
                        alignment: 'center',
                        cellTemplate: function (container, options) {
                            $('<a/>').addClass('dx-link')
                            .text('Edit')
                            .on('dxclick', function () {
                                popupType = "Edit user";
                                currentUser = options.data;
                                showPopupForm();
                            })
                            .appendTo(container);
                        }
                    }
                ],
                onRowRemoving: function (info) {
                    dataGrid = $("#usersGridContainer").dxDataGrid("instance");
                    if (isShowDialog) {
                        var result = DevExpress.ui.dialog.confirm("Are you sure you want to delete account with User Name '" + info.data.UserName + "'? Deleting this user will also delete his membership to the roles", "Confirm delete");
                        info.cancel = true;

                        result.done(function (dialogResult) {
                            isShowDialog = false;
                            dialogResult && dataGrid.deleteRow(dataGrid.getRowIndexByKey(info.key));
                            isShowDialog = true;
                            dialogResult && performCrudRequest("delete-user", info.key.Id);
                            updatePage();
                        });

                    }
                },
                noDataText: 'Add users using the "Add new user" button',
                wordWrapEnabled: true
            });
        }
    });
}

var user_form_options = {
    formData: [],
    labelLocation: 'top',
    items: [
            {
                itemType: 'simple',
                label: {
                    text: 'User name'
                },
                editorType: 'dxTextBox',
                template: function (container, options) {
                    return $('<div id="form-username"/>').dxTextBox({
                        value: ''
                    })
                },
                name: 'userName'
            },
            {
                itemType: 'simple',
                label: {
                    text: 'Email'
                },
                template: function (container, options) {
                    return $('<div id="form-email"/>').dxTextBox({
                        value: ''
                    }).dxValidator({ validationRules: [
                            { type: 'email' },
                            { type: 'required', text: 'Email is required' }
                        ],
                        onValidated: function (data) {
                            if (data.isValid) {
                                $('#saveBtn').dxButton("instance").option("disabled", false);
                                if (popupType == "Edit user" && currentUser.Confirmed)
                                    return;

                                $('#send-email').dxButton("instance").option("disabled", false);
                            }
                            else {
                                $('#saveBtn').dxButton("instance").option("disabled", true);
                                //$('#send-email').dxButton("instance").option("disabled", true);
                            }
                            return data.isValid;
                        }
                    });
                },
                editorType: 'dxTextBox',
                name: 'email'
            },
            {
                itemType: 'simple',
                editorType: 'dxTextBox',
                label: {
                    text: 'Password'
                },
                template: function (container, options) {
                    return $('<div id="form-password"/>').dxTextBox({
                        value: '',
                        mode: "password",
                        placeholder: "Enter password"
                    });
                },
                name: 'password'
            },
            {
                itemType: 'simple',
                label: {
                    text: 'Associated Roles'
                },
                template: function (container, options) {
                    return $('<div id="form-roles"/>').dxList({
                        dataSource: roles,
                        showSelectionControls: true,
                        selectionMode: "multi",
                        height: "80px"
                    });
                },
                name: 'roles',
                visible: true
            }, {
                itemType: 'simple',
                label: {
                    text: 'Account confirmed',
                    location: 'left'
                },
                template: function (container, options) {
                    return $('<div id = "form-email-confirmed"/>').dxCheckBox({
                        disabled: true,
                        value: false
                    });
                },
                name: 'confirmed',
                visible: false
            }, {
                itemType: "group",
                caption: "Additional actions",
                items: [{
                    template: function (container, options) {
                        return $('<div id="send-email"/>').dxButton({
                            text: 'Send confirmation email',
                            disabled: true,
                            onClick: function () {
                                performCrudRequest('send-confirmation', get_user_info());
                                $('#send-email').dxButton("instance").option("disabled", true);
                                $('#send-email').dxButton("instance").option("text", "Email was sended");
                            }
                        });
                    }
                }],
                name: 'actions',
                visible: false
            }
        ]
}

var get_user_info = function () {
    var userInfo = {
        userName: $("#form-username").dxTextBox("instance").option("value"),
        email: $("#form-email").dxTextBox("instance").option("value")
    }
    if (popupType == "Add new user") {
        userInfo["roles"] = $("#form-roles").dxList("instance").option("selectedItems")
        userInfo["pass"] = "tempPass";
    }
    else
        userInfo["pass"] = $("#form-password").dxTextBox("instance").option("value");
    return userInfo;
}

var load_user_info = function () {
    $("#form-username").dxTextBox("instance").option("value", currentUser.UserName);
    $("#form-email").dxTextBox("instance").option("value", currentUser.Email);
    $("#form-email-confirmed").dxCheckBox("instance").option("value", currentUser.Confirmed);
}