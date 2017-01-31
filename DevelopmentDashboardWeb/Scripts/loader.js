var Loader = {
    init: function (source) {
        $('#LoadPanel').dxLoadPanel({
            position: { of: source },
            visible: false,
            showIndicator: true,
            showPane: true,
            closeOnOutsideClick: false
        });
    },
    show: function () {
        $('#LoadPanel').dxLoadPanel("instance").show();
    },
    hide: function () {
        $('#LoadPanel').dxLoadPanel("instance").hide();
    }
}