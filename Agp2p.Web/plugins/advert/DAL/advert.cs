using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using Agp2p.DBUtility;
using Agp2p.Common;

namespace Agp2p.Web.Plugin.Advert.DAL
{
    /// <summary>
    /// 数据访问类:advert
    /// </summary>
    public partial class advert
    {
        private string databaseprefix; //数据库表名前缀
        public advert(string _databaseprefix)
        {
            databaseprefix = _databaseprefix;
        }

        #region  Method
        /// <summary>
        /// 是否存在该记录
        /// </summary>
        public bool Exists(int id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) from " + databaseprefix + "advert");
            strSql.Append(" where id=@id");
            SqlParameter[] parameters = {
					new SqlParameter("@id", SqlDbType.Int,4)};
            parameters[0].Value = id;

            return DbHelperSQL.Exists(strSql.ToString(), parameters);
        }

        /// <summary>
        /// 返回广告位名称
        /// </summary>
        public string GetTitle(int id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select top 1 title from " + databaseprefix + "advert");
            strSql.Append(" where id=" + id);
            string title = Convert.ToString(DbHelperSQL.GetSingle(strSql.ToString()));
            if (string.IsNullOrEmpty(title))
            {
                return "";
            }
            return title;
        }

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(Model.advert model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into " + databaseprefix + "advert(");
            strSql.Append("title,type,price,remark,view_num,view_width,view_height,target,add_time)");
            strSql.Append(" values (");
            strSql.Append("@title,@type,@price,@remark,@view_num,@view_width,@view_height,@target,@add_time)");
            strSql.Append(";select @@IDENTITY");
            SqlParameter[] parameters = {
					new SqlParameter("@title", SqlDbType.NVarChar,100),
					new SqlParameter("@type", SqlDbType.TinyInt,1),
					new SqlParameter("@price", SqlDbType.Decimal,9),
					new SqlParameter("@remark", SqlDbType.NVarChar,255),
					new SqlParameter("@view_num", SqlDbType.Int,4),
					new SqlParameter("@view_width", SqlDbType.Int,4),
					new SqlParameter("@view_height", SqlDbType.Int,4),
					new SqlParameter("@target", SqlDbType.NVarChar,30),
					new SqlParameter("@add_time", SqlDbType.DateTime)};
            parameters[0].Value = model.title;
            parameters[1].Value = model.type;
            parameters[2].Value = model.price;
            parameters[3].Value = model.remark;
            parameters[4].Value = model.view_num;
            parameters[5].Value = model.view_width;
            parameters[6].Value = model.view_height;
            parameters[7].Value = model.target;
            parameters[8].Value = model.add_time;

            object obj = DbHelperSQL.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(obj);
            }
        }
        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(Model.advert model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update " + databaseprefix + "advert set ");
            strSql.Append("title=@title,");
            strSql.Append("type=@type,");
            strSql.Append("price=@price,");
            strSql.Append("remark=@remark,");
            strSql.Append("view_num=@view_num,");
            strSql.Append("view_width=@view_width,");
            strSql.Append("view_height=@view_height,");
            strSql.Append("target=@target,");
            strSql.Append("add_time=@add_time");
            strSql.Append(" where id=@id");
            SqlParameter[] parameters = {
					new SqlParameter("@title", SqlDbType.NVarChar,100),
					new SqlParameter("@type", SqlDbType.TinyInt,1),
					new SqlParameter("@price", SqlDbType.Decimal,9),
					new SqlParameter("@remark", SqlDbType.NVarChar,255),
					new SqlParameter("@view_num", SqlDbType.Int,4),
					new SqlParameter("@view_width", SqlDbType.Int,4),
					new SqlParameter("@view_height", SqlDbType.Int,4),
					new SqlParameter("@target", SqlDbType.NVarChar,30),
					new SqlParameter("@add_time", SqlDbType.DateTime),
					new SqlParameter("@id", SqlDbType.Int,4)};
            parameters[0].Value = model.title;
            parameters[1].Value = model.type;
            parameters[2].Value = model.price;
            parameters[3].Value = model.remark;
            parameters[4].Value = model.view_num;
            parameters[5].Value = model.view_width;
            parameters[6].Value = model.view_height;
            parameters[7].Value = model.target;
            parameters[8].Value = model.add_time;
            parameters[9].Value = model.id;

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
        /// 删除一条数据及其子表
        /// </summary>
        public bool Delete(int id)
        {
            List<CommandInfo> sqllist = new List<CommandInfo>();
            StringBuilder strSql2 = new StringBuilder();
            strSql2.Append("delete from " + databaseprefix + "advert_banner ");
            strSql2.Append(" where aid=@aid ");
            SqlParameter[] parameters2 = {
					new SqlParameter("@aid", SqlDbType.Int,4)};
            parameters2[0].Value = id;

            CommandInfo cmd = new CommandInfo(strSql2.ToString(), parameters2);
            sqllist.Add(cmd);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from dt_advert ");
            strSql.Append(" where id=@id");
            SqlParameter[] parameters = {
					new SqlParameter("@id", SqlDbType.Int,4)};
            parameters[0].Value = id;

            cmd = new CommandInfo(strSql.ToString(), parameters);
            sqllist.Add(cmd);
            int rowsAffected = DbHelperSQL.ExecuteSqlTran(sqllist);
            if (rowsAffected > 0)
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
        public Model.advert GetModel(int id)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select  top 1 id,title,type,price,remark,view_num,view_width,view_height,target,add_time from " + databaseprefix + "advert ");
            strSql.Append(" where id=@id");
            SqlParameter[] parameters = {
					new SqlParameter("@id", SqlDbType.Int,4)};
            parameters[0].Value = id;

            Model.advert model = new Model.advert();
            DataSet ds = DbHelperSQL.Query(strSql.ToString(), parameters);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["id"] != null && ds.Tables[0].Rows[0]["id"].ToString() != "")
                {
                    model.id = int.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                }
                if (ds.Tables[0].Rows[0]["title"] != null && ds.Tables[0].Rows[0]["title"].ToString() != "")
                {
                    model.title = ds.Tables[0].Rows[0]["title"].ToString();
                }
                if (ds.Tables[0].Rows[0]["type"] != null && ds.Tables[0].Rows[0]["type"].ToString() != "")
                {
                    model.type = int.Parse(ds.Tables[0].Rows[0]["type"].ToString());
                }
                if (ds.Tables[0].Rows[0]["price"] != null && ds.Tables[0].Rows[0]["price"].ToString() != "")
                {
                    model.price = decimal.Parse(ds.Tables[0].Rows[0]["price"].ToString());
                }
                if (ds.Tables[0].Rows[0]["remark"] != null && ds.Tables[0].Rows[0]["remark"].ToString() != "")
                {
                    model.remark = ds.Tables[0].Rows[0]["remark"].ToString();
                }
                if (ds.Tables[0].Rows[0]["view_num"] != null && ds.Tables[0].Rows[0]["view_num"].ToString() != "")
                {
                    model.view_num = int.Parse(ds.Tables[0].Rows[0]["view_num"].ToString());
                }
                if (ds.Tables[0].Rows[0]["view_width"] != null && ds.Tables[0].Rows[0]["view_width"].ToString() != "")
                {
                    model.view_width = int.Parse(ds.Tables[0].Rows[0]["view_width"].ToString());
                }
                if (ds.Tables[0].Rows[0]["view_height"] != null && ds.Tables[0].Rows[0]["view_height"].ToString() != "")
                {
                    model.view_height = int.Parse(ds.Tables[0].Rows[0]["view_height"].ToString());
                }
                model.target = ds.Tables[0].Rows[0]["target"].ToString();
                if (ds.Tables[0].Rows[0]["add_time"] != null && ds.Tables[0].Rows[0]["add_time"].ToString() != "")
                {
                    model.add_time = DateTime.Parse(ds.Tables[0].Rows[0]["add_time"].ToString());
                }
                return model;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public DataSet GetList(string strWhere)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select id,title,type,price,remark,view_num,view_width,view_height,target,add_time ");
            strSql.Append(" FROM " + databaseprefix + "advert ");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            return DbHelperSQL.Query(strSql.ToString());
        }

        /// <summary>
        /// 获得前几行数据
        /// </summary>
        public DataSet GetList(int Top, string strWhere, string filedOrder)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select ");
            if (Top > 0)
            {
                strSql.Append(" top " + Top.ToString());
            }
            strSql.Append(" id,title,type,price,remark,view_num,view_width,view_height,target,add_time ");
            strSql.Append(" FROM " + databaseprefix + "advert ");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            strSql.Append(" order by " + filedOrder);
            return DbHelperSQL.Query(strSql.ToString());
        }

        /// <summary>
        /// 获得查询分页数据
        /// </summary>
        public DataSet GetList(int pageSize, int pageIndex, string strWhere, string filedOrder, out int recordCount)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select * FROM " + databaseprefix + "advert");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            recordCount = Convert.ToInt32(DbHelperSQL.GetSingle(PagingHelper.CreateCountingSql(strSql.ToString())));
            return DbHelperSQL.Query(PagingHelper.CreatePagingSql(recordCount, pageSize, pageIndex, strSql.ToString(), filedOrder));
        }

        #endregion  Method
    }
}