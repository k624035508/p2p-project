<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="apply_edit.aspx.cs" Inherits="Lip2p.Web.Plugin.Job.admin.apply_edit" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>查看应聘信息</title>
<script type="text/javascript" src="/scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="/scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="/scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="/scripts/datepicker/WdatePicker.js"></script>
<script type="text/javascript" src="/scripts/swfupload/swfupload.js"></script>
<script type="text/javascript" src="/scripts/swfupload/swfupload.queue.js"></script>
<script type="text/javascript" src="/scripts/swfupload/swfupload.handlers.js"></script>
<script type="text/javascript" charset="utf-8" src="/editor/kindeditor-min.js"></script>
<script type="text/javascript" charset="utf-8" src="/editor/lang/zh_CN.js"></script>
<script type="text/javascript" src="/admin/js/layout.js"></script>
<link href="/admin/skin/default/style.css" rel="stylesheet" type="text/css" />
</head>
<body class="mainbody">
<form id="form1" runat="server">

<div class="location">
  <a href="index.aspx" class="back"><i></i><span>返回列表页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>查看招聘</span>
</div>
<div class="line10"></div>


<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">基本信息</a></li>

      
      </ul>
    </div>
  </div>
</div>

    
  <div class="tab-content" style="display:block;">

      
            <dl>
                <dt>联系人：</dt>
                <dd><%=model.RealName%></dd>
            </dl>
            <dl>
                <dt>应聘岗位：</dt>
                <dd><%=GetJobName(model.job_id)%></dd>
            </dl>
            <dl>
                <dt>性别：</dt>
                <dd><%=model.Sex %></dd>
            </dl>
            <dl>
                <dt>出生日期：</dt>
                <dd><%=model.Birth%></dd>
            </dl>
            <dl>
                <dt>婚姻状况：</dt>
                <dd><%=model.Marital%></dd>
            </dl>
            <dl>
                <dt>籍贯：</dt>
                <dd><%=model.Origin%></dd>
            </dl>
            <dl>
                <dt>爱好：</dt>
                <dd><%=model.Hobby%></dd>
            </dl>
            <dl>
                <dt>毕业院校：</dt>
                <dd><%=model.School%></dd>
            </dl>
            <dl>
                <dt>学历：</dt>
                <dd><%=model.Degree%></dd>
            </dl>
            <dl>
                <dt>所学专业：</dt>
                <dd><%=model.Profess%></dd>
            </dl>
            <dl>
                <dt>身份证号：</dt>
                <dd><%=model.IDNum%></dd>
            </dl>
            <dl>
                <dt>联系地址：</dt>
                <dd><%=model.Address%></dd>
            </dl>
            <dl>
                <dt>联系电话：</dt>
                <dd><%=model.Tel%></dd>
            </dl>
            <dl>
                <dt>电子邮箱：</dt>
                <dd><%=model.Email %></dd>
            </dl>
            <dl>
                <dt>期望薪资：</dt>
                <dd><%=model.Salary%></dd>
            </dl>
            <dl>
                <dt>教育经历：</dt>
                <dd><%=model.EducationExperience%></dd>
            </dl>
            <dl>
                <dt>工作经历：</dt>
                <dd><%=model.WorkExperience%></dd>
            </dl>
            <dl>
                <dt>自我评价：</dt>
                <dd><%=model.SelfContent%></dd>
            </dl>
         
    </div>
    

</form>
</body>
</html>
