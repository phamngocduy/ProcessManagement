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
function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}
$('.setLang').on('click', function () {
    var lang = $(this).attr("data-lang");

    deleteCookie("LangForProcessManagementSystem");
    setCookie("LangForProcessManagementSystem", lang, 7);
    location.reload(true);
})
