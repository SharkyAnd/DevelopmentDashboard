$(document).ready(function () {
    $.ajax({
        type: 'GET',
        url: $("#gridContainer").attr('data-get-source'),
        success: function (data) {
            var newResult = $.map(data, function (item, index) {
                item['SessionBegin'] = moment(item['SessionBegin']);
                item['SessionEnd'] = item['SessionEnd'] == 0 ? '' : moment(item['SessionEnd']);
                return item;
            });
            dataGrid = $('#gridContainer').dxDataGrid(sessionGridOptions).dxDataGrid("instance").option("dataSource", data);
        }
    });

    popupOptions = {
        width: 1200,
        height: 700,
        contentTemplate: $('#content_template'),
        onShown: function () {
            $("#chartInPopup").dxChart({
                dataSource: session,
                commonSeriesSettings: {
                    argumentField: 'ChunkPoint',
                    valueField: 'UserName',
                    point: {
                        visible: false
                    }
                },
                argumentAxis: {
                    argumentType: 'datetime',
                    format: "HH:mm"
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

