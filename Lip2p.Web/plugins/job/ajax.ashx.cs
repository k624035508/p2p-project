using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.SessionState;
using Lip2p.Web.UI;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.Job
{
    /// <summary>
    /// 管理后台AJAX处理页
    /// </summary>
    public class ajax : IHttpHandler, IRequiresSessionState
    {
        int job_id;
        public void ProcessRequest(HttpContext context)
        {
            //取得处事类型
            string action = DTRequest.GetQueryString("action");
            job_id = DTRequest.GetQueryInt("job_id",0);

            switch (action)
            {
                case "add": //求职应聘
                    apply_add(context);
                    break;
            }

        }

        #region 发布求职================================
        private void apply_add(HttpContext context)
        {
            if (job_id == 0)
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"对不起，您应聘的职位不存在或已被管理员删除！\"}");
                return;
            }

            StringBuilder strTxt = new StringBuilder();
            BLL.job_apply bll = new BLL.job_apply();
            Model.job_apply model = new Model.job_apply();

            string RealName = DTRequest.GetFormString("txtRealName");
            string Sex = DTRequest.GetFormString("txtSex");
            string Birth = DTRequest.GetFormString("txtBirth");
            string Marital = DTRequest.GetFormString("txtMarital");
            string Origin = DTRequest.GetFormString("txtOrigin");
            string Hobby = DTRequest.GetFormString("txtHobby");
            string School = DTRequest.GetFormString("txtSchool");
            string Degree = DTRequest.GetFormString("Degree");
            string Profess = DTRequest.GetFormString("txtProfess");
            string IDNum = DTRequest.GetFormString("txtIDNum");
            string Address = DTRequest.GetFormString("txtAddress");
            string Tel = DTRequest.GetFormString("txtTel");
            string Email = DTRequest.GetFormString("txtEmail");
            string Salary = DTRequest.GetFormString("txtSalary");
            string EducationExperience = DTRequest.GetFormString("txtEducationExperience");
            string WorkExperience = DTRequest.GetFormString("txtWorkExperience");
            string SelfContent = Utils.ToHtml(DTRequest.GetFormString("txtSelfContent"));

            if (string.IsNullOrEmpty(Degree))
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"对不起，学历不能为空！\"}");
                return;
            }
            if (string.IsNullOrEmpty(RealName))
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"对不起，姓名不能为空！\"}");
                return;
            }
            if (string.IsNullOrEmpty(Sex))
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"对不起，性别姓名不能为空！\"}");
                return;
            }
            if (string.IsNullOrEmpty(Tel))
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"对不起，电话是必须要填的！\"}");
                return;
            }

            // 检查其他项
            if (string.IsNullOrEmpty(Birth)) Birth = "";
            if (string.IsNullOrEmpty(Marital)) Marital = "";
            if (string.IsNullOrEmpty(Origin)) Origin = "";
            if (string.IsNullOrEmpty(Hobby)) Hobby = "";
            if (string.IsNullOrEmpty(School)) School = "";
            if (string.IsNullOrEmpty(Degree)) Degree = "";
            if (string.IsNullOrEmpty(Profess)) Profess = "";
            if (string.IsNullOrEmpty(IDNum)) IDNum = "";
            if (string.IsNullOrEmpty(Address)) Address = "";
            if (string.IsNullOrEmpty(Email)) Email = "";
            if (string.IsNullOrEmpty(Salary)) Salary = "";
            if (string.IsNullOrEmpty(EducationExperience)) EducationExperience = "";
            if (string.IsNullOrEmpty(WorkExperience)) WorkExperience = "";
            if (string.IsNullOrEmpty(SelfContent)) SelfContent = "";

            model.job_id = job_id;
            model.RealName = RealName;
            model.Sex = Sex;
            model.Birth = Birth;
            model.Marital = Marital;
            model.Origin = Origin;
            model.Hobby = Hobby;
            model.School = School;
            model.Degree = Degree;
            model.Profess = Profess;
            model.IDNum = IDNum;
            model.Address = Address;
            model.Tel = Tel;
            model.Email = Email;
            model.Salary = Salary;
            model.EducationExperience = EducationExperience;
            model.WorkExperience = WorkExperience;
            model.SelfContent = SelfContent;
            model.is_see = 0;
            model.sort_id = 1;
            model.add_time = DateTime.Now;

            if (bll.Add(model))
            {
                context.Response.Write("{\"status\": 1, \"msg\": \"恭喜您，简历提交成功啦！我们会尽快和您取得联系。\"}");
                return;
            }
            context.Response.Write("{\"status\": 0, \"msg\": \"对不起，保存过程中发生错误！\"}");
            return;
        }
        #endregion

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}