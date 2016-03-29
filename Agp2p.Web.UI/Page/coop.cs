using System;
using System.Web.Services;
using System.Linq;
using System.Text.RegularExpressions;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;

namespace Agp2p.Web.UI.Page
{
    /// <summary>
    /// 融资合作
    /// </summary>
    public partial class coop : Web.UI.BasePage
    {
        [WebMethod]
        public static string Apply(string userName, string mobile, string type)
        {
            try
            {
                Agp2pDataContext context = new Agp2pDataContext();
                if (context.dt_users.SingleOrDefault(u => u.user_name == userName && u.mobile == mobile) == null)
                {
                    if (!new Regex(@"^[\u4e00-\u9fa5·]{2,15}$").IsMatch(userName))
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            status = 2,
                            msg = "请输入正确的中文姓名!"
                        });
                    }

                    if (!new Regex(@"^\d{11}$").IsMatch(mobile))
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            status = 2,
                            msg = "请输入正确的手机号码!"
                        });
                    }
                    //申请人ip，30分钟内只允许申请一次
                    string userip = DTRequest.GetIP();
                    var userA = context.dt_users.OrderByDescending(u => u.reg_time).FirstOrDefault(u => u.reg_ip == userip);
                    if (userA != null && DateTime.Now.Subtract((DateTime)userA.reg_time).TotalMinutes <= 30)
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            status = 2,
                            msg = "30分钟内只允许申请一次!"
                        });
                    }

                    var user = new dt_users();
                    user.user_name = userName;
                    user.mobile = mobile;
                    var group = context.dt_user_groups.SingleOrDefault(u => u.title.Equals("融资合作组"));
                    user.group_id = @group?.id ?? 1;
                    user.salt = Utils.GetCheckCode(6);
                    user.password = DESEncrypt.Encrypt("a123456", user.salt);
                    user.status = 1;
                    user.area = type;
                    user.reg_ip = userip;
                    user.reg_time = DateTime.Now;
                    context.dt_users.InsertOnSubmit(user);
                    context.SubmitChanges();
                }
                else
                {
                    return JsonConvert.SerializeObject(new
                    {
                        status = 2,
                        msg = "申请人信息已存在!"
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return JsonConvert.SerializeObject(new
            {
                status = 1,
                msg = "ok"
            });
        }
    }
}
