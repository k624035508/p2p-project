import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/index.less";
import "../less/footer.less";
import "bootstrap/js/transition.js"

import header from "./header.js"
window['jQuery'] = $;
window['$'] = $;
import "./jquery.superslide.2.1.1.js"

$(function () {
    header.setHeaderHighlight(0);
    //计算器初始化
    $('[data-toggle="popover"]').popover();
   
    $(".project-content").hover(function(){
        $(this).stop().animate({paddingLeft:"20px",paddingRight:"10px"},300);
    },function(){
        $(this).stop().animate({paddingLeft:"15px",paddingRight:"15px"},300);
    });
   
    var prog=$("div.progress-bar");
    var progParent = prog.parent();
    var changdu1=progParent.width();
    for(var i=0;i<prog.length;i++){
        if (prog.eq(i).offset().top < 820) {
            var changdu2 = parseInt(prog.eq(i).html()) / 100;
            prog.eq(i).width(changdu2 * changdu1);
        }
    }
    $(window).scroll(function(){          
        var windowtop = $(window).scrollTop();
        if(windowtop>99 && windowtop<1040){           
            for(var i=0;i<prog.length;i++){
                if (prog.eq(i).offset().top < 820) {
                    var changdu2 = parseInt(prog.eq(i).html()) / 100;
                    prog.eq(i).width(changdu2 * changdu1);
                }
            }      
        }
                
        if(windowtop>245){
            for (var i = 0; i < prog.length; i++) {
                if (prog.eq(i).offset().top < 1220) {
                    var changdu2 = parseInt(prog.eq(i).html()) / 100;
                    prog.eq(i).width(changdu2 * changdu1);
                }
            }
        }
    });

    //1.焦点图轮换 
    $(".m-banner").slide({titCell:".hd li",mainCell:".bd ul",effect:"fold",autoPlay:true, trigger:"mouseover" })
        .hover(function(){
            $(".prev, .next").show();
        },function(){
            $(".prev , .next").hide();
        });
  
});
