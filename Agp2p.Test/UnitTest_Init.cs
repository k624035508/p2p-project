using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_Init
    {
        private const string LoanerUser = "yiqingjixie";
        private const string UserA = "13535656867";
        private const string UserB = "13590609455";
        private const string CompanyAccount = "CompanyAccount";

        public static readonly DateTime TestStartAt = new DateTime(2016, 4, 22, 8, 20, 00);/* 开始测试前请设置好实际日期 */

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            // 准备好之后注释这行
            throw new InvalidOperationException("1. 备份好数据库；2. 设置实际日期");
        }

        [TestMethod]
        public void Day01()
        {
            Common.DeltaDay(TestStartAt, -1);

            Common.MakeSureHaveIdleMoney(LoanerUser, 10*10000);
            Common.MakeSureHaveIdleMoney(UserA, 10*10000);
            Common.MakeSureHaveIdleMoney(UserB, 10*10000);
            Common.MakeSureHaveIdleMoney(CompanyAccount, 10*10000);

            Common.AutoRepaySimulate();
        }

        [TestMethod]
        public void DoCleanUp()
        {
            Common.DoSimpleCleanUp(TestStartAt);
            Common.RestoreDate(TestStartAt);
        }
    }
}
