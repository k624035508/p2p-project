using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Core.AutoLogic;
using Agp2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_Huoqi
    {
        /**
         测试计划：
            现有架构需要进行的迁移步骤
            0. 数据库添加 li_claims 表，li_project_transactions 添加字段 gainFromClaim 及关系，增加 li_project_transactions 备注的长度至 nvchar(100)
            1. 生成旧项目的债权
            2. 设置“公司账号”
            3. 设置后台导航，添加活期项目提现功能

            需要测试的功能需求：
            1. 定期项目的投资、回款、债权转让
            2. 手动接手定期债权
            3. 活期项目的投资、回款、自动续投、提现
        */

        /* 注意：此测试需要手动备份和还原数据库，请在执行测试前先进行备份，测试完后进行还原 */

        private DateTime testStartTime;
        private int investorA, investorB, huoqiProjectId, projectAId, projectBId;

        [ClassInitialize]
        public void Setup()
        {
            var context = new Agp2pDataContext();

            // 记录当前日期
            testStartTime = DateTime.Now;

            // 设置投资者 id
            investorA = context.dt_users.Single(u => u.user_name == "13535656867").id;
            investorB = context.dt_users.Single(u => u.user_name == "13590609455").id;

            // 将现有的项目/还款计划设置为已完成

            var projects = context.li_projects.Where(p => p.status <= (int) Agp2pEnums.ProjectStatusEnum.ProjectRepaying).ToList();
            projects.ForEach(p =>
            {
                if (p.status < (int) Agp2pEnums.ProjectStatusEnum.ProjectRepaying)
                {
                    p.status = (int) Agp2pEnums.ProjectStatusEnum.FinancingApplicationCancel;
                }
                else if (p.status == (int) Agp2pEnums.ProjectStatusEnum.ProjectRepaying)
                {
                    context.EarlierRepayAll(p.id, 100);
                }
                else
                {
                    throw new InvalidOperationException("未考虑到的情况");
                }
            });

            // 创建用于测试的活期项目和定期项目
            var huoqiCategory = context.dt_article_category.Single(c => c.call_index == "huoqi");
            var huoqiProject = new li_projects
            {
                li_risks = new li_risks
                {
                    last_update_time = testStartTime,
                },
                category_id = huoqiCategory.id,
                type = (int) Agp2pEnums.LoanTypeEnum.Company,
                sort_id = 99,
                add_time = testStartTime,
                publish_time = testStartTime,
                make_loan_time = testStartTime,
                user_name = "unitTest",
                title = "活期项目测试-",
                no = "01",
                financing_amount = 100 * 10000,
                repayment_term_span_count = 1,
                repayment_term_span = (int) Agp2pEnums.ProjectRepaymentTermSpanEnum.Day,
                repayment_type = (int?) Agp2pEnums.ProjectRepaymentTypeEnum.HuoQi,
                profit_rate_year = 3.3m,
                status = (int) Agp2pEnums.ProjectStatusEnum.Financing,
            };
            context.li_projects.InsertOnSubmit(huoqiProject);
            
            throw new InvalidOperationException("如果备份了数据库，则暂时注释这行");
            context.SubmitChanges();

            // 设置活期项目 id
            huoqiProjectId = huoqiProject.id;
        }

        /*
        测试流程：
        Day 1
        发 1日 标 A，金额 50000，自动投资满，放款
        发 4日 标 B，金额 50000，定期投资 10000
        */
        [TestMethod]
        public void Day1()
        {
            Common.AutoRepaySimulate(DateTime.Today.AddHours(8));

            var now = DateTime.Now;
            var context = new Agp2pDataContext();

            var loaner = context.li_loaners.Single(l => l.dt_users.real_name == "杨长岭");
            var ypbCategory = context.dt_article_category.Single(c => c.call_index == "ypb");
            var projectA = new li_projects
            {
                li_risks = new li_risks
                {
                    last_update_time = now,
                    li_loaners = loaner
                },
                category_id = ypbCategory.id,
                type = (int)Agp2pEnums.LoanTypeEnum.Company,
                sort_id = 99,
                add_time = now,
                publish_time = now,
                make_loan_time = now,
                user_name = "unitTest",
                title = "自动投标测试-",
                no = "01",
                financing_amount = 5 * 10000,
                repayment_term_span_count = 1,
                repayment_term_span = (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Day,
                repayment_type = (int?)Agp2pEnums.ProjectRepaymentTypeEnum.DaoQi,
                profit_rate_year = 5,
                status = (int) Agp2pEnums.ProjectStatusEnum.Financing,
            };
            context.li_projects.InsertOnSubmit(projectA);
            context.SubmitChanges();
            projectAId = projectA.id;

            TransactionFacade.Invest(investorA, huoqiProjectId, 50000);
            var context2 = new Agp2pDataContext();
            context2.StartRepayment(projectAId);

            var projectB = new li_projects
            {
                li_risks = new li_risks
                {
                    last_update_time = now,
                    li_loaners = loaner
                },
                category_id = ypbCategory.id,
                type = (int)Agp2pEnums.LoanTypeEnum.Company,
                sort_id = 99,
                add_time = now,
                publish_time = now,
                make_loan_time = now,
                user_name = "unitTest",
                title = "自动投标测试-",
                no = "02",
                financing_amount = 5 * 10000,
                repayment_term_span_count = 4,
                repayment_term_span = (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Day,
                repayment_type = (int?)Agp2pEnums.ProjectRepaymentTypeEnum.DaoQi,
                profit_rate_year = 5,
                status = (int) Agp2pEnums.ProjectStatusEnum.Financing,
            };
            context2.li_projects.InsertOnSubmit(projectB);
            context2.SubmitChanges();
            projectBId = projectB.id;

            TransactionFacade.Invest(investorA, projectBId, 10000);

            Common.AutoRepaySimulate(DateTime.Today.AddHours(18));
        }

        /*
        Day 2
        检查 B 是否自动投满并进行放款（将于 Day 6 回款），并且有一部分续投失败退款
        检查债权
        */
        [TestMethod]
        public void Day2()
        {
            var context = new Agp2pDataContext();
            var projectB = context.li_projects.Single(p => p.id == projectBId);
            Assert.AreEqual(projectB.financing_amount, projectB.investment_amount);
            context.StartRepayment(projectBId);

            // 检查是否存在续投失败回款

        }

        /*
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


        [ClassCleanup]
        public void TearDown()
        {
        }
    }
}
