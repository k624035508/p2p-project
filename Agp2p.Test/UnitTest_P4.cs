using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_P4
    {
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        private const string CompanyAccount = "CompanyAccount";
        /*
        P4 测试流程：
            Day 1
                发活期标
                发 4 日标，金额 50000，A 投 50000 放款；A 提现 50000；公司账号接手 50000
            Day 2
                B 投资活期 30000
            Day 3
            Day 4
            Day 5
                回款
        */

        readonly DateTime realDate = new DateTime(2016, 03, 17); /* 开始测试前请设置好实际日期 */

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

            // 发 4 日标，金额 50000，A 投 50000 放款；A 提现 50000；公司账号接手 50000
            Common.PublishProject("P4", 4, 50000, 5);
            Common.InvestProject(UserA, "P4", 50000);
            Common.ProjectStartRepay("P4");

            Common.StaticProjectWithdraw("P4", UserA, 50000);
            Common.BuyClaim("P4", CompanyAccount, 50000);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day02()
        {
            Common.DeltaDay(realDate, 1);

            Common.AutoRepaySimulate();

            // B 投资活期 30000
            Common.InvestProject(UserB, "HP1", 30000);

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

            // 回款，总数应为 27.78
            Common.AutoRepaySimulate();

            Common.AssertWalletDelta(UserA, 0m, 0, 0, 0, 0, 0, 50000, 0m, realDate);
            Common.AssertWalletDelta(UserB, 5.42m, 0, 0, 0, 0, 0, 30000, 5.42m, realDate);
            Common.AssertWalletDelta(CompanyAccount, 22.36m, 0, 0, 0, 0, 0, 50000, 27.78m, realDate);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(new DateTime(2016, 03, 17, 10, 00, 00));
            Common.RestoreDate(realDate);
        }
    }
}
