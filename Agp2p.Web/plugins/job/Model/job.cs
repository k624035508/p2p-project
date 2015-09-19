using System;
namespace Lip2p.Web.Plugin.Job.Model
{
	/// <summary>
	/// Job:实体类(属性说明自动提取数据库字段的描述信息)
	/// </summary>
	[Serializable]
	public partial class job
	{
        public job()
		{}
		#region Model
		private int _id;
		private string _title;
		private string _depart;
		private int _number;
		private string _sex;
		private string _experience;
		private string _education;
		private string _age;
		private string _profession;
		private string _work_area;
        private string _start_time;
        private string _end_time;
		private string _content;
		private string _contactname;
		private string _phone;
		private string _fax;
		private string _email;
		private string _address;
		private int? _click;
		private int? _sort_id;
		private int? _is_lock=0;
		private DateTime? _add_time;
		/// <summary>
		/// 
		/// </summary>
		public int id
		{
			set{ _id=value;}
			get{return _id;}
		}
		/// <summary>
		/// 中文招聘职位
		/// </summary>
		public string title
		{
			set{ _title=value;}
			get{return _title;}
		}
		/// <summary>
		/// 中文招聘部门
		/// </summary>
		public string depart
		{
			set{ _depart=value;}
			get{return _depart;}
		}
		/// <summary>
		/// 招聘人数
		/// </summary>
		public int number
		{
			set{ _number=value;}
			get{return _number;}
		}
		/// <summary>
		/// 中文性别要求
		/// </summary>
		public string sex
		{
			set{ _sex=value;}
			get{return _sex;}
		}
		/// <summary>
		/// 中文工作经验
		/// </summary>
		public string experience
		{
			set{ _experience=value;}
			get{return _experience;}
		}
		/// <summary>
		/// 中文学历要求
		/// </summary>
		public string education
		{
			set{ _education=value;}
			get{return _education;}
		}
		/// <summary>
		/// 中文年龄要求
		/// </summary>
		public string age
		{
			set{ _age=value;}
			get{return _age;}
		}
		/// <summary>
		/// 中文专业名
		/// </summary>
		public string profession
		{
			set{ _profession=value;}
			get{return _profession;}
		}
		/// <summary>
		/// 中文工作地区
		/// </summary>
		public string work_area
		{
			set{ _work_area=value;}
			get{return _work_area;}
		}
		/// <summary>
		/// 开始日期
		/// </summary>
        public string start_time
		{
			set{ _start_time=value;}
			get{return _start_time;}
		}
		/// <summary>
		/// 结束日期
		/// </summary>
        public string end_time
		{
			set{ _end_time=value;}
			get{return _end_time;}
		}
		/// <summary>
		/// 中文要求与待遇
		/// </summary>
		public string Content
		{
			set{ _content=value;}
			get{return _content;}
		}
		/// <summary>
		/// 联系人姓名
		/// </summary>
		public string ContactName
		{
			set{ _contactname=value;}
			get{return _contactname;}
		}
		/// <summary>
		/// 联系电话
		/// </summary>
		public string Phone
		{
			set{ _phone=value;}
			get{return _phone;}
		}
		/// <summary>
		/// 单位传真
		/// </summary>
		public string Fax
		{
			set{ _fax=value;}
			get{return _fax;}
		}
		/// <summary>
		/// 电子邮箱
		/// </summary>
		public string Email
		{
			set{ _email=value;}
			get{return _email;}
		}
		/// <summary>
		/// 中文通信地址
		/// </summary>
		public string Address
		{
			set{ _address=value;}
			get{return _address;}
		}
		/// <summary>
		/// 
		/// </summary>
		public int? click
		{
			set{ _click=value;}
			get{return _click;}
		}
		/// <summary>
		/// 排序
		/// </summary>
		public int? sort_id
		{
			set{ _sort_id=value;}
			get{return _sort_id;}
		}
		/// <summary>
		/// 
		/// </summary>
		public int? is_lock
		{
			set{ _is_lock=value;}
			get{return _is_lock;}
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

