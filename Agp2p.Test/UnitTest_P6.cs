using System;
using System.Linq;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_P6
    {
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        private const string CompanyAccount = "CompanyAccount";
        /*
        P6, P6-1 测试流程：（测试活期自动续投）
            Day 1
                发活期标
                发 5 日标，金额 50000，A 投 50000 放款；A 提现 50000；公司账号接手 50000
            Day 2
            Day 3
                B 投资活期 30000
            Day 4
                发 4 日标 P6-1，金额 50000，A 投 50000 放款；A 提现 20000；公司账号接手 20000
            Day 5
            Day 6
                P6 回款
            Day 7
            Day 8
                P6-1 回款
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

            Common.AutoRepaySimulate();

            // 发活期标
            Common.PublishHuoqiProject("HP1");

            // 发 5 日标，金额 50000，A 投 50000 放款；A 提现 50000；公司账号接手 50000
            Common.PublishProject("P6", 5, 50000, 5);
            Common.InvestProject(UserA, "P6", 50000);
            Common.ProjectStartRepay("P6");

            Common.StaticProjectWithdraw("P6", UserA, 50000);
            Common.BuyClaim("P6", CompanyAccount, 50000);

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

            // B 投资活期 30000
            Common.InvestProject(UserB, "HP1", 30000);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day04()
        {
            Common.DeltaDay(realDate, 3);

            Common.AutoRepaySimulate();

            // 发 4 日标 P6-1，金额 50000，A 投 50000 放款；A 提现 20000；公司账号接手 20000
            Common.PublishProject("P6-1", 4, 50000, 5);
            Common.InvestProject(UserA, "P6-1", 30000);
            Common.InvestProject(UserA, "P6-1", 20000);
            Common.ProjectStartRepay("P6-1");

            Common.StaticProjectWithdraw("P6-1", UserA, 20000);
            Common.BuyClaim("P6-1", CompanyAccount, 20000);

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

            // P6 回款
            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day07()
        {
            Common.DeltaDay(realDate, 6);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day08()
        {
            Common.DeltaDay(realDate, 7);

            // P6-1 回款，总数应为 62.5
            Common.AutoRepaySimulate();

            Common.AssertWalletDelta(UserA, 16.67m, 0, 0, 0, 0, 0, 100000, 16.67m, realDate);
            Common.AssertWalletDelta(UserB, 9.16m, 0, 0, 0, 0, 0, 50000, 9.16m, realDate);
            Common.AssertWalletDelta(CompanyAccount, 36.67m, 0, 0, 0, 0, 0, 70000, 45.83m, realDate);
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
