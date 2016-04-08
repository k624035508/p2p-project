using System;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_P2_1
    {
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        private const string CompanyAccount = "CompanyAccount";
        /*
        P2-1 测试流程：（测试债权转让失败后的计息是否正确）
        Day 1
            发 4 日标，金额 60000，A 投 25000
        Day 2
            B 投 35000，放款
        Day 3
            A 提现 35000
        Day 4
            公司账号只接手 A 的提现 25000
        Day 5
        Day 6
            回款
        */

        readonly DateTime realDate = new DateTime(2016, 4, 7, 8, 30, 00); /* 开始测试前请设置好实际日期 */

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

            // 发 5 日标，金额 60000，B 投 25000
            Common.PublishProject("P2-1", 5, 60000, 5);
            Common.InvestProject(UserB, "P2-1", 25000);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day02()
        {
            Common.DeltaDay(realDate, 1);

            Common.AutoRepaySimulate();

            // A 投 35000，放款
            Common.InvestProject(UserA, "P2-1", 35000);
            Common.ProjectStartRepay("P2-1");

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day03()
        {
            Common.DeltaDay(realDate, 2);

            Common.AutoRepaySimulate();

            // A 提现 35000
            Common.StaticProjectWithdraw("P2-1", UserA, 35000);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day04()
        {
            Common.DeltaDay(realDate, 3);

            Common.AutoRepaySimulate();

            // 公司账号接手 A 的提现 25000
            Common.BuyClaim("P2-1", CompanyAccount, 25000);

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

            // 回款，总数应为 41.67
            Common.AutoRepaySimulate();

            Common.AssertWalletDelta(UserA, 24.31m, 0, 0, 0, 0, 0, 35000, 24.31m, realDate);
            Common.AssertWalletDelta(UserB, 17.36m, 0, 0, 0, 0, 0, 25000, 17.36m, realDate);
            Common.AssertWalletDelta(CompanyAccount, 0m, 0, 0, 0, 0, 0, 25000, 0m, realDate);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(realDate);
            Common.RestoreDate(realDate);
        }
    }
}
