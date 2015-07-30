<%@ Page Language="C#" AutoEventWireup="true" Inherits="Lip2p.Web.install.install_step1" Codebehind="step1.aspx.cs" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<script type="text/javascript">
var ok=true;
function hideall()
{
            hide("tr1");
            hide("tr2");
            hide("tr3");
            hide("tr4");
            hide("tr5");
            document.getElementById("ddlDbType").selectedIndex = 0;
}
function DbTypeChange(type)
{
    switch(type)
    {
        case "SqlServer":
            show("tr1");
            show("tr2");
            show("tr3");
            show("tr4");
            show("tr5");                 
            document.getElementById("txtServerName").value = "127.0.0.1";   
            document.getElementById("txtDbUserName").value = "sa";                
            document.getElementById("trspan1").innerHTML = " 服务器名或IP地址:";
            document.getElementById("litMsg").innerHTML = " ";            
            break;
        case "Access":
            hide("tr1");
            show("tr2");
            hide("tr3");
            hide("tr4");
            hide("tr5");               
            document.getElementById("trspan1").innerHTML = " 数据库名:";               
            document.getElementById("litMsg").innerHTML = " ";    
            break;
        default:
           // hideall();
            break;
    }
}
function SelectChange()
{
    DbTypeChange(document.getElementById("ddlDbType").value);
    ok=true;
}

function hide(id)
{
    document.getElementById(id).style.display = "none";
}

function show(id)
{
    document.getElementById(id).style.display = "";
}

