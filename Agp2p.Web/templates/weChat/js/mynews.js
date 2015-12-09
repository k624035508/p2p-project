import "bootstrap-webpack";
import "../less/mynews.less";
import "../less/footer.less";

import footerInit from "./footer.js";

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth / 20;
$("html").css("font-size", fontSizeUnit);
        
function deleteMessage(ids) {
    $.ajax({
        type: "POST",
        url: reqFilePath + "/AjaxDeleteMessages",
        data: JSON.stringify({ messageIds: ids}),
        contentType: "application/json",
        dataType: "json",
        success: function (msg) {
            console.log(msg.d);
        }
    }).fail(function () {
        alert("删除失败");
        location.reload();
    });
}

$(() => {
    footerInit();

    $("div.notice-cell").click(function() {
        var check = $(this).find(".select-icon:visible");
        if (check.length === 0)
            location.href = this.getAttribute("data-href");
        else
            check.toggleClass("blue");
    });
    function toggleHintAndBtn() {
        // hide delete btn if no msgs
        if ($("div.notice-cell:visible").length === 0) {
            $(".news-manage").hide();
            $("#no-content").show();
        } else {
            $(".news-manage").show();
            $("#no-content").hide();
        }
    }
    $("div.news-manage").click(function(){
        if(this.innerHTML === "管理消息"){
            // 切换按钮到删除功能
            $(this).html("删除选定消息");
            // 选择图标显示后的页面样式
            $("div.news-content").css("width", "15.6rem");
            $("span.notice-detail").css("width","100%");
            $("div.select-icon").css("display","inline-block");
        } else {
            var hiddenMsg = $("div.select-icon").filter(function(index, elem){
                return $(elem).hasClass("blue");
            }).parents(".notice-cell").hide();
            $(this).html("管理消息");
            $("div.select-icon").css("display","none");
            $("div.news-content").css("width", "18rem");
            var preDelIds = [].join.call(hiddenMsg.map(function(index, elem) {
                return elem.getAttribute("data-msgId");
            }), ";");
            if (preDelIds) {
                deleteMessage(preDelIds);
            }
            toggleHintAndBtn();
        }
    });

    var navBtns = $("div.nav-bar a");
    //$("#receiving").addClass("nav-active nav-border-active");
    navBtns.click(function(){
        navBtns.removeClass("nav-active nav-border-active");
        $(this).addClass("nav-active nav-border-active");
    });

    $('.type[data-status=' + messageStatus + ']').addClass('nav-active nav-border-active');

    toggleHintAndBtn();
});

