function deleteCookie(name) {
    document.cookie = name + '=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}
function setCookie(name, value, exday) {
    deleteCookie(name);
    var d = new Date()
    d.setTime(d.getTime() + (exday * 24 * 60 * 60));
    var expires = "expires=" + d.toUTCString();
    console.log(expires);
    document.cookie = name + "=" + value + "; " + expires+"; path=/";
}