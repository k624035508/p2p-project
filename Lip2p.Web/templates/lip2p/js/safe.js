if (/msie [6|7|8|9]/i.test(navigator.userAgent)) {
    window.document.location.href = "safeie.html"
};

if ($(document).width() < 1400) {
    window.location.href = "safeie.html"
};
	
var curIndex = 1,canRoll = true;
function showPage(index){	
	if ( curIndex== index ) return; 
	if (canRoll == false) return;
	canRoll = false;		
	$("#s"+curIndex).removeClass("active").addClass("disappear");
	$("#s"+index).css("display","block").removeClass("disappear").addClass("active");
	
	$("#side_btn_ul li").removeClass("active");
	$("#l"+index).addClass("active");

	
	eval("s"+index+"_run()");
	
	setTimeout((function(i){
		return function(){
			$("#s"+i).css("display","none");
		};
	})(curIndex),1000);
	
	setTimeout(function(){
		canRoll = true;
	},1000);
	
	curIndex = index;
}
var s1_run = s2_run = s3_run = s4_run = s5_run = s6_run = s7_run = s8_run = function () {
};
// JavaScript Document
	
$("#side_btn_ul li").bind("click", function(){
	var index = $(this).attr("index");
	showPage(index);
});


$(document).ready(function(){
	var wh = $(window).height(),
			ww = $(window).width(),
			felm = $('div.container').find('div.main'),
			sh    = felm.height(),
			top   = parseInt((wh - sh) / 2) + 10;
			//left  = parseInt((ww - 1100) / 2);
			
	$('div.container').find('div.main, div.main2').each(function(){
			var _self = $(this);
			_self.css('top', top + 'px');
			//_self.css('left', left + 'px');
			
});

//鼠标滚轮事件
function wheel(event){

	var delta = 0;

	if (!event) event = window.event;

	if (event.wheelDelta) {

		delta = event.wheelDelta/120; 

		if (window.opera) delta = -delta;

	} else if (event.detail) {

		delta = -event.detail/3;

	}

	if (delta)

		mouseWheel(delta);

}

 

if (window.addEventListener)

window.addEventListener('DOMMouseScroll', wheel, false);

window.onmousewheel = document.onmousewheel = wheel;



//键盘按键事件

$(document).keydown(

	function(e){keyDown(e);

});

$(".rollImg").bind(

	"click", function(){downBtnDown();}

);



//鼠标滚轮事件

function mouseWheel(delta) {

	var dir = delta > 0 ? "up" : "down";

	var $actived = $(".active");

	var activeIndex = parseInt($actived.attr('index'));

	var numOfChildren = $("#side_btn_ul").children().length;

	

	if( dir == "down" && activeIndex<numOfChildren && canRoll) {

		jumpPage(false);

	} else if( dir =="up" && activeIndex>1 && canRoll) {

		jumpPage(true);

	} 

}



//键盘事件

function keyDown(e) {

	var keycode = e.which || e.keyCode;

	var $actived = $(".active");

	var activeIndex = parseInt($actived.attr('index'));

	var numOfChildren = $("#side_btn_ul").children().length;



	if ((keycode == 65 || keycode == 38 || keycode ==87 || keycode ==33 ) && activeIndex>1 && canRoll){

		jumpPage(true);

		return false;

	} else if ((keycode == 40 || keycode == 83 || keycode ==68 || keycode ==34 && canRoll) && activeIndex<numOfChildren && canRoll){

		jumpPage(false);

		return false;

	} 

}



//屏幕上的向下按钮

function downBtnDown(){

	var $actived = $(".active");

	var activeIndex = parseInt($actived.attr('index'));

	var numOfChildren = $("#side_btn_ul").children().length;

	if (activeIndex<numOfChildren && canRoll){

		jumpPage(false);

	}

}

		

//显示上一个||下一个section

function jumpPage(up) {

	var $actived = $(".active");

	var activeIndex = parseInt($actived.attr('index'));

	showPage(activeIndex + (up?-1:1));

}				

});