using System;
using Agp2p.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_P4_2
    {
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        private const string CompanyAccount = "CompanyAccount";
        private const string LoanerUser = "yiqingjixie";
        /*
        P4-1 测试流程：（测试活期延期投资失败退回）
            Day 1
                发活期标
                A 投资活期 30000
            Day 2
                检查活期投资退回
                
        */

        readonly DateTime realDate = new DateTime(2016, 4, 11, 8, 00, 00); /* 开始测试前请设置好实际日期 */

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

            Common.MakeSureHaveIdleMoney(UserA, 3 * 10000);

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

            Common.AutoRepaySimulate();

            Common.AssertWalletDelta(UserA, 0m, 0, 0, 0, 0, 0, 0, 0, realDate);
            Common.AssertWalletDelta(UserB, 0m, 0, 0, 0, 0, 0, 0, 0, realDate);
            Common.AssertWalletDelta(CompanyAccount, 0, 0, 0, 0, 0, 0, 0, 0, realDate);
            Common.AssertWalletDelta(LoanerUser, 0, 0, 0, 0, 0, 0, 0, 0, realDate);
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(realDate);
            Common.RestoreDate(realDate);
        }
    }
}
