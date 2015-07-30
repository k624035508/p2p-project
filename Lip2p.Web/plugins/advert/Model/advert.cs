using System;
namespace Lip2p.Web.Plugin.Advert.Model
{
    /// <summary>
    /// 广告位:实体类
    /// </summary>
    [Serializable]
    public partial class advert
    {
        public advert()
        { }
        #region Model
        private int _id;
        private string _title;
        private int _type;
        private decimal _price = 0M;
        private string _remark;
        private int _view_num = 0;
        private int _view_width = 0;
        private int _view_height = 0;
        private string _target;
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
        /// 广告位名称
        /// </summary>
        public string title
        {
            set { _title = value; }
            get { return _title; }
        }
        /// <summary>
        /// 广告位类型
        /// </summary>
        public int type
        {
            set { _type = value; }
            get { return _type; }
        }
        /// <summary>
        /// 广告位价格
        /// </summary>
        public decimal price
        {
            set { _price = value; }
            get { return _price; }
        }
        /// <summary>
        /// 备注说明
        /// </summary>
        public string remark
        {
            set { _remark = value; }
            get { return _remark; }
        }
        /// <summary>
        /// 显示广告数
        /// </summary>
        public int view_num
        {
            set { _view_num = value; }
            get { return _view_num; }
        }
        /// <summary>
        /// 广告位宽度
        /// </summary>
        public int view_width
        {
            set { _view_width = value; }
            get { return _view_width; }
        }
        /// <summary>
        /// 广告位高度
        /// </summary>
        public int view_height
        {
            set { _view_height = value; }
            get { return _view_height; }
        }
        /// <summary>
        /// 链接目标 新窗口,原窗口
        /// </summary>
        public string target
        {
            set { _target = value; }
            get { return _target; }
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