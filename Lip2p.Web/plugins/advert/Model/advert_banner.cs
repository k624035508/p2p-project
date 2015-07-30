using System;
namespace Lip2p.Web.Plugin.Advert.Model
{
    /// <summary>
    /// 广告Banner:实体类
    /// </summary>
    [Serializable]
    public partial class advert_banner
    {
        public advert_banner()
        { }
        #region Model
        private int _id;
        private int _aid = 0;
        private string _title;
        private DateTime _start_time;
        private DateTime _end_time;
        private string _file_path;
        private string _link_url;
        private string _content;
        private int _sort_id = 99;
        private int _is_lock = 0;
        private DateTime _add_time = DateTime.Now;
        /// <summary>
        /// 自增ID
        /// </summary>
        public int id
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 广告位ID
        /// </summary>
        public int aid
        {
            set { _aid = value; }
            get { return _aid; }
        }
        /// <summary>
        /// 广告名称
        /// </summary>
        public string title
        {
            set { _title = value; }
            get { return _title; }
        }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime start_time
        {
            set { _start_time = value; }
            get { return _start_time; }
        }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime end_time
        {
            set { _end_time = value; }
            get { return _end_time; }
        }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string file_path
        {
            set { _file_path = value; }
            get { return _file_path; }
        }
        /// <summary>
        /// 链接地址
        /// </summary>
        public string link_url
        {
            set { _link_url = value; }
            get { return _link_url; }
        }
        /// <summary>
        /// 广告内容
        /// </summary>
        public string content
        {
            set { _content = value; }
            get { return _content; }
        }
        /// <summary>
        /// 排序数字
        /// </summary>
        public int sort_id
        {
            set { _sort_id = value; }
            get { return _sort_id; }
        }
        /// <summary>
        /// 是否显示
        /// </summary>
        public int is_lock
        {
            set { _is_lock = value; }
            get { return _is_lock; }
        }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime add_time
        {
            set { _add_time = value; }
            get { return _add_time; }
        }
        #endregion Model

    }
}