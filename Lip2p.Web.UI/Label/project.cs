using System.Data;
using System.Linq;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using System;
using Lip2p.Core;

namespace Lip2p.Web.UI
{
    public partial class BasePage : System.Web.UI.Page
    {
        protected DataTable get_project_list(int top, int category_id, int profit_rate_index, int repayment_index, int amount_index)
        {
            int total = 0;
            return get_project_list(top, 1, out total, category_id, profit_rate_index, repayment_index, amount_index);
        }

        protected static DataTable get_project_list(int pageSize, int pageIndex, out int total, int category_id, int profit_rate_index, int repayment_index, int amount_index)
        {
            total = 0;
            var context = new Lip2pDataContext();
            //查出所以项目类别
            //var categoryList = get_category_list(channel_name, 0);
            var query =
                context.li_projects.Where(p => p.status >= (int) Lip2pEnums.ProjectStatusEnum.FinancingAtTime)
                    .Where(p => p.tag == null || p.tag != (int) Lip2pEnums.ProjectTagEnum.Trial && p.tag != (int) Lip2pEnums.ProjectTagEnum.DailyProject);
            if (category_id > 0)
                query = query.Where(p => p.category_id == category_id);
            //项目筛选暂写死逻辑在此
            if (profit_rate_index > 0)//年化利率条件
            {
                switch (profit_rate_index)
                {
                    case 1:
                        query = query.Where(p => p.profit_rate_year == 15);
                        break;
                    case 2:
                        query = query.Where(p => p.profit_rate_year == 16);
                        break;
                    case 3:
                        query = query.Where(p => p.profit_rate_year == 17);
                        break;
                    case 4:
                        query = query.Where(p => p.profit_rate_year == 18);
                        break;
                }
            }
            if (repayment_index > 0)//借款期限条件
            {
                switch (repayment_index)
                {
                    case 1:
                        query = query.Where(p => p.repayment_term_span_count == 1);
                        break;
                    case 2:
                        query = query.Where(p => p.repayment_term_span_count == 2);
                        break;
                    case 3:
                        query = query.Where(p => p.repayment_term_span_count == 3);
                        break;
                }
            }
            if (amount_index > 0)//金额条件
            {
                switch (amount_index)
                {
                    case 1:
                        query = query.Where(p => p.financing_amount <= 100000);
                        break;
                    case 2:
                        query = query.Where(p => p.financing_amount > 100000 && p.financing_amount <= 250000);
                        break;
                    case 3:
                        query = query.Where(p => p.financing_amount > 250000 && p.financing_amount <= 500000);
                        break;
                    case 4:
                        query = query.Where(p => p.financing_amount > 500000);
                        break;
                }
            }
            total = query.Count();            

            //将对象列表转换成DataTable
            var queryToNewObj = query.OrderBy(q => q.status)
                .ThenByDescending(q => q.sort_id)
                .ThenByDescending(q => q.add_time)
                .ThenByDescending(q => q.id)
                .Skip(pageSize * (pageIndex - 1)).Take(pageSize).AsEnumerable().Select(p =>
                {
                    var pr = GetProjectInvestmentProgress(context, p.id);
                    return new
                    {
                        id = p.id,
                        img_url = GetProjectImageUrl(p.img_url, p.category_id),
                        no = p.no,
                        title = p.title,
                        status = p.status,
                        sort_id = p.sort_id,
                        repayment_type = p.repayment_type,
                        repayment_term = Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.ProjectRepaymentTermSpanEnum)p.repayment_term_span),
                        repayment_number = p.repayment_term_span_count,
                        profit_rate_year = p.profit_rate_year,
                        category_id = p.category_id,
                        amount = p.financing_amount,
                        add_time = p.publish_time ?? p.add_time,
                        publish_time = p.publish_time,
                        tag = p.tag,
                        //category_img = get_category_icon_by_categoryid(categoryList, p.category_id),//类别图标路径
                        project_repayment = get_project_repayment_str(p.repayment_term_span),//项目还款期限单位
                        project_amount_str = string.Format("{0:0.0}", p.financing_amount / 10000),//项目金额字符
                        project_investment_progress = pr.GetInvestmentProgress(),//项目进度
                        project_investment_balance = pr.GetInvestmentBalance(),//项目投资剩余金额
                        project_investment_count = context.GetInvestedUserCount(p.id)//项目投资人数
                    };
                });

