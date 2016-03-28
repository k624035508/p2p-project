using System;
using Agp2p.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_P3_1
    {
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        private const string CompanyAccount = "CompanyAccount";
        /*
        P3-1 测试流程：
        Day 1
            发 5 日标，金额 50000，A 投 20000 B 投 30000，放款
        Day 2
            B 提现 30000
            公司账号接手 30000
        Day 3
            A 提现 20000
            公司账号接手 20000
        Day 4
        Day 5
        Day 6
        Day 7
            回款
        */

        readonly DateTime realDate = new DateTime(2016, 03, 25, 8, 30, 00); /* 开始测试前请设置好实际日期 */

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

            // 发 5 日标，金额 50000，A 投 20000 B 投 30000，放款
            Common.PublishProject("P3-1", 5, 50000, 5);
            Common.InvestProject(UserA, "P3-1", 20000);
            Common.InvestProject(UserB, "P3-1", 30000);
            Common.ProjectStartRepay("P3-1");

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day02()
        {
            Common.DeltaDay(realDate, 1);

            Common.AutoRepaySimulate();

            /* B 提现 30000，公司账号接手 30000 */
            Common.StaticProjectWithdraw("P3-1", UserB, 30000);
            Common.BuyClaim("P3-1", CompanyAccount, 30000);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day03()
        {
            Common.DeltaDay(realDate, 2);

            Common.AutoRepaySimulate();

            /* A 提现 20000
               公司账号接手 20000 */
            Common.StaticProjectWithdraw("P3-1", UserA, 20000);
            Common.BuyClaim("P3-1", CompanyAccount, 20000);

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

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day07()
        {
            Common.DeltaDay(realDate, 6);

            // 回款，总数应为 34.72
            Common.AutoRepaySimulate();

            var staticClaimWithdrawCostPercent = ConfigLoader.loadCostConfig().static_withdraw/100;

            Common.AssertWalletDelta(UserA, 5.56m - 20000 * staticClaimWithdrawCostPercent, 0, 0, 0, 0, 0, 20000, 5.56m, realDate);
            Common.AssertWalletDelta(UserB, 4.16m - 30000 * staticClaimWithdrawCostPercent, 0, 0, 0, 0, 0, 30000, 4.16m, realDate);
            Common.AssertWalletDelta(CompanyAccount, 25.00m, 0, 0, 0, 0, 0, 50000, 25.00m, realDate);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(realDate);
            Common.RestoreDate(realDate);
        }
    }
}
