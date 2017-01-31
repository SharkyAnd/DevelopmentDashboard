var currentRule;

var load_rules_grid = function () {
    $.ajax({
        type: 'GET',
        async: false,
        url: $("#rulesGridContainer").attr('data-get-source'),
        success: function (data) {
            $('#rulesGridContainer').dxDataGrid({
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
                    { dataField: "Mask", allowGrouping: false, allowEditing: true },
                    { dataField: "Permissions", allowGrouping: false, allowEditing: true,
                        cellTemplate: function (cellElement, cellInfo) {
                            var permissionsString = cellInfo.value[0].Controller + '-' + cellInfo.value[0].Action;
                            for (var i = 1; i < cellInfo.value.length; i++) {
                                permissionsString += ', ' + cellInfo.value[i].Controller + '-' + cellInfo.value[i].Action;
                            }
                            $('<div/>').html(permissionsString).appendTo(cellElement);
                        }
                    },
                    {
                        width: 100,
                        alignment: 'center',
                        cellTemplate: function (container, options) {
                            $('<a/>').addClass('dx-link')
                            .text('Edit')
                            .on('dxclick', function () {
                                popupType = "Edit rule";
                                currentRule = options.data;
                                showPopupForm();
                            })
                            .appendTo(container);
                        }
                    }
                ],
                onRowRemoving: function (info) {
                    dataGrid = $("#rulesGridContainer").dxDataGrid("instance");
                    if (isShowDialog) {
                        var result = DevExpress.ui.dialog.confirm("Are you sure you want to delete this rule?", "Confirm delete");
                        info.cancel = true;

                        result.done(function (dialogResult) {
                            isShowDialog = false;
                            dialogResult && dataGrid.deleteRow(dataGrid.getRowIndexByKey(info.key));
                            isShowDialog = true;
                            dialogResult && performCrudRequest("remove-rule", info.key.Id);
                            updatePage();
                        });

                    }
                },
                wordWrapEnabled: true
            });
        }
    });
}

var rule_form_options = {
    formData: [],
    labelLocation: 'top',
    items: [
            {
                itemType: 'simple',
                label: {
                    text: 'Rule Mask'
                },
                editorType: 'dxTextBox',
                template: function (container, options) {
                    return $('<div id="form-rulemask"/>').dxTextBox({
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
                    return $('<div id="form-rule-permissions"/>').dxList({
                        dataSource: permissions,
                        showSelectionControls: true,
                        height: 400,
                        selectionMode: "multi",
                        grouped: true,
                        collapsibleGroups: false
                    });
                },
                name: 'permissions'
            }
            ]
}

var get_rule_info = function () {
    var ruleInfo = {
        RuleMask: $('#form-rulemask').dxTextBox("instance").option("value"),
        Permissions: $("#form-rule-permissions").dxList("instance").option("selectedItems")
    }
    return ruleInfo;
}

var load_rule_info = function () {
    $('#form-rulemask').dxTextBox("instance").option("value", currentRule.Mask)
    var form_permissions = $("#form-rule-permissions").dxList("instance")
    for (var i = 0; i < form_permissions.option("items").length; i++) {
        var group = form_permissions.option("items")[i];

        for (var j = 0; j < group.items.length; j++) {
            for (var k = 0; k < currentRule.Permissions.length; k++) {
                var permission = currentRule.Permissions[k];
                if (permission.Controller == group.key && permission.Action == group.items[j]) {
                    form_permissions.selectItem({ group: i, item: j });
                    break;
                }
            }
        }
    }
}