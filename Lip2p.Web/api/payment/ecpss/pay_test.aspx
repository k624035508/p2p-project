<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="pay_test.aspx.cs" Inherits="Lip2p.Web.api.payment.ecpss.pay_test" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>123</title>
    <style>@charset "utf-8";html,body,div,span,h1,h2,h3,h4,h5,h6,p,a,em,img,strong,dl,dt,dd,ol,ul,li,form,label,table,tbody,tfoot,thead,tr,th,td,footer,header,nav,section{margin:0px;padding:0px;}html{-webkit-text-size-adjust:none;height:100%;}*{-webkit-tap-highlight-color:rgba(0,0,0,0);-webkit-text-size-adjust:none;}.footer,header,nav,section{display:block;}p{padding:0px;margin:0px;}table{border-collapse:collapse;border-spacing:0;}ul,ol,li{list-style:none outside none;margin:0px;padding:0px;}em,i{font-style:normal;}img{border:0 none;}a{text-decoration:none;}input[type="button"],input[type="submit"],button{-webkit-appearance:none;border-radius:0;}.clearfix:after{content:'\20';display:block;height:0;clear:both;}.clearfix{*zoom:1;}.fl{float:left;}.fr{float:right;}body{background:#f2f2f2;font-family:"微软雅黑";padding:0px;margin:0px;color:#4d4d4d;height:100%}.warp{width:95%;margin:0 auto;}.w45{width:45%;}.w100{width:100%;}.tips{color:#888;margin:10px 0 0 0;font-size:0.8125em;}.mt10{margin-top:10px;}.mt20{margin-top:20px;}.mb20{margin-bottom:20px;}.ft_gray{color:#999;}.ft_red{color:#f00;}.ft24{font-size:1.5em;font-weight:bold;}a.txt_blue{color:#4a8df1;text-decoration:none}.header{width:100%;overflow:hidden;background:#4d4d4d;background-image:-moz-linear-gradient(top,#666,#333);background-image:-webkit-linear-gradient(top,rgb(102,102,102) 0%,rgb(51,51,51) 100%);background:-o-linear-gradient(top,#666,#333);box-shadow:0px 1px 3px #999;text-align:center;}.back,.about,.logo{width:54px;height:54px;overflow:hidden;}.back a,.about a{width:54px;height:54px;overflow:hidden;text-indent:-9999px;display:inline-block;background:url(images/icon.png) no-repeat;}.back a{background-position:10px 12px;}.about a{background-position:10px -47px;}.logo a{width:400px;height:54px;line-height:54px;font-size:1.815em;color:#fff;display:inline-block;overflow:hidden;}.err_box{display:block;position:relative;}.ic_arrow{width:11px;height:8px;background:url(images/icon.png) 0 -213px no-repeat;overflow:hidden;font-size:0;position:absolute;display:block;left:20px;top:0;z-index:1000}.errTips{color:#f00;border:1px solid #ff3514;display:block;width:100%;position:absolute;top:7px;left:0;color:#fff;border-radius:5px;box-shadow:0px 0px 4px #333;z-index:999;}.errTips em{display:block;background:#ff6c00 url(images/icon.png) 10px -151px no-repeat;padding:10px 6px 10px 32px;}.slogan{overflow:hidden;width:100%;height:19px;position:relative;margin:20px 0 5px 0;}.slogan h3{font-size:18px;line-height:19px;padding-left:1%;color:#4d4d4d;position:absolute;background:#f2f2f2;z-index:100;padding:0 0.215em;font-weight:normal;font-family:"微软雅黑";}.slogan span{height:9px;border-bottom:1px solid #cacaca;width:100%;position:absolute}#tab{height:50px;margin:0 0 15px 0;}#tab li{float:left;width:50%;text-align:center;font-size:1.2em;border-bottom:2px solid #cdcdcd;padding:10px 0;}#tab li.cur{color:#f89320;border-bottom:2px solid #f89320}#tab_con{width:95%;margin:0 auto;padding-bottom:20px;}#tab_con div{display:none;height:auto;padding-top:0.9375;}#tab_con div.actived{display:block;}.box1{border:1px solid #cad0dd;background:#e9effc;padding:15px 10px;border-radius:6px;position:relative;}.bank_name{float:left;font-size:1.2em;margin-right:10px;color:#333}.card_type{float:left;margin:4px 0 0 0;color:#666}.other_card{float:right;width:22px;height:22px;background:url(images/icon.png) -107px 0 no-repeat;overflow:hidden;display:block;text-indent:-9999px;}.crad_num{font-weight:bold;color:#333;font-size:1.6em;display:block;clear:both;overflow:hidden;padding:10px 0 0 0;}.box2{border:1px solid #dcd3b7;background:#fefaf0;padding:15px 10px;border-radius:6px;position:relative;}.card_back{overflow:hidden;padding:10px 0 0 0;}.card_back img{float:left;}.card_back input{float:left;width:auto;margin:20px 0 0 10px;width:120px;}.pos-r{position:relative;display:inline-block}.togglePwd{background:url(images/icon.png) -270px -59px no-repeat;height:24px;position:absolute;right:6px;top:50%;width:24px;}.togglePwd.hide{background-position:-270px -93px;}.bank_list{overflow:hidden;margin:20px 0 0 0;}.bank_list li{border-bottom:1px solid #cdcdcd;overflow:hidden;margin-bottom:10px;box-shadow:0px 2px 3px #e4e4e4}.box3{border:1px solid #dbdbdb;background:#ffffff;background-image:-moz-linear-gradient(top,#ffffff,#ffffff);background-image:-webkit-linear-gradient(top,rgb(255,255,255) 0%,rgb(255,255,255) 100%);background:-o-linear-gradient(top,#ffffff,#ffffff);}.box4{border:1px solid #d7d7d7;background:#fefbf4;background-image:-moz-linear-gradient(top,#fefbf4,#fefbf4);background-image:-webkit-linear-gradient(top,rgb(254,251,244) 0%,rgb(254,251,244) 100%);background:-o-linear-gradient(top,#fefbf4,#fefbf4);}.box3,.box4{border-bottom:none;width:95%;margin:0 auto;border-top-left-radius:10px;border-top-right-radius:10px;}.box3.pressed{background:#f4f4f4;}.box4.pressed{background:#f1efeb;}.card_info,.card_num{overflow:hidden;margin:0 15px}.card_info{padding:10px 0 5px 0;border-bottom:1px solid #e7e7e7;}.card_num{padding:10px 0;}.add_card{overflow:hidden;text-align:center;border-top:1px dashed #cdcdcd;margin:20px 0 0 0;background:#fff;background-image:-moz-linear-gradient(top,#f7f7f7,#e4e4e4);background-image:-webkit-linear-gradient(top,rgb(247,2427,247) 0%,rgb(228,228,228) 100%);background:-o-linear-gradient(top,#f7f7f7,#e4e4e4);}.add_card a{font-size:18px;padding:15px 0;display:block;color:#333}.i_text{width:40%;border:1px solid #bfbfbf;background:#fff;border-radius:0.3em;display:block;margin-top:0px;font-size:1em;box-shadow:0px 2px 3px #f1f1f1 inset;padding:0.7125em 1%;color:#333}.i_text.err{border:1px solid #ff3514;}.i_text.w280{width:200px;}.checkbox{padding:10px 0;}.checkbox span{padding:2px 0 0 0;display:inline-block}.select{width:100%;padding:0.7125em 1%;font-size:1em;overflow:hidden;color:#333;border:1px solid #bfbfbf;-webkit-appearance:none;border-radius:0.3em;background:#fff url(images/icon.png) right bottom no-repeat;}.btn{padding:0.375em 0;display:inline-block;outline:none;border:none;text-align:center;text-decoration:none;text-shadow:0 1px 1px rgba(0,0,0,.3);-webkit-border-radius:.3em;-moz-border-radius:0.3em;-o-border-radius:0.3em;border-radius:0.3em;-webkit-box-shadow:0 1px 2px rgba(0,0,0,.2);-moz-box-shadow:0 1px 2px rgba(0,0,0,.2);-o-box-shadow:0 1px 2px rgba(0,0,0,.2);box-shadow:0 1px 2px rgba(0,0,0,.2);font-size:1.325em;font-family:"微软雅黑";margin-top:10px}.orange{color:#fef4e9;border:solid 1px #da7c0c;background:#f78d1d;background:-webkit-gradient(linear,left top,left bottom,from(#faa51a),to(#f47a20));background:-webkit-linear-gradient(top,#faa51a,#f47a20);background:-moz-linear-gradient(top,#faa51a,#f47a20);background:-o-linear-gradient(top,#faa51a,#f47a20);}.gray{color:#f7f7f7;border:solid 1px #9b9b9b;background:#adadad;background:-webkit-gradient(linear,left top,left bottom,from(#d7d7d7),to(#adadad));background:-webkit-linear-gradient(top,#d7d7d7,#adadad);background:-moz-linear-gradient(top,#d7d7d7,#adadad);background:-o-linear-gradient(top,#d7d7d7,#adadad);}.blue{color:#f7f7f7;border:solid 1px #91c3fd;background:#91b8e8;background:-webkit-gradient(linear,left top,left bottom,from(#d2ecfe),to(#77b3fd));background:-webkit-linear-gradient(top,#d2ecfe,#77b3fd);background:-moz-linear-gradient(top,#d2ecfe,#77b3fd);background:-o-linear-gradient(top,#d2ecfe,#77b3fd);}.footer{text-align:center;color:#999;padding:2em 0 1em 0;}.footer img{height:15px;vertical-align:middle;}.footer span{height:15px;font-size:0.8em;line-height:0.8em;}.info{padding:15px;background:#fff url(images/info_bg.png) left bottom repeat-x;}.table_ui{width:100%;margin:0 auto;}.table_ui td{line-height:1.5em;padding-bottom:10px;vertical-align:top;}.result{padding:20px 0;text-align:center;border-bottom:1px dashed #cdcdcd;}.result img{vertical-align:bottom;margin:0 10px 0 0}.result h3{font-size:1.5em;font-weight:normal;}.result h3 span{background:url(images/icon.png) no-repeat;display:inline-block;padding:0 0 0 50px;height:45px;line-height:45px;}.result h3.suc span{color:#53c13a;background-position:0 -440px}.result h3.fail span{color:#ed5f3b;background-position:0 -243px}.result p{color:#888;line-height:24px;text-align:left}.btn_warp{border-top:1px dashed #cdcdcd;}.err404{background:url(images/404.jpg) center top no-repeat;margin:15px 0 0 0;padding:280px 0 0 0;}.err404 strong{display:block;text-align:center}.intro{width:100%;}.intro p{text-indent:2em;line-height:25px;color:#666;padding-bottom:20px;line-height:1.8em}.tel{background:url(images/icon.png) 0 -510px no-repeat;width:265px;height:48px;line-height:48px;margin:20px auto;text-align:center;font-size:1.125em;padding-left:35px;color:#333;overflow:hidden}.agreement{padding:15px 0;}.agreement h2{text-align:center;margin:0 0 15px 0;font-size:18px;}.agreement h3{margin:20px 0 5px 0;}.agreement p{font-size:1em;line-height:1.5em;color:#888;margin-bottom:10px}.agreement p strong{color:#4d4d4d;font-weight:bold;}.err500{text-align:center;}.err500 .ic500{display:block;width:100px;height:110px;margin:0 auto;margin-top:30px;background:url(images/icon.png) 0 -309px no-repeat;}.err500 h3{font-size:18px;}.text{font-size:1.3725em;padding:0.375em 0;color:#F89320;font-weight:bold}#mask{background:rgba(0,0,0,0.5);width:100%;position:absolute;top:0px;left:0px;right:0px;bottom:0px;z-index:9999;display:none;}#modal-dialog{width:90%;background:rgba(0,0,0,0.85);position:fixed;left:50%;top:30%;margin:0 0 0 -45%;z-index:9999;border-radius:6px;}.modal-header{padding:10px 10px 10px 20px;border-bottom:1px solid #333;font-size:18px;color:#fff;}.modal-body{padding:20px;color:#fff;}.modal-footer{text-align:center;padding:0 0 20px 0;}</style>
    <script type="text/javascript">
        function GetRadioValue(RadioName) {
            var obj;
            obj = document.getElementsByName(RadioName);
            if (obj != null) {
                var i;
                for (i = 0; i < obj.length; i++) {
                    if (obj[i].checked) {
                        return obj[i].value;
                    }
                }
            }
            return null;
        }
        function GetSelectValue(SelectName) {
            var obj;
            obj = document.getElementById(SelectName);
            return obj.options[obj.selectedIndex].value;
        }
        function dosubmit() {
            form1.action = "index.aspx";
            form1.PayID.value = GetSelectValue("SPayID");
            form1.submit();
        }
        document.onkeydown = function (event) {
            var e = event || window.event || arguments.callee.caller.arguments[0];
            if (e && e.keyCode == 13) { // enter 键
                dosubmit();
            }
        };
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            选择支付方式：
            <select name="SPayID" id="SPayID">
			<!--充值方式，神州行1 联通2 电信3 盛大11 完美12 征途14 骏网一卡通15 网易 16 网银支付总1000-->
                <option value="">宝付支付网关页面</option>
                <option value="101">神州行</option>
                <option value="1022">联通卡</option>
                <option value="1033">电信卡</option>
                <option value="111">盛大卡</option>
                <option value="112">完美卡</option>
                <option value="114">征途卡</option>
                <option value="115">骏网一卡通</option>
                <option value="116">网易卡</option>
                
				<option value="1001">招商银行</option>
				<option value="1002">工商银行</option>
				<option value="1003">建设银行</option>
				<option value="1004">浦发银行</option>
				<option value="1005">农业银行</option>
				<option value="1006">民生银行</option>
				<option value="1009">兴业银行</option>
				<option value="1020">交通银行</option>
				<option value="1022">光大银行</option>
				<option value="1026">中国银行</option>
				<option value="1032">北京银行</option>		
				<option value="1035">平安银行</option>
				<option value="1036">广发银行</option>
				<option value="1039">中信银行</option>
				<option value="1080">银联在线</option>
				
				<option value="3001">招商银行(借)</option>
				<option value="3002">工商银行(借)</option>
				<option value="3003">建设银行(借)</option>
				<option value="3004">浦发银行(借)</option>
				<option value="3005">农业银行(借)</option>
				<option value="3006">民生银行(借)</option>
				<option value="3009">兴业银行(借)</option>
				<option value="3020">交通银行(借)</option>
				<option value="3022">光大银行(借)</option>
				<option value="3026">中国银行(借)</option>
				<option value="3032">北京银行(借)</option>		
				<option value="3035">平安银行(借)</option>
				<option value="3036">广发银行(借)</option>
				<option value="3039">中信银行(借)</option>

				<option value="4001">招商银行(贷)</option>
				<option value="4002">工商银行(贷)</option>
				<option value="4003">建设银行(贷)</option>
				<option value="4004">浦发银行(贷)</option>
				<option value="4005">农业银行(贷)</option>
				<option value="4006">民生银行(贷)</option>
				<option value="4009">兴业银行(贷)</option>
				<option value="4020">交通银行(贷)</option>
				<option value="4022">光大银行(贷)</option>
				<option value="4026">中国银行(贷)</option>
				<option value="4032">北京银行(贷)</option>		
				<option value="4035">平安银行(贷)</option>
				<option value="4036">广发银行(贷)</option>
				<option value="4039">中信银行(贷)</option>
            </select>
            <input type="hidden" name="PayID" id="PayID" /> 
            <br />
            <br />
            输入充值金额：<input type="text" name="OrderMoney" />元
            <br />
            <br />
            <input id="btnsbmit" type="button" onclick="dosubmit()" value=" 提 交 " />&nbsp;<br />
        </div>
    </form>
</body>
</html>
