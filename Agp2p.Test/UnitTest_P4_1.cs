using System;
using System.Linq;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_P4_1
    {
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        private const string CompanyAccount = "CompanyAccount";
        private const string LoanerUser = "yiqingjixie";
        /*
        P4-1 测试流程：（测试活期延期投资）
            Day 1
                发活期标
                A 投资活期 30000
            Day 2
                发 3 日标，金额 50000，B 投 50000 放款；B 提现 50000；公司账号接手 50000
            Day 3
            Day 4
            Day 5
            Day 6
                回款
        */

        readonly DateTime realDate = UnitTest_Init.TestStartAt; /* 开始测试前请设置好实际日期 */

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            // 准备好之后注释这行
            throw new InvalidOperationException("1. 备份好数据库；2. 设置实际日期");
        }

        [TestMethod]
        public void Day01()
        {
            Common.DeltaDay(realDate, 0);

            Common.MakeSureHaveIdleMoney(UserA, 10 * 10000);
            Common.MakeSureHaveIdleMoney(UserB, 10 * 10000);
            Common.MakeSureHaveIdleMoney(CompanyAccount, 10 * 10000);
            Common.MakeSureHaveIdleMoney(LoanerUser, 10 * 10000);

            Common.AutoRepaySimulate();

            // 发活期标
            Common.PublishHuoqiProject("HP1");
            // A 投资活期 30000
            Common.InvestProject(UserA, "HP1", 30000);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day02()
        {
            Common.DeltaDay(realDate, 1);

            // Common.AutoRepaySimulate();

            // 发 3 日标，金额 50000，B 投 50000 放款；B 提现 50000；公司账号接手 50000
            Common.PublishProject("P4-1", 3, 50000, 5);
            Common.InvestProject(UserB, "P4-1", 50000);
            Common.ProjectStartRepay("P4-1");

            Common.StaticProjectWithdraw("P4-1", UserB, 50000);
            Common.BuyClaim("P4-1", CompanyAccount, 50000);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day03()
        {
            Common.DeltaDay(realDate, 2);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day04()
        {
            Common.DeltaDay(realDate, 3);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day05()
        {
            Common.DeltaDay(realDate, 4);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day06()
        {
            Common.DeltaDay(realDate, 5);

            // 回款，总数应为 20.83
            Common.AutoRepaySimulate();

            Common.AssertWalletDelta(UserA, 8.25m, 0, 0, 0, 10 * 10000, 0, 30000, 8.25m, realDate);
            Common.AssertWalletDelta(UserB, 0m, 0, 0, 0, 10 * 10000, 0, 50000, 0, realDate);
            Common.AssertWalletDelta(CompanyAccount, 12.58m, 0, 0, 0, 10 * 10000, 0, 50000, 20.83m, realDate);
            Assert.AreEqual(0, new Agp2pDataContext().li_projects.Single(p => p.title == "HP1").investment_amount);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(realDate);
            Common.RestoreDate(realDate);
        }
    }
}
