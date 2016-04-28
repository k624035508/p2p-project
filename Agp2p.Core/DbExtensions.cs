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

        public static bool IsTicketProject(this li_projects p)
        {
            return p.dt_article_category.call_index == "ypb" || p.dt_article_category.call_index == "ypl";
        }

        public static li_claims MakeChild(this li_claims parent, DateTime createTime)
        {
            return new li_claims
            {
                Parent = parent,
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
            var child = parent.NewPrincipalChild(createTime, childPrincipal);
            child.status = (byte)newStatus;
            return child;
        }

        public static li_claims TransferedChild(this li_claims parent, DateTime createTime,
            Agp2pEnums.ClaimStatusEnum newStatus, decimal childPrincipal, li_project_transactions byPtr)
        {
            var child = parent.NewPrincipalAndStatusChild(createTime, newStatus, childPrincipal);
            child.number = Utils.HiResNowString;
            child.profitingProjectId = byPtr.project;
            child.userId = byPtr.investor;
            child.li_project_transactions_invest = byPtr;
            return child;
        }

        public static li_claims GetHistoryClaimByTime(this li_claims childClaim, DateTime time)
        {
            if (childClaim.createTime <= time)
            {
                return childClaim;
            }
            else if (childClaim.Parent!= null && childClaim.Parent.userId == childClaim.userId)
            {
                return childClaim.Parent.GetHistoryClaimByTime(time);
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
                return !claim.Children.Any();
            }
            var historyClaimByTime = claim.GetHistoryClaimByTime(moment.Value);
            if (historyClaimByTime == null)
                return false;
            return !historyClaimByTime.Children.Any(c => c.createTime <= moment.Value);
        }

        public static bool IsProfiting(this li_claims claim, DateTime? moment = null)
        {
            // 必须是叶子债权才能收益
            if (!claim.IsLeafClaim(moment))
                return false;

            if (claim.li_projects_profiting.IsHuoqiProject())
            {
                if (claim.status < (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer)
                {
                    // 如果是昨日之前创建的 不可转让/可转让 债权，则会产生收益（提现后不再产生收益）
                    var checkPoint = moment.GetValueOrDefault(DateTime.Now).Date.AddDays(-1);
                    return claim.createTime < checkPoint ||
                           claim.li_project_transactions_invest.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.AutoInvest; // 自动续投的话会产生收益
                }
                return false;
            }
            return claim.status < (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer;
        }

        public static bool IsAgent(this dt_users user)
        {
            return user.dt_user_groups.title == AutoRepay.AgentGroup;
        }

        public static bool IsCompanyAccount(this dt_users user)
        {
            return user.dt_user_groups.title == AutoRepay.CompanyAccount;
        }

        public static bool IsChildOf(this li_claims childClaim, li_claims parentClaim)
        {
            if (parentClaim.createTime <= childClaim.createTime)
            {
                if (childClaim.Parent != null)
                {
                    return childClaim.Parent == parentClaim || childClaim.Parent.IsChildOf(parentClaim);
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
            if (claim.Parent == null)
            {
                return claim;
            }
            else if (claim.Parent.userId != claim.userId)
            {
                return claim;
            }
            else
            {
                return claim.Parent.GetSourceClaim();
            }
        }

        public static li_claims GetRootClaim(this li_claims claim)
        {
            return claim.Parent == null ? claim : claim.Parent.GetRootClaim();
        }

        public static li_claims GetHistoryClaimByOwner(this li_claims claim, int userId)
        {
            if (claim.userId == userId)
            {
                return claim;
            }
            else
            {
                return claim.Parent?.GetHistoryClaimByOwner(userId);
            }
        }

        public static li_claims GetFirstHistoryClaimByOwner(this li_claims claim, int userId)
        {
            var hisClaim = claim.GetHistoryClaimByOwner(userId);
            var olderClaim = hisClaim.Parent?.GetHistoryClaimByOwner(userId);
            if (olderClaim != null)
            {
                return hisClaim != olderClaim ? olderClaim.GetFirstHistoryClaimByOwner(userId) : olderClaim;
            }
            return hisClaim;
        }

        public static IEnumerable<li_claims> QueryLeafClaimsAtMoment(this li_claims rootClaim, DateTime? moment = null)
        {
            var realMoment = moment ?? DateTime.Now;

            var childClaimsAtMoment = rootClaim.Children.Where(c => c.createTime <= realMoment).ToList();
            if (childClaimsAtMoment.Any())
            {
                return childClaimsAtMoment.SelectMany(c => c.QueryLeafClaimsAtMoment(realMoment));
            }
            else if (rootClaim.createTime <= realMoment)
            {
                return Enumerable.Repeat(rootClaim, 1);
            }
            return Enumerable.Empty<li_claims>();
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="claim"></param>
        /// <param name="task"></param>
        /// <param name="callback">三个参数的回调：债权生效前天数，债权生效天数，债权失效后天数</param>
        /// <returns></returns>
        public static T GetProfitingSectionDays<T>(this li_claims claim, li_repayment_tasks task, Func<int, int, int, T> callback)
        {
            int claimBeforeProfitingDays, claimProfitingDays;

            var taskStartProfitingTime = task.GetStartProfitingTime();
            if (claim.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer)
            {
                var parent = claim.Parent;
                // 提现债权（返回提现成功后的实际收益天数）
                claimBeforeProfitingDays = parent.createTime <= taskStartProfitingTime
                    ? 0
                    : (int) (parent.createTime.Date - taskStartProfitingTime.Date).TotalDays;

                claimProfitingDays = (int) (claim.createTime.Date - parent.createTime.Date).TotalDays;
            }
            else
            {
                // 普通债权
                claimBeforeProfitingDays = claim.createTime <= taskStartProfitingTime
                    ? 0
                    : (int) (claim.createTime.Date - taskStartProfitingTime.Date).TotalDays;

                claimProfitingDays = (int) (task.should_repay_time.Date - new[] {claim.createTime, taskStartProfitingTime}.Max().Date).TotalDays;
            }

            var claimInvalidDays = task.GetTotalProfitingDays() - claimBeforeProfitingDays - claimProfitingDays;
            Debug.Assert(0 <= claimInvalidDays);
            return callback(claimBeforeProfitingDays, claimProfitingDays, claimInvalidDays);
        }

        public static IEnumerable<T> AsEnumerableAutoPartialQuery<T>(this IQueryable<T> src, out int totalCount, int queryAmountOnce = 32)
        {
            totalCount = src.Count();
            var totalPage = (int) Math.Ceiling((decimal)totalCount / queryAmountOnce);

            return Enumerable.Range(0, totalPage)
                .SelectMany(partIndex => src.Skip(queryAmountOnce*partIndex).Take(queryAmountOnce).ToList());
        }

        public static IEnumerable<T> AsEnumerableAutoPartialQuery<T>(this IQueryable<T> src, int queryAmountOnce = 32)
        {
            return Utils.Infinite()
                .Select(partIndex => src.Skip(queryAmountOnce * partIndex).Take(queryAmountOnce).ToList())
                .TakeWhile(ls => ls.Any())
                .SelectMany(ls => ls);
        }

        public static DateTime GetStatusChangingTime(this li_projects project)
        {
            var status = (Agp2pEnums.ProjectStatusEnum) project.status;
            if (status < Agp2pEnums.ProjectStatusEnum.Financing)
            {
                return project.add_time;
            }
            else if (status < Agp2pEnums.ProjectStatusEnum.FinancingSuccess)
            {
                return project.publish_time.Value;
            }
            else if (status < Agp2pEnums.ProjectStatusEnum.ProjectRepaying)
            {
                return project.invest_complete_time.Value;
            }
            else if (status < Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime)
            {
                return project.make_loan_time.Value;
            }
            else
            {
                return project.complete_time.Value;
            }
        }

        public static int QueryEventTimesDuring(this Agp2pDataContext context, int userId, Agp2pEnums.EventRecordTypeEnum eventType, TimeSpan timeSpan)
        {
            return context.li_event_records.Count(
                    r => r.userId == userId && r.eventType == eventType && DateTime.Now - timeSpan <= r.occurAt);
        }

        public static void MarkEventOccurNotSave(this Agp2pDataContext context, int userId, Agp2pEnums.EventRecordTypeEnum eventType, DateTime occurAt)
        {
            context.li_event_records.InsertOnSubmit(new li_event_records
            {
                userId = userId,
                eventType = Agp2pEnums.EventRecordTypeEnum.IdcardChecking,
                occurAt = occurAt
            });
        }
    }
}
