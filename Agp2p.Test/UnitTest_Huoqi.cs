using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    /**
     测试计划：
        现有架构需要进行的迁移步骤
        0. 数据库添加 li_claims 表，li_project_transactions 添加字段 gainFromClaim 及关系，增加 li_project_transactions 备注的长度至 nvchar(100)
        1. 生成旧项目的债权
        2. 设置“公司账号”
        3. 设置后台导航，添加活期项目提现功能

        大纲：
        1. 测试以前的投资/还款逻辑是否正常
        2. 测试自动投标、活期项目放款
        3. 测试定期项目完成后自动续投的情况
        4. 测试自动投标的提现，（测试提现后如果项目完成/提现后转让）
    */

    [TestClass]
    public class UnitTest_Huoqi
    {
        private DateTime setupTime;

        [TestInitialize]
        public void Setup()
        {
            // 记录当前日期
            setupTime = DateTime.Now;

            // 将当前募集中/放款中的活期项目、定期项目和还款计划都设置为完成，并记录下来，之后需要还原


            // 创建用于测试的活期项目和定期项目

            // 
        }

        /*
        测试流程：
        Day 1
        发 1日 标 A，金额 50000，自动投资满，放款
        发 4日 标 B，金额 50000，定期投资 10000

        Day 2
        检查 B 是否自动投满并进行放款（将于 Day 6 回款），并且有一部分续投失败退款
        检查债权

        Day 3
        检查 A 是否回款正常：无收益
        活期提现 30000
        找个人投资活期 10000，检查债权拆分

        Day 4
        检查活期提现是否成功 = 30000
        检查活期收益是否来自总共 10000 的债权
        检查公司账号有无接手活期提现 20000，检查债权拆分
        找个人投资活期 10000，看是否接手了公司账号投资的债权

        Day 5
        检查活期回款

        Day 6
        检查活期回款
        检查续投失败回款
        */

        [TestMethod]
        public void TestMethod1()
        {
        }

        [TestCleanup]
        public void TearDown()
        {
            // 将用于测试的项目/还款计划设置为完成
            // 还原项目/还款计划的状态
        }
    }
}
