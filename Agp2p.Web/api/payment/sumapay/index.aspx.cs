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
        private const string GroupName = "融资合作组";

        protected void Page_Load(object sender, EventArgs e)
        {
            //TODO 在页面显示错误提示
            dt_users user = null;
            BaseReqMsg reqMsg = null;
            int requestApi = DTRequest.GetQueryInt("api", 0);
            switch (requestApi)
            {
                //个人/企业开户
                case (int)Agp2pEnums.SumapayApiEnum.URegi:
                    if(!CheckUserLogin(out user, false)) return;
                    reqMsg = user.dt_user_groups.title.Equals(GroupName) ? new CompanyRegisterReqMsg(user.id) : new UserRegisterReqMsg(user.id, user.mobile, user.real_name, user.id_card_number, user.token);
                    break;
                case (int)Agp2pEnums.SumapayApiEnum.URegM:
                    if (!CheckUserLogin(out user, false)) return;
                    reqMsg = new UserRegisterMoblieReqMsg(user.id, user.mobile, user.real_name, user.id_card_number, user.token,
                        HttpContext.Current.Request.UrlReferrer.ToString().ToLower());
                    break;
                //个人账户激活
                case (int)Agp2pEnums.SumapayApiEnum.Activ:
                    if (!CheckUserLogin(out user, false)) return;
                    reqMsg = new UserActivateReqMsg(user.id);
                    break;
                //跳转托管账户
                case (int)Agp2pEnums.SumapayApiEnum.Accou:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = user.dt_user_groups.title.Equals(GroupName) ? new CompanyToAccountReqMsg(user.id) : new UserToAccountReqMsg(user.id);
                    break;
                case (int)Agp2pEnums.SumapayApiEnum.AccoM:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new UserToAccountReqMsg(user.id, HttpContext.Current.Request.UrlReferrer.ToString().ToLower());
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
                //个人/企业 自动账户/银行还款开通
                case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = user.dt_user_groups.title.Equals(GroupName) ?
                        new CompanyRepaySignReqMsg(user.id, DTRequest.GetQueryInt("projectCode"), DTRequest.GetQueryString("repayLimit")) :
                        new AutoRepaySignReqMsg(user.id, DTRequest.GetQueryInt("projectCode"), DTRequest.GetQueryString("repayLimit"), false);
                    break;
                case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new AutoRepaySignReqMsg(user.id, DTRequest.GetQueryInt("projectCode"), DTRequest.GetQueryString("repayLimit"), true);
                    break;
                //个人/企业 自动还款取消
                case (int)Agp2pEnums.SumapayApiEnum.ClRep:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = user.dt_user_groups.title.Equals(GroupName) ?
                        new CompanyAutoRepayCancelReqMsg(user.id, DTRequest.GetQueryInt("projectCode")) :
                        new AutoRepayCancelReqMsg(user.id, DTRequest.GetQueryInt("projectCode"));
                    break;
                //个人网银充值
                case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = user.dt_user_groups.title.Equals(GroupName) ?
                         new CompanyWebRechargeReqMsg(user.id, DTRequest.GetQueryString("sum"), DTRequest.GetQueryString("bankCode")) :
                    new WebRechargeReqMsg(user.id, DTRequest.GetQueryString("sum"), DTRequest.GetQueryString("bankCode"));
                    break;
                //个人一键充值
                case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new WhRechargeReqMsg(user.id,
                            DTRequest.GetQueryString("sum"));
                    break;
                //个人移动端一键充值
                case (int)Agp2pEnums.SumapayApiEnum.WhReM:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new WhRechargeReqMsg(user.id,
                            DTRequest.GetQueryString("sum"), DTRequest.GetQueryString("backUrl"), "", "3", "", "");
                    break;
                //个人/企业 提现
                case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = user.dt_user_groups.title.Equals(GroupName) ?
                        new CompanyWithdrawReqMsg(user.id, DTRequest.GetQueryString("sum"), DTRequest.GetQueryString("bankId"), DTRequest.GetQueryString("bankName"), DTRequest.GetQueryString("bankAccount")) :
                        new WithdrawReqMsg(user.id, DTRequest.GetQueryString("sum"), DTRequest.GetQueryString("bankId"), DTRequest.GetQueryString("bankName"), DTRequest.GetQueryString("bankAccount"), "3", "", "");
                    break;
                //个人移动端提现
                case (int)Agp2pEnums.SumapayApiEnum.WdraM:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new WithdrawReqMsg(user.id, DTRequest.GetQueryString("sum"),
                        DTRequest.GetQueryString("bankId"), DTRequest.GetQueryString("backUrl"), "3", "", "");
                    break;
                //个人投标 普通/集合项目
                case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                case (int)Agp2pEnums.SumapayApiEnum.McBid:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new ManualBidReqMsg(user.id,
                        DTRequest.GetQueryInt("projectCode"), DTRequest.GetQueryString("sum"),
                        DTRequest.GetQueryString("projectSum"), DTRequest.GetQueryString("projectDescription"),
                        requestApi == (int) Agp2pEnums.SumapayApiEnum.McBid);
                    break;
                //个人移动端投标 普通/集合项目
                case (int)Agp2pEnums.SumapayApiEnum.MaBiM:
                case (int)Agp2pEnums.SumapayApiEnum.McBiM:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new ManualBidReqMsg(user.id,
                        DTRequest.GetQueryInt("projectCode"), DTRequest.GetQueryString("sum"),
                        DTRequest.GetQueryString("projectSum"), DTRequest.GetQueryString("projectDescription"),
                        DTRequest.GetQueryString("backUrl"), requestApi == (int) Agp2pEnums.SumapayApiEnum.McBid);
                    break;
                //个人/企业 存管账户还款普通/集合项目 TODO Remarks移动到RequestApiHandle处理
                case (int)Agp2pEnums.SumapayApiEnum.MaRep:
                case (int)Agp2pEnums.SumapayApiEnum.McRep:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = user.dt_user_groups.title.Equals(GroupName) ?
                        new CompanyAccountRepayReqMsg(user.id, DTRequest.GetQueryInt("projectCode"), DTRequest.GetQueryString("sum"),
                        "http://" + HttpContext.Current.Request.Url.Authority.ToLower() +
                        "/user/center/index.html#/recharge", requestApi == (int)Agp2pEnums.SumapayApiEnum.McRep) :
                        new AccountRepayReqMsg(user.id, DTRequest.GetQueryInt("projectCode"), DTRequest.GetQueryString("sum"),
                        "http://" + HttpContext.Current.Request.Url.Authority.ToLower() +
                        "/user/center/index.html#/recharge", requestApi == (int) Agp2pEnums.SumapayApiEnum.McRep);
                    reqMsg.Remarks = $"isEarly=false&repayTaskId={DTRequest.GetQueryString("repayTaskId")}";
                    break;
                //个人协议还款普通/集合项目 TODO Remarks移动到RequestApiHandle处理
                case (int)Agp2pEnums.SumapayApiEnum.BaRep:
                case (int)Agp2pEnums.SumapayApiEnum.BcRep:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new BankRepayReqMsg(user.id, DTRequest.GetQueryInt("projectCode"), DTRequest.GetQueryString("sum")
                        , requestApi == (int)Agp2pEnums.SumapayApiEnum.BcRep);
                    reqMsg.Remarks = $"isEarly=false&repayTaskId={DTRequest.GetQueryString("repayTaskId")}";
                    break;
                //债权转让
                case (int)Agp2pEnums.SumapayApiEnum.CreAs:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new CreditAssignmentReqMsg(user.id, DTRequest.GetQueryInt("claimId"), DTRequest.GetQueryString("undertakeSum"));
                    break;
                //移动端债权转让
                case (int)Agp2pEnums.SumapayApiEnum.CreAM:
                    if (!CheckUserLogin(out user)) return;
                    reqMsg = new CreditAssignmentReqMsg(user.id, DTRequest.GetQueryInt("claimId"), DTRequest.GetQueryString("undertakeSum"), DTRequest.GetQueryString("backUrl"), "1", "", "");
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

        private bool CheckUserLogin(out dt_users user, bool checkToken = true)
        {
            user = BasePage.GetUserInfoByLinq();
            if (user == null)
            {
                Response.Write("对不起，用户尚未登录或已超时！");
                return false;
            }
            if (checkToken && string.IsNullOrEmpty(user.identity_id))
            {
                Response.Write("请先开通托管账户！");
                return false;
            }
            return true;
        }
    }
}
