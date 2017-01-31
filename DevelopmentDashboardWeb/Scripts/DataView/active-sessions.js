var dataGrid = null;
$(function () {

    $.ajax({
        type: 'GET',
        url: $("#gridContainer").attr('data-get-parameters'),
        success: function (data) {
            setInterval(function () {
                fillGrid();
                $('#last-update').html(moment(new Date()).format("DD.MM.YYYY HH:mm:ss"));
            }, data)
        }
    });

    $('#last-update').html(moment(new Date()).format("DD.MM.YYYY HH:mm:ss"));
    $.connection.hub.start();
    // Declare a proxy to reference the hub. 
    var hub = $.connection.activeSessionsHub;
    // Create a function that the hub can call to broadcast messages.
    hub.client.update = function () {
        fillGrid();
        $('#last-update').html(moment(new Date()).format("DD.MM.YYYY HH:mm:ss"));
    };

    fillGrid();
    function fillGrid() {
        $.ajax({
            type: 'GET',
            url: $("#gridContainer").attr('data-get-source'),
            success: function (data) {
                var newResult = $.map(data, function (item, index) {
                    var date = new Date(item['SessionBegin']);
                    var newDate = date.toUTCString();
                    item['SessionBegin'] = new Date(date);
                    item['SessionEnd'] = item['SessionEnd'] == 0 ? '' : new Date(item['SessionEnd']);
                    return item;
                });
                dataGrid = $('#gridContainer').dxDataGrid(sessionGridOptions).dxDataGrid("instance");
                dataGrid.option("dataSource", data)
                dataGrid.addColumn("UserActivity");
            }
        });
    }

    popupOptions = {
        width: 1200,
        height: 700,
        contentTemplate: $('#content_template'),
        onShown: function () {
            $("#chartInPopup").dxChart({
                dataSource: session,
                commonSeriesSettings: {
                    argumentField: 'ChunkPoint',
                    valueField: 'Position',
                    point: {
                        visible: false
                    }
                },
                argumentAxis: {
                    argumentType: 'datetime'
                },
                seriesTemplate: {
                    nameField: "SessionId"
                },
                legend: {
                    visible: true
                }
            });
        },
        showTitle: true,
        title: "Sessions Activity Profiles",
        visible: false,
        dragEnabled: false,
        closeOnOutsideClick: true
    };
});