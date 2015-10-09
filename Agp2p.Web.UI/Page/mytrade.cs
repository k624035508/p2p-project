using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using Agp2p.Core;
using Newtonsoft.Json;

namespace Agp2p.Web.UI.Page
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
                        his.li_project_transactions.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest
                            ? "投资项目[{0}]"
                            : "投资放款[{0}]", his.li_project_transactions.li_projects.title);
            }
            if (his.li_bank_transactions != null)
                return his.li_bank_transactions.remarks;
            return his.li_activity_transactions != null ? his.li_activity_transactions.remarks : "";
        }

        private static readonly Dictionary<Agp2pEnums.TransactionDetailsDropDownListEnum, Agp2pEnums.WalletHistoryTypeEnum[]>
            MyTradeTypeMapHistoryEnum = new Dictionary<Agp2pEnums.TransactionDetailsDropDownListEnum, Agp2pEnums.WalletHistoryTypeEnum[]>
            {
                {
                    Agp2pEnums.TransactionDetailsDropDownListEnum.Charge, new[] {
                        Agp2pEnums.WalletHistoryTypeEnum.Charging, Agp2pEnums.WalletHistoryTypeEnum.ChargeConfirm, Agp2pEnums.WalletHistoryTypeEnum.ChargeCancel
                    }
                },
                {
                    Agp2pEnums.TransactionDetailsDropDownListEnum.Withdraw, new[] {
                        Agp2pEnums.WalletHistoryTypeEnum.Withdrawing, Agp2pEnums.WalletHistoryTypeEnum.WithdrawConfirm, Agp2pEnums.WalletHistoryTypeEnum.WithdrawCancel
                    }
                },
                {
                    Agp2pEnums.TransactionDetailsDropDownListEnum.Invest, new[] {
                        Agp2pEnums.WalletHistoryTypeEnum.Invest, /*Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess,*/ 
                    }
                },
                {
                    Agp2pEnums.TransactionDetailsDropDownListEnum.Repay, new[] {
                        Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest, Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipal, Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipalAndInterest
                    }
                },
                {
                    Agp2pEnums.TransactionDetailsDropDownListEnum.Others, new[] {
                        Agp2pEnums.WalletHistoryTypeEnum.InvestorRefund,
                        Agp2pEnums.WalletHistoryTypeEnum.Gaining, Agp2pEnums.WalletHistoryTypeEnum.Losting,
                        Agp2pEnums.WalletHistoryTypeEnum.GainConfirm, Agp2pEnums.WalletHistoryTypeEnum.GainCancel,
                        Agp2pEnums.WalletHistoryTypeEnum.LostConfirm, Agp2pEnums.WalletHistoryTypeEnum.LostCancel,
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
        protected static List<li_wallet_histories> query_transaction_history(int userId, Agp2pEnums.TransactionDetailsDropDownListEnum type, int pageIndex,
            string startTime, string endTime, short pageSize, out int count)
        {
            var context = new Agp2pDataContext();
            var query = context.li_wallet_histories.Where(h => h.user_id == userId);
            query = query.Where(w =>
                        w.action_type != (int) Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess &&
                        w.action_type != (int) Agp2pEnums.WalletHistoryTypeEnum.Charging &&
                        w.action_type != (int)Agp2pEnums.WalletHistoryTypeEnum.ChargeCancel); // 屏蔽项目满标的历史，没必要展示

            if (MyTradeTypeMapHistoryEnum.Keys.Contains(type))
                query = query.Where(w => MyTradeTypeMapHistoryEnum[type].Cast<int>().Contains(w.action_type));
            if (startTime != "")
                query = query.Where(h => Convert.ToDateTime(startTime) <= h.create_time);
            if (endTime != "")
                query = query.Where(h => h.create_time <= Convert.ToDateTime(endTime));

            count = query.Count();
            return query.OrderByDescending(h => h.id).Skip(pageSize * pageIndex).Take(pageSize).ToList();
        }

        [WebMethod]
        public static string AjaxQueryTransactionHistory(short type, short pageIndex, short pageSize, string startTime = "", string endTime = "")
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            int count;
            var his = query_transaction_history(userInfo.id, (Agp2pEnums.TransactionDetailsDropDownListEnum)type, pageIndex, startTime, endTime, pageSize, out count);
            var os = his.Select(h => new
            {
                h.id,
                type = Utils.GetAgp2pEnumDes((Agp2pEnums.WalletHistoryTypeEnum) h.action_type),
                income = QueryTransactionIncome(h),
                outcome = QueryTransactionOutcome(h),
                idleMoney = h.idle_money.ToString("c"),
                createTime = h.create_time.ToString("yyyy/MM/dd HH:mm"),
                remark = QueryRemark(h)
            });
            return JsonConvert.SerializeObject(new { totalCount = count, data = os });
        }
    }
}
