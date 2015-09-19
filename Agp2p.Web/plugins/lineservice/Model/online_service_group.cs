using System;
namespace Lip2p.Web.Plugin.LineService.Model
{
    /// <summary>
    /// online_service_group:实体类
    /// </summary>
    [Serializable]
    public partial class online_service_group
    {
        public online_service_group()
        { }
        #region Model
        private int _id;
        private string _title;
        private int _default_view = 0;
        private int _sort_id = 99;
        private int _is_lock = 0;
        /// <summary>
        /// 自增ID
        /// </summary>
        public int id
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 标题
        /// </summary>
        public string title
        {
            set { _title = value; }
            get { return _title; }
        }
        /// <summary>
        /// 默认展开
        /// </summary>
        public int default_view
        {
            set { _default_view = value; }
            get { return _default_view; }
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
        /// 是否不显示
        /// </summary>
        public int is_lock
        {
            set { _is_lock = value; }
            get { return _is_lock; }
        }
        #endregion Model

    }
}