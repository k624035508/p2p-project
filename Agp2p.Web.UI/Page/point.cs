using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Web;

namespace Agp2p.Web.UI.Page
{
    public partial class point : Web.UI.BasePage
    {
        /// <summary>
        /// 重写父类的虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            Init += Page_Init;
        }

        /// <summary>
        /// OnInit事件
        /// </summary>
        void Page_Init(object sender, EventArgs e)
        {
           
        }

        Agp2pDataContext context = new Agp2pDataContext();

        //检查今天是否签到
        protected bool IsTodaySign()
        {
            var userInfo = GetUserInfoByLinq();
            var todaySign = context.dt_user_sign_log.Where(s => s.user_id == userInfo.id && s.sign_time == DateTime.Today);
            return (todaySign == null);
        }

    }
}
