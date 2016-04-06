export default {
    setHeaderHighlight: function (index) {
        $("ul.in-header > li:nth(" + index + ") > a:first").addClass("nav-active");
        $("ul.in-header > li:nth(" + index + ")").addClass("li-active");
    }
};

$(function(){


    $("#financial ul li").click(function(){
        var indexed=$("#financial ul li").index(this);
        $(".consume ul li").eq(indexed).addClass("bluecon").siblings().removeClass("bluecon");   
        $(".fincategory>div").eq(indexed).removeClass("hidden").siblings().addClass("hidden");
    });
});


