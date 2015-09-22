using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using System.Web;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.UI
{
    public partial class BasePage : System.Web.UI.Page
    {
        /// <summary>
        /// OAuth平台列表
        /// </summary>
        /// <param name="top">显示条数</param>
        /// <param name="strwhere">查询条件</param>
        /// <returns>DataTable</returns>
        protected DataTable get_oauth_app_list(int top, string strwhere)
        {
            string _where = "is_lock=0";
            if (!string.IsNullOrEmpty(strwhere))
            {
                _where += " and " + strwhere;
            }
            return new BLL.user_oauth_app().GetList(top, "is_lock=0", "sort_id asc,id desc").Tables[0];
        }

        /// <summary>
        /// 返回用户头像图片地址
        /// </summary>
        /// <param name="user_name">用户名</param>
        /// <returns>String</returns>
        protected string get_user_avatar(string user_name)
        {
            BLL.users bll = new BLL.users();
            if (!bll.Exists(user_name))
            {
                return "";
            }
            return bll.GetModel(user_name).avatar;
        }

        /// <summary>
        /// 统计短信息数量
        /// </summary>
        /// <param name="strwhere">查询条件</param>
        /// <returns>Int</returns>
        protected int get_user_message_count(string strwhere)
        {
            return new BLL.user_message().GetCount(strwhere);
        }

        /// <summary>
        /// 短信息列表
        /// </summary>
        /// <param name="top">显示条数</param>
        /// <param name="strwhere">查询条件</param>
        /// <returns>DataTable</returns>
        protected DataTable get_user_message_list(int top, string strwhere)
        {
            return new BLL.user_message().GetList(top, strwhere, "sort_id asc,post_time desc").Tables[0];
        }

        /// <summary>
        /// 短信息分页列表
        /// </summary>
        /// <param name="page_size">页面大小</param>
        /// <param name="page_index">当前页码</param>
        /// <param name="strwhere">查询条件</param>
        /// <param name="totalcount">总记录数</param>
        /// <returns>DateTable</returns>
        protected DataTable get_user_message_list(int page_size, int page_index, string strwhere, out int totalcount)
        {
            return new BLL.user_message().GetList(page_size, page_index, strwhere, "is_read asc,post_time desc", out totalcount).Tables[0];
        }

        /// <summary>
        /// 积分明细分页列表
        /// </summary>
        /// <param name="page_size">页面大小</param>
        /// <param name="page_index">当前页码</param>
        /// <param name="strwhere">查询条件</param>
        /// <param name="totalcount">总记录数</param>
        /// <returns>DateTable</returns>
        protected DataTable get_user_point_list(int page_size, int page_index, string strwhere, out int totalcount)
        {
            return new BLL.user_point_log().GetList(page_size, page_index, strwhere, "add_time desc,id desc", out totalcount).Tables[0];
        }
        /// <summary>
        /// 余额明细分页列表
        /// </summary>
        /// <param name="page_size">页面大小</param>
        /// <param name="page_index">当前页码</param>
        /// <param name="strwhere">查询条件</param>
        /// <param name="totalcount">总记录数</param>
        /// <returns>DateTable</returns>
        protected DataTable get_user_amount_list(int page_size, int page_index, string strwhere, out int totalcount)
        {
            return new BLL.user_amount_log().GetList(page_size, page_index, strwhere, "add_time desc,id desc", out totalcount).Tables[0];
        }

        /// <summary>
        /// 用户邀请码列表
        /// </summary>
        /// <param name="top">显示条数</param>
        /// <param name="strwhere">查询条件</param>
        /// <returns></returns>
        protected DataTable get_user_invite_list(int top, string strwhere)
        {
            string _where = "type='" + DTEnums.CodeEnum.Register.ToString() + "'";
            if (!string.IsNullOrEmpty(strwhere))
            {
                _where += " and " + strwhere;
            }
            return new BLL.user_code().GetList(top, _where, "add_time desc,id desc").Tables[0];
        }
        /// <summary>
        /// 返回邀请码状态
        /// </summary>
        /// <param name="str_code">邀请码</param>
        /// <returns>bool</returns>
        protected bool get_invite_status(string str_code)
        {
            Model.user_code model = new BLL.user_code().GetModel(str_code);
            if (model != null)
            {
                return true;
            }
            return false;
        }

        protected long JsTimeToDotNetTime(long jsTime)
        {
            // http://stackoverflow.com/questions/7966559/how-to-convert-javascript-date-object-to-ticks
            return 0 < jsTime ? jsTime*10000 + 621355968000000000 : jsTime;
        }

        /// <summary>
        /// 获取用户绑定银行卡号
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        protected List<li_bank_accounts> get_bank_accounts(int user_id)
        {
            return new Agp2pDataContext().li_bank_accounts.Where(b => b.owner == user_id).ToList();
        }

        protected string get_bank_css(string bank_name) {
            if ("中国工商银行".IndexOf(bank_name) >= 0)
                return "icbc";
            else if ("中国农业银行".IndexOf(bank_name) >= 0)
                return "abc";
            else if (bank_name.IndexOf("中国银行") >= 0)
                return "boc";
            else if ("中国建设银行".IndexOf(bank_name) >= 0)
                return "ccb";
            else if ("中国光大银行".IndexOf(bank_name) >= 0)
                return "ceb";
            else if ("兴业银行".IndexOf(bank_name) >= 0)
                return "cib";
            else if ("招商银行".IndexOf(bank_name) >= 0)
                return "cmb";
            else if ("中国民生银行".IndexOf(bank_name) >= 0)
                return "cmbc";
            else if ("广发银行".IndexOf(bank_name) >= 0)
                return "gdb";
            else if ("华厦银行".IndexOf(bank_name) >= 0)
                return "huaxia";
            else if ("交通银行".IndexOf(bank_name) >= 0)
                return "jiaotong";
            else if ("中国邮政储蓄银行".IndexOf(bank_name) >= 0)
                return "post";
            else if ("浦发银行".IndexOf(bank_name) >= 0)
                return "pufa";
            else if ("中信银行".IndexOf(bank_name) >= 0)
                return "zhongxin";
            else if ("平安银行".IndexOf(bank_name) >= 0)
                return "pingan";

            return "";
        }

        private static readonly int[] LotteryType =
        {
            (int) Agp2pEnums.ActivityTransactionActivityTypeEnum.Trial,
        };

        protected int QueryUnusedLottery(int userId)
        {
            return
                new Agp2pDataContext().li_activity_transactions.Count(
                    t => t.user_id == userId && t.status == (int) Agp2pEnums.ActivityTransactionStatusEnum.Acting
                         && LotteryType.Contains(t.activity_type));
        }

        protected static int GetSessionIDHash()
        {
            return HttpContext.Current.Session.SessionID.GetHashCode();
        }

    }
}
