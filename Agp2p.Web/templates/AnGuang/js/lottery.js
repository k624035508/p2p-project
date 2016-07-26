import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/lottery.less";
import "../less/footerSmall.less";
import alert from "../components/tips_alert.js";

window['jQuery'] = $;
window['$'] = $;
import header from "./header.js";
   

var lottery = {
    index: -1,    //当前转动到哪个位置，起点位置
    count: 0,    //总共有多少个位置
    timer: 0,    //setTimeout的ID，用clearTimeout清除
    speed: 20,    //初始转动速度
    times: 0,    //转动次数
    cycle: 50,    //转动基本次数：即至少需要转动多少次再进入抽奖环节
    prize: -1,    //中奖位置
    name:0,      //中奖积分
    init: function () {
        if ($("#lottery").find(".lottery-unit").length > 0) {
            var $lottery = $("#lottery");
            var $units = $lottery.find(".lottery-unit");
            this.obj = $lottery;
            this.count = $units.length;
            $lottery.find(".lottery-unit-" + this.index).addClass("active");
        };
    },
    roll: function () {
        var index = this.index;
        var count = this.count;
        var lottery = this.obj;
        $(lottery).find(".lottery-unit-" + index).removeClass("active");
        index += 1;
        if (index > count - 1) {
            index = 0;
        };
        $(lottery).find(".lottery-unit-" + index).addClass("active");
        this.index = index;
        return false;
    },
    stop: function (index) {
        this.prize = index;
        return false;
    }
};
function roll() {
    lottery.times += 1;
    lottery.roll();
    if (lottery.times > lottery.cycle + 10 && lottery.prize == lottery.index) {
        clearTimeout(lottery.timer);
        lottery.prize = -1;
        lottery.times = 0;
        click = false;
        $.ajax({
            type: "post",
            dataType:"JSON",
            url:"/tools/submit_ajax.ashx?action=point_lottery",
            data: {
                getPoints: $(".active .shuzhi").text()
            },
            success: function (data) {
                if (data.status == 0) {
                    alert(data.msg);
                } else {
                    alert(lottery.name);
                }
            },
            error:function(xhr, status, error) {
                alert("操作超时，请重试");
            }
        });
    } else {
        if (lottery.times < lottery.cycle) {
            lottery.speed -= 10;
        } else if (lottery.times == lottery.cycle) {
            //var index = Math.random() * (lottery.count) | 0;
            $.ajax({
                type: "post",
                dataType: "JSON",
                contentType: "application/json",
                url: "/aspx/main/lottery.aspx/ZhuanPan",
                success:function(data) {
                    let { name, result } = JSON.parse(data.d);
                    lottery.prize = result;
                    lottery.name = name;                    
                },
                error:function(xhr, status, error) {
                    alert("操作超时，请重试");
                }
            });           
        } else {
            if (lottery.times > lottery.cycle + 10 && ((lottery.prize == 0 && lottery.index == 7) || lottery.prize == lottery.index + 1)) {
                lottery.speed += 110;
            } else {
                lottery.speed += 20;
            }
        }
        if (lottery.speed < 40) {
            lottery.speed = 40;
        };
        //console.log(lottery.times+'^^^^^^'+lottery.speed+'^^^^^^^'+lottery.prize);
        lottery.timer = setTimeout(roll, lottery.speed);        
    }
    return false;
}
var click = false;
window.onload = function () {
    header.setHeaderHighlight(3);
    lottery.init();
    $("#lottery a.denglu").click(function () {
        if (click) {
            return false;
        } else {
            lottery.speed = 100;    
            roll();
            click = true;
            return false;
        }
    });
    $("#lottery a.weidenglu").click(function() {
        alert("请先登录");
    });
};