function checkid(obj,id)
{
    var v = obj.value;
    
    if(v.length == 0)
    {
        document.getElementById('msg'+id).innerHTML='<span style=\'color:#ff0000\'>此处不能为空！</span>';
        ok=false;
    }
    else
    {
        document.getElementById('msg'+id).innerHTML='';
        ok=true;
    }
} 
</script>
<HEAD>
<title>安装 DTCMS V<%= Lip2p.Common.Utils.GetVersion()%> </title>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<LINK rev="stylesheet" media="all" href="css/styles.css" type="text/css" rel="stylesheet">
</HEAD>
<body >
<form name="Form1" id="Form1" runat="server">
  <table cellspacing="1" cellpadding="0" width="700" align="center" bgcolor="#666666"
            border="0">
    <tr>
      <td bgcolor="#ffffff"><table cellspacing="0" cellpadding="0" width="100%" align="center" border="0">
          <tr>
            <td bgcolor="#333" colspan="2"><table cellspacing="0" cellpadding="8" width="100%" border="0">
                <tr>
                  <td><font color="#ffffff">初始化...</font></td>
                </tr>
              </table></td>
          </tr>
          <tr>
            <td valign="top" width="180"><img src="images/logo.jpg" width="180" height="300"> </td>
            <td valign="top" width="520"><br>
              <table cellspacing="0" cellpadding="8" width="100%" border="0">
                <tr>
                  <td><p> 
                      <asp:Label ID="labState" runat="server" Text="状态"></asp:Label>
                      </p>
                    <table cellspacing="0" cellpadding="8" width="100%" border="0">
                      <tr>
                        <td width="30%">网站的名称:</td>
                        <td style="width: 352px"><asp:TextBox ID="txtSiteName" runat="server"></asp:TextBox>
                             <asp:RequiredFieldValidator ID="RequiredFieldValidator1" 
                                runat="server" ControlToValidate="txtSiteName" ErrorMessage="请填写网站名称"></asp:RequiredFieldValidator>
                                                  </td>
                      </tr>
                      <tr>
                        <td width="30%">网站安装目录:</td>
                        <td style="width: 352px"><asp:TextBox ID="txtSiteUrl" Text="/" runat="server"></asp:TextBox>
                             <asp:RequiredFieldValidator ID="RequiredFieldValidator11" 
                                runat="server" ControlToValidate="txtSiteUrl" ErrorMessage="请填写网站安装目录"></asp:RequiredFieldValidator>
                                                  </td>
                      </tr>
                      <tr>
                        <td colspan="2">系统管理员帐号和密码(第一次登陆时创建分别为：admin,admin888)</td>
                      </tr>
                      <tr style="display:none">
                        <td style="background-color: #f5f5f5"> 数据库类型:</td>
                        <td style="background-color: #f5f5f5; width: 352px;">
                        <select name="ddlDbType" id="ddlDbType" onchange="SelectChange(this)" runat="server">
                            <option value="0">请选择数据库类型</option>
                            <option value="SqlServer">SqlServer</option>
                                 
                          </select>
                           
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" 
                                ControlToValidate="ddlDbType" ErrorMessage="选择数据库类型"></asp:RequiredFieldValidator>
                           
                        </td>
                      </tr>
                      <tr id="tr1">
                        <td style="background-color: #f5f5f5"><span id="trspan1"> 服务器名或IP地址:</span></td>
                        <td style="background-color: #f5f5f5; width: 352px;"><asp:TextBox ID="txtServerName" runat="server" Text="127.0.0.1"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" 
                                ControlToValidate="txtServerName" ErrorMessage="请填写完整"></asp:RequiredFieldValidator>
                        <span id="msg1"></span></td></tr>

                      <tr id="tr2">
                        <td style="background-color: #f5f5f5"> 数据库名称:</td>
                        <td style="background-color: #f5f5f5; width: 352px;"><asp:TextBox ID="txtDbName" runat="server" Text="DTCMS3.0"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" 
                                ControlToValidate="txtDbName" ErrorMessage="请填写数据库名称"></asp:RequiredFieldValidator>
                        <span id="msg2"></span></td></tr>
                      <tr id="tr5">
                        <td style="background-color: #f5f5f5"> 数据库表前缀:</td>
                        <td style="background-color: #f5f5f5; width: 352px;"><asp:TextBox ID="txtDbPrefix" runat="server">dt_</asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator18" runat="server" 
                                ControlToValidate="txtDbPrefix" ErrorMessage="请填写数据库表前缀"></asp:RequiredFieldValidator>
                        <span id="msg5"></span></td></tr>
                      <tr id="tr3">
                        <td style="background-color: #f5f5f5"> 数据库用户名称:</td>
                        <td style="background-color: #f5f5f5; width: 352px;"><asp:TextBox ID="txtDbUserName" runat="server">sa</asp:TextBox>
                            </td></tr>
                      <tr id="tr4">
                        <td style="background-color: #f5f5f5"> 数据库用户口令:</td>
                        <td style="background-color: #f5f5f5; width: 352px;">
                            <asp:TextBox ID="txtDbUserPass" runat="server" TextMode="Password"></asp:TextBox>
                             </td>
                      </tr>

                      <tr>
                        <td colspan="2" style="color:#ff0000; font-size:14px; font-weight:bold;">&nbsp;<asp:Label ID="litMsg" runat="server"></asp:Label></td>
                      </tr>
                      <tr>
                        <td>&nbsp;</td>
                        <td style="width: 352px">
                            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text=" 安装 " Height="30px" Width="88px" />
                        </td>
                      </tr>
                    </table>
                    </td>
                </tr>
              </table>
              </td>
          </tr>
          <tr>
            <td colspan="2">&nbsp;</td>
          </tr>
        </table></td>
    </tr>
  </table>
  <table width="700" border="0" align="center" cellpadding="0" cellspacing="0" ID="Table1">
    <tr>
      <td align="center"><div align="center" style="position:relative ; padding-top:60px;font-size:14px; font-family: Arial">
          <hr style="height:1; width:600; height:1; color:#CCCCCC" />
          Powered by DTCMS V<%= Lip2p.Common.Utils.GetVersion()%>        &nbsp;         &nbsp;<br />
          &copy; 2009-2013 dtcms.net. All Rights Reserved</div></td>
    </tr>
  </table>
 <%-- <script type="text/javascript">
 hideall()
 </script>--%>
</form>
</body>
</html>
