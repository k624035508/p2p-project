<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="projects_costing_predict.aspx.cs" Inherits="Agp2p.Web.admin.predict.projects_costing_predict" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>业务预测—资金成本</title>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<script type="text/javascript" src="../jsbuild/predict_table.bundle.js"></script>
<style>
tr.sum td { color: red; }
td.money { text-align: right; }
td.center { text-align: center; }
tbody td { text-align: center; }
@media print {
    .noPrint {
        display: none;
    }
}
tr.pointer td { cursor: pointer;}

tr:hover a { display: inline;position: absolute; }
tr a { display: none; }

div.rl { margin-left: 10px;}

.toolbar2{ padding:5px 0 0 15px; min-height:32px; font-size:12px; color:#333; }
.keyword{ display:block; margin:0; padding:0 5px; width:110px; height:30px; line-height:28px; font-size:12px; border:1px solid #eee; color:#444; }
</style>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location noPrint">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>业务预测—资金成本</span>
</div>
<!--/导航栏-->
    <div class="toolbar2">
        <div style="display: inline-block;">默认项目金额：</div>
        <div style="display: inline-block; ">
            <input id="financingAmount" type="text" class="keyword defaultValueSetter" style="width: 5em" />
        </div>
        <div style="display: inline-block;">垫付率（%）：</div>
        <div style="display: inline-block; ">
            <input type="text" id="prepayRatePercent" class="keyword defaultValueSetter" style="width: 2em" />
        </div>
        <div style="display: inline-block;">资金年化利率（%）：</div>
        <div style="display: inline-block; ">
            <input type="text" id="profitRateYearlyPercent" class="keyword defaultValueSetter" style="width: 1em" />
        </div>
        <div style="display: inline-block;">期限（天）：</div>
        <div style="display: inline-block; ">
            <input type="text" id="termLength" class="keyword defaultValueSetter" style="width: 1em" />
        </div>
        <div style="display: inline-block;">错配期（天）：</div>
        <div style="display: inline-block; ">
            <input type="text" id="repayDelayDays" class="keyword defaultValueSetter" style="width: 1em" />
        </div>
        <div style="display: inline-block;">结算手续费率（%）：</div>
        <div style="display: inline-block; ">
            <input type="text" id="handlingFeePercent" class="keyword defaultValueSetter" style="width: 4em" />
        </div>
    </div>
    <!--工具栏-->
    <div class="toolbar-wrap noPrint">
        <div id="floatHead" class="toolbar">
            <div class="l-list">
               <ul class="icon-list">
                    <li><a class="quotes" onclick="print()" href="javascript:"><i></i><span>打印</span></a></li>
                    <li><a class="quotes" onclick="exportExcel(['editingTable', 'groupByTermLengthTable', 'groupByPrepayRateTable'], ['明细表', '汇总表（按品种）', '汇总表（按垫付率）'], '业务预测—资金成本.xls')"
                        href="javascript:"><i></i><span>导出 Excel</span></a></li>
                    <li>　</li>
                    <li><a class="add" onclick="appendPredict()" href="javascript:"><i></i><span>添加当日估算</span></a></li>
                    <li><a class="add" onclick="appendNextDayPredict()" href="javascript:"><i></i><span>添加下一日估算</span></a></li>
                    <li><a class="add" onclick="repeatPredict()" href="javascript:"><i></i><span>重复首日估算</span></a></li>
                    <li>　</li>
                    <li><a class="copy" onclick="openEditingTable()" href="javascript:"><i></i><span>明细表</span></a></li>
                    <li><a class="copy" onclick="openGroupByTermLengthTable()" href="javascript:"><i></i><span>汇总表（按品种）</span></a></li>
                    <li><a class="copy" onclick="openGroupByPrepayRateTable()" href="javascript:"><i></i><span>汇总表（按垫付率）</span></a></li>
                </ul>
            </div>
            
        </div>
    </div>
    <!--/工具栏-->

<div id="editingTable-mountingPoint"></div>
<div id="groupByTermLengthTable-mountingPoint" style="display: none"></div>
<div id="groupByPrepayRateTable-mountingPoint" style="display: none"></div>


</form>
</body>
</html>
