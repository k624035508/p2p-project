using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using Agp2p.DBUtility;
using Agp2p.Common;


namespace Agp2p.DAL
{
    /// <summary>
    /// 数据访问类:部门管理
    /// </summary>
    public partial class department
    {
        private string databaseprefix; //数据库表名前缀
        public department(string _databaseprefix)
        {
            databaseprefix = _databaseprefix;
        }

        #region 基本方法========================================
        /// <summary>
        /// 是否存在该记录
        /// </summary>
        public bool Exists(int id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) from li_departments");
            strSql.Append(" where id=@id ");
            SqlParameter[] parameters = {
					new SqlParameter("@id", SqlDbType.Int,4)};
            parameters[0].Value = id;
            return DbHelperSQL.Exists(strSql.ToString(), parameters);
        }

        /// <summary>
        /// 查询是否存在该记录
        /// </summary>
        public bool Exists(string department_name)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) from li_departments");
            strSql.Append(" where department_name=@department_name ");
            SqlParameter[] parameters = {
					new SqlParameter("@department_name", SqlDbType.VarChar,50)};
            parameters[0].Value = department_name;
            return DbHelperSQL.Exists(strSql.ToString(), parameters);
        }

        /// <summary>
        /// 返回部门名称
        /// </summary>
        public string GetDepartmentName(int id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select top 1 department_name from li_departments");
            strSql.Append(" where id=" + id);
            string department_name = Convert.ToString(DbHelperSQL.GetSingle(strSql.ToString()));
            if (string.IsNullOrEmpty(department_name))
            {
                return "";
            }
            return department_name;
        }

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(Model.department model)
        {
            using (SqlConnection conn = new SqlConnection(DbHelperSQL.connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        StringBuilder strSql = new StringBuilder();
                        strSql.Append("insert into li_departments(");
                        strSql.Append("department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default)");
                        strSql.Append(" values (");
                        strSql.Append("@department_name,@sort_id,@is_lock,@remark,@parent_id,@class_list,@class_layer,@is_sys,@is_default)");
                        strSql.Append(";select @@IDENTITY");
                        SqlParameter[] parameters = {
					            new SqlParameter("@department_name", SqlDbType.NVarChar,100),
					            new SqlParameter("@sort_id", SqlDbType.Int,4),
					            new SqlParameter("@is_lock", SqlDbType.TinyInt,1),
					            new SqlParameter("@remark", SqlDbType.NVarChar,500),
					            new SqlParameter("@parent_id", SqlDbType.Int,4),
					            new SqlParameter("@class_list", SqlDbType.NVarChar,500),
					            new SqlParameter("@class_layer", SqlDbType.Int,4),
                                new SqlParameter("@is_sys", SqlDbType.TinyInt,1),
                                new SqlParameter("@is_default",SqlDbType.Int,4)};
                        parameters[0].Value = model.department_name;
                        parameters[1].Value = model.sort_id;
                        parameters[2].Value = model.is_lock;
                        parameters[3].Value = model.remark;
                        parameters[4].Value = model.parent_id;
                        parameters[5].Value = model.class_list;
                        parameters[6].Value = model.class_layer;
                        parameters[7].Value = model.is_sys;
                        parameters[8].Value = model.is_default;
                        object obj = DbHelperSQL.GetSingle(conn, trans, strSql.ToString(), parameters); //带事务

                        model.id = Convert.ToInt32(obj); //得到刚插入的新ID
                        if (model.parent_id > 0)
                        {
                            Model.department model2 = GetModel(conn, trans, model.parent_id); //带事务
                            model.class_list = model2.class_list + model.id + ",";
                            model.class_layer = model2.class_layer + 1;
                        }
                        else
                        {
                            model.class_list = "," + model.id + ",";
                            model.class_layer = 1;
                        }
                        //修改节点列表和深度
                        DbHelperSQL.ExecuteSql(conn, trans, "update li_departments set class_list='" + model.class_list + "', class_layer=" + model.class_layer + " where id=" + model.id); //带事务
                        //如无异常则提交事务
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return 0;
                    }
                }
            }
            return model.id;
        }

        /// <summary>
        /// 修改一列数据
        /// </summary>
        public bool UpdateField(int id, string strValue)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update li_departments set " + strValue);
            strSql.Append(" where id=" + id);
            int rows = DbHelperSQL.ExecuteSql(strSql.ToString());
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 修改一列数据
        /// </summary>
        public bool UpdateField(string name, string strValue)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update li_departments set " + strValue);
            strSql.Append(" where department_name='" + name + "'");
            int rows = DbHelperSQL.ExecuteSql(strSql.ToString());
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 修改导航名称和标题
        /// </summary>
        public bool Update(string old_name, string new_name, int sort_id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update li_departments set ");
            strSql.Append("department_name=@new_name,");
            strSql.Append("sort_id=@sort_id");
            strSql.Append(" where department_name=@old_name");
            SqlParameter[] parameters = {
					new SqlParameter("@new_name", SqlDbType.NVarChar,50),
                    new SqlParameter("@sort_id", SqlDbType.Int,4),
					new SqlParameter("@old_name", SqlDbType.NVarChar,50)};
            parameters[0].Value = new_name;
            parameters[1].Value = sort_id;
            parameters[2].Value = old_name;
            int rows = DbHelperSQL.ExecuteSql(strSql.ToString(), parameters);
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(Model.department model)
        {
            Model.department oldModel = GetModel(model.id); //旧的数据
            using (SqlConnection conn = new SqlConnection(DbHelperSQL.connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        //先判断选中的父节点是否被包含
                        if (IsContainNode(model.id, model.parent_id))
                        {
                            //查找旧父节点数据
                            string class_list = "," + model.parent_id + ",";
                            int class_layer = 1;
                            if (oldModel.parent_id > 0)
                            {
                                Model.department oldParentModel = GetModel(conn, trans, oldModel.parent_id); //带事务
                                class_list = oldParentModel.class_list + model.parent_id + ",";
                                class_layer = oldParentModel.class_layer + 1;
                            }
                            //先提升选中的父节点
                            DbHelperSQL.ExecuteSql(conn, trans, "update li_departments set parent_id=" + oldModel.parent_id + ",class_list='" + class_list + "', class_layer=" + class_layer + " where id=" + model.parent_id); //带事务
                            UpdateChilds(conn, trans, model.parent_id); //带事务
                        }
                        //更新子节点
                        if (model.parent_id > 0)
                        {
                            Model.department model2 = GetModel(conn, trans, model.parent_id); //带事务
                            model.class_list = model2.class_list + model.id + ",";
                            model.class_layer = model2.class_layer + 1;
                        }
                        else
                        {
                            model.class_list = "," + model.id + ",";
                            model.class_layer = 1;
                        }
                        StringBuilder strSql = new StringBuilder();
                        strSql.Append("update li_departments set ");
                        strSql.Append("department_name=@department_name,");
                        strSql.Append("sort_id=@sort_id,");
                        strSql.Append("is_lock=@is_lock,");
                        strSql.Append("remark=@remark,");
                        strSql.Append("parent_id=@parent_id,");
                        strSql.Append("class_list=@class_list,");
                        strSql.Append("class_layer=@class_layer,");
                        strSql.Append("is_sys=@is_sys,");
                        strSql.Append("is_default=@is_default");
                        strSql.Append(" where id=@id");
                        SqlParameter[] parameters = {
					            new SqlParameter("@department_name", SqlDbType.NVarChar,100),
					            new SqlParameter("@sort_id", SqlDbType.Int,4),
					            new SqlParameter("@is_lock", SqlDbType.TinyInt,1),
					            new SqlParameter("@remark", SqlDbType.NVarChar,500),
					            new SqlParameter("@parent_id", SqlDbType.Int,4),
					            new SqlParameter("@class_list", SqlDbType.NVarChar,500),
					            new SqlParameter("@class_layer", SqlDbType.Int,4),
                                new SqlParameter("@is_sys", SqlDbType.TinyInt,1),
                                new SqlParameter("@is_default",SqlDbType.Int,4), 
                                new SqlParameter("@id", SqlDbType.Int,4)};
                        parameters[0].Value = model.department_name;
                        parameters[1].Value = model.sort_id;
                        parameters[2].Value = model.is_lock;
                        parameters[3].Value = model.remark;
                        parameters[4].Value = model.parent_id;
                        parameters[5].Value = model.class_list;
                        parameters[6].Value = model.class_layer;
                        parameters[7].Value = model.is_sys;
                        parameters[8].Value = model.is_default;
                        parameters[9].Value = model.id;
                        DbHelperSQL.ExecuteSql(conn, trans, strSql.ToString(), parameters);
                        //更新子节点
                        UpdateChilds(conn, trans, model.id);
                        //如无发生错误则提交事务
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool Delete(int id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from li_departments ");
            strSql.Append(" where class_list like '%," + id + ",%' ");
            int rows = DbHelperSQL.ExecuteSql(strSql.ToString());

            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public Model.department GetModel(string department_name)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select  top 1 id,department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default");
            strSql.Append(" from li_departments ");
            strSql.Append(" where department_name=@department_name");
            SqlParameter[] parameters = {
					new SqlParameter("@department_name", SqlDbType.NVarChar,100)};
            parameters[0].Value = department_name;

            Model.department model = new Model.department();
            DataSet ds = DbHelperSQL.Query(strSql.ToString(), parameters);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["id"].ToString() != "")
                {
                    model.id = int.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                }
                model.department_name = ds.Tables[0].Rows[0]["department_name"].ToString();
                if (ds.Tables[0].Rows[0]["sort_id"].ToString() != "")
                {
                    model.sort_id = int.Parse(ds.Tables[0].Rows[0]["sort_id"].ToString());
                }
                if (ds.Tables[0].Rows[0]["is_lock"].ToString() != "")
                {
                    model.is_lock = int.Parse(ds.Tables[0].Rows[0]["is_lock"].ToString());
                }
                model.remark = ds.Tables[0].Rows[0]["remark"].ToString();
                if (ds.Tables[0].Rows[0]["parent_id"].ToString() != "")
                {
                    model.parent_id = int.Parse(ds.Tables[0].Rows[0]["parent_id"].ToString());
                }
                model.class_list = ds.Tables[0].Rows[0]["class_list"].ToString();
                if (ds.Tables[0].Rows[0]["class_layer"].ToString() != "")
                {
                    model.class_layer = int.Parse(ds.Tables[0].Rows[0]["class_layer"].ToString());
                }
                if (ds.Tables[0].Rows[0]["is_sys"].ToString() != "")
                {
                    model.is_sys = int.Parse(ds.Tables[0].Rows[0]["is_sys"].ToString());
                }
                if (ds.Tables[0].Rows[0]["is_default"].ToString() != "")
                {
                    model.is_default = int.Parse(ds.Tables[0].Rows[0]["is_default"].ToString());
                }
                return model;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public Model.department GetModel(int id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select  top 1 id,department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default");
            strSql.Append(" from li_departments ");
            strSql.Append(" where id=@id");
            SqlParameter[] parameters = {
					new SqlParameter("@id", SqlDbType.Int,4)};
            parameters[0].Value = id;

            Model.department model = new Model.department();
            DataSet ds = DbHelperSQL.Query(strSql.ToString(), parameters);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["id"].ToString() != "")
                {
                    model.id = int.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                }
                model.department_name = ds.Tables[0].Rows[0]["department_name"].ToString();
                if (ds.Tables[0].Rows[0]["sort_id"].ToString() != "")
                {
                    model.sort_id = int.Parse(ds.Tables[0].Rows[0]["sort_id"].ToString());
                }
                if (ds.Tables[0].Rows[0]["is_lock"].ToString() != "")
                {
                    model.is_lock = int.Parse(ds.Tables[0].Rows[0]["is_lock"].ToString());
                }
                model.remark = ds.Tables[0].Rows[0]["remark"].ToString();
                if (ds.Tables[0].Rows[0]["parent_id"].ToString() != "")
                {
                    model.parent_id = int.Parse(ds.Tables[0].Rows[0]["parent_id"].ToString());
                }
                model.class_list = ds.Tables[0].Rows[0]["class_list"].ToString();
                if (ds.Tables[0].Rows[0]["class_layer"].ToString() != "")
                {
                    model.class_layer = int.Parse(ds.Tables[0].Rows[0]["class_layer"].ToString());
                }
                if (ds.Tables[0].Rows[0]["is_sys"].ToString() != "")
                {
                    model.is_sys = int.Parse(ds.Tables[0].Rows[0]["is_sys"].ToString());
                }
                if (ds.Tables[0].Rows[0]["is_default"].ToString() != "")
                {
                    model.is_default = int.Parse(ds.Tables[0].Rows[0]["is_default"].ToString());
                }
                return model;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 得到一个对象实体(重载，带事务)
        /// </summary>
        public Model.department GetModel(SqlConnection conn, SqlTransaction trans, int id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select  top 1 id,department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default");
            strSql.Append(" from li_departments");
            strSql.Append(" where id=@id");
            SqlParameter[] parameters = {
					new SqlParameter("@id", SqlDbType.Int,4)};
            parameters[0].Value = id;

            Model.department model = new Model.department();
            DataSet ds = DbHelperSQL.Query(conn, trans, strSql.ToString(), parameters);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["id"].ToString() != "")
                {
                    model.id = int.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                }
                model.department_name = ds.Tables[0].Rows[0]["department_name"].ToString();
                if (ds.Tables[0].Rows[0]["sort_id"].ToString() != "")
                {
                    model.sort_id = int.Parse(ds.Tables[0].Rows[0]["sort_id"].ToString());
                }
                if (ds.Tables[0].Rows[0]["is_lock"].ToString() != "")
                {
                    model.is_lock = int.Parse(ds.Tables[0].Rows[0]["is_lock"].ToString());
                }
                model.remark = ds.Tables[0].Rows[0]["remark"].ToString();
                if (ds.Tables[0].Rows[0]["parent_id"].ToString() != "")
                {
                    model.parent_id = int.Parse(ds.Tables[0].Rows[0]["parent_id"].ToString());
                }
                model.class_list = ds.Tables[0].Rows[0]["class_list"].ToString();
                if (ds.Tables[0].Rows[0]["class_layer"].ToString() != "")
                {
                    model.class_layer = int.Parse(ds.Tables[0].Rows[0]["class_layer"].ToString());
                }
                if (ds.Tables[0].Rows[0]["is_sys"].ToString() != "")
                {
                    model.is_sys = int.Parse(ds.Tables[0].Rows[0]["is_sys"].ToString());
                }
                if (ds.Tables[0].Rows[0]["is_default"].ToString() != "")
                {
                    model.is_default = int.Parse(ds.Tables[0].Rows[0]["is_default"].ToString());
                }
                return model;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 取得所有类别列表(没有排序好)
        /// </summary>
        /// <param name="parent_id">父ID，0为所有类别</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataList(int parent_id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select id,department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default");
            strSql.Append(" from li_departments");
            strSql.Append(" where parent_id=" + parent_id + " order by sort_id asc,id asc");
            DataSet ds = DbHelperSQL.Query(strSql.ToString());
            return ds.Tables[0];
        }

        /// <summary>
        /// 取得所有类别列表(已经排序好)
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetList(int parent_id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select id,department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default");
            strSql.Append(" from li_departments");
            strSql.Append(" order by sort_id asc,id asc");
            DataSet ds = DbHelperSQL.Query(strSql.ToString());
            DataTable oldData = ds.Tables[0] as DataTable;
            if (oldData == null)
            {
                return null;
            }
            //复制结构
            DataTable newData = oldData.Clone();
            //调用迭代组合成DAGATABLE
            GetChilds(oldData, newData, parent_id);
            return newData;
        }

        /// <summary>
        /// 取得所有类别列表(已经排序好)
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetListByOkParentId(int id, int parent_id)
        {
            StringBuilder strSql = new StringBuilder();
            //strSql.Append("select id,department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default");
            //strSql.Append(" from li_departments where id =" + id + "");
            //strSql.Append(" union all ");
            //strSql.Append(" select id,department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default from li_departments where parent_id=" + id + " ");
            //strSql.Append(" order by sort_id asc,id asc");
            //DataTable oldData = DbHelperSQL.Query(strSql.ToString()).Tables[0] as DataTable;
            strSql.Append(
                "select id,department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default");
            strSql.Append(" from li_departments");
            strSql.Append(" order by sort_id asc,id asc");
            DataSet ds = DbHelperSQL.Query(strSql.ToString());
            DataTable oldData = ds.Tables[0] as DataTable;

            if (oldData == null)
            {
                return null;
            }
            //复制结构
            DataTable newData = oldData.Clone();
            //调用迭代组合成DAGATABLE
            GetChilds(oldData, newData, id);

            var strSql2 = new StringBuilder();
            strSql2.Append("select id,department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default");
            strSql2.Append(" from li_departments where id =" + id + "");
            DataSet ds2 = DbHelperSQL.Query(strSql2.ToString());
            DataTable oldData2 = ds2.Tables[0] as DataTable;


            DataTable listDataTable = oldData2.Clone();


            object[] obj = new object[listDataTable.Columns.Count];
            //添加dt的数据

            for (int i = 0; i < oldData2.Rows.Count; i++)
            {
                oldData2.Rows[i].ItemArray.CopyTo(obj, 0);
                listDataTable.Rows.Add(obj);
            }

            for (int i = 0; i < newData.Rows.Count; i++)
            {
                newData.Rows[i].ItemArray.CopyTo(obj, 0);
                listDataTable.Rows.Add(obj);
            }

            DataTable newData2 = listDataTable.Clone();
            GetChilds4(listDataTable, newData2, parent_id);

            return newData2;
        }

        /// <summary>
        /// 取得所有类别列表(已经排序好)
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetListByOkId(int id, int parent_id)
        {
            StringBuilder strSql = new StringBuilder();

            strSql.Append("select id,department_name,sort_id,is_lock,remark,parent_id,class_list,class_layer,is_sys,is_default");
            strSql.Append(" from li_departments where id =" + id + "");
            strSql.Append(" order by sort_id asc,id asc");
            DataSet ds = DbHelperSQL.Query(strSql.ToString());
            DataTable oldData = ds.Tables[0] as DataTable;
            if (oldData == null)
            {
                return null;
            }
            //复制结构
            //DataTable newData = oldData.Clone();
            //调用迭代组合成DAGATABLE
           //GetChilds(oldData, newData, parent_id);
            return oldData;
        }

        /// <summary>
        /// 根据管理员ID取到管理员所在部门
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetListByMid(int mid)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select s.id,s.department_name,s.sort_id,s.is_lock,s.remark,s.parent_id,s.class_list,s.class_layer,s.is_sys,s.is_default,(select count(*) from dt_manager where department_id=s.id) as dnum,m.id as mid,m.real_name");
            strSql.Append(" from li_departments s inner join dt_manager m on m.department_id=s.id");
            strSql.Append(" where m.id=" + mid + " order by sort_id asc,id asc");
            DataSet ds = DbHelperSQL.Query(strSql.ToString());
            return ds.Tables[0];
        }

        /// <summary>
        /// 根据管理员ID取到管理员所在部门
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetListByDid(int mid)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select s.id,s.department_name,s.sort_id,s.is_lock,s.remark,s.parent_id,s.class_list,s.class_layer,s.is_sys,s.is_default,(select count(*) from dt_manager where department_id=s.id) as dnum,m.id as mid,m.real_name");
            strSql.Append(" from li_departments s inner join dt_manager m on m.department_id=s.id");
            strSql.Append(" where m.id=" + mid + " order by sort_id asc,id asc");
            DataSet ds = DbHelperSQL.Query(strSql.ToString());
            DataTable oldData = ds.Tables[0] as DataTable;
            if (oldData == null)
            {
                return null;
            }
            //复制结构
            DataTable newData = oldData.Clone();
            //调用迭代组合成DAGATABLE
            GetChildsByParentid(oldData, newData, 0);
            return newData;
        }

        /// <summary>
        /// 取到管理员所在部门
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetListByOldTable(DataTable oldTable,int parent_id)
        {
            DataTable oldData = oldTable;
            if (oldData == null)
            {
                return null;
            }
            //复制结构
            DataTable newData = oldData.Clone();
            //调用迭代组合成DAGATABLE
            GetChildsByParentid(oldData, newData, parent_id);
            return newData;
        }

        /// <summary>
        /// 取得所有类别列表(已经排序好)
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetLists(int parent_id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select s.id,s.department_name,s.sort_id,s.is_lock,s.remark,s.parent_id,s.class_list,s.class_layer,s.is_sys,is_default,(select count(*) from dt_manager where department_id=s.id) as dnum");
            strSql.Append(" from li_departments s");
            strSql.Append(" order by sort_id asc,id asc");
            DataSet ds = DbHelperSQL.Query(strSql.ToString());
            DataTable oldData = ds.Tables[0] as DataTable;
            if (oldData == null)
            {
                return null;
            }
            //复制结构
            DataTable newData = oldData.Clone();
            //调用迭代组合成DAGATABLE
            GetChilds2(oldData, newData, parent_id);
            return newData;
        }

        /// <summary>
        /// 取得还没有选过的部门类别列表(已经排序好)
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <param name="departmentStr">已经选过的部门ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetListsByNotDepartmentStr(int parent_id, string departmentStr)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select s.id,s.department_name,s.sort_id,s.is_lock,s.remark,s.parent_id,s.class_list,s.class_layer,s.is_sys,s.is_default,(select count(*) from dt_manager where department_id=s.id) as dnum");
            strSql.Append("  from li_departments s where s.id not in (" + departmentStr + ")");
            strSql.Append(" order by sort_id asc,id asc");
            DataSet ds = DbHelperSQL.Query(strSql.ToString());
            DataTable oldData = ds.Tables[0] as DataTable;
            if (oldData == null)
            {
                return null;
            }
            //复制结构
            DataTable newData = oldData.Clone();
            DataTable newData2 = oldData.Clone();
            //调用迭代组合成DAGATABLE
            GetChilds2(oldData, newData, parent_id);
            GetChilds3(oldData, newData2, 0);

          DataTable newDataTable = newData.Clone();

          object[] obj = new object[newDataTable.Columns.Count];
          //添加DataTable1的数据
          for (int i = 0; i < newData.Rows.Count; i++)
          {
              newData.Rows[i].ItemArray.CopyTo(obj, 0);
              newDataTable.Rows.Add(obj);
          }
          //添加DataTable2的数据
          for (int i = 0; i < newData2.Rows.Count; i++)
          {
              newData2.Rows[i].ItemArray.CopyTo(obj, 0);
              newDataTable.Rows.Add(obj);
          }

         newDataTable= DeleteSameRow(newDataTable,"id");
            return newDataTable;
        }

        #region 删除DataTable重复列，类似distinct
        /// <summary>   
        /// 删除DataTable重复列，类似distinct   
        /// </summary>   
        /// <param name="dt">DataTable</param>   
        /// <param name="Field">字段名</param>   
        /// <returns></returns>   
        public static DataTable DeleteSameRow(DataTable dt, string Field)
        {
            ArrayList indexList = new ArrayList();
            // 找出待删除的行索引   
            for (int i = 0; i < dt.Rows.Count - 1; i++)
            {
                if (!IsContain(indexList, i))
                {
                    for (int j = i + 1; j < dt.Rows.Count; j++)
                    {
                        if (dt.Rows[i][Field].ToString() == dt.Rows[j][Field].ToString())
                        {
                            indexList.Add(j);
                        }
                    }
                }
            }
            indexList.Sort();
            // 排序
            for (int i = indexList.Count - 1; i >= 0; i--)// 根据待删除索引列表删除行  
            {
                int index = Convert.ToInt32(indexList[i]);
                dt.Rows.RemoveAt(index);
            }
            return dt;
        }

        /// <summary>   
        /// 判断数组中是否存在   
        /// </summary>   
        /// <param name="indexList">数组</param>   
        /// <param name="index">索引</param>   
        /// <returns></returns>   
        public static bool IsContain(ArrayList indexList, int index)
        {
            for (int i = 0; i < indexList.Count; i++)
            {
                int tempIndex = Convert.ToInt32(indexList[i]);
                if (tempIndex == index)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// 取得已经选过的部门类别列表(已经排序好)
        /// </summary>
        /// <param name="parent_id">父ID</param>
        /// <param name="departmentStr">已经选过的部门ID</param>
        /// <returns>DataTable</returns>
        public DataTable GetListsByOktDepartmentStr(int parent_id, string departmentStr)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select s.id,s.department_name,s.sort_id,s.is_lock,s.remark,s.parent_id,s.class_list,s.class_layer,s.is_sys,s.is_default,(select count(*) from dt_manager where department_id=s.id) as dnum");
            strSql.Append("  from li_departments s where s.id in (" + departmentStr + ")");
            strSql.Append(" order by sort_id asc,id asc");
            DataSet ds = DbHelperSQL.Query(strSql.ToString());
            DataTable oldData = ds.Tables[0] as DataTable;
            if (oldData == null)
            {
                return null;
            }
            else
            {
                if (oldData.Rows.Count > 1)
                {
                    //复制结构
                    DataTable newData = oldData.Clone();
                    //调用迭代组合成DAGATABLE
                    GetChilds2(oldData, newData, parent_id);
                    return newData;
                }
                else
                {
                    return oldData;
                }
            } 
        }


        #endregion

        #region 扩展方法================================
        /// <summary>
        /// 根据导航的名称查询其ID
        /// </summary>
        public int GetNavId(string department_name)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select top 1 id from li_departments");
            strSql.Append(" where department_name=@department_name");
            SqlParameter[] parameters = {
					new SqlParameter("@department_name", SqlDbType.NVarChar,100)};
            parameters[0].Value = department_name;
            string str = Convert.ToString(DbHelperSQL.GetSingle(strSql.ToString(), parameters));
            return Utils.StrToInt(str, 0);
        }
        /// <summary>
        /// 修改菜单的调用名称
        /// </summary>
        /// <param name="old_nav_name">旧调用名称</param>
        /// <param name="new_nav_name">新调用名称</param>
        /// <returns></returns>
        public bool UpdateNavName(string old_nav_name, string new_nav_name)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update li_departments set department_name=@new_nav_name");
            strSql.Append(" where department_name=@old_nav_name");
            SqlParameter[] parameters = {
					            new SqlParameter("@new_nav_name", SqlDbType.NVarChar,100),
					            new SqlParameter("@old_nav_name", SqlDbType.NVarChar,100)};
            parameters[0].Value = new_nav_name;
            parameters[1].Value = old_nav_name;
            int rows = DbHelperSQL.ExecuteSql(strSql.ToString(), parameters);
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 私有方法================================
        /// <summary>
        /// 从内存中取得所有下级类别列表（自身迭代）
        /// </summary>
        private void GetChilds(DataTable oldData, DataTable newData, int parent_id)
        {
            DataRow[] dr = oldData.Select("parent_id=" + parent_id);
            for (int i = 0; i < dr.Length; i++)
            {
                //添加一行数据
                DataRow row = newData.NewRow();
                row["id"] = int.Parse(dr[i]["id"].ToString());
                row["department_name"] = dr[i]["department_name"].ToString();
                row["sort_id"] = int.Parse(dr[i]["sort_id"].ToString());
                row["is_lock"] = int.Parse(dr[i]["is_lock"].ToString());
                row["remark"] = dr[i]["remark"].ToString();
                row["parent_id"] = int.Parse(dr[i]["parent_id"].ToString());
                row["class_list"] = dr[i]["class_list"].ToString();
                row["class_layer"] = int.Parse(dr[i]["class_layer"].ToString());
                row["is_sys"] = int.Parse(dr[i]["is_sys"].ToString());
                row["is_default"] = int.Parse(dr[i]["is_default"].ToString());
                newData.Rows.Add(row);
                //调用自身迭代
                this.GetChilds(oldData, newData, int.Parse(dr[i]["id"].ToString()));
            }
        }

        /// <summary>
        /// 从内存中取得所有下级类别列表（自身迭代）
        /// </summary>
        private void GetChilds4(DataTable oldData, DataTable newData, int parent_id)
        {
            DataRow[] dr = oldData.Select("parent_id=" + parent_id);
            for (int i = 0; i < dr.Length; i++)
            {
                //添加一行数据
                DataRow row = newData.NewRow();
                row["id"] = int.Parse(dr[i]["id"].ToString());
                row["department_name"] = dr[i]["department_name"].ToString();
                row["sort_id"] = int.Parse(dr[i]["sort_id"].ToString());
                row["is_lock"] = int.Parse(dr[i]["is_lock"].ToString());
                row["remark"] = dr[i]["remark"].ToString();
                row["parent_id"] = int.Parse(dr[i]["parent_id"].ToString());
                row["class_list"] = dr[i]["class_list"].ToString();
                row["class_layer"] = int.Parse(dr[i]["class_layer"].ToString());
                row["is_sys"] = int.Parse(dr[i]["is_sys"].ToString());
                row["is_default"] = int.Parse(dr[i]["is_default"].ToString());
                newData.Rows.Add(row);
                //调用自身迭代
                this.GetChilds4(oldData, newData, int.Parse(dr[i]["id"].ToString()));
            }
        }

        /// <summary>
        /// 从内存中取得所有下级类别列表（自身迭代）
        /// </summary>
        private void GetChildsByParentid(DataTable oldData, DataTable newData, int parent_id)
        {
            DataRow[] dr = oldData.Select("parent_id="+parent_id);
            for (int i = 0; i < dr.Length; i++)
            {
                //添加一行数据
                DataRow row = newData.NewRow();
                row["id"] = int.Parse(dr[i]["id"].ToString());
                row["department_name"] = dr[i]["department_name"].ToString();
                row["sort_id"] = int.Parse(dr[i]["sort_id"].ToString());
                row["is_lock"] = int.Parse(dr[i]["is_lock"].ToString());
                row["remark"] = dr[i]["remark"].ToString();
                row["parent_id"] = int.Parse(dr[i]["parent_id"].ToString());
                row["class_list"] = dr[i]["class_list"].ToString();
                row["class_layer"] = int.Parse(dr[i]["class_layer"].ToString());
                row["is_sys"] = int.Parse(dr[i]["is_sys"].ToString());
                row["is_default"] = int.Parse(dr[i]["is_default"].ToString());
                row["dnum"] = int.Parse(dr[i]["dnum"].ToString());
                row["mid"] = int.Parse(dr[i]["mid"].ToString());
                row["real_name"] = dr[i]["real_name"].ToString();
                newData.Rows.Add(row);
                //调用自身迭代
                this.GetChildsByParentid(oldData, newData, int.Parse(dr[i]["id"].ToString()));
            }
        }


        /// <summary>
        /// 从内存中取得所有下级类别列表（自身迭代）
        /// </summary>
        private void GetChilds2(DataTable oldData, DataTable newData, int parent_id)
        {
            DataRow[] dr = oldData.Select("parent_id=" + parent_id);
            for (int i = 0; i < dr.Length; i++)
            {
                //添加一行数据
                DataRow row = newData.NewRow();
                row["id"] = int.Parse(dr[i]["id"].ToString());
                row["department_name"] = dr[i]["department_name"].ToString();
                row["sort_id"] = int.Parse(dr[i]["sort_id"].ToString());
                row["is_lock"] = int.Parse(dr[i]["is_lock"].ToString());
                row["remark"] = dr[i]["remark"].ToString();
                row["parent_id"] = int.Parse(dr[i]["parent_id"].ToString());
                row["class_list"] = dr[i]["class_list"].ToString();
                row["class_layer"] = int.Parse(dr[i]["class_layer"].ToString());
                row["is_sys"] = int.Parse(dr[i]["is_sys"].ToString());
                row["is_default"] = int.Parse(dr[i]["is_default"].ToString());
                row["dnum"] = int.Parse(dr[i]["dnum"].ToString());
                newData.Rows.Add(row);
                //调用自身迭代
                this.GetChilds2(oldData, newData, int.Parse(dr[i]["id"].ToString()));
            }
        }

        /// <summary>
        /// 从内存中取得所有下级类别列表（自身迭代）
        /// </summary>
        private void GetChilds3(DataTable oldData, DataTable newData, int parent_id)
        {
            DataRow[] dr = oldData.Select("parent_id <>" + parent_id);
            for (int i = 0; i < dr.Length; i++)
            {
                //添加一行数据
                DataRow row = newData.NewRow();
                row["id"] = int.Parse(dr[i]["id"].ToString());
                row["department_name"] = dr[i]["department_name"].ToString();
                row["sort_id"] = int.Parse(dr[i]["sort_id"].ToString());
                row["is_lock"] = int.Parse(dr[i]["is_lock"].ToString());
                row["remark"] = dr[i]["remark"].ToString();
                row["parent_id"] = int.Parse(dr[i]["parent_id"].ToString());
                row["class_list"] = dr[i]["class_list"].ToString();
                row["class_layer"] = int.Parse(dr[i]["class_layer"].ToString());
                row["is_sys"] = int.Parse(dr[i]["is_sys"].ToString());
                row["is_default"] = int.Parse(dr[i]["is_default"].ToString());
                row["dnum"] = int.Parse(dr[i]["dnum"].ToString());
                newData.Rows.Add(row);
                //调用自身迭代
                this.GetChilds2(oldData, newData, int.Parse(dr[i]["id"].ToString()));
            }
        }

        /// <summary>
        /// 修改子节点的ID列表及深度（自身迭代）
        /// </summary>
        /// <param name="parent_id"></param>
        private void UpdateChilds(SqlConnection conn, SqlTransaction trans, int parent_id)
        {
            //查找父节点信息
            Model.department model = GetModel(conn, trans, parent_id);
            if (model != null)
            {
                //查找子节点
                string strSql = "select id from li_departments where parent_id=" + parent_id;
                DataSet ds = DbHelperSQL.Query(conn, trans, strSql); //带事务
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //修改子节点的ID列表及深度
                    int id = int.Parse(dr["id"].ToString());
                    string class_list = model.class_list + id + ",";
                    int class_layer = model.class_layer + 1;
                    DbHelperSQL.ExecuteSql(conn, trans, "update li_departments set class_list='" + class_list + "', class_layer=" + class_layer + " where id=" + id); //带事务

                    //调用自身迭代
                    this.UpdateChilds(conn, trans, id); //带事务
                }
            }
        }

        /// <summary>
        /// 验证节点是否被包含
        /// </summary>
        /// <param name="id">待查询的节点</param>
        /// <param name="parent_id">父节点</param>
        /// <returns></returns>
        private bool IsContainNode(int id, int parent_id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) from li_departments ");
            strSql.Append(" where class_list like '%," + id + ",%' and id=" + parent_id);
            return DbHelperSQL.Exists(strSql.ToString());
        }

        #endregion
    }
}
