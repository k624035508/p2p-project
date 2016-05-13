using System;
using System.Linq;
using System.Transactions;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Agp2p.Core.Message;
using TinyMessenger;
using System.Diagnostics;
using System.Threading;

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

                var now = DateTime.Now;
                context.MarkEventOccurNotSave(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, now.AddHours(-1));
                context.MarkEventOccurNotSave(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, now.AddHours(-2));
                context.MarkEventOccurNotSave(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, now.AddHours(-3));

                context.MarkEventOccurNotSave(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, now.AddDays(-1).AddHours(-1));
                context.MarkEventOccurNotSave(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, now.AddDays(-1).AddHours(-2));
                context.MarkEventOccurNotSave(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, now.AddDays(-1).AddHours(-3));

                context.SubmitChanges();

                Assert.AreEqual(3, context.QueryEventTimesDuring(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, TimeSpan.FromDays(1)));
                Assert.AreEqual(6, context.QueryEventTimesDuring(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, TimeSpan.FromDays(2)));

                ts.Dispose();
            }
        }

        [TestMethod]
        public void TestMsgCallback()
        {
            MessageBus.Main.Subscribe<GenericTinyMessage<string>>(m => Debug.WriteLine("Handling msg: " + m.Content));
            MessageBus.Main.PublishAsync(new GenericTinyMessage<string>("sender", "123"), msg => {
                Debug.WriteLine("async callback: " + msg.Content);
            });
            Thread.Sleep(1000);
        }
    }
}
