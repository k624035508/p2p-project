using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Core.AutoLogic;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lip2p.Core.ActivityLogic;

namespace Agp2p.Test
{
    public static class Common
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
        
        public static void SetSystemTime(DateTime cSharpTime)
        {
            cSharpTime = cSharpTime.ToUniversalTime();
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

            var setOk = SetSystemTime(ref st); // invoke this method.
            if (!setOk)
            {
                throw new Exception("Can not set system time, you need to run visual studio as admin");
            }
        }

        public static void AutoRepaySimulate(DateTime? runAt = null)
        {
            CheckDelayInvestOverTime.DoCheckDelayInvestOverTime(TimerMsg.Type.AutoRepayTimer, false);

            AutoRepay.DoGainLoanerRepayment(TimerMsg.Type.LoanerRepayTimer, false);

            AutoRepay.CheckStaticProjectWithdrawOvertime(TimerMsg.Type.AutoRepayTimer, false);
            AutoRepay.GenerateHuoqiRepaymentTask(TimerMsg.Type.AutoRepayTimer, false);
            AutoRepay.DoRepay(TimerMsg.Type.AutoRepayTimer, false);
            ProjectWithdraw.HuoqiClaimTransferToCompanyWhenNeeded(TimerMsg.Type.AutoRepayTimer, false);
            ProjectWithdraw.DoHuoqiProjectWithdraw(TimerMsg.Type.AutoRepayTimer, false, runAt.GetValueOrDefault(DateTime.Now));

            TrialActivity.HandleTimerMsg(TimerMsg.Type.AutoRepayTimer, false);
        }

        public static void DoSimpleCleanUp(DateTime deleteAfter)
        {
            var context = new Agp2pDataContext();

            // 在 deleteAfter 之后产生的数据都删除
            var preDelProj = context.li_projects.Where(p => deleteAfter <= p.add_time).ToList();
            preDelProj.ForEach(p =>
            {
                context.li_repayment_tasks.DeleteAllOnSubmit(p.li_repayment_tasks);

                var ptrs = p.li_project_transactions.Where(ptr => deleteAfter <= ptr.create_time).ToList();
                ptrs.ForEach(ptr =>
                {
                    context.li_invitations.DeleteAllOnSubmit(ptr.li_invitations);
                    context.li_wallet_histories.DeleteAllOnSubmit(ptr.li_wallet_histories);
                });
                context.li_project_transactions.DeleteAllOnSubmit(ptrs);

                var preDelClaims = p.li_claims.Where(c => deleteAfter <= c.createTime).ToList();
                context.li_claims.DeleteAllOnSubmit(preDelClaims);

                context.li_risks.DeleteOnSubmit(p.li_risks);

                context.li_company_inoutcome.DeleteAllOnSubmit(p.li_company_inoutcome);
            });
            context.li_projects.DeleteAllOnSubmit(preDelProj);

            context.dt_users.Where(u => deleteAfter <= u.li_wallets.last_update_time).ForEach(u =>
            {
                // 还原钱包数值至 deleteAfter 的时候
                var hisAtThatTime = u.li_wallet_histories.OrderByDescending(h => h.create_time)
                    .FirstOrDefault(h => h.create_time < deleteAfter);
                var wallet = u.li_wallets;
                if (hisAtThatTime == null)
                {
                    wallet.idle_money = 0;
                    wallet.investing_money = 0;
                    wallet.unused_money = 0;
                    wallet.locked_money = 0;
                    wallet.profiting_money = 0;
                    wallet.last_update_time = deleteAfter;
                    wallet.total_charge = 0;
                    wallet.total_withdraw = 0;
                    wallet.total_investment = 0;
                    wallet.total_profit = 0;
                }
                else
                {
                    wallet.idle_money = hisAtThatTime.idle_money;
                    wallet.investing_money = hisAtThatTime.investing_money;
                    wallet.locked_money = hisAtThatTime.locked_money;
                    wallet.profiting_money = hisAtThatTime.profiting_money;
                    wallet.last_update_time = hisAtThatTime.create_time;
                    wallet.total_investment = hisAtThatTime.total_investment;
                    wallet.total_profit = hisAtThatTime.total_profit;
                }

                var preDelBtr = u.li_bank_transactions.Where(btr => deleteAfter <= btr.create_time).ToList();
                preDelBtr.ForEach(chargeRecord =>
                {
                    context.li_wallet_histories.DeleteAllOnSubmit(chargeRecord.li_wallet_histories);
                });
                context.li_bank_transactions.DeleteAllOnSubmit(preDelBtr);

                u.li_bank_accounts.ForEach(account =>
                {
                    var preDelWithdrawBtr = account.li_bank_transactions.Where(btr => deleteAfter <= btr.create_time).ToList();
                    preDelWithdrawBtr.ForEach(withdrawRecord =>
                    {
                        context.li_wallet_histories.DeleteAllOnSubmit(withdrawRecord.li_wallet_histories);
                    });
                    context.li_bank_transactions.DeleteAllOnSubmit(preDelWithdrawBtr);
                });

                var preDelAtr = u.li_activity_transactions.Where(atr => deleteAfter <= atr.create_time).ToList();
                preDelAtr.ForEach(atr =>
                {
                    context.li_wallet_histories.DeleteAllOnSubmit(atr.li_wallet_histories);
                });
                context.li_activity_transactions.DeleteAllOnSubmit(preDelAtr);
            });

            var managerLogs = context.dt_manager_log.Where(l => deleteAfter <= l.add_time).ToList();
            context.dt_manager_log.DeleteAllOnSubmit(managerLogs);

            context.SubmitChanges();
        }

