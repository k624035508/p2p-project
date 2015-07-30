using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Lip2p.DBUtility;
using Lip2p.Common;
namespace Lip2p.Web.Plugin.Job.DAL
{
	/// <summary>
	/// 数据访问类:Job
	/// </summary>
	public partial class job
	{
  private string databaseprefix; //数据库表名前缀
  public job(string _databaseprefix)
		{
            databaseprefix = _databaseprefix;

        }
		#region  Method

		/// <summary>
		/// 得到最大ID
		/// </summary>
		public int GetMaxId()
		{
            return DbHelperSQL.GetMaxID("id", "dt_Job"); 
		}

		/// <summary>
		/// 是否存在该记录
		/// </summary>
		public bool Exists(int id)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select count(1) from dt_Job");
			strSql.Append(" where id=@id");
            SqlParameter[] parameters = {
					new SqlParameter("@id", SqlDbType.Int,4)
			};
			parameters[0].Value = id;

			return DbHelperSQL.Exists(strSql.ToString(),parameters);
		}


		/// <summary>
		/// 增加一条数据
		/// </summary>
		public bool Add(Model.job model)
		{
			StringBuilder strSql=new StringBuilder();
            strSql.Append("insert into " + databaseprefix + "Job(");
			strSql.Append("[title],[depart],[number],[sex],[experience],[education],[age],[profession],[work_area],[start_time],[end_time],[Content],[ContactName],[Phone],[Fax],[Email],[Address],[click],[sort_id],[is_lock],[add_time])");
			strSql.Append(" values (");
			strSql.Append("@title,@depart,@number,@sex,@experience,@education,@age,@profession,@work_area,@start_time,@end_time,@Content,@ContactName,@Phone,@Fax,@Email,@Address,@click,@sort_id,@is_lock,@add_time)");
			SqlParameter[] parameters = {
					new SqlParameter("@title", SqlDbType.NVarChar,255),
					new SqlParameter("@depart", SqlDbType.NVarChar,50),
					new SqlParameter("@number", SqlDbType.TinyInt),
					new SqlParameter("@sex", SqlDbType.NVarChar,50),
					new SqlParameter("@experience", SqlDbType.NVarChar,50),
					new SqlParameter("@education", SqlDbType.NVarChar,50),
					new SqlParameter("@age", SqlDbType.NVarChar,50),
					new SqlParameter("@profession", SqlDbType.NVarChar,50),
					new SqlParameter("@work_area", SqlDbType.NVarChar,50),
					new SqlParameter("@start_time", SqlDbType.NVarChar,50),
					new SqlParameter("@end_time", SqlDbType.NVarChar,50),
					new SqlParameter("@Content", SqlDbType.NVarChar),
					new SqlParameter("@ContactName", SqlDbType.NVarChar,50),
					new SqlParameter("@Phone", SqlDbType.NVarChar,50),
					new SqlParameter("@Fax", SqlDbType.NVarChar,50),
					new SqlParameter("@Email", SqlDbType.NVarChar,50),
					new SqlParameter("@Address", SqlDbType.NVarChar,200),
					new SqlParameter("@click", SqlDbType.Int,4),
					new SqlParameter("@sort_id", SqlDbType.Int,4),
					new SqlParameter("@is_lock", SqlDbType.Int,4),
					new SqlParameter("@add_time", SqlDbType.DateTime)};
			parameters[0].Value = model.title;
			parameters[1].Value = model.depart;
			parameters[2].Value = model.number;
			parameters[3].Value = model.sex;
			parameters[4].Value = model.experience;
			parameters[5].Value = model.education;
			parameters[6].Value = model.age;
			parameters[7].Value = model.profession;
			parameters[8].Value = model.work_area;
			parameters[9].Value = model.start_time;
			parameters[10].Value = model.end_time;
			parameters[11].Value = model.Content;
			parameters[12].Value = model.ContactName;
			parameters[13].Value = model.Phone;
			parameters[14].Value = model.Fax;
			parameters[15].Value = model.Email;
			parameters[16].Value = model.Address;
			parameters[17].Value = model.click;
			parameters[18].Value = model.sort_id;
			parameters[19].Value = model.is_lock;
			parameters[20].Value = model.add_time;

			int rows=DbHelperSQL.ExecuteSql(strSql.ToString(),parameters);
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
        public void UpdateField(int id, string strValue)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update " + databaseprefix + "Job set " + strValue);
            strSql.Append(" where id=" + id);
            DbHelperSQL.ExecuteSql(strSql.ToString());
        }

