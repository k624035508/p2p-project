using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lip2p.Web.UI.Page
{
    public class mylottery : usercenter
    {
        protected int lottery_status;

        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();
            lottery_status = DTRequest.GetQueryInt("lottery_status");
        }

        private readonly Dictionary<int, Func<li_activity_transactions, Dictionary<string, string>>> typeSourceMap = new Dictionary
            <int, Func<li_activity_transactions, Dictionary<string, string>>>
        {
            {
                (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.HongBaoActivation,
                atr => new Dictionary<string, string>
                {
                    {"lottery-value", "￥" + atr.value.ToString("F0")},
                    {"lottery-spec", "投 " + QueryDetails(atr, "InvestUntil", "?") + " 送 " + atr.value.ToString("F0")},
                    {"lottery-source", "注册奖励红包"},
                    {"lottery-condition", "投资满 " + QueryDetails(atr, "InvestUntil", "?") + " 元可用"},
                }
            },
            {
                (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.Trial, atr => new Dictionary<string, string>
                {
                    { "lottery-value", "￥" + ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value").ToString("F0") },
                    {"lottery-spec", "新手标体验券，利润归您"},
                    {"lottery-source", "新手注册送体验券"},
                    {"lottery-condition", "只可用于投资新手标"},
                    {
                        "lottery-valid-time", atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Acting
                            ? atr.create_time.ToString("yy/MM/dd") + "-" + atr.create_time.AddDays(30).ToString("yy/MM/dd")
                            : (atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Confirm
                                ? "于 " + Convert.ToDateTime(QueryDetails(atr, "RepayTime")).ToString("yyyy.MM.dd") + " 收益"
                                : "已过期")
                    }
                }
            },
            {
                (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject, atr => new Dictionary<string, string>
                {
                    { "lottery-face-class", atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Acting ? "lottery-face-red" : "lottery-face-grey" },
                    { "lottery-value", "￥" + ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value").ToString("F0") },
                    {"lottery-spec", "投多少返多少"},
                    {"lottery-source", "投资送天标券"},
                    {"lottery-condition", "只可用于投资天标"},
                    {
                        "lottery-valid-time", atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Acting
                            ? atr.create_time.ToString("yy/MM/dd") + "-" + Convert.ToDateTime(QueryDetails(atr, "Deadline")).ToString("yy/MM/dd")
                            : (atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Confirm
                                ? "于 " + Convert.ToDateTime(QueryDetails(atr, "RepayTime")).ToString("yyyy.MM.dd") + " 收益"
                                : "已过期")
                    }
                }
            },
        };

        private static readonly int[] LotteryType =
        {
            (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.HongBaoActivation,
            (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.Trial,
            (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject,
        };
        protected List<Dictionary<string, string>> QueryLottery()
        {
            var context = new Lip2pDataContext();
            var query = context.li_activity_transactions.Where(a => a.user_id == userModel.id && LotteryType.Contains(a.activity_type));
            if (lottery_status != 0)
            {
                query = query.Where(a => a.status == lottery_status);
            }
            return query.AsEnumerable().Select(atr => new Dictionary<string, string>
            {
                { "lottery-face-class", atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Acting ? "lottery-face" : "lottery-face-grey" },
                {
                    "lottery-valid-time", atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Acting
                        ? "长期有效"
                        : (atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Confirm ? "已使用" : "已过期")
                }
            }.Merge(typeSourceMap[atr.activity_type](atr))).ToList();
        }

        protected static string QueryDetails(li_activity_transactions tr, string key, string defVal = "")
        {
            if (string.IsNullOrWhiteSpace(tr.details))
            {
                return defVal;
            }
            var jsonObj = (JObject)JsonConvert.DeserializeObject(tr.details);
            return jsonObj.Value<string>(key) ?? defVal;
        }
    }
}
