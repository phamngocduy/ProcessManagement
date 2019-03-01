﻿$(function () {
    //var lang = CapstoneProject.Cookies.getCookie("LangFOrLangForProcessManagementSystem");
    //$('.setLang[data-lang="' + lang + '"]').addClass('active-lang');
    var pathHaveLang = window.location.pathname;

    var path = pathHaveLang.replace("/vi", "").replace("/en", "");
    console.log(path);
    $("#langen").attr("href", `/en${path}`);
    $("#langvi").attr("href", `/vi${path}`);


    //theme
    setTheme();
    var themeOptions = $("ul.choose-skin>li");
    themeOptions.each(function (i, option) {
        let colorData = option.getAttribute("data-theme");
        let color = getCookie('colortheme');
        if (colorData == color) {
            option.classList.add("active")
            return false;
        }
    });
    $('.choose-skin li').on('click', function () {
        let color = $(this).attr('data-theme');
        setCookie('colortheme', color, 7);
        $('body').removeClass();
        setTheme();
    });

    //set up active menu
    var menuItem = $(".main-menu.metismenu li>a");
    menuItem.each(function () {
        var href = $(this).attr("href");
        if (pathHaveLang == href) {
            $(this).parent('li').addClass('active');
            var parent = $(this).parents('ul');
            if (parent.hasClass("collapse")) {
                parent.addClass("in");
                parent.parent("li").addClass("active");
            }
        }
    });
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
function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    var cookiee = cname + "=" + escape(cvalue) + ";" + expires + ";path=/";
    document.cookie = cookiee;
}
function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
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
function deleteCookie(name) {
    document.cookie = name + '=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}
function setTheme() {
    let theme = "theme-";
    let defaultColor = "blue";
    let color = getCookie('colortheme');
    color = color == "" ? defaultColor : color;

    switch (color) {
        case 'blue':
            theme += 'blue';
            break;
        case 'purple':
            theme += 'purple';
            break;
        case 'cyan':
            theme += 'cyan';
            break;
        case 'green':
            theme += 'green';
            break;
        case 'orange':
            theme += 'orange';
            break;
        case 'blush':
            theme += 'blush';
            break;

        default:
            theme += defaultColor;
    }
    $('body').removeClass().addClass(theme);
}