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
    public class UnitTest_HongBao
    {
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        /*
            测试流程：
                Day 1:
                    创建 2 日 项目 PT3，借款 30000，募集 3 日，发标
                    给用户发红包
                    用户 A 投资 5000 到 PT3
                    用户 A 投资 5000 到 PT3
                    项目截标，放款
                Day 2:
                Day 3:
                    检查红包奖励，应该获得 75 元


        */

        readonly DateTime realDate = new DateTime(2016, 7, 4, 17, 0, 0); /* 开始测试前请设置好实际日期 */

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            // 准备好之后注释这行
            //throw new InvalidOperationException("1. 备份好数据库；2. 设置实际日期");
        }

        [TestMethod]
        public void Day01()
        {
            Common.DeltaDay(realDate, 0);

            Common.AutoRepaySimulate();

            Common.PublishProject("PT3", 2, 30000, 5);

            var investor = new Agp2pDataContext().dt_users.Single(u => u.user_name == UserA);
            HongBaoActivity.GiveUser(investor.id, 100, 10);
            Common.InvestProject(UserA, "PT3", 5000);
            Common.InvestProject(UserA, "PT3", 5000);

            Common.InvestProject(UserB, "PT3", 20000);
            Common.ProjectStartRepay("PT3");

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
            var p = context.li_projects.Single(p0 => p0.title == "PT3");
            context.ExecuteRepaymentTask(p.li_repayment_tasks.Single().id, Agp2pEnums.RepaymentStatusEnum.ManualPaid);

            Common.AssertWalletDelta(UserA, 2.78m + 75, 0m, 0m, 0m, 0, 0, 10000, 2.78m, realDate);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(realDate);
            Common.RestoreDate(realDate);
        }
    }
}
