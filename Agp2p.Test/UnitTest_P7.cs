using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_P7
    {
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        private const string CompanyAccount = "CompanyAccount";
        /*
        P7 测试流程：（测试月度项目的活期还款）
            Day 1
                发活期标
                发 2 月标，金额 50000，B 投 50000 放款；B 提现 50000；公司账号接手 50000
                A 投资活期 30000
            Day 2~60
            Day 61
                回款
        */

        readonly DateTime TestStartAt = UnitTest_Init.TestStartAt; /* 开始测试前请设置好实际日期 */
        private static int _totalDays;

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

            Common.MakeSureHaveIdleMoney(UserA, 10*10000);
            Common.MakeSureHaveIdleMoney(UserB, 10*10000);
            Common.MakeSureHaveIdleMoney(CompanyAccount, 10*10000);

            Common.AutoRepaySimulate();

            // 发活期标
            Common.PublishHuoqiProject("HP1");

            // 发 2 月标，金额 50000，B 投 50000，放款
            Common.PublishMonthlyProject("P7", 2, 50000, 5);
            Common.InvestProject(UserB, "P7", 50000);
            Common.ProjectStartRepay("P7");

            // B 提现 50000；公司账号接手 50000
            Common.StaticProjectWithdraw("P7", UserB, 50000);
            Common.BuyClaim("P7", CompanyAccount, 50000);

            // A 投资活期 30000
            Common.InvestProject(UserA, "HP1", 30000);

            Common.AutoRepaySimulate();

            _totalDays = (int)(DateTime.Today.AddMonths(2) - DateTime.Today).TotalDays;
        }

        [TestMethod]
        public void Day02_LastDay()
        {
            Enumerable.Range(0, _totalDays).ForEach(dayDelta =>
            {
                Common.DeltaDay(TestStartAt, dayDelta + 1);
                Common.AutoRepaySimulate();
            });
        }

        [TestMethod]
        public void DayLastNextDay()
        {
            // already next day
            // Common.DeltaDay(TestStartAt, _totalDays);
            // Common.AutoRepaySimulate();

            // 回款，总数应为 417.81

            Common.AssertWalletDelta(UserA, 165m, 0, 0, 0, 0, 0, 30000, 165m, TestStartAt);
            Common.AssertWalletDelta(UserB, 0, 0, 0, 0, 0, 0, 50000, 0m, TestStartAt);
            Common.AssertWalletDelta(CompanyAccount, 252.81m, 0, 0, 0, 0, 0, 50000, 417.81m, TestStartAt);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(TestStartAt);
            Common.RestoreDate(TestStartAt);
        }
    }
}
