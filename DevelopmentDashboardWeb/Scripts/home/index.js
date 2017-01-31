var types = ['users', 'last-10-sessions', 'last-10-commits']
$(document).ready(function () {
    $('#last-update').html(moment(new Date()).format("DD.MM.YYYY HH:mm:ss"));
    for (var i = 0; i < types.length; i++) {
        switchButton($('#' + types[i]), types[i])
    }
    $.ajax({
        type: 'GET',
        url: $("#buttons-panel").attr('data-get-parameters'),
        success: function (data) {
            setInterval(function () {
                $('#last-update').html(moment(new Date()).format("DD.MM.YYYY HH:mm:ss"));
                for (var i = 0; i < types.length; i++) {
                    var widget = $('#' + types[i] + '-grid');
                    getWidgetData(types[i], widget);
                }
            }, data)
        }
    });
    popupOptions = {
        width: 300,
        height: 300,
        contentTemplate: $('#content_template'),
        onShown: function () {
            $.ajax({
                type: 'GET',
                url: $("#users").attr('data-get-parameters'),
                success: function (data) {
                    $('#yellow-status-spin').dxNumberBox({
                        value: data.yellowStatusValue,
                        showSpinButtons: true
                    })
                    $('#red-status-spin').dxNumberBox({
                        value: data.redStatusValue,
                        showSpinButtons: true
                    })
                    $('#save').dxButton({
                        text: 'Save',
                        onClick: function (data) {
                            yellowStatusVal = $('#yellow-status-spin').dxNumberBox("instance").option("value");
                            redStatusVal = $('#red-status-spin').dxNumberBox("instance").option("value");

                            $.ajax({
                                type: 'POST',
                                url: $('#users').attr('data-save-options'),
                                data: { yellowStatusVal: yellowStatusVal, redStatusVal: redStatusVal },
                                async: false,
                                success: function (data) {
                                    popup.hide();
                                    getWidgetData('users', $('#users-grid'))
                                }
                            });
                        }
                    })
                }
            });
        },
        showTitle: true,
        title: "Options",
        visible: false,
        dragEnabled: false,
        closeOnOutsideClick: true
    };
    $('.btn').click(function () {
        var type = $(this).attr('id');
        switchButton($(this), type);
    })
})

var switchButton = function (btn, type) {
    if (btn.hasClass('btn-default')) {
        btn.removeClass('btn-default');
        btn.addClass('btn-success');
        loadWidget(type);
    }
    else if (btn.hasClass('btn-success')) {
        btn.removeClass('btn-success');
        btn.addClass('btn-default');
        $('#' + type + '-widget').lobiPanel('close');
    }
    var options = {
        sortable: true,
        resize: 'both',
        editTitle: false,
        expand: false,
        reload: false,
        tooltips: true,
        minHeight: 480
    }
    if (type == "users")
        options["reload"] = { icon: 'glyphicon glyphicon-cog',
            tooltip: 'Options'
        }
    $('#lobipanel-multiple').find('.panel').lobiPanel(options);

    $('#lobipanel-multiple').find('.panel').on('beforeClose.lobiPanel', function (ev, lobiPanel) {
        var btnId = $(this).attr("data-button");
        $('#' + btnId).removeClass('btn-success');
        $('#' + btnId).addClass('btn-default');
    });

    $('#' + type + '-widget a[data-title="Options"]').unbind()
    .click(function () {
        popup && $(".popup").remove();
        $popupContainer = $("<div />")
                            .addClass("popup")
                            .appendTo($("#popup"));
        popup = $popupContainer.dxPopup(popupOptions).dxPopup("instance");
        popup.show();
    });

    $('#lobipanel-multiple').find('.panel').on('beforeFullScreen.lobiPanel', function (ev, lobiPanel) {
        var type = $(this).attr("data-button");
        openWidgetSettings(type);
        return false;
    });
}
var popup = null;
var popupOptions = null

var popover = $("#popover1").dxPopup({
    target: '#users-widget a[data-title="Options"]',
    position: "bottom",
    width: 300,
    contentTemplate: $('#content_template'),
    onShown: function () {

    }
}).dxPopover("instance");

