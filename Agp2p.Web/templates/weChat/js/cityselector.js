import "bootstrap-webpack";
import "fullpage.js/jquery.fullPage.css"
import "fullpage.js"
import citylist from "../../AnGuang/js/city.min.js"

function fixAndroid2xOverflowCannotScroll($affectedElem) {
    var ua = navigator.userAgent;
    if (ua.indexOf("Android") >= 0) {
        var androidversion = parseFloat(ua.slice(ua.indexOf("Android") + 8));
        if (androidversion < 3) {
            var script = document.createElement('script');
            script.src = "js/touchscroll.js";
            script.onload = function () {
                $affectedElem.each(function (index, elem) {
                    touchScroll(elem);
                });
            };
            document.head.appendChild(script);
        }
    }
}
function createFacade(pros) {
    var province = null, city = null, area = null;
    return {
        setProvince: function (index) {
            province = index;
            city = area = null;
            this.loadCities();
            $(".slide [data-level=area]").html("");
        },
        setCity: function (index) {
            city = index;
            area = null;
            this.loadAreas();
        },
        setArea: function (index) { area = index; },

        loadProvinces: function () {
            var page = $(".slide [data-level=province]");
            page.html("");
            $.each(pros, function (index, pro) {
                page.append("<div class='selection'>" + pro.p + "</div>");
            });
        },
        loadCities: function () {
            var page = $(".slide [data-level=city]");
            page.html("");
            $.each(pros[province].c, function (index, city) {
                page.append("<div class='selection'>" + city.n + "</div>");
            });
        },
        loadAreas: function() {
            var page = $(".slide [data-level=area]");
            page.html("");
            $.each(pros[province].c[city].a, function (index, area) {
                page.append("<div class='selection'>" + area.s + "</div>");
            });
        },
        getLoc: function() {
            return [
                pros[province].p,
                city == null ? "" : pros[province].c[city].n,
                area == null ? "" : pros[province].c[city].a[area].s
            ].join(";");
        }
    };
}

window.pickDone = loc => alert(loc);

$(function () {
    $('#fullpage').fullpage({
        controlArrows: false,
        verticalCentered: false,
        loopHorizontal: false,
        anchors: ['pager']
    });
    $.fn.fullpage.setAllowScrolling(false); // 允许上下滚动

    // 加载地理数据
    var facade = createFacade(citylist);
    facade.loadProvinces();

    $(".slide").on("click", "div.selection", function () {
        var parent = $(this).parents("[data-level]");
        var level = parent.attr("data-level");
        var clickedIndex = $(this).index();
        if (level !== "area") {
            try {
                if (level === "province") {
                    facade.setProvince(clickedIndex);
                } else if (level === "city") {
                    facade.setCity(clickedIndex);
                }
                $.fn.fullpage.moveSlideRight();
            } catch (e) {
                pickDone(facade.getLoc());
            } 
        } else {
            facade.setArea(clickedIndex);
            pickDone(facade.getLoc());
        }
    });

    fixAndroid2xOverflowCannotScroll($(".inner-scrollable"));
});