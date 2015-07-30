<%@ Page Language="C#" AutoEventWireup="true" Inherits="Lip2p.Web.install.install_step2" Codebehind="step2.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<HEAD>
<title>安装 DTCMS V<%= Lip2p.Common.Utils.GetVersion()%> </title>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<LINK rev="stylesheet" media="all" href="css/styles.css" type="text/css" rel="stylesheet">
    <style type="text/css">
        .style1
        {
            width: 66%;
        }
    </style>
</HEAD>

<body> 
    <form id="form1" runat="server">
<table width="700" border="0" align="center" cellpadding="0" cellspacing="1" bgcolor="#666666">
  <tr>
    <td bgcolor="#ffffff"><table width="100%" border="0" align="center" cellpadding="0" cellspacing="0">
        <tr>
          <td colspan="2" bgcolor="#333333"><table width="100%" border="0" cellspacing="0" cellpadding="8">
              <tr>
                <td><font color="#ffffff">欢迎安装  DTCMS V<%= Lip2p.Common.Utils.GetVersion()%> </font> </td>
              </tr>
            </table></td>
        </tr>
        <tr>
          <td width="180" valign="top"><img src="images/logo.jpg" width="180" height="300"> </td>
          <td width="520" valign="top"><br>
            <br>
            <table id="Table2" cellspacing="1" cellpadding="1" width="90%" align="center" border="0">
              <tr>
                <td height="79"><p> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 欢迎您选择安装DTCMS V<%= Lip2p.Common.Utils.GetVersion()%> </p>
                 <br />  &nbsp;&nbsp;&nbsp;&nbsp;
                 <asp:Label ID="Label7" runat="server" Text=""></asp:Label>
                </td>
              </tr>
            </table>
            <table id="Table3" cellspacing="1" cellpadding="1" width="90%" align="center" border="0">
              <tr>
                <td height="129" valign="top">
                 <p> 
                    <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
                    <asp:Label ID="Label2" runat="server" Text=""></asp:Label>
                    <asp:Label ID="Label3" runat="server" Text=""></asp:Label>
                 </p>
                </td>
              </tr>
            </table>
            <p> </p>
          </td>
        </tr>
        <tr>
          <td>&nbsp;</td>
          <td><table width="90%" border="0" cellspacing="0" cellpadding="8">
              <tr>
                <td align="right" style="color:#ff0000;" class="style1">
                   系统默认帐号:admin　密码：admin888
                </td>
                <td width="41%" align="right"><asp:Button ID="Button1" runat="server"  Visible="false" OnClick="Button1_Click" Text="转到网站后台" Height="30px" Width="88px" />
                </td>
              </tr>
            </table></td>
        </tr>
      </table></td>
  </tr>
</table>
<table width="700" border="0" align="center" cellpadding="0" cellspacing="0" ID="Table1">
  <tr>
    <td align="center"><div align="center" style="position:relative ; padding-top:60px;font-size:14px; font-family: Arial">
        <hr style="height:1; width:600; height:1; color:#CCCCCC" />
        Powered by DTCMS V<%= Lip2p.Common.Utils.GetVersion()%> &nbsp;<br />
        © 2009-2013 dtcms.net. All Rights Reserved.</div></td>
  </tr>
</table>
    </form>
</body>
</html>
