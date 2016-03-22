using System;
using Agp2p.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_P3
    {
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        private const string CompanyAccount = "CompanyAccount";
        /*
        P3 测试流程：
        Day 1
            发 5 日标，金额 50000，A 投 10000
        Day 2
            B 投 40000，放款
        Day 3
            B 提现 40000
            A 接手 20000
            公司账号接手 20000
        Day 4
            A 提现 20000
            公司账号接手 20000
        Day 5
        Day 6
        Day 7
            回款
        */

        readonly DateTime realDate = new DateTime(2016, 03, 21, 8, 30, 00); /* 开始测试前请设置好实际日期 */

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

            // 发 5 日标，金额 50000，A 投 10000
            Common.PublishProject("P3", 5, 50000, 5);
            Common.InvestProject(UserA, "P3", 10000);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day02()
        {
            Common.DeltaDay(realDate, 1);

            Common.AutoRepaySimulate();

            // B 投 40000，放款
            Common.InvestProject(UserB, "P3", 40000);
            Common.ProjectStartRepay("P3");

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day03()
        {
            Common.DeltaDay(realDate, 2);

            Common.AutoRepaySimulate();

            /* B 提现 40000
               A 接手 20000
               公司账号接手 20000 */
            Common.StaticProjectWithdraw("P3", UserB, 40000);
            Common.BuyClaim("P3", UserA, 20000);
            Common.BuyClaim("P3", CompanyAccount, 20000);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day04()
        {
            Common.DeltaDay(realDate, 3);

            Common.AutoRepaySimulate();

            /* A 提现 20000
               公司账号接手 20000 */
            Common.StaticProjectWithdraw("P3", UserA, 20000);
            Common.BuyClaim("P3", CompanyAccount, 20000);

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

            Common.AssertWalletDelta(UserA, 9.72m - 20000 * staticClaimWithdrawCostPercent, 0, 0, 0, 0, 0, 30000, 9.72m, realDate);
            Common.AssertWalletDelta(UserB, 5.56m - 40000 * staticClaimWithdrawCostPercent, 0, 0, 0, 0, 0, 40000, 5.56m, realDate);
            Common.AssertWalletDelta(CompanyAccount, 19.44m, 0, 0, 0, 0, 0, 40000, 19.44m, realDate);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(realDate);
            Common.RestoreDate(realDate);
        }
    }
}
