var resizeFun = function () {
    // 不支持 transform, 自己计算 left
    var winWidth = $(window).width();
    var lp = ((winWidth - 1900) / 2 / winWidth) * 100;
    $('div#banner #carouselpannel').css('left', lp + "%");
};
if (!document.addEventListener) { // ie8 or lower
    $(resizeFun);
    $(window).resize(resizeFun);
}

$(function () {
    $('[data-toggle="popover"]').popover({ trigger: 'hover', html: true });

    //发标公告跑马灯效果
    $('.data-box').marquee({
        delayBeforeStart: -26000,
        duration: 30000,
        duplicated: true
    });

    //flowplayer播放器播放结束设置封面
    setTimeout(function () {
        var p = flowplayer();
        p.on("finish", function () {
            p.stop();
        });
    }, 2500);

    //借款项目展示、公告与动态、媒体报道内容部分hover事件
    var selectA = $(".memos ul li a");
    selectA.hover(function () {
        $(this).siblings("i").removeClass("bid-grey");
        $(this).siblings("i").addClass("bid-blue");
    }).mouseleave(function () {
        $(this).siblings("i").removeClass("bid-blue");
        $(this).siblings("i").addClass("bid-grey");
    });

    //借款项目展示、公告与动态、媒体报道hover事件
    var loan = $("#loan");
    var dynamic = $("#dynamic");
    var report = $("#report");
    var content1 = $("div.content1");
    var content2 = $("div.content2");
    var content3 = $("div.content3");
    var line = $("img#active-line");
    content2.hide();
    content3.hide();
    dynamic.on("mouseenter", function () {
        content2.hide();
        content3.hide();
        content1.show();
        line.removeClass();
        line.addClass("line-left");
    });
    loan.on("mouseenter", function () {
        content1.hide();
        content3.hide();
        content2.show();
        line.removeClass();
        line.addClass("line-center");
    });
    report.on("mouseenter", function () {
        content1.hide();
        content2.hide();
        content3.show();
        line.removeClass();
        line.addClass("line-right");
    });

});