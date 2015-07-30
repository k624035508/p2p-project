using System;
using System.Collections.Generic;
using System.Text;

namespace Lip2p.Model
{  
    /// <summary>
    /// 部门信息表:实体类
    /// </summary>
    [Serializable]
    public partial class department
    {
        public department()
        { }
        #region Model
        private int _id;
        private string _department_name = "";
        private int _sort_id = 99;
        private int _is_lock = 0;
        private string _remark = "";
        private int _parent_id = 0;
        private string _class_list = "";
        private int _class_layer = 1;
        private int _is_sys = 0;
        private int _is_default = 0;
        /// <summary>
        /// 自增ID
        /// </summary>
        public int id
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string department_name
        {
            set { _department_name = value; }
            get { return _department_name; }
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
        /// 是否隐藏0显示1隐藏
        /// </summary>
        public int is_lock
        {
            set { _is_lock = value; }
            get { return _is_lock; }
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
        /// 所属父导航ID
        /// </summary>
        public int parent_id
        {
            set { _parent_id = value; }
            get { return _parent_id; }
        }
        /// <summary>
        /// 菜单ID列表(逗号分隔开)
        /// </summary>
        public string class_list
        {
            set { _class_list = value; }
            get { return _class_list; }
        }
        /// <summary>
        /// 导航深度
        /// </summary>
        public int class_layer
        {
            set { _class_layer = value; }
            get { return _class_layer; }
        }
        /// <summary>
        /// 系统默认
        /// </summary>
        public int is_sys
        {
            set { _is_sys = value; }
            get { return _is_sys; }
        }
        /// <summary>
        /// 是否是部门成员1是没有0是有
        /// </summary>
        public int is_default
        {
            set { _is_default = value; }
            get { return _is_default; }
        }
        #endregion Model

    }
}
