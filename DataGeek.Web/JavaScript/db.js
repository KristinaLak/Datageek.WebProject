var is_chrome = navigator.userAgent.toLowerCase().indexOf('chrome') > -1;
var is_ie = navigator.userAgent.toLowerCase().indexOf('ie') > -1;
var is_firefox = navigator.userAgent.toLowerCase().indexOf('firefox') > -1;
var is_netscape = navigator.userAgent.toLowerCase().indexOf('netscape') > -1;

if (is_netscape) {
    alert('Warning: Your browser is not fully supported. Some features on this website may not work. Please change your browser to Google Chrome!');
}
function IE9Err() {
    alert('Please wait for the page to fully load before performing this action. If you are using Internet Explorer, please turn compatibility mode on to perform this action.');
}
function grab(id) {
    return document.getElementById(id);
}
String.prototype.trim = function() {
    return this.replace(/^\s+|\s+$/g, "");
};
function commaSeparateString(nStr) {
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}
function DisableEnterKey(e) {
    var key;
    if (window.event)
        key = window.event.keyCode; //IE
    else
        key = e.which; //firefox      
    return (key != 13);
}
function AlertifySuccess(message, position) {
    alertify.set('notifier', 'position', position);
    alertify.success(message);
    return false;
}
function Alertify(message, title) {
    alertify.alert(title, message).set('modal', false);
    return false;
}
function AlertifySized(message, title, width, height) {
    alertify.alert(title, message).set('resizable', true).set('modal', false).resizeTo(width, height);
    return false;
}
function AlertifyConfirm(message, title, ok_button_cl_id, use_conf_msgs) {
    alertify.confirm(title, message,
      function () {
          if (use_conf_msgs)
            alertify.success('Ok');
          grab(ok_button_cl_id).click();
      },
      function () {
          if (use_conf_msgs)
            alertify.error('Cancel');
      }).set('labels', { ok: 'Sure', cancel: 'Nope' });
    return false;
}
function getOffset(el) {
    var _x = 0;
    var _y = 0;
    while (el && !isNaN(el.offsetLeft) && !isNaN(el.offsetTop)) {
        _x += el.offsetLeft - el.scrollLeft;
        _y += el.offsetTop - el.scrollTop;
        el = el.offsetParent;
    }
    return { top: _y, left: _x };
}
function isInt(value) {
    return !isNaN(value) &&
           parseInt(Number(value)) == value &&
           !isNaN(parseInt(value, 10));
}
function HtmlEncode(value) {
    // Create a in-memory div, set its inner text (which jQuery automatically encodes)
    // Then grab the encoded contents back out. The div never exists on the page.
    return $('<div/>').text(value).html();
}
function HtmlDecode(value) {
    return $('<div/>').html(value).text();
}
function CopyToClipboard(text) {
    var ta = document.createElement("textarea");
    ta.style.position = 'fixed';
    ta.value = text;
    document.body.appendChild(ta);
    ta.select();
    document.execCommand('copy');
    document.body.removeChild(ta);
    return false;
}