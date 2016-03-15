import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/index.less";
import "../less/footer.less";
import "bootstrap/js/transition.js"

import header from "./header.js"
window['$'] = $;

$(function () {
    header.setHeaderHighlight(0);
   //计算器初始化
    $('[data-toggle="popover"]').popover();

    $(".project-content").hover(function(){
        $(this).animate({  paddingRight:"10px",paddingLeft:"20px"},300);
    },function(){
        $(this).animate({  paddingRight:"15px",paddingLeft:"15px"},300);
    });         
    });
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
