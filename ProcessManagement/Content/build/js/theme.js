$('.choose-skin li').on('click', function () {
    let color = $(this).attr('data-theme');
    setCookie('colortheme', color, 7);
    $('body').removeClass();

})
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
setTheme();