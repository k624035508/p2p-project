using System;
using System.Web.Services;
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

        protected void Page_Load(object sender, EventArgs e)
        {

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
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return JsonConvert.SerializeObject("ok");
        }

        [WebMethod]
        public static string ApplyLoan(int loanId, string loaner_content, string loan_usage, string source_of_repayment, int category_id, int amount)
        {

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

            };

            return JsonConvert.SerializeObject("ok");
        }
    }
}
