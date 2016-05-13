using System;
using System.ComponentModel;
using System.Web.Services;
using System.Linq;
using System.Net;
using System.Web;
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
        protected LoanApplyStep step = LoanApplyStep.Unlogin;
        protected int user_id = 0;
        protected string user_name = "";
        protected int loaner_id = 0;
        protected int pending_project_id = 0;
        protected int quota_use = 0;

        protected li_loaners loaner;
        protected li_projects project;

        protected string loaner_name = "";
        protected string loaner_mobile = "";
        protected string loaner_age = "";
        protected string loaner_native_place = "";
        protected string loaner_job = "";
        protected string loaner_working_at = "";
        protected string loaner_working_company = "";
        protected string loaner_educational_background = "";
        protected string loaner_marital_status = "";
        protected string loaner_income = "";

        protected int project_category_id = 0;
        protected int project_amount = 0;
        protected string project_loan_usage = "";
        protected string project_loaner_content = "";
        protected string project_source_of_repayment = "";

        /// <summary>
        /// 重写父类的虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            Init += Page_Init; 
        }

        public enum LoanApplyStep
        {
            [Description("未登录")]
            Unlogin = 0,
            [Description("未申请借款人")]
            UnApplyAsLoaner = 1,
            [Description("申请借款人审核中")]
            ApplyAsLoanerAuditting = 2,
            [Description("未申请借款项目")]
            UnApplyProject = 3,
            [Description("申请借款项目审核中")]
            ProjectAuditting = 4,
            [Description("完成借款申请")]
            ProjectApplyCompleted = 5,
        }

        void Page_Init(object sender, EventArgs e)
        {
            var user = GetUserInfoByLinq();
            if (user == null)
                step = LoanApplyStep.Unlogin;
            else
            {
                user_id = user.id;
                user_name = user.user_name;
                //查看是否已为借款人
                Agp2pDataContext context = new Agp2pDataContext();
                loaner = context.li_loaners.SingleOrDefault(l => l.user_id == user.id);
                if (loaner == null)
                {
                    step = LoanApplyStep.UnApplyAsLoaner;
                    loaner_name = user.real_name;
                    loaner_mobile = user.mobile;
                }
                else
                {
                    //借款人信息
                    loaner_id = loaner.id;
                    loaner_age = loaner.age.ToString();
                    loaner_educational_background = loaner.educational_background;
                    loaner_income = loaner.income;
                    loaner_job = loaner.job;
                    loaner_marital_status = loaner.marital_status.ToString();
                    loaner_mobile = loaner.dt_users.mobile;
                    loaner_name = loaner.dt_users.real_name;
                    loaner_native_place = loaner.native_place;
                    loaner_working_at = loaner.working_at;
                    loaner_working_company = loaner.working_company;

                    if (loaner.status != (int) Agp2pEnums.LoanerStatusEnum.Normal)
                    {
                        step = LoanApplyStep.ApplyAsLoanerAuditting;
                        return;
                    }

                    //查看是否有在审批中的借款申请
                    project = context.li_projects.FirstOrDefault(p => p.li_risks.li_loaners.dt_users.id == user.id &&
                                                                      p.status <= (int) Agp2pEnums.ProjectStatusEnum.FinancingApplicationChecking);
                    if (project != null)
                    {
                        step = LoanApplyStep.ProjectAuditting; //显示正在审批步骤
                        project_category_id = project.category_id;
                        project_amount = (int)project.financing_amount;
                        project_loan_usage = project.li_risks.loan_usage;
                        project_source_of_repayment = project.li_risks.source_of_repayment;
                        project_loaner_content = project.li_risks.loaner_content;
                        quota_use = loaner.quota - (int)project.financing_amount;
                    }
                    else
                    {
                        //查询已审核通过的借款
                        var projectAll = context.li_projects.Where(p =>
                                    p.li_risks.li_loaners.dt_users.id == user.id &&
                                    (int) Agp2pEnums.ProjectStatusEnum.FinancingApplicationSuccess <= p.status &&
                                    p.status != (int) Agp2pEnums.ProjectStatusEnum.FinancingFail);
                        //查询可用额度
                        if (projectAll.Any())
                        {
                            step = LoanApplyStep.ProjectApplyCompleted;
                            quota_use = loaner.quota - (int)projectAll.Sum(p => p.financing_amount);
                        }
                        else
                        {
                            step = LoanApplyStep.UnApplyProject;
                            quota_use = loaner.quota;
                        }
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
        public static string ApplyLoaner(short age, string nativePlace, string job, string workingCompany, string educationalBackground, int maritalStatus, string income)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            try
            {
                var loaner = context.li_loaners.FirstOrDefault(l => l.dt_users.id == userInfo.id);
                if (loaner != null)
                {
                    loaner.age = age;
                    loaner.native_place = nativePlace;
                    loaner.job = job;
                    loaner.working_at = "";
                    loaner.working_company = workingCompany;
                    loaner.educational_background = educationalBackground;
                    loaner.marital_status = (byte)maritalStatus;
                    loaner.income = income;
                    loaner.status = (int)Agp2pEnums.LoanerStatusEnum.Pending;
                    loaner.last_update_time = DateTime.Now;
                }
                else
                {
                    loaner = new li_loaners()
                    {
                        user_id = userInfo.id,
                        age = age,
                        native_place = nativePlace,
                        job = job,
                        working_at = "",
                        working_company = workingCompany,
                        educational_background = educationalBackground,
                        marital_status = (byte)maritalStatus,
                        income = income,
                        status = (int)Agp2pEnums.LoanerStatusEnum.Pending,
                        last_update_time = DateTime.Now
                    };
                    context.li_loaners.InsertOnSubmit(loaner);
                }
                context.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return "ok";
        }

        /// <summary>
        /// 申请借款
        /// </summary>
        /// <param name="loaner_id"></param>
        /// <param name="user_name"></param>
        /// <param name="loaner_content"></param>
        /// <param name="loan_usage"></param>
        /// <param name="source_of_repayment"></param>
        /// <param name="category_id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [WebMethod]
        public static string ApplyLoan(string loanerContent, string loanUsage, string sourceOfRepayment, decimal amount)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var project = context.li_projects.FirstOrDefault(p => p.li_risks.li_loaners.dt_users.id == userInfo.id &&
                                                              p.status < (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationChecking);
            if (project != null)
            {
                project.li_risks.loaner_content = loanerContent;
                project.li_risks.loan_usage = loanUsage;
                project.li_risks.source_of_repayment = sourceOfRepayment;
                project.financing_amount = amount;
                project.status = (int) Agp2pEnums.ProjectStatusEnum.FinancingApplicationUncommitted;

                context.SubmitChanges();
                return "ok";
            }

            try
            {
                var risk = new li_risks()
                {
                    loaner = userInfo.li_loaners.Single().id,
                    loaner_content = loanerContent,
                    loan_usage = loanUsage,
                    source_of_repayment = sourceOfRepayment,
                    last_update_time = DateTime.Now
                };
                project = new li_projects()
                {
                    li_risks = risk,
                    category_id = 61,
                    financing_amount = amount,
                    user_name = userInfo.user_name,
                    add_time = DateTime.Now,
                    status = (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationUncommitted,
                    repayment_type = (int)Agp2pEnums.ProjectRepaymentTypeEnum.DengEr,
                    no = "",
                    type = (int)Agp2pEnums.LoanTypeEnum.Personal
                };
                //项目编号
                var latestProject = context.li_projects.Where(p => p.category_id == project.category_id).OrderByDescending(p => p.id).FirstOrDefault();
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

            return "ok";
        }
    }
}
