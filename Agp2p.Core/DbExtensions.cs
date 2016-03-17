using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Core.AutoLogic;
using Agp2p.Linq2SQL;

namespace Agp2p.Core
{
    public static class DbExtensions
    {
        public static void AppendAdminLog(this Agp2pDataContext context, string actionType, string remark, bool sysLog = true, int userId = 1, string userName = "admin")
        {
            var dtManagerLog = new dt_manager_log
            {
                user_id = userId,
                user_name = userName,
                action_type = actionType,
                remark = remark,
                user_ip = sysLog ? "" : DTRequest.GetIP(),
                add_time = DateTime.Now
            };
            context.dt_manager_log.InsertOnSubmit(dtManagerLog);
        }

        public static void AppendAdminLogAndSave(this Agp2pDataContext context, string actionType, string remark, bool sysLog = true, int userId = 1, string userName = "admin")
        {
            context.AppendAdminLog(actionType, remark, sysLog, userId, userName);
            context.SubmitChanges();
        }

        public static void AppendAdminMessage(this Agp2pDataContext context, dt_manager receiver,
            Agp2pEnums.ManagerMessageSourceEnum source, string title, string body, DateTime createTime)
        {
            var newManagerMsg = new li_manager_messages
            {
                body = body,
                creationTime = createTime,
                title = title,
                source = (int)source,
                receiver = receiver.id
            };
            context.li_manager_messages.InsertOnSubmit(newManagerMsg);
        }

        public static void AppendAdminMessageAndSave(this Agp2pDataContext context, dt_manager receiver,
            Agp2pEnums.ManagerMessageSourceEnum source, string title, string body, DateTime createTime)
        {
            context.AppendAdminMessage(receiver, source, title, body, createTime);
            context.SubmitChanges();
        }

        public static string GetFriendlyUserName(this dt_users user)
        {
            if (!string.IsNullOrWhiteSpace(user.real_name))
            {
                return $"{user.user_name}（{user.real_name}）";
            }
            return !string.IsNullOrWhiteSpace(user.nick_name) ? $"{user.user_name}（{user.nick_name}）" : user.user_name;
        }

        public static bool IsNewbieProject(this li_projects p)
        {
            return p.dt_article_category.call_index == "newbie";
        }

        public static bool IsHuoqiProject(this li_projects p)
        {
            return p.dt_article_category.call_index == "huoqi";
        }

        public static li_claims MakeChild(this li_claims parent, DateTime createTime)
        {
            return new li_claims
            {
                li_claims1 = parent,
                createFromInvestment = parent.createFromInvestment,
                createTime = createTime,
                userId = parent.userId,
                principal = parent.principal,
                projectId = parent.projectId,
                profitingProjectId = parent.profitingProjectId,
                number = parent.number,
                status = parent.status,
                agent = parent.agent,
            };
        }

        public static li_claims NewStatusChild(this li_claims parent, DateTime createTime, Agp2pEnums.ClaimStatusEnum newStatus)
        {
            var child = parent.MakeChild(createTime);
            child.status = (byte)newStatus;
            return child;
        }

        public static li_claims NewPrincipalChild(this li_claims parent, DateTime createTime, decimal childPrincipal)
        {
            var child = parent.MakeChild(createTime);
            child.principal = childPrincipal;
            return child;
        }

        public static li_claims NewPrincipalAndStatusChild(this li_claims parent, DateTime createTime, Agp2pEnums.ClaimStatusEnum newStatus, decimal childPrincipal)
        {
            var child = parent.NewStatusChild(createTime, newStatus);
            child.principal = childPrincipal;
            return child;
        }

        public static li_claims TransferedChild(this li_claims parent, DateTime createTime,
            Agp2pEnums.ClaimStatusEnum newStatus, decimal childPrincipal, li_project_transactions byPtr)
        {
            var child = parent.NewPrincipalAndStatusChild(createTime, newStatus, childPrincipal);
            child.number = Utils.HiResNowString;
            child.profitingProjectId = byPtr.project;
            child.userId = byPtr.investor;
            child.li_project_transactions1 = byPtr;
            return child;
        }

        public static li_claims GetHistoryClaimByTime(this li_claims childClaim, DateTime time)
        {
            if (childClaim.createTime <= time)
            {
                return childClaim;
            }
            else if (childClaim.li_claims1 != null && childClaim.li_claims1.userId == childClaim.userId)
            {
                return childClaim.li_claims1.GetHistoryClaimByTime(time);
            }
            else
            {
                return null;
            }
        }

        public static Agp2pEnums.ClaimStatusEnum? GetStatusByTime(this li_claims childClaim, DateTime time)
        {
            return (Agp2pEnums.ClaimStatusEnum?)childClaim.GetHistoryClaimByTime(time)?.status;
        }

        public static bool IsLeafClaim(this li_claims claim, DateTime? moment = null)
        {
            if (moment == null)
            {
                return !claim.li_claims2.Any();
            }
            var historyClaimByTime = claim.GetHistoryClaimByTime(moment.Value);
            if (historyClaimByTime == null)
                return false;
            return !historyClaimByTime.li_claims2.Any(c => c.createTime <= moment.Value);
        }