            var dt = queryToNewObj.ToDataTable(p => new object[] { queryToNewObj });
            return dt ?? new DataTable();
        }

        protected static string GetProjectImageUrl(string url, int category_id)
        {
            if (string.IsNullOrEmpty(url))
            {
                return category_id == 33 ? "/templates/lip2p/images/img_noting_house.png" :
                    "/templates/lip2p/images/img_noting_car.png";
            }
            else
                return url;
        }

        /// <summary>
        /// 获取项目进度参数
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        protected static ProjectInvestmentProgress GetProjectInvestmentProgress(Lip2pDataContext context, int projectId)
        {
            return context.GetInvestmentProgress(projectId, (total, projectAmount) => new ProjectInvestmentProgress{ total = total, projectAmount = projectAmount });
        }

        /// <summary>
        /// 获取项目还款期限字符串
        /// </summary>
        /// <param name="repaymentNumber"></param>
        /// <param name="repaymentTerm"></param>
        /// <returns></returns>
        protected static string get_project_repayment_str(int repaymentTerm)
        {
            var repaymentTermEnum = (Lip2pEnums.ProjectRepaymentTermSpanEnum)repaymentTerm;
            string repaymentstr = repaymentTermEnum == Lip2pEnums.ProjectRepaymentTermSpanEnum.Month ? "个" : "";
            return repaymentstr + Utils.GetLip2pEnumDes(repaymentTermEnum);
        }

        public class ProjectInvestmentProgress
        {
            public decimal total;
            public decimal projectAmount;

            public string GetInvestmentProgress()
            {
                return (total / projectAmount).ToString("P1").Split('%')[0];
            }

            public string GetInvestmentBalance()
            {
                return (projectAmount - total).ToString("N0") + "元";
            }
        }

        /// <summary>
        /// 获取年化利率筛选枚举
        /// </summary>
        /// <returns></returns>
        protected DataTable get_project_rate_enum()
        {
            DataTable dt = new DataTable();            
            dt.Columns.Add("key", Type.GetType("System.String"));
            dt.Columns.Add("value", Type.GetType("System.String"));
            dt.Columns.Add("text", Type.GetType("System.String"));         

            foreach (Lip2pEnums.InterestRateTypeEnum e in Enum.GetValues(typeof(Lip2pEnums.InterestRateTypeEnum)))
            {
                DataRow dr = dt.NewRow();
                dr["key"] = e.ToString();
                dr["value"] = (int)e;
                dr["text"] = Utils.GetLip2pEnumDes(e);
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 获取还款期限筛选枚举
        /// </summary>
        /// <returns></returns>
        protected DataTable get_project_term_enum()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("key", Type.GetType("System.String"));
            dt.Columns.Add("value", Type.GetType("System.String"));
            dt.Columns.Add("text", Type.GetType("System.String"));

            foreach (Lip2pEnums.RepaymentTermEnum e in Enum.GetValues(typeof(Lip2pEnums.RepaymentTermEnum)))
            {
                DataRow dr = dt.NewRow();
                dr["key"] = e.ToString();
                dr["value"] = (int)e;
                dr["text"] = Utils.GetLip2pEnumDes(e);
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 获取金额筛选枚举
        /// </summary>
        /// <returns></returns>
        protected DataTable get_project_amount_enum()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("key", Type.GetType("System.String"));
            dt.Columns.Add("value", Type.GetType("System.String"));
            dt.Columns.Add("text", Type.GetType("System.String"));

            foreach (Lip2pEnums.AmountTypeEnum e in Enum.GetValues(typeof(Lip2pEnums.AmountTypeEnum)))
            {
                DataRow dr = dt.NewRow();
                dr["key"] = e.ToString();
                dr["value"] = (int)e;
                dr["text"] = Utils.GetLip2pEnumDes(e);
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}
