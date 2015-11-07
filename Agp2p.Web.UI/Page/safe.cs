using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.UI.Page
{
    public class safe : usercenter
    {
        private Agp2pDataContext context;
        private Agp2pDataContext DBContext { get { return context ?? (context = new Agp2pDataContext()); } }

        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();

            var nvc = Utils.ParceQueryString();

            if (nvc["safe_act"] == "verify_email")
            {
                var codeFromEmail = nvc["code"];
                var cachedCode = (string)SessionHelper.Get("verifying_email_code");
                var sendVerifyMailAt = (DateTime?)SessionHelper.Get("last_send_verifying_mail_at");
                if (sendVerifyMailAt != null && SessionHelper.GetSessionTimeout() < DateTime.Now.Subtract(sendVerifyMailAt.Value).TotalMinutes)
                {
                    ShowJsAlert("邮箱验证码已失效");
                }
                else if (sendVerifyMailAt != null && !string.IsNullOrWhiteSpace(cachedCode) && codeFromEmail == cachedCode)
                {
                    var dtUsers = DBContext.dt_users.Single(u => u.id == userModel.id);
                    dtUsers.email = userModel.email = SessionHelper.Get<string>("verifying_email");
                    DBContext.SubmitChanges();
                    SessionHelper.Remove("verifying_email");
                    SessionHelper.Remove("last_send_verifying_mail_at");
                    SessionHelper.Remove("verifying_email_code");
                    ShowJsAlert("邮箱设置成功");
                }
            }
            else if (Request.HttpMethod == "POST" && Request.Form.AllKeys.Contains("truename")) // 现在这里的逻辑没有执行
            {
                if (!string.IsNullOrWhiteSpace(userModel.real_name))
                {
                    ShowJsAlert("实名认证后不能再修改，实在需要修改的话请联系客服人员");
                    return;
                }
                var truename = Request.Form["truename"];
                var idcard = Request.Form["idcard"];
                // 判断用户输入
                if (!new Regex(@"^[\u4e00-\u9fa5]{2,5}$").IsMatch(truename))
                {
                    ShowJsAlert("请输入正确的中文名称");
                    return;
                }
                if (!new Regex(@"^(\d{6})(\d{4})(\d{2})(\d{2})(\d{3})([0-9]|X)$", RegexOptions.IgnoreCase).IsMatch(idcard))
                {
                    ShowJsAlert("请输入正确的 18 位中华人民共和国公民身份号码");
                    return;
                }
                // 判断身份证是否重复
                var count = DBContext.dt_users.Count(u => u.id != userModel.id && u.id_card_number == idcard);
                if (count != 0)
                {
                    ShowJsAlert("身份证号已经存在");
                    return;
                }

                var user = DBContext.dt_users.Single(u => u.id == userModel.id);
                userModel.real_name = user.real_name = truename;
                userModel.id_card_number = user.id_card_number = idcard;
                LoadAlbum(user, Agp2pEnums.AlbumTypeEnum.IdCard);
                DBContext.SubmitChanges();
            }
        }

        protected List<li_albums> query_idcard_album()
        {
            return DBContext.li_albums.Where(a => a.the_user == userModel.id).ToList();
        } 

        private void LoadAlbum(dt_users model, Agp2pEnums.AlbumTypeEnum type)
        {
            string[] albumArr = Request.Form.GetValues("hid_photo_name");
            string[] remarkArr = Request.Form.GetValues("hid_photo_remark");
            DBContext.li_albums.DeleteAllOnSubmit(DBContext.li_albums.Where(a => a.the_user == model.id && a.type == (int)type));
            if (albumArr != null && remarkArr != null)
            {
                var preAdd = albumArr.Zip(remarkArr, (album, remark) =>
                {
                    var albumSplit = album.Split('|');
                    return new li_albums
                    {
                        original_path = albumSplit[1],
                        thumb_path = albumSplit[2],
                        remark = remark,
                        add_time = DateTime.Now,
                        dt_users = model,
                        type = (byte)type
                    };
                });
                DBContext.li_albums.InsertAllOnSubmit(preAdd);
            }
        }
    }
}
