using System;
using System.Web;
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
            {
                //个人开户/激活
                case (int)Agp2pEnums.SumapayApiEnum.URegi:
                case (int)Agp2pEnums.SumapayApiEnum.Activ:
                    //实名验证接口
                    if(!CheckUserLogin(out user)) return;
                    reqMsg = new UserRegisterReqMsg(user.id, user.mobile, user.real_name, user.id_card_number, user.token);
                    break;
                //跳转托管账户
                case (int)Agp2pEnums.SumapayApiEnum.Accou:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new UserToAccountReqMsg(user.id);
                    break;
                //个人自动投标续约
                case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new AutoBidSignReqMsg(user.id);
                    break;
                //个人自动投标取消
                case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                    if (!CheckUserLogin(out user)) return;
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
                //个人投标 普通/集合项目
                case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                case (int)Agp2pEnums.SumapayApiEnum.McBid:
                    reqMsg = new ManualBidReqMsg(DTRequest.GetQueryInt("userId", 0),
                        DTRequest.GetQueryString("projectCode"), DTRequest.GetQueryString("sum"),
                        DTRequest.GetQueryString("projectSum"), DTRequest.GetQueryString("projectDescription"),
                        requestApi == (int) Agp2pEnums.SumapayApiEnum.McBid);
                    break;
                //个人存管账户还款普通/集合项目
                case (int)Agp2pEnums.SumapayApiEnum.MaRep:
                case (int)Agp2pEnums.SumapayApiEnum.McRep:
                    reqMsg = new AccountRepayReqMsg(DTRequest.GetQueryInt("userId", 0), DTRequest.GetQueryString("projectCode"), DTRequest.GetQueryString("sum"),
                        "http://" + HttpContext.Current.Request.Url.Authority.ToLower() +
                        "/user/center/index.html#/recharge", requestApi == (int) Agp2pEnums.SumapayApiEnum.McRep);
                    reqMsg.Remarks = $"isEarly=false&repayTaskId={DTRequest.GetQueryString("repayTaskId")}";
                    break;
                //个人协议还款普通/集合项目
                case (int)Agp2pEnums.SumapayApiEnum.BaRep:
                case (int)Agp2pEnums.SumapayApiEnum.BcRep:
                    reqMsg = new BankRepayReqMsg(DTRequest.GetQueryInt("userId", 0), DTRequest.GetQueryString("projectCode"), DTRequest.GetQueryString("sum")
                        , requestApi == (int)Agp2pEnums.SumapayApiEnum.BcRep);
                    reqMsg.Remarks = $"isEarly=false&repayTaskId={DTRequest.GetQueryString("repayTaskId")}";
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

        private bool CheckUserLogin(out dt_users user)
        {
            user = BasePage.GetUserInfoByLinq();
            if (user == null)
            {
                Response.Write("对不起，用户尚未登录或已超时！");
                return false;
            }
            if (string.IsNullOrEmpty(user.token))
            {
                Response.Write("请先开通托管账户！");
                return false;
            }
            return true;
        }
    }
}
