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
using Agp2p.Core.ActivityLogic;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_TrialTicket
    {
        private const string UserA = "13535656867";
        /*
            测试流程：
                Day 1:
                    创建 2 日 项目 PT1，借款 30000，募集 3 日，发标
                    给予用户 A 体验券 价值 10000
                    A 投体验标
                Day 2:
                Day 3:
                    检查体验券放款


        */

        readonly DateTime realDate = new DateTime(2016, 6, 30, 17, 27, 0); /* 开始测试前请设置好实际日期 */

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            // 准备好之后注释这行
            // throw new InvalidOperationException("1. 备份好数据库；2. 设置实际日期");
        }

        [TestMethod]
        public void Day01()
        {
            Common.DeltaDay(realDate, 0);

            Common.AutoRepaySimulate();

            Common.PublishProject("PT1", 2, 30000, 5);

            var investor = new Agp2pDataContext().dt_users.Single(u => u.user_name == UserA);
            var ticketId = TrialTicketActivity.GiveUser(investor.id, 10000);
            Common.InvestProjectWithTicket(UserA, "PT1", 10000, ticketId);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day02()
        {
            Common.DeltaDay(realDate, 1);
            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day03()
        {
            Common.DeltaDay(realDate, 2);
            Common.AutoRepaySimulate();

            Common.AssertWalletDelta(UserA, 2.78m, 0m, 0m, 0m, 0, 0, 0, 2.78m, realDate);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(realDate);
            Common.RestoreDate(realDate);
        }
    }
}
