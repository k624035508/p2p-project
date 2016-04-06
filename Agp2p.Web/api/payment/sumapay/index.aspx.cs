using System;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;
using Agp2p.Web.UI;

namespace Agp2p.Web.api.payment.sumapay
{
    /// <summary>
    /// 所有第三方托管前台接口统一处理跳转
    /// </summary>
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //TODO 在页面显示错误提示
            dt_users user = null;
            BaseReqMsg reqMsg = null;
            int requestApi = DTRequest.GetQueryInt("api", 0);
            switch (requestApi)
            {//个人开户/激活
                case (int)Agp2pEnums.SumapayApiEnum.URegi:
                case (int)Agp2pEnums.SumapayApiEnum.Activ:
                    //实名验证接口
                    user = CheckUserLogin();
                    reqMsg = new UserRegisterReqMsg(user.id, user.mobile, user.real_name, user.id_card_number, user.token);
                    break;
                //跳转托管账户
                case (int)Agp2pEnums.SumapayApiEnum.Accou:
                    user = CheckUserLogin();
                    reqMsg = new UserToAccountReqMsg(user.id);
                    break;
                //个人自动投标续约
                case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                    user = CheckUserLogin();
                    reqMsg = new AutoBidSignReqMsg(user.id);
                    break;
                //个人自动投标取消
                case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                    user = CheckUserLogin();
                    reqMsg = new AutoBidSignReqMsg(user.id, true, user.protocolCode);
                    break;
                //个人自动账户/银行还款开通
                case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                    reqMsg = new AutoRepaySignReqMsg(DTRequest.GetQueryInt("userId", 0), DTRequest.GetQueryString("projectCode"), DTRequest.GetQueryString("repayLimit"), requestApi != (int)Agp2pEnums.SumapayApiEnum.AcReO);
                    break;
                //个人自动还款取消
                case (int)Agp2pEnums.SumapayApiEnum.ClRep:
                    reqMsg = new AutoRepayCancelReqMsg(DTRequest.GetQueryInt("userId", 0), DTRequest.GetQueryString("projectCode"));
                    break;
                //个人网银充值
                case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                    reqMsg = new WebRechargeReqMsg(DTRequest.GetQueryInt("userId", 0),
                            DTRequest.GetQueryString("sum"), DTRequest.GetQueryString("bankCode"));
                    break;
                //个人一键充值
                case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                    reqMsg = new WhRechargeReqMsg(DTRequest.GetQueryInt("userId", 0),
                            DTRequest.GetQueryString("sum"));
                    break;
                //个人提现
                case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                    reqMsg = new WithdrawReqMsg(DTRequest.GetQueryInt("userId", 0), DTRequest.GetQueryString("sum"),
                        DTRequest.GetQueryString("bankId"), DTRequest.GetQueryString("bankName"),
                        DTRequest.GetQueryString("bankAccount"));
                    break;
                default:
                    reqMsg = new BaseReqMsg();
                    break;
            }
            //生成、保存请求内容
            MessageBus.Main.Publish(reqMsg);
            //跳转丰付平台开通托管账户
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("GBK");
            Response.Write(reqMsg.RequestContent);
        }

        private dt_users CheckUserLogin()
        {
            var user = BasePage.GetUserInfoByLinq();
            if (user == null)
            {
                Response.Write("对不起，用户尚未登录或已超时！");
                return null;
            }
            if (string.IsNullOrEmpty(user.token))
            {
                Response.Write("请先开通托管账户！");
                return null;
            }
            return user;
        }
    }
}