var loadWidget = function (type) {
    switch (type) {
        case "users":
            title = "Users";
            break;
        case "last-10-sessions":
            title = "Last 10 sessions";
            break;
        case "last-10-commits":
            title = "Last 10 commits";
            break;
        default:
            break;
    }

    var panel = $('<div data-button="' + type + '" id="' + type + '-widget" class="panel panel-default"/>').append($('<div class="panel-heading"></div>').append($('<h3 class="panel-title">' + title + '</h3>')))

    var widget = getWidget(type);

    panel.append($('<div class="panel-body">').append(widget))

    if (type == "users")
        $('#left-sortable').append(panel);
    else
        $('#right-sortable').append(panel);
}

var getWidget = function (type) {

    var grid = $('<div id="' + type + '-grid">').dxDataGrid({
        dataSource: [{}],
        showColumnLines: true,
        showRowLines: true,
        allowColumnResizing: true,
        //columnAutoWidth: true,
        allowColumnReordering: true,
        wordWrapEnabled: true,
        cacheEnabled: false,
        columns: [{}],
        width: '100%'
    })

    getWidgetData(type, grid);
    return grid;
}
var getWidgetData = function (type, grid) {
    var gridInstance = grid.dxDataGrid("instance");
    switch (type) {
        case "users":
            url = $('#users').attr('data-get-widget');
            columns = [
            { dataField: "UserName" },
            {
                dataField: "LastInputTime",
                dataType: "date",
                format: 'dd.MM.yyyy HH:mm:ss'
            },
            { dataField: "DayActiveHours", dataType: "number", precision: 4 },
            { dataField: "WeekActiveHours", dataType: "number", precision: 4 },
            { dataField: "WeekCommitsCount" }
            ];
            break;
        case "last-10-sessions":
            url = $('#last-10-sessions').attr('data-get-widget');
            columns = [
            { dataField: "Id" },
            { dataField: "UserName" },
            {
                dataField: "SessionBegin",
                dataType: "date",
                format: 'dd.MM.yyyy HH:mm:ss'
            },
            {
                dataField: "SessionEnd",
                dataType: "date",
                format: 'dd.MM.yyyy HH:mm:ss'
            },
            { dataField: "ActiveHours", dataType: "number", precision: 4 },
            { dataField: "SessionState" }
            ]
            break;
        case "last-10-commits":
            url = $('#last-10-commits').attr('data-get-widget');
            columns = [
            {
                dataField: "Moment",
                dataType: "date",
                format: 'dd.MM.yyyy HH:mm:ss'
            },
            { dataField: "UserName" },
            { dataField: "RevisionNumber" },
            { dataField: "RevisionMessage" },
            ];
            break;
        default:
            break;
    }

    $.ajax({
        type: 'GET',
        async: false,
        url: url,
        cache: false,
        success: function (data) {
            $.map(data, function (item, index) {

                item['LastInputTime'] = item['LastInputTime'] == 0 ? '' : moment(item['LastInputTime']);

                item['SessionBegin'] = item['SessionBegin'] == 0 ? '' : moment(item['SessionBegin']);

                item['SessionEnd'] = item['SessionEnd'] == 0 ? '' : moment(item['SessionEnd']);

                item['Moment'] = item['Moment'] == 0 ? '' : moment(item['Moment']);
                return item;
            });

            gridInstance.option('dataSource', data);
            gridInstance.option('columns', columns);
            if (type == "users" || type == "last-10-sessions") {
                gridInstance.on('rowPrepared', usersGridOnRowPrepared)
            }
        }
    });
}
var usersGridOnRowPrepared = function (rowInfo) {
    if (rowInfo.data) {
        if (rowInfo.data.Status == "green")
            rowInfo.rowElement.css('background', '#86CD82');
        else if (rowInfo.data.Status == "yellow")
            rowInfo.rowElement.css('background', '#FFE066');
        else if (rowInfo.data.Status == "red")
            rowInfo.rowElement.css('background', '#F25F5C');
    }
}