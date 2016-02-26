import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/loan.less";
import "../less/footerSmall.less";

import header from "./header.js";

window['$'] = $;

$(function () {
    header.setHeaderHighlight(5);

    //data-toggle ≥ı ºªØ
    $('[data-toggle="popover"]').popover();

    var $step1 = $("ul.application-nav li.step1");
    var $step2 = $("ul.application-nav li.step2");
    var $step3 = $("ul.application-nav li.step3");
    var $forms = $(".form-wrapper form");
    var $login = $(".form-wrapper form.login-form");
    var $personalInfo = $(".form-wrapper form.personal-info-form");
    var $loanDetail = $(".form-wrapper form.loan-detail-form");

    $step1.click(function(){
        $forms.hide();
        $login.show();
    });

    $step2.click(function(){
        $forms.hide();
        $personalInfo.show();
    });

    $step3.click(function(){
        $forms.hide();
        $loanDetail.show();
    });
});