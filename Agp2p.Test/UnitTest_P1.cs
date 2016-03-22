using System;
using Agp2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_P1
    {
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        /*
        P1 测试流程：
        Day 1
            发 1 日标，金额 50000，A 投 15000 * 2，B 投 20000，放款
        Day 2
            回款
        */

        readonly DateTime realDate = new DateTime(2016, 03, 22, 8, 50, 00); /* 开始测试前请设置好实际日期 */

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

            Common.PublishProject("P1", 1, 50000, 5);
            Common.InvestProject(UserA, "P1", 15000);
            Common.InvestProject(UserA, "P1", 15000);
            Common.InvestProject(UserB, "P1", 20000);
            Common.ProjectStartRepay("P1");

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day02()
        {
            Common.DeltaDay(realDate, 1);
            Common.AutoRepaySimulate();

            Common.AssertWalletDelta(UserA, 4.16m, 0m, 0m, 0m, 0, 0, 30000m, 4.16m, realDate);
            Common.AssertWalletDelta(UserB, 2.78m, 0m, 0m, 0m, 0, 0, 20000m, 2.78m, realDate);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(realDate);
            Common.RestoreDate(realDate);
        }
    }
}
