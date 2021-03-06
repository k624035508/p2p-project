﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Core.Message.UserPointMsg;
using Agp2p.Core.Message.PayApiMsg.Transaction;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;

namespace Agp2p.Web.UI.Page
{
    public class mycard : usercenter
    {
        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();
        }

        [WebMethod]
        public new static string AjaxAppendCard(string cardNumber, string bankName)
        {
            var userInfo = GetUserInfoByLinq();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            // 检查用户的输入
            if (!new Regex(@"^\d{16,}$").IsMatch(cardNumber))
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "银行卡号格式不正确";
            }
            if (!new Regex(@"^[\u4e00-\u9fa5]+$").IsMatch(bankName))
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "银行名称格式不正确";
            }
            
            var context = new Agp2pDataContext();
            var alreadyHave = userInfo.li_bank_accounts.Any(c => c.account == cardNumber);
            if (alreadyHave)
            {
                HttpContext.Current.Response.StatusCode = (int) HttpStatusCode.Conflict;
                return "你已经添加了卡号为 " + cardNumber + " 的银行卡，不能重复添加";
            }

            if (3 <= userInfo.li_bank_accounts.Count)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "最多只能添加 3 张银行卡";
            }

            //查询该客户是否已经在丰付绑定了银行卡
            var user = context.dt_users.Single(u => u.id == userInfo.id);
            var req = new SignBankCardQueryRequest(user.id);
            MessageBus.Main.Publish(req);
            var resp = BaseRespMsg.NewInstance<SignBankCardQueryRespone>(req.SynResult);
            if (resp.RechargeProtocolList != null)
            {
                if (resp.CheckRechargeProtocol(bankName, cardNumber))
                {
                    var card = new li_bank_accounts
                    {
                        dt_users = user,
                        account = cardNumber,
                        bank = bankName,
                        last_access_time = DateTime.Now,
                        opening_bank = "",
                        location = "",
                        type = (int)Common.Agp2pEnums.BankAccountType.QuickPay,
                    };
                    context.li_bank_accounts.InsertOnSubmit(card);
                    context.SubmitChanges();
                    var msg = new UserPointMsg(user.id, user.user_name, (int)Agp2pEnums.PointEnum.BindingBank);
                    MessageBus.Main.Publish(msg);
                    return "保存银行卡信息成功";
                }
                else
                {
                    return "添加银行卡失败，您输入的银行卡号与丰付平台绑定的银行卡号不一致！";
                }
            }
            var cardUnknown = new li_bank_accounts
            {
                dt_users = user,
                account = cardNumber,
                bank = bankName,
                last_access_time = DateTime.Now,
                opening_bank = "",
                location = "",
                type = (int)Common.Agp2pEnums.BankAccountType.Unknown,
            };
            context.li_bank_accounts.InsertOnSubmit(cardUnknown);
            context.SubmitChanges();
            var msg2 = new UserPointMsg(user.id, user.user_name, (int)Agp2pEnums.PointEnum.BindingBank);
            MessageBus.Main.Publish(msg2);
            return "保存银行卡信息成功";
        }

        /// <summary>
        /// 有过提现记录的卡不能修改卡号，简单起见直接不能修改卡号
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="bankName"></param>
        /// <param name="bankLocation"></param>
        /// <param name="openingBank"></param>
        /// <returns></returns>
        [WebMethod]
        public new static string AjaxModifyCard(int cardId, string bankName)
        {
            var userInfo = GetUserInfo();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            var context = new Agp2pDataContext();
            var card = context.li_bank_accounts.SingleOrDefault(c => c.owner == userInfo.id && c.id == cardId);
            if (card == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return "找不到卡的信息";
            }
            // 检查用户的输入
            if (!new Regex(@"^[\u4e00-\u9fa5]+$").IsMatch(bankName))
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "银行名称格式不正确";
            }

            card.bank = bankName;

            context.SubmitChanges();
            return "修改成功";
        }

        [WebMethod]
        public new static string AjaxDeleteCard(int cardId)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var card = context.li_bank_accounts.SingleOrDefault(c => c.owner == userInfo.id && c.id == cardId);
            if (card == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return "该卡已被删除";
            }
            if (card.li_bank_transactions.Any())
            {
                return "该卡已关联有提现操作，无法删除";
            }
            context.li_bank_accounts.DeleteOnSubmit(card);
            context.SubmitChanges();
            return "删除成功";
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static string AjaxQueryCardInfo(int cardId) // 手机用到
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var context = new Agp2pDataContext();
            var card = context.li_bank_accounts.SingleOrDefault(c => c.owner == userInfo.id && c.id == cardId);
            if (card == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return "找不到卡的信息";
            }
            return JsonConvert.SerializeObject(new
            {
                CardNumber = card.account,
                BankName = card.bank,
                OpeningBank = card.opening_bank,
                BankLocation = card.location
            });
        }
    }
}
