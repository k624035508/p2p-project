import "bootstrap-webpack!./bootstrap.config.js";

import "../less/head.less";
import "../less/footerSmall.less";
import "../less/projects.less";

import header from "./header.js"
window['$'] = $;

$(function(){
    header.setHeaderHighlight(1);

    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();

    //进度条动画
    var prog=$("div.progress-bar");       
    var changdu1=prog.parent().width();
    var changdu2=parseInt(prog.eq(0).html())/100;     
    for(var i=0;i<9;i++){
        var changdu2=parseInt(prog.eq(i).html())/100;              
        prog.eq(i).width(changdu2*changdu1);        
    }
   
    $(window).scroll(function(){
        var windowtop = $(window).scrollTop();     
        if(windowtop>100){
            for( var j=0;j<(prog.length);j++){
                var changdu2=parseInt(prog.eq(j).html())/100;              
                prog.eq(j).width(changdu2*changdu1);        
            }
        }           
    });

    //头部导航条
    var $consume=$(".consume ul li");
    var index=0;
    $consume.click(function(){
       index=$consume.index(this);
        $consume.eq(index).addClass("bluecon").siblings().removeClass("bluecon");   
        $(".fincategory>div").eq(index).removeClass("hidden").siblings().addClass("hidden");
    });
}); 


$(window).load(function(){
    var thisurl=document.location.href;
    var thisnameindex=thisurl.indexOf("#")+1;
    var thisname=thisurl.substring(thisnameindex);
    var nameclass="."+thisname;
    var linameclass="."+thisname+"-li";
    $(nameclass).removeClass("hidden").siblings().addClass("hidden");
    $(linameclass).addClass("bluecon").siblings().removeClass("bluecon");  
});
