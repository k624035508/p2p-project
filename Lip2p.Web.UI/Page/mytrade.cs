using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using Lip2p.Core;
using Newtonsoft.Json;

namespace Lip2p.Web.UI.Page
{
    public partial class mytrade : usercenter
    {
        static readonly protected short PageSize = 8;
        protected int transaction_type;
        protected int page;
        protected long time_span_start;
        protected long time_span_end;

        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();
            transaction_type = DTRequest.GetQueryInt("transaction_type");
            page = Math.Max(1, DTRequest.GetQueryInt("page"));
            time_span_start = DTRequest.GetQueryLong("time_span_start");
            time_span_end = DTRequest.GetQueryLong("time_span_end");
        }

        protected static string QueryTransactionIncome(li_wallet_histories his)
        {
            return TransactionFacade.QueryTransactionIncome<decimal?>(his);
        }

        protected static string QueryTransactionOutcome(li_wallet_histories his)
        {
            var outcome = TransactionFacade.QueryTransactionOutcome(his);
            return outcome == null ? "" : outcome.Value.ToString();
        }

        protected static string QueryRemark(li_wallet_histories his)
        {
            if (his.li_project_transactions != null)
            {
                return
                    string.Format(
                        his.li_project_transactions.type == (int)Lip2pEnums.ProjectTransactionTypeEnum.Invest
                            ? "投资项目[{0}]"
                            : "投资放款[{0}]", his.li_project_transactions.li_projects.title);
            }
            if (his.li_bank_transactions != null)
                return his.li_bank_transactions.remarks;
            return his.li_activity_transactions != null ? his.li_activity_transactions.remarks : "";
        }

        private static readonly Dictionary<Lip2pEnums.TransactionDetailsDropDownListEnum, Lip2pEnums.WalletHistoryTypeEnum[]>
            MyTradeTypeMapHistoryEnum = new Dictionary<Lip2pEnums.TransactionDetailsDropDownListEnum, Lip2pEnums.WalletHistoryTypeEnum[]>
            {
                {
                    Lip2pEnums.TransactionDetailsDropDownListEnum.Charge, new[] {
                        Lip2pEnums.WalletHistoryTypeEnum.Charging, Lip2pEnums.WalletHistoryTypeEnum.ChargeConfirm, Lip2pEnums.WalletHistoryTypeEnum.ChargeCancel
                    }
                },
                {
                    Lip2pEnums.TransactionDetailsDropDownListEnum.Withdraw, new[] {
                        Lip2pEnums.WalletHistoryTypeEnum.Withdrawing, Lip2pEnums.WalletHistoryTypeEnum.WithdrawConfirm, Lip2pEnums.WalletHistoryTypeEnum.WithdrawCancel
                    }
                },
                {
                    Lip2pEnums.TransactionDetailsDropDownListEnum.Invest, new[] {
                        Lip2pEnums.WalletHistoryTypeEnum.Invest, /*Lip2pEnums.WalletHistoryTypeEnum.InvestSuccess,*/ Lip2pEnums.WalletHistoryTypeEnum.InvestorRefund
                    }
                },
                {
                    Lip2pEnums.TransactionDetailsDropDownListEnum.Repay, new[] {
                        Lip2pEnums.WalletHistoryTypeEnum.RepaidInterest, Lip2pEnums.WalletHistoryTypeEnum.RepaidPrincipal, Lip2pEnums.WalletHistoryTypeEnum.RepaidPrincipalAndInterest
                    }
                },
                {
                    Lip2pEnums.TransactionDetailsDropDownListEnum.Others, new[] {
                        Lip2pEnums.WalletHistoryTypeEnum.Gaining, Lip2pEnums.WalletHistoryTypeEnum.Losting,
                        Lip2pEnums.WalletHistoryTypeEnum.GainConfirm, Lip2pEnums.WalletHistoryTypeEnum.GainCancel,
                        Lip2pEnums.WalletHistoryTypeEnum.LostConfirm, Lip2pEnums.WalletHistoryTypeEnum.LostCancel,
                    }
                }
            };

        /// <summary>
        /// 查询交易明细
        /// </summary>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <returns></returns>
        protected static List<li_wallet_histories> query_transaction_history(int userId, Lip2pEnums.TransactionDetailsDropDownListEnum type, int pageIndex,
            long startTick, long endTick, short pageSize, out int count)
        {
            var context = new Lip2pDataContext();
            var query = context.li_wallet_histories.Where(h => h.user_id == userId);
            query = query.Where(w =>
                        w.action_type != (int) Lip2pEnums.WalletHistoryTypeEnum.InvestSuccess &&
                        w.action_type != (int) Lip2pEnums.WalletHistoryTypeEnum.Charging &&
                        w.action_type != (int)Lip2pEnums.WalletHistoryTypeEnum.ChargeCancel); // 屏蔽项目满标的历史，没必要展示

            if (MyTradeTypeMapHistoryEnum.Keys.Contains(type))
                query = query.Where(w => MyTradeTypeMapHistoryEnum[type].Cast<int>().Contains(w.action_type));
            if (startTick != 0)
                query = query.Where(h => new DateTime(startTick) <= h.create_time);
            if (endTick != 0)
                query = query.Where(h => h.create_time <= new DateTime(endTick));

            count = query.Count();
            return query.OrderByDescending(h => h.id).Skip(pageSize * pageIndex).Take(pageSize).ToList();
        }

        [WebMethod]
        public static string AjaxQueryTransactionHistory(short pageIndex, short pageSize)
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            int count;
            var his = query_transaction_history(userInfo.id, Lip2pEnums.TransactionDetailsDropDownListEnum.All, pageIndex, 0, 0, pageSize, out count);
            var os = his.Select(h => new
            {
                h.id,
                type = Utils.GetLip2pEnumDes((Lip2pEnums.WalletHistoryTypeEnum) h.action_type),
                income = QueryTransactionIncome(h),
                outcome = QueryTransactionOutcome(h),
                idleMoney = h.idle_money.ToString("c"),
                createTime = h.create_time.ToString("yyyy/MM/dd HH:mm"),
                remark = QueryRemark(h)
            });
            return JsonConvert.SerializeObject(os);
        }
    }
}
