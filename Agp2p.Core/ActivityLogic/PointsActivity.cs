using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agp2p.Core.ActivityLogic
{
    public class PointsActivity
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<UserInvestedMsg>(m => UserInvestedPointMsg(m.ProjectTransactionId,m.InvestTime));  //投资送积分
        }

        public class JiFen
        {
            public readonly li_activity_transactions atr;

            public JiFen(li_activity_transactions atr)
            {
                this.atr = atr;
            }

            public void SetInvested(decimal? invested)
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                if (invested == null)
                {
                    if (jsonObj.Value<decimal?>("Invested") != null)
                        jsonObj.Remove("Invested");
                }
                else
                    jsonObj["Invested"] = invested.Value;
                atr.details = jsonObj.ToString(Formatting.None);
            }

            public decimal GetInvested()
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                return jsonObj.Value<decimal?>("Invested") ?? 0;
            }

            public decimal GetInvestUntil()
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                return jsonObj.Value<decimal>("InvestUntil");
            }

            public void Activate(DateTime investTime)
            {
                SetInvested(null);
                atr.status = (byte)Agp2pEnums.ActivityTransactionStatusEnum.Confirm;
                atr.transact_time = investTime;
                atr.remarks = string.Format("投资 {0:c} 激活 {1:c} 积分", GetInvestUntil(), atr.value);
            }

            public DateTime GetDeadline()
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                return Convert.ToDateTime(jsonObj.Value<string>("Deadline"));
            }
        }

        private static void UserInvestedPointMsg(int projectTransactionId, DateTime investTime)
        {
            var context = new Agp2pDataContext();
            var projectTransaction = context.li_project_transactions.SingleOrDefault(p => p.id == projectTransactionId);
            var unactivated = context.li_activity_transactions.Where(a => 
                    a.user_id == projectTransaction.investor && a.status == (int)Agp2pEnums.ActivityTransactionStatusEnum.Acting &&
                    a.type == (int)Agp2pEnums.ActivityTransactionTypeEnum.GainPoint &&
                    a.activity_type == (int)Agp2pEnums.ActivityTransactionActivityTypeEnum.Point).ToList();
            if(!unactivated.Any()) return;

            decimal investAmount = projectTransaction.principal;

        }
    }
}
