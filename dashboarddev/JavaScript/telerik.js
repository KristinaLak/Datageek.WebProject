function BasicRadConfirm(sender, args) {
    args.set_cancel(!window.confirm("Are you sure?"));
}
function CloseRadWindow() {
    GetRadWindow().close();
    return false;
}
function CenterRadWindow(radWindow) {
    radWindow.center();
}
function CenterRadWindow(sender, args) {
    sender.center();
}
function GetRadWindow() {
    var w = null;
    if (window.radWindow) w = window.radWindow;
    else if (window.frameElement.radWindow) w = window.frameElement.radWindow;
    return w;
}
var maxWidth = 950;
function ResizeRadToolTip(active) {
    setTimeout(function () {
        var width = active._tableElement.offsetWidth < maxWidth ? active._tableElement.offsetWidth : maxWidth;
        active.get_popupElement().style.width = width + "px";
        active.set_width(width);
        active._show();
    }, 0);
}
function ResizeRadWindow(sender, eventArgs) {
    setTimeout("GetRadWindow().autoSize();", 400);
}
function ResizeRadWindowQuick(sender, eventArgs) {
    GetRadWindow().autoSize();
}