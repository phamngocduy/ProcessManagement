$(document).ready(function () {
    //var lang = CapstoneProject.Cookies.getCookie("LangFOrLangForProcessManagementSystem");
    //$('.setLang[data-lang="' + lang + '"]').addClass('active-lang');
    var path = window.location.pathname;
    path = path.replace("/vi", "").replace("/en", "");
    console.log(path);
    $("#langen").attr("href", `/en${path}`);
    $("#langvi").attr("href", `/vi${path}`);

})
function getCurrentLang() {
    var path = window.location.pathname;
    var lang = path.substring(1, 3);
    if (lang == "vi") {
        return "vi";
    } else {
        return "en";
    }
}