        public static bool IsProfiting(this li_claims claim, DateTime? moment = null, bool includeWithdrawing = false)
        {
            // 必须是叶子债权才能收益
            if (!claim.IsLeafClaim(moment))
                return false;

            if (claim.li_projects1.IsHuoqiProject())
            {
                if (claim.status < (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer
                    || (includeWithdrawing && claim.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer))
                {
                    // 如果是昨日之前创建的 不可转让/可转让 债权，则会产生收益（提现后不再产生收益）
                    var checkPoint = moment.GetValueOrDefault(DateTime.Now).Date.AddDays(-1);
                    return claim.createTime < checkPoint;
                }
                return false;
            }
            return claim.status < (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer ||
                   (includeWithdrawing && claim.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer);
        }

        public static bool IsCompanyAccount(this dt_users user)
        {
            return user.dt_user_groups.title == AutoRepay.ClaimTakeOverGroupName;
        }

        public static bool IsChildOf(this li_claims childClaim, li_claims parentClaim)
        {
            if (parentClaim.createTime <= childClaim.createTime)
            {
                if (childClaim.li_claims1 != null)
                {
                    return childClaim.li_claims1 == parentClaim || childClaim.li_claims1.IsChildOf(parentClaim);
                }
            }
            return false;
        }

        public static bool IsParentOf(this li_claims parentClaim, li_claims childClaim)
        {
            return childClaim.IsChildOf(parentClaim);
        }

        public static DateTime GetStartProfitingTime(this li_repayment_tasks task)
        {
            if (task.term == 1)
            {
                return task.li_projects.make_loan_time.Value;
            }
            var prevTask = task.li_projects.li_repayment_tasks.OrderByDescending(ta => ta.should_repay_time)
                .First(ta => ta.should_repay_time < task.should_repay_time);
            return prevTask.should_repay_time;
        }

        public static li_claims GetSourceClaim(this li_claims claim)
        {
            if (claim.li_claims1 == null)
            {
                return claim;
            }
            else if (claim.li_claims1.userId != claim.userId)
            {
                return claim;
            }
            else
            {
                return claim.li_claims1.GetSourceClaim();
            }
        }

        public static li_claims GetHistoryClaimByOwner(this li_claims claim, int userId)
        {
            if (claim.userId == userId)
            {
                return claim;
            }
            else
            {
                return claim.li_claims1?.GetHistoryClaimByOwner(userId);
            }
        }

        public static li_claims GetFirstHistoryClaimByOwner(this li_claims claim, int userId)
        {
            var hisClaim = claim.GetHistoryClaimByOwner(userId);
            var olderClaim = hisClaim.li_claims1?.GetHistoryClaimByOwner(userId);
            if (olderClaim != null)
            {
                return hisClaim != olderClaim ? olderClaim.GetFirstHistoryClaimByOwner(userId) : olderClaim;
            }
            return hisClaim;
        }

        public static int GetTotalProfitingDays(this li_repayment_tasks task)
        {
            return (int)(task.should_repay_time.Date - task.GetStartProfitingTime().Date).TotalDays;
        }

        public static int GetProfitingDaysByTime(this li_repayment_tasks task, DateTime? moment = null)
        {
            Debug.Assert(task.IsUnpaid());
            var startTime = moment.GetValueOrDefault(DateTime.Now).Date;

            Debug.Assert(task.GetStartProfitingTime().Date <= startTime);
            Debug.Assert(startTime <= task.should_repay_time.Date);

            return (int)(startTime - task.GetStartProfitingTime().Date).TotalDays;
        }

        public static T GetProfitingDays<T>(this li_claims claim, li_repayment_tasks task, Func<int, int, T> callback, DateTime? moment = null)
        {
            /* 1、债权转让人的收益从提交转让申请当天停止计息，若转让失败则恢复转让期间利息并继续计息；
               2、购买债权转让项目从债权转让人提交债权转让申请当天开始计息；*/
            Debug.Assert(task.project == claim.projectId);
            Debug.Assert(claim.status < (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer);

            var startProfitingPoint = new[]
                {
                    claim.createTime.Date,
                    task.GetStartProfitingTime().Date
                }.Max();
            var endProfitingPoint = new[]
                {
                    moment.GetValueOrDefault(DateTime.Now).Date,
                    task.should_repay_time.Date
                }.Min();

            var claimPrifitingDays = (int)(endProfitingPoint - startProfitingPoint).TotalDays;

            var taskTotalProfitingDay = task.GetTotalProfitingDays();
            Debug.Assert(claimPrifitingDays <= taskTotalProfitingDay);
            return callback(claimPrifitingDays, taskTotalProfitingDay);
        }

        public static T GetWithdrawClaimProfitingDays<T>(this li_claims claim, li_repayment_tasks task, Func<int, int, int, T> callback)
        {
            Debug.Assert(task.project == claim.projectId);
            Debug.Assert(claim.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer);

            var ct = claim.li_claims1.GetProfitingDays(task, (claimProfitingDays, taskProfitingDays) => new { claimProfitingDays, taskProfitingDays}, task.should_repay_time);

            var withdrawClaimStartProfitingPoint = new[]
            {
                claim.li_claims1.createTime.Date, task.GetStartProfitingTime().Date
            }.Max();
            var withdrawClaimEndProfitingPoint = new[]
            {
                // 利润从开始提现的那天开始算
                claim.createTime.Date, task.should_repay_time.Date
            }.Min();
            var withdrawClaimPrifitedDays = (int)(withdrawClaimEndProfitingPoint - withdrawClaimStartProfitingPoint).TotalDays;
            return callback(withdrawClaimPrifitedDays, ct.claimProfitingDays, ct.taskProfitingDays);
        }
    }
}
