using System;
namespace Lip2p.Web.Plugin.Job.Model
{
	/// <summary>
	/// job_apply:实体类(属性说明自动提取数据库字段的描述信息)
	/// </summary>
	[Serializable]
	public partial class job_apply
	{
        public job_apply()
		{}
		#region Model
		private int _id;
		private int _job_id;
		private string _realname;
		private string _sex;
		private string _birth;
		private string _marital;
		private string _origin;
		private string _hobby;
		private string _school;
        private string _degree;
		private string _profess;
		private string _idnum;
		private string _address;
		private string _tel;
		private string _email;
		private string _salary;
        private string _educationexperience;
        private string _workexperience;
		private string _selfcontent;
        private int _is_see;
        private int _sort_id;
		private DateTime? _add_time;
		/// <summary>
		/// 
		/// </summary>
		public int ID
		{
			set{ _id=value;}
			get{return _id;}
		}
		/// <summary>
		/// 
		/// </summary>
		public int job_id
		{
			set{ _job_id=value;}
			get{return _job_id;}
		}
		/// <summary>
		/// 真实姓名
		/// </summary>
		public string RealName
		{
			set{ _realname=value;}
			get{return _realname;}
		}
		/// <summary>
		/// 性别
		/// </summary>
		public string Sex
		{
			set{ _sex=value;}
			get{return _sex;}
		}
		/// <summary>
		/// 出生日期
		/// </summary>
		public string Birth
		{
			set{ _birth=value;}
			get{return _birth;}
		}
		/// <summary>
		/// 婚姻状况
		/// </summary>
		public string Marital
		{
			set{ _marital=value;}
			get{return _marital;}
		}
		/// <summary>
		/// 籍贯
		/// </summary>
		public string Origin
		{
			set{ _origin=value;}
			get{return _origin;}
		}
		/// <summary>
		/// 爱好
		/// </summary>
		public string Hobby
		{
			set{ _hobby=value;}
			get{return _hobby;}
		}
		/// <summary>
		/// 毕业院校
		/// </summary>
		public string School
		{
			set{ _school=value;}
			get{return _school;}
		}
        /// <summary>
		/// 学历
		/// </summary>
        public string Degree
		{
            set { _degree = value; }
            get { return _degree; }
		}
		/// <summary>
		/// 所学专业
		/// </summary>
		public string Profess
		{
			set{ _profess=value;}
			get{return _profess;}
		}
		/// <summary>
		/// 身份证号
		/// </summary>
		public string IDNum
		{
			set{ _idnum=value;}
			get{return _idnum;}
		}
		/// <summary>
		/// 联系地址
		/// </summary>
		public string Address
		{
			set{ _address=value;}
			get{return _address;}
		}
		/// <summary>
		/// 联系电话
		/// </summary>
		public string Tel
		{
			set{ _tel=value;}
			get{return _tel;}
		}
		/// <summary>
		/// 联系邮箱
		/// </summary>
		public string Email
		{
			set{ _email=value;}
			get{return _email;}
		}
		/// <summary>
		/// 期望薪资
		/// </summary>
		public string Salary
		{
			set{ _salary=value;}
			get{return _salary;}
		}
        /// <summary>
        /// 教育经历
        /// </summary>
        public string EducationExperience
        {
            set { _educationexperience = value; }
            get { return _educationexperience; }
        }
        /// <summary>
        /// 工作经历
        /// </summary>
        public string WorkExperience
        {
            set { _workexperience = value; }
            get { return _workexperience; }
        }
		/// <summary>
		/// 自我评价
		/// </summary>
		public string SelfContent
		{
			set{ _selfcontent=value;}
			get{return _selfcontent;}
		}

        /// <summary>
        /// 是否查看
        /// </summary>
        public int is_see
        {
            set { _is_see = value; }
            get { return _is_see; }
        }
        /// <summary>
        /// 排序
        /// </summary>
        public int sort_id
        {
            set { _sort_id = value; }
            get { return _sort_id; }
        }
		/// <summary>
		/// 
		/// </summary>
		public DateTime? add_time
		{
			set{ _add_time=value;}
			get{return _add_time;}
		}
		#endregion Model

	}
}

