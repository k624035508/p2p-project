using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    [TestClass]
    public class UnitTest_Huoqi
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);

        private static void SetSystemTime(DateTime cSharpTime)
        {
            var st = new SYSTEMTIME
            {
                wYear = (short) cSharpTime.Year,
                wMonth = (short) cSharpTime.Month,
                wDay = (short) cSharpTime.Day,
                wHour = (short) cSharpTime.Hour,
                wMinute = (short) cSharpTime.Minute,
                wSecond = (short) cSharpTime.Second,
                wMilliseconds = (short) cSharpTime.Millisecond
            };

            SetSystemTime(ref st); // invoke this method.
        }

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

        /* 注意：此测试需要手动备份和还原数据库，请在执行测试前先进行备份，测试完后进行还原 */

        private static readonly string str = "server=192.168.5.98;uid=sa;pwd=Zxcvbnm,;database=agrh;";
        private DateTime testStartTime;

        [TestInitialize]
        public void Setup()
        {
            throw new InvalidOperationException("如果备份了数据库，则暂时注释这行");

            var context = new Agp2pDataContext(str);

            // 记录当前日期
            testStartTime = DateTime.Now;

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
            var emptyRisk = new li_risks
            {
                last_update_time = testStartTime,
            };
            var huoqiProject = new li_projects
            {
                li_risks = emptyRisk,
                category_id = huoqiCategory.id,
                type = (int) Agp2pEnums.LoanTypeEnum.Company,
                sort_id = 99,
                add_time = testStartTime,
                user_name = "unitTest",
                title = "活期项目测试-",
                no = "01",
                financing_amount = 100 * 10000,
                repayment_term_span_count = 1,
                repayment_term_span = (int) Agp2pEnums.ProjectRepaymentTermSpanEnum.Day,
                repayment_type = (int?) Agp2pEnums.ProjectRepaymentTypeEnum.HuoQi,
                profit_rate_year = 3.3m,
            };
            context.li_projects.InsertOnSubmit(huoqiProject);
            
            context.SubmitChanges();
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
        public void Day1()
        {

        }

        [TestCleanup]
        public void TearDown()
        {
        }
    }
}
