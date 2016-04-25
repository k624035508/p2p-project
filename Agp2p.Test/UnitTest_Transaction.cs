using System;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Transactions;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Core.ActivityLogic;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agp2p.Test
{
    /// <summary>
    /// UnitTest1 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTest_Transaction
    {
        public UnitTest_Transaction()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region 附加测试特性

        //
        // 编写测试时，可以使用以下附加特性:
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        [ClassInitialize]
        public static void MyTestInitialize(TestContext testContext)
        {
        }

        private static string GetUserPassword(dt_users user)
        {
            return DESEncrypt.Decrypt(user.password, user.salt);
        }

        [TestMethod]
        public void CleanAllProjectAndTransactionRecord()
        {
            //Common.DoSimpleCleanUp(new DateTime(2015,1,1));
        }

        [TestMethod]
        public void RemoveTransactPassword()
        {
            var context = new Agp2pDataContext();
            var dtUsers = context.dt_users.Single(u => u.user_name == "13535656867");
            dtUsers.pay_password = null;
            context.SubmitChanges();
        }

        [TestMethod]
        public void TestRepayNotice()
        {
            var context = new Agp2pDataContext();
            var rt = context.li_repayment_tasks.OrderByDescending(t => t.id).Take(5).ToList();
            Core.AutoLogic.AutoRepay.SendRepayNotice(rt, context);
        }

        [TestMethod]
        public void TestPerfectRounding()
        {
            CollectionAssert.AreEqual(Utils.GetPerfectRounding(
                new List<decimal> {3.333m, 3.334m, 3.333m}, 10, 2),
                new List<decimal> {3.33m, 3.34m, 3.33m});

            CollectionAssert.AreEqual(Utils.GetPerfectRounding(
                new List<decimal> {3.33m, 3.34m, 3.33m}, 10, 1),
                new List<decimal> {3.3m, 3.4m, 3.3m});

            CollectionAssert.AreEqual(Utils.GetPerfectRounding(
                new List<decimal> {3.333m, 3.334m, 3.333m}, 10, 1),
                new List<decimal> {3.3m, 3.4m, 3.3m});


            CollectionAssert.AreEqual(Utils.GetPerfectRounding(
                new List<decimal> {13.626332m, 47.989636m, 9.596008m, 28.788024m}, 100, 0),
                new List<decimal> {14, 48, 9, 29});
            CollectionAssert.AreEqual(Utils.GetPerfectRounding(
                new List<decimal> {16.666m, 16.666m, 16.666m, 16.666m, 16.666m, 16.666m}, 100, 0),
                new List<decimal> {17, 17, 17, 17, 16, 16});
            CollectionAssert.AreEqual(Utils.GetPerfectRounding(
                new List<decimal> {33.333m, 33.333m, 33.333m}, 100, 0),
                new List<decimal> {34, 33, 33});
            CollectionAssert.AreEqual(Utils.GetPerfectRounding(
                new List<decimal> {33.3m, 33.3m, 33.3m, 0.1m}, 100, 0),
                new List<decimal> {34, 33, 33, 0});
        }

        [TestMethod]
        public void FixRepaymentTaskByRepaidSum()
        {
            var context = new Agp2pDataContext();
            int count = 0;
            context.li_repayment_tasks.Where(t => t.status >= (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid)
                .AsEnumerable()
                .ForEach(
                    task =>
                    {
                        var repayments = context.li_project_transactions.Where(
                            ptr => ptr.project == task.project && ptr.create_time == task.repay_at.Value).ToList();
                        var sumOfRepaidInterest = repayments.Sum(r => r.interest.GetValueOrDefault());
                        if (task.repay_interest != sumOfRepaidInterest)
                        {
                            count++;
                            Debug.WriteLine(
                                $"Fix project（{task.li_projects.title}） repayment task（{task.term}）, from {task.repay_interest} to {sumOfRepaidInterest}");
                            task.repay_interest = sumOfRepaidInterest;
                        }
                    });
            Debug.WriteLine("Fix repayment task repay interest: " + count);
            //context.SubmitChanges();
        }

        private static void DoHistoryFixing(Agp2pDataContext context, li_wallet_histories his)
        {
            // 计算出偏差：和上一个历史记录对比后得出原待收益
            var prevHis = his.dt_users.li_wallet_histories.OrderByDescending(h => h.create_time)
                .First(h => h.create_time < his.create_time);
            var originalProfiting = his.profiting_money - prevHis.profiting_money;

            var project = his.li_project_transactions.li_projects;
            var realInvestment = project.li_project_transactions.Where(
                tr =>
                    tr.investor == his.user_id &&
                    tr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    tr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success).Sum(pt => pt.principal);


            // 计算出正确的待收益金额和：通过 GenerateRepayTransactions
            var myPtr = project.li_repayment_tasks.AsEnumerable().SelectMany(
                t => TransactionFacade.GenerateRepayTransactions(t, t.repay_at ?? t.should_repay_time))
                .Where(ptr => ptr.investor == his.user_id)
                .ToList();
            var predictProfiting = myPtr.Sum(pt => pt.interest.GetValueOrDefault());
            var predictInvesting = myPtr.Sum(pt => pt.principal);

            if (realInvestment != predictInvesting)
            {
                throw new Exception("归还的本金不等于投入的本金");
            }

            // 修正往后的历史
            if (predictProfiting != originalProfiting)
            {
                var deltaProfiting = predictProfiting - originalProfiting;
                var prefixHis = his.dt_users.li_wallet_histories.OrderByDescending(h => h.create_time)
                    .Where(h => his.create_time <= h.create_time).ToList();
                Debug.WriteLine(string.Format("修正 {0} 用户从 {1} 开始出现待收益偏差：{2}，影响历史记录 {3} 条",
                    his.dt_users.GetFriendlyUserName(), his.create_time, deltaProfiting, prefixHis.Count));
                prefixHis.ForEach(h =>
                {
                    h.profiting_money += deltaProfiting;
                });
            }
        }

        [TestMethod]
        public void FixProfitingMoneyBias()
        {
            /* 2016-02-15
            调试跟踪:
                修正 18925950154（覃菲） 用户从 2016/1/18 9:39:45 开始出现待收益偏差：0.01，影响历史记录 26 条
                修正 13612512742（陈茂强） 用户从 2016/1/21 9:36:22 开始出现待收益偏差：0.01，影响历史记录 78 条
                修正 13612512742（陈茂强） 用户从 2016/1/21 14:06:34 开始出现待收益偏差：-0.01，影响历史记录 75 条
                修正 15816951818（庞敏清） 用户从 2016/1/22 16:06:40 开始出现待收益偏差：-0.01，影响历史记录 97 条
                修正 13612512742（陈茂强） 用户从 2016/1/25 9:13:54 开始出现待收益偏差：0.01，影响历史记录 60 条
                修正 13612512742（陈茂强） 用户从 2016/1/26 16:25:53 开始出现待收益偏差：-0.01，影响历史记录 51 条
                修正 13612512742（陈茂强） 用户从 2016/1/28 15:11:21 开始出现待收益偏差：0.01，影响历史记录 39 条
            */
            var context = new Agp2pDataContext();
            var biasSources = context.li_wallet_histories.Where(h => new DateTime(2016, 1, 18) < h.create_time)
                .Where(h => h.action_type == (int) Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess).ToList();

            biasSources.ForEach(h => DoHistoryFixing(context, h));

            //context.SubmitChanges();
        }

        [TestMethod]
        public void FixNewbieProjectMissGenerateRepaymentTask()
        {
            var context = new Agp2pDataContext();
            context.li_projects.Where(p => p.dt_article_category.call_index == "newbie").ForEach(p =>
            {
                var investments = p.li_project_transactions.Where(
                    ptr =>
                        ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                        ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                    .ToDictionary(ptr => ptr.investor);
                var repayments =
                    p.li_repayment_tasks.Where(r => r.only_repay_to != null)
                        .ToDictionary(r => r.only_repay_to.GetValueOrDefault());
                if (repayments.Count < investments.Count)
                {
                    investments.Keys.Except(repayments.Keys).ForEach(userId =>
                    {
                        try
                        {
                            TrialActivity.CheckNewbieInvest(investments[userId].id);
                            Debug.WriteLine("补充遗漏的新手标还款计划，投资人：" + investments[userId].dt_users.GetFriendlyUserName());
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    });
                }
            });
        }

        [TestMethod]
        public void TestSumapayApi()
        {
            //1.请求前台接口
            //1.1发送请求
            var msgReq = new UserRegisterReqMsg(1030, "18681406981", "罗明星", "440233198602010019", "0");
            MessageBus.Main.Publish(msgReq);
            //正式请求时，进行如下异步调用
            //MessageBus.Main.PublishAsync(msgReq, s =>
            //{
            //    Utils.HttpPost(msgReq.ApiInterface, msgReq.RequestContent);
            //});

            //1.2模拟响应返回
            Agp2pDataContext context = new Agp2pDataContext();
            var responseLog = new li_pay_response_log()
            {
                request_id = msgReq.RequestId,
                result = "00000",
                status = (int) Agp2pEnums.SumapayResponseEnum.Return,
                response_time = DateTime.Now,
                response_content = "{request:'" + msgReq.RequestId + "',result:'00000'}"
            };
            context.li_pay_response_log.InsertOnSubmit(responseLog);
            //1.3发送响应消息
            var respMsg = BaseRespMsg.NewInstance<UserRegisterRespMsg>(responseLog.response_content);
            MessageBus.Main.PublishAsync(respMsg,
                s =>
                {
                    if (respMsg.HasHandle)
                    {
                        var req = context.li_pay_request_log.SingleOrDefault(r => r.id == responseLog.request_id);
                        req.complete_time = DateTime.Now;
                        req.status = (int) Agp2pEnums.SumapayRequestEnum.Complete;

                        responseLog.user_id = respMsg.UserIdIdentity;
                        responseLog.status = (int) Agp2pEnums.SumapayResponseEnum.Complete;
                    }
                    responseLog.remarks = respMsg.Remarks;
                    context.SubmitChanges();
                    Assert.IsTrue(s.IsCompleted);
                });
        }

        [TestMethod]
        public void GenerateClaimFromOldData()
        {
            // 补充旧的债权
            var context = new Agp2pDataContext();

            // 定期项目：每笔投资产生一个债权，已完成的项目的债权状态为已完成，其余为不可转让
            var ptrs =
                context.li_project_transactions.Where(
                    ptr => ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest)
                    .ToList();

            int count = 0;
            ptrs.ForEach(ptr =>
            {
                if (ptr.li_claims_invested.Any()) return;

                var claimFromInvestment = new li_claims
                {
                    principal = ptr.principal,
                    projectId = ptr.project,
                    profitingProjectId = ptr.project,
                    createFromInvestment = ptr.id,
                    createTime = ptr.create_time,
                    userId = ptr.investor,
                    status = (byte) Agp2pEnums.ClaimStatusEnum.Nontransferable,
                    number = Utils.HiResNowString,
                };
                context.li_claims.InsertOnSubmit(claimFromInvestment);
                count += 1;

                if (ptr.li_projects.IsNewbieProject())
                {
                    var task = ptr.li_projects.li_repayment_tasks.Single(ta => ta.only_repay_to == ptr.investor);
                    if (task.status != (int) Agp2pEnums.RepaymentStatusEnum.Unpaid)
                    {
                        var claim = claimFromInvestment.NewStatusChild(task.repay_at.Value,
                            Agp2pEnums.ClaimStatusEnum.Completed);
                        context.li_claims.InsertOnSubmit(claim);
                        count += 1;
                    }
                }
                else
                {
                    if (ptr.li_projects.complete_time.HasValue)
                    {
                        var claim = claimFromInvestment.NewStatusChild(ptr.li_projects.complete_time.Value,
                            Agp2pEnums.ClaimStatusEnum.Completed);
                        context.li_claims.InsertOnSubmit(claim);
                        count += 1;
                    }
                }
            });
            //context.SubmitChanges();
            Debug.WriteLine("创建债权：" + count);
        }

        [TestMethod]
        public void TestHiResTimeTickUtil()
        {
            Enumerable.Range(0, 10)
                .Select(i => Utils.HiResNowString)
                .ToList()
                .ForEach(time => Debug.WriteLine(time));
        }

        [TestMethod]
        public void DoChargeTestAccount()
        {
            var now = DateTime.Now;
            Common.DeltaDay(now, -1);
            Common.MakeSureHaveIdleMoney("13535656867", 10*10000);
            Common.MakeSureHaveIdleMoney("13590609455", 10*10000);
            Common.MakeSureHaveIdleMoney("CompanyAccount", 10*10000);
            Common.DeltaDay(now, 0);
        }

        private class DebugTextWriter : TextWriter
        {
            public int writeCount = 0;

            public override void Write(char[] buffer, int index, int count)
            {
                var str = new String(buffer, index, count);
                if (str.Contains("-- Context"))
                {
                    writeCount += 1;
                }
                // Debug.Write(str);
            }

            public override void Write(string value)
            {
                Debug.Write(value);
            }

            public override Encoding Encoding => Encoding.Default;
        }

        [TestMethod]
        public void TestAutoPartialQuery()
        {
            var debugTextWriter = new DebugTextWriter();
            var context = new Agp2pDataContext {Log = debugTextWriter};

            var users = context.dt_users.AsEnumerableAutoPartialQuery().Skip(1000).ToList();
            Debug.WriteLine("Query count: " + debugTextWriter.writeCount);
            Debug.WriteLine("Entities count: " + users.Count);
        }

        [TestMethod]
        public void TestAutoPartialQuery2()
        {
            var debugTextWriter = new DebugTextWriter();
            var context = new Agp2pDataContext {Log = debugTextWriter};

            int totalCount = 0;
            var users = context.dt_users.AsEnumerableAutoPartialQuery(out totalCount).ToList();
            Debug.WriteLine("Query count: " + debugTextWriter.writeCount);
            Debug.WriteLine("Entities count: " + users.Count);
        }

        [TestMethod]
        public void TestMatchSteam()
        {
            var dict = ConfigLoader.loadSumapayErrorNumberDescDict();
            Debug.Assert(dict[110170006] == "请求流水为空或不符合规范");
            Debug.Assert(dict[200200002] == "支付系统异常，请联系管理员4008908970");
            Debug.Assert(dict[110170055] == "付款总笔数不符合规范");
            Debug.Assert(dict[310070319] == "查无此交易");
            Debug.Assert(dict[110400021] == "项目不存在");
            Debug.Assert(dict[110490001] == "通讯异常，请联系丰付客服");
        }

        [TestMethod]
        public void TestQueryLeafClaimsAtMoment()
        {
            var baseTime = DateTime.Today;

            using (var ts = new TransactionScope())
            {
                var rootClaim = new li_claims { createTime = baseTime };
                var c2 = rootClaim.MakeChild(baseTime.AddDays(1));
                var c2c1 = c2.MakeChild(c2.createTime.AddDays(-1));

                var c3 = rootClaim.MakeChild(baseTime.AddDays(2));
                var c3c4 = c3.MakeChild(c3.createTime.AddDays(1));
                var c3c3 = c3.MakeChild(c3.createTime);

                var c3c4c5 = c3c4.MakeChild(c3c4.createTime.AddDays(1));

                var d1 = rootClaim.QueryLeafClaimsAtMoment(baseTime).ToList();
                Assert.AreEqual(1, d1.Count);
                CollectionAssert.Contains(d1, rootClaim);

                var d2 = rootClaim.QueryLeafClaimsAtMoment(baseTime.AddDays(1)).ToList();
                Assert.AreEqual(1, d2.Count);
                CollectionAssert.Contains(d2, c2c1);

                var d3 = rootClaim.QueryLeafClaimsAtMoment(baseTime.AddDays(2)).ToList();
                Assert.AreEqual(2, d3.Count);
                CollectionAssert.Contains(d3, c2c1);
                CollectionAssert.Contains(d3, c3c3);

                var d4 = rootClaim.QueryLeafClaimsAtMoment(baseTime.AddDays(3)).ToList();
                Assert.AreEqual(3, d4.Count);
                CollectionAssert.Contains(d4, c2c1);
                CollectionAssert.Contains(d4, c3c4);
                CollectionAssert.Contains(d4, c3c3);

                var d5 = rootClaim.QueryLeafClaimsAtMoment(baseTime.AddDays(4)).ToList();
                Assert.AreEqual(3, d5.Count);
                CollectionAssert.Contains(d5, c2c1);
                CollectionAssert.Contains(d5, c3c4c5);
                CollectionAssert.Contains(d5, c3c3);

                ts.Dispose();
            }
        }
    }
}
