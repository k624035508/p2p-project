using System;
using System.Data;
using System.Collections.Generic;
using Agp2p.Common;

namespace Agp2p.BLL
{
    /// <summary>
    /// 部门信息表
    /// </summary>
    public partial class department
    {
        private readonly Model.siteconfig siteConfig = new BLL.siteconfig().loadConfig(); //获得站点配置信息
        private readonly DAL.department dal;
        public department()
        {
            dal = new DAL.department(siteConfig.sysdatabaseprefix);
        }

        #region  Method
        /// <summary>
        /// 是否存在该记录
        /// </summary>
        public bool Exists(int id)
        {
            return dal.Exists(id);
        }

        /// <summary>
        /// 查询名称是否存在
        /// </summary>
        public bool Exists(string name)
        {
            return dal.Exists(name);
        }

        /// <summary>
        /// 返回部门名称
        /// </summary>
        public string GetDepartmentName(int id)
        {
            return dal.GetDepartmentName(id);
        }

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(Model.department model)
        {
            return dal.Add(model);
        }

        /// <summary>
        /// 修改一列数据
        /// </summary>
        public bool UpdateField(int id, string strValue)
        {
            return dal.UpdateField(id, strValue);
        }

        /// <summary>
        /// 修改一列数据
        /// </summary>
        public bool UpdateField(string name, string strValue)
        {
            return dal.UpdateField(name, strValue);
        }

        /// <summary>
        /// 修改导航名称和标题
        /// </summary>
        /// <param name="old_name">旧名称</param>
        /// <param name="new_name">新名称</param>
        /// <param name="title">标题</param>
        /// <returns></returns>
        public bool Update(string old_name, string new_name, int sort_id)
        {
            return dal.Update(old_name, new_name, sort_id);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(Model.department model)
        {
            return dal.Update(model);
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool Delete(int id)
        {
            return dal.Delete(id);
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public Model.department GetModel(string name)
        {
            return dal.GetModel(name);
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public Model.department GetModel(int id)
        {
            return dal.GetModel(id);
        }

        /// <summary>
        /// 取得所有类别列表
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetList(int parent_id)
        {
            return dal.GetList(parent_id);
        }

        /// <summary>
        /// 取得所有类别列表(已经排序好)
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetListByOkParentId(int id, int parent_id)
        {
            return dal.GetListByOkParentId(id, parent_id);
        }

        /// <summary>
        /// 取得所有类别列表
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetLists(int parent_id)
        {
            return dal.GetLists(parent_id);
        }

        /// <summary>
        /// 取得自己没有选过的部门列表
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <param name="id">部门ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetListsByNotDepartmentStr(int parent_id, string departmentStr)
        {
            return dal.GetListsByNotDepartmentStr(parent_id, departmentStr);
        }

        /// <summary>
        /// 取得自己选过的部门列表
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <param name="id">部门ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetListsByOktDepartmentStr(int parent_id, string departmentStr)
        {
            return dal.GetListsByOktDepartmentStr(parent_id, departmentStr);
        }

        /// <summary>
        /// 取得所有类别列表(已经排序好)
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetListByOkId(int id, int parent_id)
        {
            return dal.GetListByOkId(id, parent_id);
        }


        /// <summary>
        /// 取得会员组所属管理员的部门
        /// </summary>
        /// <param name="oldTable">所有管理员的部门信息</param>
        /// <returns>DataTable</returns>
        public DataTable GetListByOldTable(DataTable oldTable, int parent_id)
        {
            return dal.GetListByOldTable(oldTable, parent_id);
        }

        /// <summary>
        /// 根据管理员ID取到管理员所在部门
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetListByMid(int mid)
        {
            return dal.GetListByMid(mid);
        }


        /// <summary>
        /// 根据部门ID取到管理员所在部门
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetListByDid(int did)
        {
            return dal.GetListByDid(did);
        }

        #endregion

        #region 扩展方法================================
        /// <summary>
        /// 根据导航的名称查询其ID
        /// </summary>
        public int GetNavId(string nav_name)
        {
            return dal.GetNavId(nav_name);
        }

        /// <summary>
        /// 修改菜单的调用名称
        /// </summary>
        /// <param name="old_nav_name">旧调用名称</param>
        /// <param name="new_nav_name">新调用名称</param>
        /// <returns></returns>
        public bool UpdateNavName(string old_nav_name, string new_nav_name)
        {
            if (old_nav_name == new_nav_name)
            {
                return true;
            }
            return dal.UpdateNavName(old_nav_name, new_nav_name);
        }


        /// <summary>
        /// 取得所有类别列表(没有排序好，只有数据)
        /// </summary>
        /// <param name="parent_id">父ID，0为所有类别</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataList(int parent_id)
        {
            return dal.GetDataList(parent_id);
        }
        #endregion
    }
}
