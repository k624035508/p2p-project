import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/index.less";
import "../less/footer.less";
import "bootstrap/js/transition.js"

import header from "./header.js"
window['jQuery'] = $;
window['$'] = $;
import "./jquery.superslide.2.1.1.js"


//赏金接力弹窗
function openRelayLayer(){
    var nowdate="20160311";
    var relayck="88bankrelay"+nowdate;
    if(getCookie("88bankrelay")!=relayck&&nowdate<"20160311"){
        $("body").css("overflow","hidden");
        $(".m-indexlayer").show();
        clearCookie("88bankrelay");
        setCookie("88bankrelay",relayck,1);
    }
}
function closeRelayLayer(){
    $("body").css("overflow","auto");
    $(".m-indexlayer").hide();
}

$(function () {
    header.setHeaderHighlight(0);
   //计算器初始化
    $('[data-toggle="popover"]').popover();

$(function(){  
   
    var prog=$("div.progress-bar");       
    var changdu1=prog.parent().width();
    var changdu2=parseInt(prog.eq(0).html())/100;     
    prog.eq(0).animate({width:changdu2*changdu1},500,"linear");    
   
        $(window).stop(true).delay(500).scroll(function(){
           
            var windowtop = $(window).scrollTop();
            if(windowtop>180 && windowtop<1000){        
   
                for(var i=1;i<5;i++){
                    var changdu2=parseInt(prog.eq(i).html())/100;              
                    prog.eq(i).stop(true).animate({width:changdu2*changdu1},500,"linear");        
                }      

            }
                
            if(windowtop>550){
    
                for(var i=5;i<(prog.length);i++){
                    var changdu2=parseInt(prog.eq(i).html())/100;              
                    prog.eq(i).stop(true).animate({width:changdu2*changdu1},500,"linear");        
                }
            }
         
            
        });
       
       
});

    //1.焦点图轮换
    $(".m-banner").slide({titCell:".hd li",mainCell:".bd ul",effect:"fold",autoPlay:true, trigger:"mouseover" });

    $('#nav_home').addClass('on');
    $(".slideTxtBox").slide()
    /*$("img.lazy").lazyload({
        effect:'fadeIn'
    });*/
    //openRelayLayer();
});
