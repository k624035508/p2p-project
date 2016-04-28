using System;
using System.Linq;
using System.Transactions;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_Misc
    {
        private const string UserA = "13535656867";

        [TestMethod]
        public void TestEventRecordUtils()
        {
            using (var ts = new TransactionScope())
            {
                var context = new Agp2pDataContext();
                var user = context.dt_users.Single(u => u.user_name == UserA);

                context.li_event_records.InsertOnSubmit(NewRecord(user.id, DateTime.Now.AddHours(-1)));
                context.li_event_records.InsertOnSubmit(NewRecord(user.id, DateTime.Now.AddHours(-2)));
                context.li_event_records.InsertOnSubmit(NewRecord(user.id, DateTime.Now.AddHours(-3)));

                context.li_event_records.InsertOnSubmit(NewRecord(user.id, DateTime.Now.AddDays(-1).AddHours(-1)));
                context.li_event_records.InsertOnSubmit(NewRecord(user.id, DateTime.Now.AddDays(-1).AddHours(-2)));
                context.li_event_records.InsertOnSubmit(NewRecord(user.id, DateTime.Now.AddDays(-1).AddHours(-3)));

                context.SubmitChanges();

                Assert.AreEqual(3, context.QueryEventTimesDuring(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, TimeSpan.FromDays(1)));
                Assert.AreEqual(6, context.QueryEventTimesDuring(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, TimeSpan.FromDays(2)));

                ts.Dispose();
            }
        }

        private static li_event_records NewRecord(int userId, DateTime occurAt)
        {
            return new li_event_records
            {
                userId = userId,
                eventType = Agp2pEnums.EventRecordTypeEnum.IdcardChecking,
                occurAt = occurAt
            };
        }
    }
}
