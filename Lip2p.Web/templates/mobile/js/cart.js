//使商品数量加减一
function GoodsAddNum(objId, num){
	var goodsNum = parseFloat($(objId).val()) + parseFloat(num);
	if(goodsNum < 1){
		goodsNum = 1;
	}
	$(objId).val(goodsNum);
}

//添加进购物车
function CartAdd(obj, webpath, linktype, linkurl){
	if($("#goods_id").val()=="" || $("#goods_quantity").val()==""){
		return false;
	}
	$.ajax({
		type: "post",
		url: webpath + "tools/submit_ajax.ashx?action=cart_goods_add",
		data: {
			"goods_id" : $("#goods_id").val(),
			"goods_quantity" : $("#goods_quantity").val()
		},
		dataType: "json",
		beforeSend: function(XMLHttpRequest) {
			//发送前动作
		},
		success: function(data, textStatus) {
			if (data.status == 1) {
				if(linktype==1){
					location.href=linkurl;
				}else{
					$.dialog({
						lock: true,
						icon: 'success.gif',
						title: '提示信息',
						content: '<p>购物车共有<b>' + data.quantity + '</b>件商品，合计：<b class="red">' + data.amount + '</b>元</p>',
						okVal: '去结算',
						ok: function () {
							location.href=linkurl;
						},
						cancelVal: '再逛逛',
						cancel: true
					});
				}
			} else {
				$.dialog.alert(data.msg);
			}
		},
		error: function (XMLHttpRequest, textStatus, errorThrown) {
			$.dialog.alert("状态：" + textStatus + "；出错提示：" + errorThrown);
		},
		timeout: 20000
	});
	return false;
}

//删除购物车商品
function DeleteCart(obj, webpath, goods_id){
	if(goods_id == ""){
		return false;
	}
	$.dialog.confirm("您确认要从购物车中移除吗？",function(){
		$.ajax({
			type: "post",
			url: webpath + "tools/submit_ajax.ashx?action=cart_goods_delete",
			data: {"goods_id" : goods_id},
			dataType: "json",
			beforeSend: function(XMLHttpRequest) {
				//发送前动作
			},
			success: function(data, textStatus) {
				if (data.status == 1) {
					location.reload();
				} else {
					$.dialog.alert(data.msg);
				}
			},
			error: function (XMLHttpRequest, textStatus, errorThrown) {
				$.dialog.alert("状态：" + textStatus + "；出错提示：" + errorThrown);
			},
			timeout: 20000
		});
	});
	return false;
}

//计算购物车金额
function CartAmountTotal(obj, webpath, goods_id){
	if(isNaN($(obj).val())){
		$.dialog.alert('商品数量只能输入数字!',function(){
			$(obj).val("1");
		});
	}
	$.ajax({
		type: "post",
		url: webpath + "tools/submit_ajax.ashx?action=cart_goods_update",
		data: {
			"goods_id" : goods_id,
			"goods_quantity" : $(obj).val()
		},
		dataType: "json",
		beforeSend: function(XMLHttpRequest) {
			//发送前动作
		},
		success: function(data, textStatus) {
			if (data.status == 1) {
				location.reload();
			} else {
				$.dialog.alert(data.msg,function(){
					location.reload();
				});
			}
		},
		error: function (XMLHttpRequest, textStatus, errorThrown) {
			$.dialog.alert("状态：" + textStatus + "；出错提示：" + errorThrown);
		},
		timeout: 20000
	});
	return false;
}
//购物车数量加减
function CartComputNum(obj, webpath, goods_id, num){
	if(num > 0){
		var goods_quantity = $(obj).prev("input[name='goods_quantity']");
		$(goods_quantity).val(parseInt($(goods_quantity).val()) + 1);
		//计算购物车金额
		CartAmountTotal($(goods_quantity), webpath, goods_id);
	}else{
		var goods_quantity = $(obj).next("input[name='goods_quantity']");
		if(parseInt($(goods_quantity).val()) > 1){
			$(goods_quantity).val(parseInt($(goods_quantity).val()) - 1);
			//计算购物车金额
			CartAmountTotal($(goods_quantity), webpath, goods_id);
		}
	}
}
//计算支付手续费总金额
function PaymentAmountTotal(obj){
	var payment_price = $(obj).children("option:selected").attr("fee");
	$("#payment_fee").text(payment_price); //运费
	OrderAmountTotal();
}
//计算配送费用总金额
function FreightAmountTotal(obj){
	var express_price = $(obj).children(":selected").attr("fee");
	$("#express_fee").text(express_price); //运费
	OrderAmountTotal();
}
//计算订单总金额
function OrderAmountTotal(){
	var goods_amount = $("#goods_amount").text(); //商品总金额
	var payment_fee = $("#payment_fee").text(); //手续费
	var express_fee = $("#express_fee").text(); //运费
	var order_amount = parseFloat(goods_amount) + parseFloat(payment_fee) + parseFloat(express_fee); //订单总金额 = 商品金额 + 手续费 + 运费
	$("#order_amount").text(order_amount.toFixed(2));
}