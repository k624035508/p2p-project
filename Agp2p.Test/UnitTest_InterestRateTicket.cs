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
    public class UnitTest_InterestRateTicket
    {
        private const string UserA = "13535656867";
        /*
            测试流程：
                Day 1:
                    创建 2 日 项目 PT2，借款 30000，募集 3 日，发标
                    给予用户 A 加息券 加息 3%
                    A 投 PT2 30000 并使用加息券
                Day 2:
                Day 3:
                    检查用户收益


        */

        readonly DateTime realDate = new DateTime(2016, 7, 4, 14, 30, 0); /* 开始测试前请设置好实际日期 */

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

            Common.PublishProject("PT2", 2, 30000, 5);

            var investor = new Agp2pDataContext().dt_users.Single(u => u.user_name == UserA);
            var ticketId = InterestRateTicketActivity.GiveUser(investor.id, 3, 10000, 1);
            Common.InvestProjectWithTicket(UserA, "PT2", 30000, ticketId);
            Common.ProjectStartRepay("PT2");

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
            // 手动还款
            var context = new Agp2pDataContext();
            var p = context.li_projects.Single(p0 => p0.title == "PT2");
            context.ExecuteRepaymentTask(p.li_repayment_tasks.Single().id, Agp2pEnums.RepaymentStatusEnum.ManualPaid);

            Common.AssertWalletDelta(UserA, 13.33m, 0m, 0m, 0m, 0, 0, 30000, 13.33m, realDate);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(realDate);
            Common.RestoreDate(realDate);
        }
    }
}
