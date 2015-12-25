<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="projects_costing_predict.aspx.cs" Inherits="Agp2p.Web.admin.predict.projects_costing_predict" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>发标成本预测</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<script type="text/javascript" src="../jsbuild/predict_table.bundle.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
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
</style>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location noPrint">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>发标成本预测</span>
</div>
<!--/导航栏-->

    <!--工具栏-->
    <div class="toolbar-wrap noPrint">
        <div id="floatHead" class="toolbar">
            <div class="l-list">
               <ul class="icon-list">
                    <li><a class="quotes" onclick="print()" href="javascript:"><i></i><span>打印</span></a></li>
                    <li><a class="add" onclick="appendPredict()" href="javascript:"><i></i><span>添加当日估算</span></a></li>
                    <li><a class="add" onclick="appendTomorrowPredict()" href="javascript:"><i></i><span>添加明日估算</span></a></li>
                    <li><a class="add" onclick="repeatPredict()" href="javascript:"><i></i><span>重复首日估算</span></a></li>
                </ul>
            </div>
            <div class="r-list">
            </div>
        </div>
    </div>
    <!--/工具栏-->

<div id="mounting-point"></div>


</form>
</body>
</html>
