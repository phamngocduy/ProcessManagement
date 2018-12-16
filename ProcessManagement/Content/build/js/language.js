$(document).ready(function () {
    //var lang = CapstoneProject.Cookies.getCookie("LangFOrLangForProcessManagementSystem");
    //$('.setLang[data-lang="' + lang + '"]').addClass('active-lang');
    $('.setLang').on('click', function () {
        var lang = $(this).attr("data-lang");

        deleteCookie("LangForProcessManagementSystem");
        setCookie("LangForProcessManagementSystem", lang, 7);
        location.reload(true);
    })

})