using System;
using System.Web.Services;
using System.Linq;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;

namespace Agp2p.Web.UI.Page
{
    /// <summary>
    /// 我要借款
    /// </summary>
    public partial class loan : Web.UI.BasePage
    {
        protected dt_users user;
        protected int step = 2;
        protected int userId = 0;
        protected int loanerId = 0;
        protected int pendingProjectId = 0;
        protected int quota_use = 0;

        /// <summary>
        /// 重写父类的虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            Init += Page_Init; 
        }

        void Page_Init(object sender, EventArgs e)
        {
            user = GetUserInfoByLinq();
            if (user == null)
                step = 1;//显示登录步骤
            else
            {
                userId = user.id;
                //查看是否已为借款人
                Agp2pDataContext context = new Agp2pDataContext();
                var loaner = context.li_loaners.SingleOrDefault(l => l.user_id == user.id);
                if (loaner == null)
                {
                    step = 2; //显示申请借款人步骤
                }
                else
                {
                    //查看借款人状态
                    switch (loaner.status)
                    {
                        case (int)Agp2pEnums.LoanerStatusEnum.Pending:
                            step = 21;//显示申请借款人审核中
                            return;
                        case (int)Agp2pEnums.LoanerStatusEnum.PendingFail:
                            step = 22;//显示申请借款人失败，重新申请
                            return;
                        case (int)Agp2pEnums.LoanerStatusEnum.Disable:
                            step = 23;//显示禁止再申请借款人
                            return;
                    }

                    //查看是否有在审批中的借款申请
                    var project = context.li_projects.Where(p => p.user_name == user.user_name && p.status == (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationUncommitted).ToList();
                    if (project.Any())
                        step = 4;//显示正在审批步骤
                    else
                    {
                        //查询已审核通过的借款
                        var projectAll =
                            context.li_projects.Where(
                                p =>
                                    p.user_name == user.user_name &&
                                    p.status > (int) Agp2pEnums.ProjectStatusEnum.FinancingApplicationUncommitted &&
                                    p.status != (int) Agp2pEnums.ProjectStatusEnum.FinancingFail);
                        //查询可用额度
                        quota_use = loaner.quota - (int)projectAll.Sum(p => p.financing_amount);

                        step = 5;//显示已发布借款步骤
                    }
                }
            }

        }

        /// <summary>
        /// 申请成为借款人
        /// </summary>
        /// <param name="age"></param>
        /// <param name="native_place"></param>
        /// <param name="job"></param>
        /// <param name="working_at"></param>
        /// <param name="working_company"></param>
        /// <param name="educational_background"></param>
        /// <param name="marital_status"></param>
        /// <param name="income"></param>
        /// <returns></returns>
        [WebMethod]
        public static string ApplyLoaner(int userId, short age, string native_place, string job, string working_at, string working_company, string educational_background, int marital_status, string income)
        {
            try
            {
                li_loaners loaner = new li_loaners()
                {
                    user_id = userId,
                    age = age,
                    native_place = native_place,
                    job = job,
                    working_at = working_at,
                    working_company = working_company,
                    educational_background = educational_background,
                    marital_status = (byte)marital_status,
                    income = income,
                    status = (int)Agp2pEnums.LoanerStatusEnum.Pending
                };
                Agp2pDataContext context = new Agp2pDataContext();
                context.li_loaners.InsertOnSubmit(loaner);
                context.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return JsonConvert.SerializeObject("ok");
        }

        /// <summary>
        /// 申请借款
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="userName"></param>
        /// <param name="loaner_content"></param>
        /// <param name="loan_usage"></param>
        /// <param name="source_of_repayment"></param>
        /// <param name="category_id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [WebMethod]
        public static string ApplyLoan(int loanId, string userName, string loaner_content, string loan_usage, string source_of_repayment, int category_id, int amount)
        {
            try
            {
                Agp2pDataContext context = new Agp2pDataContext();
                var risk = new li_risks()
                {
                    loaner = loanId,
                    loaner_content = loaner_content,
                    loan_usage = loan_usage,
                    source_of_repayment = source_of_repayment
                };
                var project = new li_projects()
                {
                    li_risks = risk,
                    category_id = category_id,
                    financing_amount = amount,
                    user_name = userName,
                    add_time = DateTime.Now,
                    status = (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationUncommitted
                };
                //项目编号
                var latestProject = context.li_projects.Where(p => p.category_id == project.category_id).OrderByDescending(p => p.add_time).FirstOrDefault();
                int prjectCount = latestProject == null ? 0 : Utils.StrToInt(latestProject.title.Substring(latestProject.title.Length - 5), 0);
                project.title += new BLL.article_category().GetModel(project.category_id).call_index.ToUpper() + (prjectCount + 1).ToString("00000");
                context.li_risks.InsertOnSubmit(risk);
                context.li_projects.InsertOnSubmit(project);
                context.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return JsonConvert.SerializeObject("ok");
        }
    }
}
