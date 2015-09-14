import $ from "jquery";

export default {
    setHeaderHighlight: function (index) {
        $("ul.in-header > li:nth(" + index + ") > a:first").addClass("nav-active");
    }
};
