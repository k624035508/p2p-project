$(function () {
    $(".statustime").each(function () {
        var sid = $(this).val();
        var dtime = $(this).attr("publishtime");
        var id = $(this).attr("pid");
        //alert(id);
        if (sid == "55") {
            if (dtime != null) {
                var endtime = new Date(Date.parse(dtime.replace(/-/g, "/"))).getTime();
                timer(endtime, id);
            }
        }
    });
});

function timer(endtime, itemid) {
    window.setInterval(function () {
        //alert(itemid);
        var nowtime = new Date();
        var totalSeconds = parseInt((endtime - nowtime.getTime()) / 1000);
        var intDiff = parseInt(totalSeconds) + 2; //倒计时总秒数量
        var hour = "00",
            minute = 0,
            second = 0; //时间默认值
        if (intDiff > 0) {
            hour = Math.floor(intDiff % 86400 / 3600);
            minute = Math.floor(intDiff % 3600 / 60);
            second = Math.floor(intDiff % 60);
            $("#clock" + itemid + "").html(minute + "分" + second + "秒后发标");
            intDiff--;
        } else {
            clearInterval(timer);
            location.reload();
        }
    }, 1000);
}