using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_P8
    {
        private const string LoanerUser = "yiqingjixie";
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        /*
        P8 测试流程：（测试借款人的放款/收回还款逻辑）
            Day 1
                发 3 日标，金额 50000，A 投 30000，B 投 20000
                放款，判断借款人金额变动
            Day 2
            Day 3
            Day 4
                回款 判断借款人金额变动
        */

        private readonly DateTime TestStartAt = new DateTime(2016, 04, 7, 8, 20, 00);/* 开始测试前请设置好实际日期 */

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            // 准备好之后注释这行
            throw new InvalidOperationException("1. 备份好数据库；2. 设置实际日期");
        }

        [TestMethod]
        public void Day01()
        {
            Common.DeltaDay(TestStartAt, 0);

            Common.MakeSureHaveIdleMoney(LoanerUser, 10*10000);
            Common.MakeSureHaveIdleMoney(UserA, 10*10000);
            Common.MakeSureHaveIdleMoney(UserB, 10*10000);

            Common.GainLoanerRepaymentSimulate();
            Common.AutoRepaySimulate();

            // 发 3 日标，金额 50000，A 投 30000，B 投 20000，放款
            Common.PublishProject("P8", 3, 50000, 5);
            Common.InvestProject(UserA, "P8", 30000);
            Common.InvestProject(UserB, "P8", 20000);
            Common.ProjectStartRepay("P8");

            Common.AssertWalletDelta(LoanerUser, 15 * 10000, 0, 0, 0, 10 * 10000, 0, 0, 0, TestStartAt);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day02()
        {
            Common.DeltaDay(TestStartAt, 1);
            Common.GainLoanerRepaymentSimulate();
            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day03()
        {
            Common.DeltaDay(TestStartAt, 2);
            Common.GainLoanerRepaymentSimulate();
            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void Day04()
        {
            Common.DeltaDay(TestStartAt, 3);

            // 回款，总数：20.83
            Common.GainLoanerRepaymentSimulate();
            Common.AutoRepaySimulate();

            Common.AssertWalletDelta(UserA, 12.5m, 0, 0, 0, 10 * 10000, 0, 30000, 12.5m, TestStartAt);
            Common.AssertWalletDelta(UserB, 8.33m, 0, 0, 0, 10 * 10000, 0, 20000, 8.33m, TestStartAt);
            Common.AssertWalletDelta(LoanerUser, 10*10000-20.83m, 0, 0, 0, 10 * 10000, 0, 00000, 0m, TestStartAt);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(TestStartAt);
            Common.RestoreDate(TestStartAt);
        }
    }
}
