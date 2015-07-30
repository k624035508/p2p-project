using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.API.Payment.Chinabank;
using Lip2p.Common;

namespace Lip2p.Web.api.payment.chinabank
{
    public partial class notify_url : System.Web.UI.Page
    {
        protected string v_oid; //订单号
        protected string v_pstatus; //支付状态码
        //20（支付成功，对使用实时银行卡进行扣款的订单）；
        //30（支付失败，对使用实时银行卡进行扣款的订单）；
        protected string v_pstring; //支付状态描述
        protected string v_pmode; //支付银行
        protected string v_md5str; //MD5校验码
        protected string v_amount; //支付金额
        protected string v_moneytype; //币种		
        protected string remark1;//备注1
        protected string remark2;//备注1

        protected void Page_Load(object sender, EventArgs e)
        {
            //读取站点配置信息
            Model.siteconfig siteConfig = new BLL.siteconfig().loadConfig();

            v_oid = DTRequest.GetString("v_oid").ToUpper();
            v_pstatus = DTRequest.GetString("v_pstatus");
            v_pstring = DTRequest.GetString("v_pstring");
            v_pmode = DTRequest.GetString("v_pmode");
            v_md5str = DTRequest.GetString("v_md5str");
            v_amount = DTRequest.GetString("v_amount");
            v_moneytype = DTRequest.GetString("v_moneytype");
            remark1 = DTRequest.GetString("remark1");
            remark2 = DTRequest.GetString("remark2");

            // 拼凑加密串
            string signtext = v_oid + v_pstatus + v_amount + v_moneytype + Config.Key;
            signtext = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(signtext, "md5").ToUpper();

            //写日志
            //System.IO.File.AppendAllText(Utils.GetMapPath("chinabanklog.txt"), "现签名：" + signtext + "，原签名：" + v_md5str + "验证结果：" + v_pstatus + "\n", System.Text.Encoding.UTF8);

            if (signtext == v_md5str && v_pstatus.Equals("20"))
            {
                //成功状态
                if (v_oid.StartsWith("R")) //充值订单
                {
                    //写日志
                    //System.IO.File.AppendAllText(Utils.GetMapPath("chinabanklog.txt"), "充值订单号：" + v_oid + "\n", System.Text.Encoding.UTF8);

                    BLL.user_amount_log bll = new BLL.user_amount_log();
                    Model.user_amount_log model = bll.GetModel(v_oid);
                    if (model == null)
                    {
                        //写日志
                        System.IO.File.AppendAllText(Utils.GetMapPath("chinabanklog.txt"), "充值记录不存在\n", System.Text.Encoding.UTF8);
                        Response.Write("error");
                        return;
                    }
                    if (model.status == 1) //已成功
                    {
                        Response.Write("ok");
                        return;
                    }
                    if (model.value != decimal.Parse(v_amount))
                    {
                        //写日志
                        //System.IO.File.AppendAllText(Utils.GetMapPath("chinabanklog.txt"), "金额不相同，记录值:" + model.value + "，返回值：" + v_amount + "\n", System.Text.Encoding.UTF8);
                        Response.Write("error");
                        return;
                    }
                    model.status = 1;
                    model.complete_time = DateTime.Now;
                    bool result = bll.Update(model);
                    if (!result)
                    {
                        //写日志
                        //System.IO.File.AppendAllText(Utils.GetMapPath("chinabanklog.txt"), "更新记录失败\n", System.Text.Encoding.UTF8);
                        Response.Write("error");
                        return;
                    }
                }
                else if (v_oid.StartsWith("B")) //商品订单
                {
                    //写日志
                    //System.IO.File.AppendAllText(Utils.GetMapPath("chinabanklog.txt"), "商品订单号：" + v_oid + "\n", System.Text.Encoding.UTF8);

                    BLL.orders bll = new BLL.orders();
                    Model.orders model = bll.GetModel(v_oid);
                    if (model == null)
                    {
                        //写日志
                        //System.IO.File.AppendAllText(Utils.GetMapPath("chinabanklog.txt"), "充值记录不存在\n", System.Text.Encoding.UTF8);
                        Response.Write("error");
                        return;
                    }
                    if (model.payment_status == 2) //已付款
                    {
                        Response.Write("ok");
                        return;
                    }
                    if (model.order_amount != decimal.Parse(v_amount))
                    {
                        //写日志
                        //System.IO.File.AppendAllText(Utils.GetMapPath("chinabanklog.txt"), "金额不相同，记录值:" + model.order_amount + "，返回值：" + v_amount + "\n", System.Text.Encoding.UTF8);
                        Response.Write("error");
                        return;
                    }
                    bool result = bll.UpdateField(v_oid, "status=2,payment_status=2,payment_time='" + DateTime.Now + "'");
                    if (!result)
                    {
                        //写日志
                        //System.IO.File.AppendAllText(Utils.GetMapPath("chinabanklog.txt"), "更新记录失败\n", System.Text.Encoding.UTF8);
                        Response.Write("error");
                        return;
                    }

                    //扣除积分
                    if (model.point < 0)
                    {
                        new BLL.user_point_log().Add(model.user_id, model.user_name, model.point, "换购扣除积分，订单号：" + model.order_no, false);
                    }
                }

                //成功状态
                Response.Write("ok");
                return;
            }

            //失败状态
            Response.Write("error");
            return;
        }
    }
}