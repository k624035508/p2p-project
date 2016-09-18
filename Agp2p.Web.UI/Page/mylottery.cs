using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agp2p.Web.UI.Page
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

        private static readonly int[] LotteryType =
        {
            (int) Agp2pEnums.ActivityTransactionActivityTypeEnum.HongBao,
            (int) Agp2pEnums.ActivityTransactionActivityTypeEnum.InterestRateTicket
        };

        private readonly Dictionary<int, Func<li_activity_transactions, Dictionary<string, string>>> typeSourceMap = new Dictionary
            <int, Func<li_activity_transactions, Dictionary<string, string>>>
        {
            {
                (int) Agp2pEnums.ActivityTransactionActivityTypeEnum.HongBao, atr => new Dictionary<string, string>
                {
                    //{ "lottery-value", "￥" + ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value").ToString("F0") },
                    {"lottery-value", atr.value.ToString("f0") + "元" },
                    {"lottery-spec", "积分商城兑换"},
                    {"lottery-source", "红包"},
                    {"lottery-condition", "投资" + ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("InvestUntil") + "元以上可用"},
                    {"lottery-valid-time", 
                             "有效期至" + ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<DateTime>("Deadline").ToString("yyyy-MM-dd")                            
                    }
                }
            },
        };

        
        protected List<Dictionary<string, string>> QueryLottery()
        {
            var context = new Agp2pDataContext();
            var query = context.li_activity_transactions.Where(a => a.user_id == userModel.id && LotteryType.Contains(a.activity_type));
            if (lottery_status != 0)
            {
                query = query.Where(a => a.status == lottery_status);
            }
            return query.AsEnumerable().Select(atr => new Dictionary<string, string>
            {
                { "lottery-face-class", atr.status == (int) Agp2pEnums.ActivityTransactionStatusEnum.Acting ? 
                        atr.activity_type == (int) Agp2pEnums.ActivityTransactionActivityTypeEnum.HongBao ? "lottery-face-hongbao" : "lottery-face-jiaxi" : "lottery-face-grey" },
                { "lottery-spec", "积分商城兑换" },
                { "lottery-valid-time", "有效期至" + ((JObject)JsonConvert.DeserializeObject(atr.details)).Value<DateTime>("Deadline").ToString("yyyy-MM-dd") },
                { "lottery-condition", atr.activity_type == (int) Agp2pEnums.ActivityTransactionActivityTypeEnum.HongBao ?
                    "投资" + ((JObject)JsonConvert.DeserializeObject(atr.details)).Value<int>("InvestUntil") + "元以上可用" :
                    "投资" + ((JObject)JsonConvert.DeserializeObject(atr.details)).Value<int>("minInvestValue")/10000 + "万元以下可用"
                },
                {"lottery-source", atr.activity_type == (int) Agp2pEnums.ActivityTransactionActivityTypeEnum.HongBao ? "红包" : "加息券"},
                { "lottery-value", atr.activity_type == (int) Agp2pEnums.ActivityTransactionActivityTypeEnum.HongBao ? (int)atr.value + "元" : "1%" }
            }).ToList();
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
