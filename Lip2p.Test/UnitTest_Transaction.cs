using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lip2p.Common;
using Lip2p.Core;
using Lip2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Lip2p.Core.AutoLogic;
using Newtonsoft.Json.Linq;

namespace Lip2p.Test
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
        [TestInitialize]
        public void MyTestInitialize()
        {
            // 创建测试用户
            // 为其充值
            // 充值确认
            // 创建测试项目，设置期限、期数和还款类型

        }

        private static readonly string str = "server=192.168.5.108;uid=sa;pwd=a123456;database=DTcmsdb3;";

        // 投满标
        [TestMethod]
        public void CheckInvestAmount()
        {
            Lip2pDataContext context = new Lip2pDataContext(str);
            var projects = context.li_projects.Where(p => (int)Lip2pEnums.ProjectStatusEnum.Financing <= p.status && p.investment_amount == 0).ToList();
            foreach (var p in projects)
            {
                var investmentAmount = p.li_project_transactions.Where(
                    tr =>
                        tr.status == (int)Lip2pEnums.ProjectTransactionStatusEnum.Success &&
                        tr.type == (int)Lip2pEnums.ProjectTransactionTypeEnum.Invest)
                    .Select(t => t.value)
                    .AsEnumerable()
                    .Sum();
                if (p.investment_amount != investmentAmount)
                {
                    throw new InvalidDataException("投资总额应该等于计算出来的投资总额");
                }
            }
        }

        [TestMethod]
        public void EvalToFixProjectStatusMethod()
        {
            var context = new Lip2pDataContext(str);
            var projects =
                context.li_projects.Where(
                    p =>
                        p.status == (int) Lip2pEnums.ProjectStatusEnum.Financing &&
                        p.tag != (int) Lip2pEnums.ProjectTagEnum.Trial && p.tag != (int)Lip2pEnums.ProjectTagEnum.DailyProject).ToList();
            int doneCount = 0;
            projects.ForEach(p =>
            {
                var investmentAmount = p.li_project_transactions.Where(
                    tr =>
                        tr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success &&
                        tr.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest)
                    .Select(t => t.value)
                    .AsEnumerable()
                    .DefaultIfEmpty(0)
                    .Sum();
                if (p.investment_amount != investmentAmount)
                {
                    p.investment_amount = investmentAmount; // 修正总额
                    Debug.WriteLine("Fix Project {0} investmentAmount: {1}", p.title, investmentAmount);
                    context.SubmitChanges();
                }
                if (p.investment_amount == p.financing_amount)
                {
                    context.FinishInvestment(p.id); // 满标
                    Debug.WriteLine("Set Project {0} finish", p.title);
                }
                else
                {
                    p.invest_complete_time = null;
                    context.SubmitChanges();
                }
                doneCount += 1;
            });
            Debug.WriteLine("Fix Project status count: " + doneCount);
        }

        [TestMethod]
        public void FixAllProjectStatusError() // 将完全放款的项目设为完成
        {
            var context = new Lip2pDataContext(str);
            var errorProjects =
                context.li_projects.Where(
                    p =>
                        p.tag != (int) Lip2pEnums.ProjectTagEnum.Trial &&
                        (int) Lip2pEnums.ProjectStatusEnum.Financing < p.status &&
                        p.status < (int) Lip2pEnums.ProjectStatusEnum.RepayCompleteIntime &&
                        p.li_repayment_tasks.All(r => r.status != (int) Lip2pEnums.RepaymentStatusEnum.Unpaid)).ToList();
            errorProjects.ForEach(p =>
            {
                Debug.WriteLine(p.title);
                p.status = (int) Lip2pEnums.ProjectStatusEnum.RepayCompleteIntime;
            });
            context.SubmitChanges();
        }

        [TestMethod]
        public void FixTotalReChargeAndWithDraw() // 计算累计充值和累计提现
        {
            var context = new Lip2pDataContext(str);

            var walletList = context.li_wallets.ToList();
            //累计充值
            walletList.ForEach(w =>
            {
                List<li_bank_transactions> transactions = context.li_bank_transactions.Where(b => b.charger == w.user_id && b.type == (int)Lip2pEnums.BankTransactionTypeEnum.Charge &&
                b.status == (int)Lip2pEnums.BankTransactionStatusEnum.Confirm).ToList();
                List<li_activity_transactions> activity_transactions = context.li_activity_transactions.Where(a => a.user_id == w.user_id &&
                    a.activity_type == (int)Lip2pEnums.ActivityTransactionActivityTypeEnum.ManualOperation
                    && a.type == (int)Lip2pEnums.ActivityTransactionTypeEnum.Gain).ToList();

                decimal transactions_sum = transactions != null && transactions.Count > 0 ? transactions.Sum(b => b.value) : 0;
                decimal activity_transactions_sum = activity_transactions != null && activity_transactions.Count > 0 ? activity_transactions.Sum(b => b.value) : 0;
                w.total_charge = transactions_sum + activity_transactions_sum;
            });
            //累计提现
            walletList.ForEach(w =>
            {
                List<li_bank_transactions> transactions = context.li_bank_transactions.Where(b => b.li_bank_accounts.owner == w.user_id
                && b.type == (int)Lip2pEnums.BankTransactionTypeEnum.Withdraw
                && b.status == (int)Lip2pEnums.BankTransactionStatusEnum.Confirm).ToList();

                decimal transactions_sum = transactions != null && transactions.Count > 0 ? transactions.Sum(b => b.value) : 0;

                w.total_withdraw = transactions_sum;
            });
            context.SubmitChanges();
        }

        private static readonly Dictionary<decimal, decimal> HongbaoActivateMoneyMap = new Dictionary<decimal, decimal>
        {
            {10m, 500m},
            {15m, 3000m},
            {50m, 6000m}
        };

        [TestMethod]
        public void UpdateOldUserHongbao() // 为以前未投资过的用户发放红包
        {
            var context = new Lip2pDataContext(str);
            var userList =
                context.li_wallets.Where(
                    w => w.total_investment == 0 && w.dt_users.reg_time <= DateTime.Parse("2015-5-3"))
                    .Select(w => w.dt_users)
                    .AsEnumerable()
                    .Where(u => !u.li_activity_transactions.Any(a => a.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.HongBaoActivation))
                    .ToList();

            userList.ForEach(u => 
            {
                var deadline = new DateTime(2015, 5, 20);
                if (new DateTime(2015, 5, 3) <= DateTime.Now && DateTime.Now <= deadline)
                {
                    var trs = HongbaoActivateMoneyMap.Select(m => new li_activity_transactions
                    {
                        user_id = u.id,
                        create_time = DateTime.Now,
                        value = m.Key,
                        details = JsonConvert.SerializeObject(new { Deadline = deadline.AddYears(1).ToString("yyyy-MM-dd"), InvestUntil = m.Value }),
                        type = (byte)Lip2pEnums.ActivityTransactionTypeEnum.Gain,
                        status = (byte)Lip2pEnums.ActivityTransactionStatusEnum.Acting,
                        activity_type = (byte)Lip2pEnums.ActivityTransactionActivityTypeEnum.HongBaoActivation,
                    });
                    context.li_activity_transactions.InsertAllOnSubmit(trs);
                    context.SubmitChanges();
                }
            });
        }

        [TestMethod]
        public void CheckProfitingMoney() // 计算个人的待收金额是否与钱包的数值相符
        {
            var context = new Lip2pDataContext(str);
            var sortedList = QueryAbnormalProfitingMoneyList(context);
            sortedList.ForEach(
                kv =>
                {
                    var u = kv.Key;
                    var sumOfProfiting = kv.Value;
                    Debug.WriteLine("用户[{0}]的钱包待收利息[{1}]与实际待收利息[{2}]不匹配，相差{3}",
                        GetFriendlyUserName(u), u.li_wallets.profiting_money,
                        sumOfProfiting, u.li_wallets.profiting_money - sumOfProfiting);
                });
            Debug.WriteLine("总差额: {0}", sortedList.Sum(kv => kv.Key.li_wallets.profiting_money - kv.Value));
        }

        private static List<KeyValuePair<dt_users, decimal>> QueryAbnormalProfitingMoneyList(Lip2pDataContext context)
        {
            // 筛选出等待还款的用户
            var users = context.dt_users.Where(u => 0 < u.li_wallets.investing_money).ToList();

            // 查出所有未放款的还款计划，求和用户应得利息
            var sumOfProfitingMap = users.ToDictionary(u => u, u =>
            {
                var projectInvestmentMap = u.li_project_transactions.Where(p =>
                    p.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success &&
                    p.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                    p.li_projects.status < (int) Lip2pEnums.ProjectStatusEnum.RepayCompleteIntime)
                    .GroupBy(p => p.li_projects)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.value));

                var sumOfProfiting = projectInvestmentMap.Keys.SelectMany(
                    p =>
                        p.li_repayment_tasks.Where(
                            t => t.status == (int) Lip2pEnums.RepaymentStatusEnum.Unpaid && 0 < t.repay_interest)).Sum(
                                t =>
                                {
                                    var userInvest = projectInvestmentMap[t.li_projects];
                                    var ratio = userInvest/t.li_projects.investment_amount;
                                    return Math.Round(t.repay_interest*ratio, 2);
                                });
                return sumOfProfiting;
            });
            return sumOfProfitingMap.Where(kv => kv.Key.li_wallets.profiting_money != kv.Value)
                .OrderByDescending(kv => kv.Key.li_wallets.profiting_money - kv.Value)
                .ToList();
        }

        [TestMethod]
        public void FixInvestSuccessHistory() // 幂等：修正投资完成历史的创建时间和关联投资记录 (创建时间应等于应等于项目满标时间，投资记录是用户最后一次对某项目的投资记录)
        {
            var context = new Lip2pDataContext(str);
            var users = context.dt_users.Where(u =>
                u.li_wallet_histories.Any(h =>
                        h.action_type == (int) Lip2pEnums.WalletHistoryTypeEnum.InvestSuccess &&
                        h.li_project_transactions == null)).ToList();
            int fixCount = 0;
            users.ForEach(u =>
            {
                // 满标时间不等于投资完成时间！
                // 通过匹配满标收益金额来匹配满标项目
                // 1. 用户当时投资了但是未满标的项目
                // 2. 查出用户应得利息，进行匹配

                var prematchHis =
                    u.li_wallet_histories.Where(h =>
                            h.action_type == (int) Lip2pEnums.WalletHistoryTypeEnum.InvestSuccess &&
                            h.li_project_transactions == null).ToList();
                prematchHis.ForEach(h =>
                {
                    var prevHis = u.li_wallet_histories.OrderByDescending(ph => ph.id).First(ph => ph.id < h.id);
                    var deltaProfiting = h.profiting_money - Math.Max(0, prevHis.profiting_money); // 避免钱包历史为负数影响

                    // 查出用户投资的最后一个满标的项目
                    var projs = context.li_projects.Where(
                        p =>
                            p.invest_complete_time != null && p.invest_complete_time < h.create_time &&
                            p.li_project_transactions.Any(tr => tr.investor == u.id))
                        .OrderByDescending(p => p.invest_complete_time).ToList();

                    var projectProfitMap = projs.ToDictionary(p => p, proj =>
                    {
                        // 查出用户的投资金额
                        var investments = proj.li_project_transactions.Where(
                            tr =>
                                tr.investor == u.id && tr.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                                tr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success).ToList();

                        return Math.Round(investments.Sum(tr => tr.value)*proj.profit_rate, 2);
                    });

                    var pairs = projectProfitMap.OrderBy(m => Math.Abs(deltaProfiting - m.Value)).ToList();

                    var pair = pairs.First();
                    if (Math.Abs(deltaProfiting - pair.Value) < 0.05m) // 不一定相等，因为实际计算的时候是分开几期的利息再求和
                    {
                        var proj = pair.Key;
                        h.create_time = proj.invest_complete_time.Value;
                        h.li_project_transactions = proj.li_project_transactions.Last(tr =>
                                    tr.investor == u.id && tr.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                                    tr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success);
                        fixCount += 1;
                    }
                    else
                    {
                        throw new Exception("Another project!");
                    }
                });
            });

            context.SubmitChanges();
            Debug.WriteLine("Fix histories Count: " + fixCount);
        }

        [TestMethod]
        public void QueryProfitingMoneyMistakeProject() // 查询投资和满标都加了待收利息的项目
        {
            var context = new Lip2pDataContext(str);
            context.li_wallet_histories.Where(
                h =>
                    h.action_type == (int)Lip2pEnums.WalletHistoryTypeEnum.Invest ||
                    h.action_type == (int)Lip2pEnums.WalletHistoryTypeEnum.InvestSuccess)
                .GroupBy(h => h.li_project_transactions.li_projects)
                .Where(g => g.Any(h => h.action_type == (int)Lip2pEnums.WalletHistoryTypeEnum.InvestSuccess))
                .ToDictionary(g => g.Key, g => g.GroupBy(h => h.dt_users).First())
                .Where(m =>
                {
                    var firstInvestment = m.Value.First(h => h.action_type == (int)Lip2pEnums.WalletHistoryTypeEnum.Invest);
                    var prevHistory = context.li_wallet_histories.Where(h => h.user_id == m.Value.Key.id && h.id < firstInvestment.id)
                        .OrderByDescending(h => h.id)
                        .First();
                    return firstInvestment.profiting_money != prevHistory.profiting_money;
                }).ForEach(m =>
                {
                    Debug.WriteLine(m.Key.title);
                });
        }

        [TestMethod]
        public void FixHistoryAboutProfitingMoney() // 幂等
        {
            // 将此项目的投资所加的待收利息减去: 440F2金屋子1504008子项目（三）-01    440F2金屋子1504009子项目（三）-03
            var context = new Lip2pDataContext(str);
            //var proj = context.li_projects.Single(p => p.title == "440F2金屋子1504009子项目（三）-03");
            var proj = context.li_projects.Single(p => p.title == "440F2金屋子1504008子项目（三）-01");
            var investers = proj.li_project_transactions.Where(
                t =>
                    t.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                    t.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success)
                .Select(t => t.dt_users)
                .Distinct().ToList();
            var fixMoneyTotal = 0m;
            investers.ForEach(i =>
            {
                // 将这个项目的投资所加的待收利润减去，包括往后的历史
                var userHis = i.li_wallet_histories.ToList();
                var mistakeProfitingMoney = 0m;
                for (int index = 0; index < userHis.Count; index++)
                {
                    var curr = userHis[index];
                    curr.profiting_money -= mistakeProfitingMoney; // 与前一个都减去 mistakeProfitingMoney 再计算，避免差距过大
                    if (curr.action_type == (int) Lip2pEnums.WalletHistoryTypeEnum.Invest && curr.li_project_transactions.project == proj.id)
                    {
                        var prev = userHis[index - 1];
                        mistakeProfitingMoney += curr.profiting_money - prev.profiting_money;
                        curr.profiting_money = prev.profiting_money;
                    }
                }

                Debug.WriteLine("Fixed user {0} profiting money, delta: {1}", GetFriendlyUserName(i), mistakeProfitingMoney);
                fixMoneyTotal += mistakeProfitingMoney;
                // 修正钱包的待收金额
                i.li_wallets.profiting_money = userHis.Last().profiting_money;
            });

            Debug.WriteLine("Fix Profiting Money Total: " + fixMoneyTotal);
            context.SubmitChanges();
        }

        private static string GetFriendlyUserName(dt_users user)
        {
            return string.IsNullOrWhiteSpace(user.real_name)
                ? user.user_name
                : string.Format("{0}({1})", user.user_name, user.real_name);
        }

        private static string GetUserPassword(dt_users user)
        {
            return DESEncrypt.Decrypt(user.password, user.salt);
        }

        [TestMethod]
        public void FixMissingRepayment() // 幂等
        {
            var context = new Lip2pDataContext(str);
            var tasks = context.li_repayment_tasks.Where(t => t.status != (int) Lip2pEnums.RepaymentStatusEnum.Unpaid).ToList();
            var projectInvesters = context.li_project_transactions.Where(
                t =>
                    t.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                    t.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success)
                .GroupBy(t => t.li_projects)
                .ToDictionary(g => g.Key, g => g.Select(t => t.dt_users).Distinct());
            tasks.ForEach(t =>
            {
                var investers = projectInvesters[t.li_projects].ToDictionary(u => u.id);
                var repaidInvestor = context.li_project_transactions.Where(
                    tr => tr.type != (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                          tr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success &&
                          tr.create_time == t.repay_at).Select(tr => tr.investor).ToList();
                if (investers.Count() != repaidInvestor.Count)
                {
                    var unpaidUser = investers.Keys.Except(repaidInvestor).ToList();
                    unpaidUser.ForEach(userId =>
                    {
                        Debug.WriteLine("项目的 {0} 还款计划未对用户 {1} 放款，现在执行放款...", t.repay_at, GetFriendlyUserName(investers[userId]));
                        var taskPtrs = TransactionFacade.GenerateRepayTransactions(t, t.repay_at.Value).ToDictionary(tr => tr.investor);
                        taskPtrs.Where(p => p.Key != userId).ForEach(p => p.Value.dt_users = null); // 移除多余关联

                        var ptr = taskPtrs[userId];
                        context.li_project_transactions.InsertOnSubmit(ptr);

                        // 增加钱包空闲金额与减去待收本金和待收利润
                        var wallet = ptr.dt_users.li_wallets;
                        wallet.idle_money += ptr.repay_interest.GetValueOrDefault(0) + ptr.value;
                        wallet.investing_money -= ptr.value;
                        wallet.profiting_money -= ptr.repay_interest.GetValueOrDefault(0);
                        wallet.total_profit += ptr.repay_interest.GetValueOrDefault(0);
                        wallet.last_update_time = ptr.create_time;

                        // 添加钱包历史
                        var his = TransactionFacade.CloneFromWallet(wallet, TransactionFacade.PTrTypeToHisActTypeMap[(Lip2pEnums.ProjectTransactionTypeEnum)ptr.type]);
                        his.li_project_transactions = ptr;
                        context.li_wallet_histories.InsertOnSubmit(his);
                    });
                }
            });
            context.SubmitChanges();
        }

        [TestMethod]
        public void FixProfitingMoneyIncreasementByPaidValue() // 幂等, 根据实际收益，修复不正确的待收益增长（投资的时候）历史记录
        {
            var context = new Lip2pDataContext(str);
            var sortedList = QueryAbnormalProfitingMoneyList(context);
            sortedList.ForEach(info =>
            {
                var user = info.Key;
                var his = user.li_wallet_histories.ToList();

                decimal fixDelta = 0;
                for (int i = 1; i < his.Count; i++)
                {
                    li_wallet_histories curr = his[i], prev = his[i - 1];
                    curr.profiting_money += fixDelta;

                    decimal increasement = curr.profiting_money - prev.profiting_money;
                    if (0 < increasement)
                    {
                        if (curr.create_time < new DateTime(2015, 5, 3, 18, 0, 0))
                        {
                            var title = curr.li_project_transactions.li_projects.title;
                            var assest = curr.action_type == (int) Lip2pEnums.WalletHistoryTypeEnum.Invest ||
                                         curr.create_time < prev.create_time ||
                                         title.Equals("440F2金屋子1504009子项目（三）-03") ||
                                         title.Equals("440F2金屋子1504008子项目（三）-01");
                            if (!assest)
                            {
                                throw new Exception(string.Format("用户{0}数据有问题：{1} 的历史记录应该是投资记录", GetFriendlyUserName(user), curr.create_time));
                            }
                        }
                        else
                            Debug.Assert(curr.action_type == (int)Lip2pEnums.WalletHistoryTypeEnum.InvestSuccess); // TODO 修正不正常的项目满标（由于不在正常时间完成项目导致的）

                        var project = curr.li_project_transactions.li_projects;
                        if (project.invest_complete_time == null)
                        {
                            continue;
                        }
                        var investments = user.li_project_transactions.Where(tr =>
                                    tr.project == project.id &&
                                    (int)Lip2pEnums.ProjectTransactionTypeEnum.Invest == tr.type &&
                                    tr.status == (int)Lip2pEnums.ProjectTransactionStatusEnum.Success).ToList();
                        // 取得用户的实际收益，修正到增长
                        if (investments.Count == 1) // 只投资了一次的项目
                        {
                            var realProfit = CalcUserTotalProfitFromProject(user, project);
                            var delta = realProfit - increasement;
                            fixDelta += delta;
                            curr.profiting_money += delta;
                        }
                        else // 多次投资同一个项目
                        {
                            if (curr.action_type == (int) Lip2pEnums.WalletHistoryTypeEnum.InvestSuccess) continue; // 暂时忽略新平台的多次投资
                            Debug.Assert(1 < investments.Count);
                            // 只修正最后一次的投资利息增长
                            if (curr.li_project_transactions == investments.Last()) // 这次是最后一次投资
                            {
                                // 求和所有投资历史的 increasement
                                var increasements = investments.Select(inv => inv.li_wallet_histories.Single()).Sum(h => h.profiting_money - his.Last(myhis => myhis.id < h.id).profiting_money);

                                var realProfit = CalcUserTotalProfitFromProject(user, project);
                                var delta = realProfit - increasements;
                                fixDelta += delta;
                                curr.profiting_money += delta;
                            }
                        }
                    }
                }
                Debug.WriteLine("{0} 修正偏移总量: {1}, 最终待收金额: {2}", GetFriendlyUserName(user), fixDelta, his.Last().profiting_money);
                user.li_wallets.profiting_money = his.Last().profiting_money;
            });
            context.SubmitChanges();
        }

        [TestMethod]
        public void FixProfitingMoneyDecreasementByPaidValue() // 幂等, 根据实际收益，修复不正确的待收益缩减（放款的时候）历史记录
        {
            var context = new Lip2pDataContext(str);
            var sortedList = QueryAbnormalProfitingMoneyList(context);
            sortedList.ForEach(info =>
            {
                var user = info.Key;
                var his = user.li_wallet_histories.ToList();

                decimal fixDelta = 0;
                for (int i = 1; i < his.Count; i++)
                {
                    li_wallet_histories curr = his[i], prev = his[i - 1];
                    curr.profiting_money += fixDelta;

                    decimal decreasement = curr.profiting_money - prev.profiting_money;
                    if (decreasement < 0)
                    {
                        if (curr.li_project_transactions.repay_interest != decreasement)
                        {
                            var delta = (-curr.li_project_transactions.repay_interest.Value) - decreasement;
                            fixDelta += delta;
                            curr.profiting_money += delta;
                        }
                    }
                }
                Debug.WriteLine("{0} 修正偏移总量: {1}, 最终待收金额: {2}", GetFriendlyUserName(user), fixDelta, his.Last().profiting_money);
                user.li_wallets.profiting_money = his.Last().profiting_money;
            });
            context.SubmitChanges();
        }

        private static decimal CalcUserTotalProfitFromProject(dt_users user, li_projects project) // may include old platform project repayment task
        {
            var profited = user.li_project_transactions.Where(
                tr => tr.project == project.id &&
                      tr.type != (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                      tr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success)
                .Select(tr => tr.repay_interest.GetValueOrDefault(0))
                .DefaultIfEmpty(0)
                .Sum();
            var unpaidTasks = project.li_repayment_tasks.Where(t => t.status == (int) Lip2pEnums.RepaymentStatusEnum.Unpaid).ToList();
            if (!unpaidTasks.Any())
            {
                return profited;
            }
            else
            {
                var sumOfProfiting = unpaidTasks.Sum(ta =>
                {
                    var trs = TransactionFacade.GenerateRepayTransactions(ta, ta.should_repay_time);
                    var profitingTr = trs.Single(tr => tr.investor == user.id);
                    trs.ForEach(tr => tr.dt_users = null);
                    return profitingTr.repay_interest.GetValueOrDefault(0);
                });
                return sumOfProfiting + profited;
            }
        }

        /// <summary>
        /// 财富中心、市场管理部人员的客户加入对应的组
        /// </summary>
        [TestMethod]
        public void SetInvestorGroup()
        {
            var context = new Lip2pDataContext(str);
            //找出财富中心和市场管理部管理员
            var managerList = context.dt_manager.Where(m => m.user_name == "cfzx" || m.user_name == "scgl");
            
            managerList.ForEach(m =>
                {
                    Debug.WriteLine("管理员‘{0}’下面的组（{1}组）：", m.real_name, m.li_user_group_access_keys.Count());
                    //找出管理员组成员
                    m.li_user_group_access_keys.ForEach(g =>
                    {
                        Debug.WriteLine(string.Format("组‘{0}’的全部客户：", g.dt_user_groups.title));
                        //找出组成员关联的前台会员号
                        g.dt_user_groups.li_user_group_servers.ForEach(u =>
                            {
                                //找出前台会员号的所有客户
                                var userList = context.li_invitations.Where(i => i.inviter == u.serving_user).Select(i => i.dt_users).ToList();
                                Debug.WriteLine("共{0}个：", userList.Count());
                                //所有会员加入当前组
                                userList.ForEach(user => 
                                {
                                    Debug.WriteLine(user.user_name + "（" + user.real_name + "）");
                                    user.group_id = u.group_id; 
                                });
                            });
                    });
                });
            context.SubmitChanges();
        }

        [TestMethod]
        public void FixInviterBonusNotGranted() // 幂等，补充赋予 10% 被推荐人首次投资奖励
        {
            var context = new Lip2pDataContext(str);
            Action<int> func = projectId =>
            {
                var invs = context.li_invitations.Where(i => i.li_project_transactions.project == projectId).ToList();
                if (!invs.Any()) return;

                var project = context.li_projects.Single(p => p.id == projectId);
                var newComerIDs = invs.Select(i => i.user_id).ToList();
                var firstInvestProfits = project.li_project_transactions.Where(
                    tr =>
                        newComerIDs.Contains(tr.investor) &&
                        tr.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                        tr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success)
                    .ToLookup(t => t.dt_users)
                    .ToDictionary(t => t.Key.li_invitations, t => t.First().value*project.profit_rate);

                var profitDate = project.li_repayment_tasks.Max(t => t.should_repay_time);
                var atr =
                    firstInvestProfits.ToList().Where(d =>
                    {
                        var granted = d.Key.dt_users1.li_activity_transactions.Any(
                            a =>
                                a.activity_type ==
                                (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.RefereeFirstTimeProfitBonus &&
                                a.create_time == project.invest_complete_time.Value);
                        if (!granted)
                        {
                            Debug.WriteLine("Fount not granted: " + GetFriendlyUserName(d.Key.dt_users1) +
                                            GetUserPassword(d.Key.dt_users1) + " invited " +
                                            GetFriendlyUserName(d.Key.dt_users));
                        }
                        return !granted;
                    }).Select(d => new li_activity_transactions // 邀请人创建 activity_transaction
                    {
                        user_id = d.Key.inviter,
                        create_time = project.invest_complete_time.Value,
                        value = d.Value/10,
                        remarks =
                            string.Format("好友 {0} 首次投资获利 {1:c}，你于 {2} 得到 {3:c}",
                                string.IsNullOrWhiteSpace(d.Key.dt_users.real_name)
                                    ? d.Key.dt_users.user_name
                                    : d.Key.dt_users.real_name, d.Value, profitDate.ToString("yyyy-MM-dd"), d.Value/10),
                        details = JsonConvert.SerializeObject(new {Invitee = d.Key.user_id}), // 记录被邀请人
                        type = (byte) Lip2pEnums.ActivityTransactionTypeEnum.Gain,
                        status = (byte) Lip2pEnums.ActivityTransactionStatusEnum.Acting,
                        activity_type =
                            (byte) Lip2pEnums.ActivityTransactionActivityTypeEnum.RefereeFirstTimeProfitBonus,
                    });
                context.li_activity_transactions.InsertAllOnSubmit(atr);
            };

            context.li_projects.Where(p => p.invest_complete_time != null).ForEach(p => func(p.id));
            context.SubmitChanges();
        }

        /*[TestMethod]
        public void AutoSendRepayNotice()
        {
            var context = new Lip2pDataContext(str);
            var shouldRepayTask = context.li_repayment_tasks.Where(r => r.li_projects.add_time.Date == DateTime.Today).ToList();
            if (shouldRepayTask.Any())            
                AutoRepay.SendRepayNotice(shouldRepayTask, context);
        }*/
    }
}