        public static void RestoreDate(DateTime realDate)
        {
            var now = DateTime.Now;
            if (now.Date == realDate.Date) return;

            var realNow = DateTime.Now.AddDays((realDate.Date - now.Date).TotalDays);
            SetSystemTime(realNow);
        }

        public static void PublishHuoqiProject(string projectName, decimal financingAmount = 1000000, decimal profitingYearly = 3.3m)
        {
            var context = new Agp2pDataContext();
            var now = DateTime.Now;

            var loaner = context.li_loaners.Single(l => l.dt_users.user_name == "CompanyAccount");
            var huoqiCategory = context.dt_article_category.Single(c => c.call_index == "huoqi");
            var project = new li_projects
            {
                li_risks = new li_risks
                {
                    last_update_time = now,
                    li_loaners = loaner
                },
                category_id = huoqiCategory.id,
                type = (int)Agp2pEnums.LoanTypeEnum.Company,
                sort_id = 99,
                add_time = now,
                publish_time = now,
                make_loan_time = now,
                user_name = "admin",
                title = projectName,
                no = projectName,
                financing_amount = financingAmount,
                repayment_term_span_count = 1,
                repayment_term_span = (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Day,
                repayment_type = (int)Agp2pEnums.ProjectRepaymentTypeEnum.HuoQi,
                profit_rate_year = profitingYearly,
                status = (int)Agp2pEnums.ProjectStatusEnum.Financing,
            };
            context.li_projects.InsertOnSubmit(project);

            context.SubmitChanges();
        }

        public static void PublishMonthlyProject(string projectName, int repayMonth, decimal financingAmount, decimal profitingYearly)
        {
            var context = new Agp2pDataContext();
            var now = DateTime.Now;

            var loaner = context.li_loaners.Single(l => l.dt_users.real_name == "杨长岭");
            var ypbCategory = context.dt_article_category.Single(c => c.call_index == "rtb");
            var project = new li_projects
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
                user_name = "admin",
                title = projectName,
                no = projectName,
                financing_amount = financingAmount,
                repayment_term_span_count = repayMonth,
                repayment_term_span = (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Month,
                repayment_type = (int?)Agp2pEnums.ProjectRepaymentTypeEnum.XianXi,
                profit_rate_year = profitingYearly,
                status = (int)Agp2pEnums.ProjectStatusEnum.Financing,
            };
            context.li_projects.InsertOnSubmit(project);

            context.SubmitChanges();
        }

        public static void PublishProject(string projectName, int repayDays, decimal financingAmount, decimal profitingYearly)
        {
            var context = new Agp2pDataContext();
            var now = DateTime.Now;

            var loaner = context.li_loaners.Single(l => l.dt_users.real_name == "杨长岭");
            var ypbCategory = context.dt_article_category.Single(c => c.call_index == "ypb");
            var project = new li_projects
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
                user_name = "admin",
                title = projectName,
                no = projectName,
                financing_amount = financingAmount,
                repayment_term_span_count = repayDays,
                repayment_term_span = (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Day,
                repayment_type = (int)Agp2pEnums.ProjectRepaymentTypeEnum.DaoQi,
                profit_rate_year = profitingYearly,
                status = (int)Agp2pEnums.ProjectStatusEnum.Financing,
            };
            context.li_projects.InsertOnSubmit(project);

            context.SubmitChanges();
        }

        public static void InvestProject(string userName, string projectName, int amount)
        {
            var context = new Agp2pDataContext();
            var investor = context.dt_users.Single(u => u.user_name == userName);
            var project = context.li_projects.Single(p => p.title == projectName);
            TransactionFacade.Invest(investor.id, project.id, amount);
        }

        internal static void InvestProjectWithTicket(string userName, string projectName, decimal amount, int ticketId)
        {
            var context = new Agp2pDataContext();
            var investor = context.dt_users.Single(u => u.user_name == userName);
            var project = context.li_projects.Single(p => p.title == projectName);

            TransactionFacade.Invest(investor.id, project.id, amount, "", ticketId);
        }

        public static void ProjectStartRepay(string projectName)
        {
            var context = new Agp2pDataContext();
            var project = context.li_projects.Single(p => p.title == projectName);
            new Agp2pDataContext().StartRepayment(project.id);
        }

        public static void DeltaDay(DateTime realDate, int delta)
        {
            var delt = (int)(realDate.Date - DateTime.Today).TotalDays + delta;
            if (delt == 0) return;
            SetSystemTime(DateTime.Now.AddDays(delt));
        }

        public static void AssertWalletDelta(string userName, decimal idleMoney, decimal investingMoney, decimal lockedMoney,
            decimal profitingMoney, int totalCharge, int totalWithdraw, decimal totalInvestment, decimal totalProfit, DateTime compareAt)
        {
            var context = new Agp2pDataContext();
            var user = context.dt_users.Single(u => u.user_name == userName);
            var comparingHis = user.li_wallet_histories.OrderByDescending(h => h.create_time)
                .FirstOrDefault(h => h.create_time < compareAt);
            var wallet = user.li_wallets;

            Assert.AreEqual((comparingHis?.idle_money).GetValueOrDefault() + idleMoney, wallet.idle_money);
            Assert.AreEqual((comparingHis?.investing_money).GetValueOrDefault() + investingMoney, wallet.investing_money);
            Assert.AreEqual((comparingHis?.locked_money).GetValueOrDefault() + lockedMoney, wallet.locked_money);
            Assert.AreEqual((comparingHis?.profiting_money).GetValueOrDefault() + profitingMoney, wallet.profiting_money);

            Assert.AreEqual((comparingHis?.total_profit).GetValueOrDefault() + totalProfit, wallet.total_profit);
            Assert.AreEqual((comparingHis?.total_investment).GetValueOrDefault() + totalInvestment, wallet.total_investment);

            var btrs = context.li_bank_transactions.Where(btr => compareAt <= btr.create_time).ToList();
            var chargeDelta = btrs.Where(
                btr =>
                    btr.type == (int) Agp2pEnums.BankTransactionTypeEnum.Charge &&
                    btr.status == (int) Agp2pEnums.BankTransactionStatusEnum.Confirm)
                .Aggregate(0m, (sum, btr) => sum + btr.value);
            Assert.AreEqual(chargeDelta, totalCharge);

            var withdrawDelta = btrs.Where(
                btr =>
                    btr.type == (int) Agp2pEnums.BankTransactionTypeEnum.Withdraw &&
                    btr.status == (int) Agp2pEnums.BankTransactionStatusEnum.Confirm)
                .Aggregate(0m, (sum, btr) => sum + btr.value);
            Assert.AreEqual(withdrawDelta, totalWithdraw);
        }

        public static void StaticProjectWithdraw(string projectName, string userName, decimal amount, decimal keepInterestPercent = 1)
        {
            var context = new Agp2pDataContext();
            var project = context.li_projects.Single(p => p.title == projectName);
            var user = context.dt_users.Single(u => u.user_name == userName);
            var preWithdrawClaim = project.li_claims.Where(c => c.userId == user.id && c.principal == amount).AsEnumerable().Single(c => c.IsProfiting());
            TransactionFacade.StaticProjectWithdraw(context, preWithdrawClaim.id, keepInterestPercent);
        }

        public static void BuyClaim(string projectName, string userName, decimal amount)
        {
            var context = new Agp2pDataContext();
            var project = context.li_projects.Single(p => p.title == projectName);
            var user = context.dt_users.Single(u => u.user_name == userName);

            var preBuyClaim = project.li_claims.Where(
                c =>
                    c.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                    c.IsLeafClaim()).OrderBy(c => Math.Abs(c.principal - amount)).First();

            TransactionFacade.BuyClaim(context, preBuyClaim.id, user.id, amount);

            context.SubmitChanges();
        }

        public static void HuoqiProjectWithdraw(string projectName, string userName, decimal amount)
        {
            var context = new Agp2pDataContext();
            var project = context.li_projects.Single(p => p.title == projectName);
            var user = context.dt_users.Single(u => u.user_name == userName);
            context.HuoqiProjectWithdraw(user.id, project.id, amount);
        }

        public static void MakeSureHaveIdleMoney(string userName, decimal amount)
        {
            var context = new Agp2pDataContext();
            var wallet = context.li_wallets.Single(w => w.dt_users.user_name == userName);
            if (wallet.idle_money < amount)
            {
                Debug.WriteLine($"为用户 {wallet.dt_users.GetFriendlyUserName()} 充值 {amount - wallet.idle_money}");
                var btr = context.Charge(wallet.user_id, amount - wallet.idle_money, Agp2pEnums.PayApiTypeEnum.ManualAppend);
                context.ConfirmBankTransaction(btr.id, null);
            }
        }
    }
}
