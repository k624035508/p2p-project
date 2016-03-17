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
        protected string step = "0";
        protected int user_id = 0;
        protected string user_name = "";
        protected int loaner_id = 0;
        protected int pending_project_id = 0;
        protected int quota_use = 0;

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

        void Page_Init(object sender, EventArgs e)
        {
            var user = GetUserInfoByLinq();
            if (user == null)
                step = "1";//显示登录步骤
            else
            {
                user_id = user.id;
                user_name = user.user_name;
                //查看是否已为借款人
                Agp2pDataContext context = new Agp2pDataContext();
                var loaner = context.li_loaners.SingleOrDefault(l => l.user_id == user.id);
                if (loaner == null)
                {
                    step = "2"; //显示申请借款人步骤
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

                    //查看借款人状态
                    switch (loaner.status)
                    {
                        case (int)Agp2pEnums.LoanerStatusEnum.Pending:
                            step = "21";//显示申请借款人审核中
                            return;
                        case (int)Agp2pEnums.LoanerStatusEnum.PendingFail:
                            step = "22";//显示申请借款人失败，重新申请
                            return;
                        case (int)Agp2pEnums.LoanerStatusEnum.Disable:
                            step = "23";//显示禁止再申请借款人
                            return;
                    }

                    //查看是否有在审批中的借款申请
                    var project = context.li_projects.Where(p => p.user_name == user.user_name && p.status == (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationUncommitted).ToList();
                    if (project.Any())
                    {
                        step = "4"; //显示正在审批步骤
                        var p = project.FirstOrDefault();
                        if (p != null)
                        {
                            project_category_id = p.category_id;
                            project_amount = (int) p.financing_amount;
                            project_loan_usage = p.li_risks.loan_usage;
                            project_source_of_repayment = p.li_risks.source_of_repayment;
                            project_loaner_content = p.li_risks.loaner_content;
                        }
                    }
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
                        quota_use = projectAll.Any()
                            ? loaner.quota - (int) projectAll.Sum(p => p.financing_amount)
                            : loaner.quota;
                        //借款人额度为0表示不限额，或者大于0则可继续申请借款
                        step = loaner.quota == 0 || quota_use > 0 ? "3" : "5";
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
                Agp2pDataContext context = new Agp2pDataContext();
                var loaner = context.li_loaners.FirstOrDefault(l => l.dt_users.id == userId);
                if (loaner != null)
                {
                    loaner.age = age;
                    loaner.native_place = native_place;
                    loaner.job = job;
                    loaner.working_at = working_at;
                    loaner.working_company = working_company;
                    loaner.educational_background = educational_background;
                    loaner.marital_status = (byte)marital_status;
                    loaner.income = income;
                    loaner.status = (int)Agp2pEnums.LoanerStatusEnum.Pending;
                    loaner.last_update_time = DateTime.Now;
                }
                else
                {
                    loaner = new li_loaners()
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

            return JsonConvert.SerializeObject("ok");
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
        public static string ApplyLoan(int loaner_id, string user_name, string loaner_content, string loan_usage, string source_of_repayment, int category_id, int amount)
        {
            try
            {
                Agp2pDataContext context = new Agp2pDataContext();
                var risk = new li_risks()
                {
                    loaner = loaner_id,
                    loaner_content = loaner_content,
                    loan_usage = loan_usage,
                    source_of_repayment = source_of_repayment,
                    last_update_time = DateTime.Now
                };
                var project = new li_projects()
                {
                    li_risks = risk,
                    category_id = category_id,
                    financing_amount = amount,
                    user_name = user_name,
                    add_time = DateTime.Now,
                    status = (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationUncommitted,
                    repayment_type = (int)Agp2pEnums.ProjectRepaymentTypeEnum.DengEr,
                    no = "",
                    type = (int)Agp2pEnums.LoanTypeEnum.Personal
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
