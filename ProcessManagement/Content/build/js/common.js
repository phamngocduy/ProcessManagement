$(document).ready(function(){
    // $('.cw-dropdown').on('shown.bs.dropdown',function(){
    //     $(this).find('button.dropdown-toggle').addClass('cw-active');
    // });
    // $('.cw-dropdown').on('hide.bs.dropdown',function(){
    //     $(this).find('button.dropdown-toggle').removeClass('cw-active');
    // });

    //dropdown
    $('.cw-dropdown').mouseenter(function(){
        // $('.cw-open').removeClass('cw-open');
        // $('.cw-active').removeClass('cw-active');

        // var parent = $(this).parent('.cw-dropdown');
        // var child = parent.find('.cw-dropdown-menu');
        var child = $(this).find('.cw-dropdown-menu');
        child.addClass('cw-open');
        $(this).addClass('cw-active');
    });
    $('.cw-dropdown').mouseleave(function(){
        // $('.cw-open').removeClass('cw-open');
        // $('.cw-active').removeClass('cw-active');

        // var parent = $(this).parent('.cw-dropdown');
        // var child = parent.find('.cw-dropdown-menu');

        var child = $(this).find('.cw-dropdown-menu');
        child.removeClass('cw-open');
        $(this).removeClass('cw-active');
    });
    $('.cw-input-has-icon').on('focus',function(){
        $(this).next('i').addClass('cw-active');
    });
    $('.cw-input-has-icon').on('blur',function(){
        if(!Boolean($(this).val())){
            $(this).next('i').removeClass('cw-active');
        }
    })
})