		/// <summary>
		/// 更新一条数据
		/// </summary>
		public bool Update(Model.job model)
		{
			StringBuilder strSql=new StringBuilder();
            strSql.Append("update " + databaseprefix + "Job set ");
			strSql.Append("[title]=@title,");
			strSql.Append("[depart]=@depart,");
			strSql.Append("[number]=@number,");
			strSql.Append("[sex]=@sex,");
			strSql.Append("[experience]=@experience,");
			strSql.Append("[education]=@education,");
			strSql.Append("[age]=@age,");
			strSql.Append("[profession]=@profession,");
			strSql.Append("[work_area]=@work_area,");
			strSql.Append("[start_time]=@start_time,");
			strSql.Append("[end_time]=@end_time,");
			strSql.Append("[Content]=@Content,");
			strSql.Append("[ContactName]=@ContactName,");
			strSql.Append("[Phone]=@Phone,");
			strSql.Append("[Fax]=@Fax,");
			strSql.Append("[Email]=@Email,");
			strSql.Append("[Address]=@Address,");
			strSql.Append("[click]=@click,");
			strSql.Append("[sort_id]=@sort_id,");
			strSql.Append("[is_lock]=@is_lock,");
			strSql.Append("[add_time]=@add_time");
			strSql.Append(" where [id]=@id");
			SqlParameter[] parameters = {
					new SqlParameter("@title", SqlDbType.NVarChar,255),
					new SqlParameter("@depart", SqlDbType.NVarChar,50),
					new SqlParameter("@number", SqlDbType.TinyInt),
					new SqlParameter("@sex", SqlDbType.NVarChar,50),
					new SqlParameter("@experience", SqlDbType.NVarChar,50),
					new SqlParameter("@education", SqlDbType.NVarChar,50),
					new SqlParameter("@age", SqlDbType.NVarChar,50),
					new SqlParameter("@profession", SqlDbType.NVarChar,50),
					new SqlParameter("@work_area", SqlDbType.NVarChar,50),
					new SqlParameter("@start_time", SqlDbType.NVarChar,50),
					new SqlParameter("@end_time", SqlDbType.NVarChar,50),
					new SqlParameter("@Content", SqlDbType.NVarChar,0),
					new SqlParameter("@ContactName", SqlDbType.NVarChar,50),
					new SqlParameter("@Phone", SqlDbType.NVarChar,50),
					new SqlParameter("@Fax", SqlDbType.NVarChar,50),
					new SqlParameter("@Email", SqlDbType.NVarChar,50),
					new SqlParameter("@Address", SqlDbType.NVarChar,200),
					new SqlParameter("@click", SqlDbType.Int,4),
					new SqlParameter("@sort_id", SqlDbType.Int,4),
					new SqlParameter("@is_lock", SqlDbType.Int,4),
					new SqlParameter("@add_time", SqlDbType.DateTime),
					new SqlParameter("@id", SqlDbType.Int,4)};
			parameters[0].Value = model.title;
			parameters[1].Value = model.depart;
			parameters[2].Value = model.number;
			parameters[3].Value = model.sex;
			parameters[4].Value = model.experience;
			parameters[5].Value = model.education;
			parameters[6].Value = model.age;
			parameters[7].Value = model.profession;
			parameters[8].Value = model.work_area;
			parameters[9].Value = model.start_time;
			parameters[10].Value = model.end_time;
			parameters[11].Value = model.Content;
			parameters[12].Value = model.ContactName;
			parameters[13].Value = model.Phone;
			parameters[14].Value = model.Fax;
			parameters[15].Value = model.Email;
			parameters[16].Value = model.Address;
			parameters[17].Value = model.click;
			parameters[18].Value = model.sort_id;
			parameters[19].Value = model.is_lock;
			parameters[20].Value = model.add_time;
			parameters[21].Value = model.id;

			int rows=DbHelperSQL.ExecuteSql(strSql.ToString(),parameters);
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
		/// 删除一条数据
		/// </summary>
		public bool Delete(int id)
		{
			
			StringBuilder strSql=new StringBuilder();
            strSql.Append("delete from " + databaseprefix + "Job ");
			strSql.Append(" where id=@id");
			SqlParameter[] parameters = {
					new SqlParameter("@id", SqlDbType.Int,4)
			};
			parameters[0].Value = id;

			int rows=DbHelperSQL.ExecuteSql(strSql.ToString(),parameters);
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
		/// 批量删除数据
		/// </summary>
		public bool DeleteList(string idlist )
		{
			StringBuilder strSql=new StringBuilder();
            strSql.Append("delete from " + databaseprefix + "Job ");
			strSql.Append(" where id in ("+idlist + ")  ");
			int rows=DbHelperSQL.ExecuteSql(strSql.ToString());
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
		public Model.job GetModel(int id)
		{
			
			StringBuilder strSql=new StringBuilder();
            strSql.Append("select id,title,depart,number,sex,experience,education,age,profession,work_area,start_time,end_time,Content,ContactName,Phone,Fax,Email,Address,click,sort_id,is_lock,add_time from " + databaseprefix + "Job ");
			strSql.Append(" where id=@id");
			SqlParameter[] parameters = {
					new SqlParameter("@id", SqlDbType.Int,4)
			};
			parameters[0].Value = id;

			Model.job model=new Model.job();
			DataSet ds=DbHelperSQL.Query(strSql.ToString(),parameters);
			if(ds.Tables[0].Rows.Count>0)
			{
				if(ds.Tables[0].Rows[0]["id"]!=null && ds.Tables[0].Rows[0]["id"].ToString()!="")
				{
					model.id=int.Parse(ds.Tables[0].Rows[0]["id"].ToString());
				}
				if(ds.Tables[0].Rows[0]["title"]!=null && ds.Tables[0].Rows[0]["title"].ToString()!="")
				{
					model.title=ds.Tables[0].Rows[0]["title"].ToString();
				}
				if(ds.Tables[0].Rows[0]["depart"]!=null && ds.Tables[0].Rows[0]["depart"].ToString()!="")
				{
					model.depart=ds.Tables[0].Rows[0]["depart"].ToString();
				}
				if(ds.Tables[0].Rows[0]["number"]!=null && ds.Tables[0].Rows[0]["number"].ToString()!="")
				{
					model.number=int.Parse(ds.Tables[0].Rows[0]["number"].ToString());
				}
				if(ds.Tables[0].Rows[0]["sex"]!=null && ds.Tables[0].Rows[0]["sex"].ToString()!="")
				{
					model.sex=ds.Tables[0].Rows[0]["sex"].ToString();
				}
				if(ds.Tables[0].Rows[0]["experience"]!=null && ds.Tables[0].Rows[0]["experience"].ToString()!="")
				{
					model.experience=ds.Tables[0].Rows[0]["experience"].ToString();
				}
				if(ds.Tables[0].Rows[0]["education"]!=null && ds.Tables[0].Rows[0]["education"].ToString()!="")
				{
					model.education=ds.Tables[0].Rows[0]["education"].ToString();
				}
				if(ds.Tables[0].Rows[0]["age"]!=null && ds.Tables[0].Rows[0]["age"].ToString()!="")
				{
					model.age=ds.Tables[0].Rows[0]["age"].ToString();
				}
				if(ds.Tables[0].Rows[0]["profession"]!=null && ds.Tables[0].Rows[0]["profession"].ToString()!="")
				{
					model.profession=ds.Tables[0].Rows[0]["profession"].ToString();
				}
				if(ds.Tables[0].Rows[0]["work_area"]!=null && ds.Tables[0].Rows[0]["work_area"].ToString()!="")
				{
					model.work_area=ds.Tables[0].Rows[0]["work_area"].ToString();
				}
				if(ds.Tables[0].Rows[0]["start_time"]!=null && ds.Tables[0].Rows[0]["start_time"].ToString()!="")
				{
                    model.start_time = ds.Tables[0].Rows[0]["start_time"].ToString();
				}
				if(ds.Tables[0].Rows[0]["end_time"]!=null && ds.Tables[0].Rows[0]["end_time"].ToString()!="")
				{
					model.end_time=ds.Tables[0].Rows[0]["end_time"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Content"]!=null && ds.Tables[0].Rows[0]["Content"].ToString()!="")
				{
					model.Content=ds.Tables[0].Rows[0]["Content"].ToString();
				}
				if(ds.Tables[0].Rows[0]["ContactName"]!=null && ds.Tables[0].Rows[0]["ContactName"].ToString()!="")
				{
					model.ContactName=ds.Tables[0].Rows[0]["ContactName"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Phone"]!=null && ds.Tables[0].Rows[0]["Phone"].ToString()!="")
				{
					model.Phone=ds.Tables[0].Rows[0]["Phone"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Fax"]!=null && ds.Tables[0].Rows[0]["Fax"].ToString()!="")
				{
					model.Fax=ds.Tables[0].Rows[0]["Fax"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Email"]!=null && ds.Tables[0].Rows[0]["Email"].ToString()!="")
				{
					model.Email=ds.Tables[0].Rows[0]["Email"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Address"]!=null && ds.Tables[0].Rows[0]["Address"].ToString()!="")
				{
					model.Address=ds.Tables[0].Rows[0]["Address"].ToString();
				}
				if(ds.Tables[0].Rows[0]["click"]!=null && ds.Tables[0].Rows[0]["click"].ToString()!="")
				{
					model.click=int.Parse(ds.Tables[0].Rows[0]["click"].ToString());
				}
				if(ds.Tables[0].Rows[0]["sort_id"]!=null && ds.Tables[0].Rows[0]["sort_id"].ToString()!="")
				{
					model.sort_id=int.Parse(ds.Tables[0].Rows[0]["sort_id"].ToString());
				}
				if(ds.Tables[0].Rows[0]["is_lock"]!=null && ds.Tables[0].Rows[0]["is_lock"].ToString()!="")
				{
					model.is_lock=int.Parse(ds.Tables[0].Rows[0]["is_lock"].ToString());
				}
				if(ds.Tables[0].Rows[0]["add_time"]!=null && ds.Tables[0].Rows[0]["add_time"].ToString()!="")
				{
					model.add_time=DateTime.Parse(ds.Tables[0].Rows[0]["add_time"].ToString());
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
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select id,title,depart,number,sex,experience,education,age,profession,work_area,start_time,end_time,Content,ContactName,Phone,Fax,Email,Address,click,sort_id,is_lock,add_time ");
            strSql.Append(" FROM " + databaseprefix + "Job ");
			if(strWhere.Trim()!="")
			{
				strSql.Append(" where "+strWhere);
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
            strSql.Append("id,title,depart,number,sex,experience,education,age,profession,work_area,start_time,end_time,Content,ContactName,Phone,Fax,Email,Address,click,sort_id,is_lock,add_time ");
            strSql.Append(" FROM " + databaseprefix + "Job ");
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
            strSql.Append("select * FROM " + databaseprefix + "Job");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            recordCount = Convert.ToInt32(DbHelperSQL.GetSingle(PagingHelper.CreateCountingSql(strSql.ToString())));
            return DbHelperSQL.Query(PagingHelper.CreatePagingSql(recordCount, pageSize, pageIndex, strSql.ToString(), filedOrder));
        }

        /// <summary>
        /// 获得查询分页数据
        /// </summary>
        public DataSet GetPageList(int pageSize, int currentPage, string strWhere, string filedOrder)
        {
            int topSize = pageSize * currentPage;
            StringBuilder strSql = new StringBuilder();

            strSql.Append("select top " + pageSize + " * from " + databaseprefix + "Job");
            if (currentPage > 0)
            {
                strSql.Append(" where Id not in(select top " + topSize + " Id from dt_Job");
                if (strWhere.Trim() != "")
                {
                    strSql.Append(" where " + strWhere);
                }
                strSql.Append(" order by " + filedOrder + ")");
            }
            if (strWhere.Trim() != "")
            {
                if (currentPage > 0)
                {
                    strSql.Append(" and " + strWhere);
                }
                else
                {
                    strSql.Append(" where " + strWhere);
                }
            }
            strSql.Append(" order by " + filedOrder);

            return DbHelperSQL.Query(strSql.ToString());
        }

		/// <summary>
		/// 获取记录总数
		/// </summary>
		public int GetRecordCount(string strWhere)
		{
			StringBuilder strSql=new StringBuilder();
            strSql.Append("select count(1) FROM " + databaseprefix + "Job ");
			if(strWhere.Trim()!="")
			{
				strSql.Append(" where "+strWhere);
			}
            object obj = DbHelperSQL.GetSingle(strSql.ToString());
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
		/// 分页获取数据列表
		/// </summary>
		public DataSet GetListByPage(string strWhere, string orderby, int startIndex, int endIndex)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("SELECT * FROM ( ");
			strSql.Append(" SELECT ROW_NUMBER() OVER (");
			if (!string.IsNullOrEmpty(orderby.Trim()))
			{
				strSql.Append("order by T." + orderby );
			}
			else
			{
				strSql.Append("order by T.id desc");
			}
            strSql.Append(")AS Row, T.*  from " + databaseprefix + "Job T ");
			if (!string.IsNullOrEmpty(strWhere.Trim()))
			{
				strSql.Append(" WHERE " + strWhere);
			}
			strSql.Append(" ) TT");
			strSql.AppendFormat(" WHERE TT.Row between {0} and {1}", startIndex, endIndex);
			return DbHelperSQL.Query(strSql.ToString());
		}


		#endregion  Method
	}
}

