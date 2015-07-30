$(function() {
	var screenwidth = $(document).width();
	var elementwidth = 0 ;
	var left = 0;
	
	$("#content>div").css("width",$(document).width()+"px !important");
	$("#content>div:not(:first-child)").css("display","none");
	
	$(".aligncenter").each(function(index, element) {
		if($(this).attr("data-left")!= undefined )
		{
			left=$("."+$(this).attr("data-left")).css("left");
			var targetwidth =  $("."+$(this).attr("data-left")).width();
			left=parseFloat(left.substring(0,left.indexOf("p")))+targetwidth-10;
		}
		else if($(this).attr("data-right")!= undefined)
		{
			left=$("."+$(this).attr("data-right")).css("left");
			var targetwidth =  $("."+$(this).attr("data-right")).width();
			left=parseFloat(left.substring(0,left.indexOf("p")))-$(this).width()+20;
		}
		else if($(this).attr("data-top")!= undefined)
		{
			left=$("."+$(this).attr("data-top")).css("left");
			var targetwidth =  $("."+$(this).attr("data-top")).width();
			left=parseFloat(left.substring(0,left.indexOf("p")))+80;
		}
		else 
		{
			elementwidth = $(this).width();  
			left = (screenwidth-elementwidth)/2;
		}
		switch($(this).attr("data-parent"))
		{
			case "5":
				left+=100;
				break;
			case "7":
				left+=85;
				break;
			case "8":
				left+=50;
				break;
			default:
				break;
		}
		$(this).css("left",left+"px");
	});
	
	$(".alignleft").each(function(index, element) {
		elementwidth = 1000;  
		left = (screenwidth-elementwidth)/2;
		$(this).css("left",left+"px");
	});
	
	$(".alignright").each(function(index, element) {					
		if($(this).attr("data-left")!= undefined )
		{
			left=$("."+$(this).attr("data-left")).css("left");
			var targetwidth =  $("."+$(this).attr("data-left")).width();
			left=parseFloat(left.substring(0,left.indexOf("p")))+targetwidth+20;
			$(this).css("left",left+"px");
		}
		else if($(this).attr("data-top")!= undefined )
		{
			$(this).css("left",$("."+$(this).attr("data-top")).css("left"));
		}
		else
		{
			elementwidth = 1000;  
			left = (screenwidth-elementwidth)/2;
			$(this).css("right",left+"px");
		}
	});
	
	$(".side_container ul li").click(function(e) {
		$(this).addClass("active").siblings().removeClass("active");
		var divindex = $(this).attr("index");
		$("#content>div").each(function(index, element) {
			if($(this).attr("id").indexOf(divindex) != -1 )
			{
					$(this).css("display","block");
			}
			else
			{
					$(this).css("display","none");
			}
		});
	});
	

	$(".rollImg").click(function(e) {
		$(this).addClass("active").siblings().removeClass("active");
		var divindex = $(this).attr("target");
		$("#content>div").each(function(index, element) {
			if($(this).attr("id").indexOf(divindex) != -1 )
			{
					$(this).css("display","block");
			}
			else
			{
					$(this).css("display","none");
			}
		});
		$(".side_container ul li").each(function(index, element) {
			if(divindex.indexOf($(this).attr("index")) != -1 )
			{
					$(this).addClass("active");
			}
			else
			{
					$(this).removeClass("active");
			}
		});
	});	
})


	setInterval(function(){$(".rollImg").animate(
			{top:'+=20'},
			500,
			function(){
				$(this).animate({top:'-=20'},500)
			}
		)},1000)



