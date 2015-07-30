using System;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using Lip2p.DBUtility;//Please add references
using Lip2p.Common;
namespace Lip2p.Web.Plugin.Job.DAL
{
	/// <summary>
	/// 数据访问类:job_apply
	/// </summary>
	public partial class job_apply
	{
  private string databaseprefix; //数据库表名前缀
  public job_apply(string _databaseprefix)
		{
            databaseprefix = _databaseprefix;

        }
		#region  Method

		/// <summary>
		/// 得到最大ID
		/// </summary>
		public int GetMaxId()
		{
            return DbHelperSQL.GetMaxID("ID", databaseprefix + "job_apply"); 
		}

		/// <summary>
		/// 是否存在该记录
		/// </summary>
		public bool Exists(int ID)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select count(1) from " + databaseprefix + "job_apply");
			strSql.Append(" where ID=@ID");
			SqlParameter[] parameters = {
					new SqlParameter("@ID", SqlDbType.Int,4)
			};
			parameters[0].Value = ID;

			return DbHelperSQL.Exists(strSql.ToString(),parameters);
		}


		/// <summary>
		/// 增加一条数据
		/// </summary>
		public bool Add(Model.job_apply model)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("insert into " + databaseprefix + "job_apply(");
            strSql.Append("[job_id],[RealName],[Sex],[Birth],[Marital],[Origin],[Hobby],[School],[Degree],[Profess],[IDNum],[Address],[Tel],[Email],[Salary],[EducationExperience],[WorkExperience],[SelfContent],[is_see],[sort_id],[add_time])");
			strSql.Append(" values (");
            strSql.Append("@job_id,@RealName,@Sex,@Birth,@Marital,@Origin,@Hobby,@School,@Degree,@Profess,@IDNum,@Address,@Tel,@Email,@Salary,@EducationExperience,@WorkExperience,@SelfContent,@is_see,@sort_id,@add_time)");
			SqlParameter[] parameters = {
					new SqlParameter("@job_id", SqlDbType.Int,4),
					new SqlParameter("@RealName", SqlDbType.NVarChar,50),
					new SqlParameter("@Sex", SqlDbType.NVarChar,50),
					new SqlParameter("@Birth", SqlDbType.NVarChar,50),
					new SqlParameter("@Marital", SqlDbType.NVarChar,50),
					new SqlParameter("@Origin", SqlDbType.NVarChar,50),
					new SqlParameter("@Hobby", SqlDbType.NVarChar,50),
					new SqlParameter("@School", SqlDbType.NVarChar,50),
                    new SqlParameter("@Degree", SqlDbType.NVarChar,50),
					new SqlParameter("@Profess", SqlDbType.NVarChar,50),
					new SqlParameter("@IDNum", SqlDbType.NVarChar,50),
					new SqlParameter("@Address", SqlDbType.NVarChar,200),
					new SqlParameter("@Tel", SqlDbType.NVarChar,50),
					new SqlParameter("@Email", SqlDbType.NVarChar,100),
					new SqlParameter("@Salary", SqlDbType.NVarChar,50),
                    new SqlParameter("@EducationExperience", SqlDbType.Text),
                    new SqlParameter("@WorkExperience", SqlDbType.Text),
					new SqlParameter("@SelfContent", SqlDbType.Text),
                    new SqlParameter("@is_see", SqlDbType.Int,4),
                    new SqlParameter("@sort_id", SqlDbType.Int,4),
					new SqlParameter("@add_time", SqlDbType.DateTime)};
            parameters[0].Value = model.job_id;
			parameters[1].Value = model.RealName;
			parameters[2].Value = model.Sex;
			parameters[3].Value = model.Birth;
			parameters[4].Value = model.Marital;
			parameters[5].Value = model.Origin;
			parameters[6].Value = model.Hobby;
			parameters[7].Value = model.School;
            parameters[8].Value = model.Degree;
			parameters[9].Value = model.Profess;
			parameters[10].Value = model.IDNum;
			parameters[11].Value = model.Address;
			parameters[12].Value = model.Tel;
			parameters[13].Value = model.Email;
			parameters[14].Value = model.Salary;
            parameters[15].Value = model.EducationExperience;
            parameters[16].Value = model.WorkExperience;
			parameters[17].Value = model.SelfContent;
            parameters[18].Value = model.is_see;
            parameters[19].Value = model.sort_id;
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
            strSql.Append("update " + databaseprefix + "job_apply set " + strValue);
            strSql.Append(" where id=" + id);
            DbHelperSQL.ExecuteSql(strSql.ToString());
        }

		/// <summary>
		/// 更新一条数据
		/// </summary>
		public bool Update(Model.job_apply model)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("update " + databaseprefix + "job_apply set ");
			strSql.Append("[job_id]=@job_id,");
			strSql.Append("[RealName]=@RealName,");
			strSql.Append("[Sex]=@Sex,");
			strSql.Append("[Birth]=@Birth,");
			strSql.Append("[Marital]=@Marital,");
			strSql.Append("[Origin]=@Origin,");
			strSql.Append("[Hobby]=@Hobby,");
			strSql.Append("[School]=@School,");
            strSql.Append("[Degree]=@Degree,");
			strSql.Append("]Profess]=@Profess,");
			strSql.Append("[IDNum]=@IDNum,");
			strSql.Append("[Address]=@Address,");
			strSql.Append("[Tel]=@Tel,");
			strSql.Append("[Email]=@Email,");
			strSql.Append("[Salary]=@Salary,");
            strSql.Append("[EducationExperience]=@EducationExperience,");
            strSql.Append("[WorkExperience]=@WorkExperience,");
			strSql.Append("[SelfContent]=@SelfContent,");
            strSql.Append("[is_see]=@is_see,");
            strSql.Append("[sort_id]=@sort_id,");
			strSql.Append("[add_time]=@add_time");
			strSql.Append(" where ID=@ID");
			SqlParameter[] parameters = {
					new SqlParameter("@job_id", SqlDbType.Int,4),
					new SqlParameter("@RealName", SqlDbType.NVarChar,50),
					new SqlParameter("@Sex", SqlDbType.NVarChar,50),
					new SqlParameter("@Birth", SqlDbType.NVarChar,50),
					new SqlParameter("@Marital", SqlDbType.NVarChar,50),
					new SqlParameter("@Origin", SqlDbType.NVarChar,50),
					new SqlParameter("@Hobby", SqlDbType.NVarChar,50),
					new SqlParameter("@School", SqlDbType.NVarChar,50),
                    new SqlParameter("@Degree", SqlDbType.NVarChar,50),
					new SqlParameter("@Profess", SqlDbType.NVarChar,50),
					new SqlParameter("@IDNum", SqlDbType.NVarChar,50),
					new SqlParameter("@Address", SqlDbType.NVarChar,200),
					new SqlParameter("@Tel", SqlDbType.NVarChar,50),
					new SqlParameter("@Email", SqlDbType.NVarChar,100),
					new SqlParameter("@Salary", SqlDbType.NVarChar,50),
                    new SqlParameter("@EducationExperience", SqlDbType.Text),
                    new SqlParameter("@WorkExperience", SqlDbType.Text),
					new SqlParameter("@SelfContent", SqlDbType.Text),
                    new SqlParameter("@is_see", SqlDbType.Int,4),
                    new SqlParameter("@sort_id", SqlDbType.Int,4),
					new SqlParameter("@add_time", SqlDbType.DateTime),
					new SqlParameter("@ID", SqlDbType.Int,4)};
            parameters[0].Value = model.job_id;
            parameters[1].Value = model.RealName;
            parameters[2].Value = model.Sex;
            parameters[3].Value = model.Birth;
            parameters[4].Value = model.Marital;
            parameters[5].Value = model.Origin;
            parameters[6].Value = model.Hobby;
            parameters[7].Value = model.School;
            parameters[8].Value = model.Degree;
            parameters[9].Value = model.Profess;
            parameters[10].Value = model.IDNum;
            parameters[11].Value = model.Address;
            parameters[12].Value = model.Tel;
            parameters[13].Value = model.Email;
            parameters[14].Value = model.Salary;
            parameters[15].Value = model.EducationExperience;
            parameters[16].Value = model.WorkExperience;
            parameters[17].Value = model.SelfContent;
            parameters[18].Value = model.is_see;
            parameters[19].Value = model.sort_id;
            parameters[20].Value = model.add_time;
			parameters[21].Value = model.ID;

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
		public bool Delete(int ID)
		{
			
			StringBuilder strSql=new StringBuilder();
			strSql.Append("delete from " + databaseprefix + "job_apply ");
			strSql.Append(" where ID=@ID");
			SqlParameter[] parameters = {
					new SqlParameter("@ID", SqlDbType.Int,4)
			};
			parameters[0].Value = ID;

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
		public bool DeleteList(string IDlist )
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("delete from " + databaseprefix + "job_apply ");
			strSql.Append(" where ID in ("+IDlist + ")  ");
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
		public Model.job_apply GetModel(int ID)
		{
			
			StringBuilder strSql=new StringBuilder();
            strSql.Append("select ID,job_id,RealName,Sex,Birth,Marital,Origin,Hobby,School,Degree,Profess,IDNum,Address,Tel,Email,Salary,EducationExperience,WorkExperience,SelfContent,is_see,sort_id,add_time from " + databaseprefix + "job_apply ");
			strSql.Append(" where ID=@ID");
			SqlParameter[] parameters = {
					new SqlParameter("@ID", SqlDbType.Int,4)
			};
			parameters[0].Value = ID;

			Model.job_apply model=new Model.job_apply();
			DataSet ds=DbHelperSQL.Query(strSql.ToString(),parameters);
			if(ds.Tables[0].Rows.Count>0)
			{
				if(ds.Tables[0].Rows[0]["ID"]!=null && ds.Tables[0].Rows[0]["ID"].ToString()!="")
				{
					model.ID=int.Parse(ds.Tables[0].Rows[0]["ID"].ToString());
				}
				if(ds.Tables[0].Rows[0]["job_id"]!=null && ds.Tables[0].Rows[0]["job_id"].ToString()!="")
				{
                    model.job_id = int.Parse(ds.Tables[0].Rows[0]["job_id"].ToString());
				}
				if(ds.Tables[0].Rows[0]["RealName"]!=null && ds.Tables[0].Rows[0]["RealName"].ToString()!="")
				{
					model.RealName=ds.Tables[0].Rows[0]["RealName"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Sex"]!=null && ds.Tables[0].Rows[0]["Sex"].ToString()!="")
				{
					model.Sex=ds.Tables[0].Rows[0]["Sex"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Birth"]!=null && ds.Tables[0].Rows[0]["Birth"].ToString()!="")
				{
					model.Birth=ds.Tables[0].Rows[0]["Birth"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Marital"]!=null && ds.Tables[0].Rows[0]["Marital"].ToString()!="")
				{
					model.Marital=ds.Tables[0].Rows[0]["Marital"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Origin"]!=null && ds.Tables[0].Rows[0]["Origin"].ToString()!="")
				{
					model.Origin=ds.Tables[0].Rows[0]["Origin"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Hobby"]!=null && ds.Tables[0].Rows[0]["Hobby"].ToString()!="")
				{
					model.Hobby=ds.Tables[0].Rows[0]["Hobby"].ToString();
				}
				if(ds.Tables[0].Rows[0]["School"]!=null && ds.Tables[0].Rows[0]["School"].ToString()!="")
				{
					model.School=ds.Tables[0].Rows[0]["School"].ToString();
				}
                if (ds.Tables[0].Rows[0]["Degree"] != null && ds.Tables[0].Rows[0]["Degree"].ToString() != "")
                {
                    model.Degree = ds.Tables[0].Rows[0]["Degree"].ToString();
                }
				if(ds.Tables[0].Rows[0]["Profess"]!=null && ds.Tables[0].Rows[0]["Profess"].ToString()!="")
				{
					model.Profess=ds.Tables[0].Rows[0]["Profess"].ToString();
				}
				if(ds.Tables[0].Rows[0]["IDNum"]!=null && ds.Tables[0].Rows[0]["IDNum"].ToString()!="")
				{
					model.IDNum=ds.Tables[0].Rows[0]["IDNum"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Address"]!=null && ds.Tables[0].Rows[0]["Address"].ToString()!="")
				{
					model.Address=ds.Tables[0].Rows[0]["Address"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Tel"]!=null && ds.Tables[0].Rows[0]["Tel"].ToString()!="")
				{
					model.Tel=ds.Tables[0].Rows[0]["Tel"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Email"]!=null && ds.Tables[0].Rows[0]["Email"].ToString()!="")
				{
					model.Email=ds.Tables[0].Rows[0]["Email"].ToString();
				}
				if(ds.Tables[0].Rows[0]["Salary"]!=null && ds.Tables[0].Rows[0]["Salary"].ToString()!="")
				{
					model.Salary=ds.Tables[0].Rows[0]["Salary"].ToString();
				}
                if (ds.Tables[0].Rows[0]["EducationExperience"] != null && ds.Tables[0].Rows[0]["EducationExperience"].ToString() != "")
                {
                    model.EducationExperience = ds.Tables[0].Rows[0]["EducationExperience"].ToString();
                }
                if (ds.Tables[0].Rows[0]["WorkExperience"] != null && ds.Tables[0].Rows[0]["WorkExperience"].ToString() != "")
                {
                    model.WorkExperience = ds.Tables[0].Rows[0]["WorkExperience"].ToString();
                }
				if(ds.Tables[0].Rows[0]["SelfContent"]!=null && ds.Tables[0].Rows[0]["SelfContent"].ToString()!="")
				{
					model.SelfContent=ds.Tables[0].Rows[0]["SelfContent"].ToString();
				}
                if (ds.Tables[0].Rows[0]["is_see"] != null && ds.Tables[0].Rows[0]["is_see"].ToString() != "")
                {
                    model.is_see = int.Parse(ds.Tables[0].Rows[0]["is_see"].ToString());
                }
                if (ds.Tables[0].Rows[0]["sort_id"] != null && ds.Tables[0].Rows[0]["sort_id"].ToString() != "")
                {
                    model.sort_id = int.Parse(ds.Tables[0].Rows[0]["sort_id"].ToString());
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
            strSql.Append("select ID,job_id,RealName,Sex,Birth,Marital,Origin,Hobby,School,Degree,Profess,IDNum,Address,Tel,Email,Salary,EducationExperience,WorkExperience,SelfContent,is_see,sort_id,add_time ");
			strSql.Append(" FROM " + databaseprefix + "job_apply ");
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
            strSql.Append("ID,job_id,RealName,Sex,Birth,Marital,Origin,Hobby,School,Degree,Profess,IDNum,Address,Tel,Email,Salary,EducationExperience,WorkExperience,SelfContent,is_see,sort_id,add_time ");
            strSql.Append(" FROM " + databaseprefix + "job_apply ");
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
            strSql.Append("select * FROM " + databaseprefix + "job_apply");
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

            strSql.Append("select top " + pageSize + " * from " + databaseprefix + "job_apply");
            if (currentPage > 0)
            {
                strSql.Append(" where Id not in(select top " + topSize + " Id from " + databaseprefix + "job_apply");
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
			strSql.Append("select count(1) FROM " + databaseprefix + "job_apply ");
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
				strSql.Append("order by T.ID desc");
			}
			strSql.Append(")AS Row, T.*  from " + databaseprefix + "job_apply T ");
			if (!string.IsNullOrEmpty(strWhere.Trim()))
			{
				strSql.Append(" WHERE " + strWhere);
			}
			strSql.Append(" ) TT");
			strSql.AppendFormat(" WHERE TT.Row between {0} and {1}", startIndex, endIndex);
			return DbHelperSQL.Query(strSql.ToString());
		}

		/*
		/// <summary>
		/// 分页获取数据列表
		/// </summary>
		public DataSet GetList(int PageSize,int PageIndex,string strWhere)
		{
			SqlParameter[] parameters = {
					new SqlParameter("@tblName", SqlDbType.NVarChar, 255),
					new SqlParameter("@fldName", SqlDbType.NVarChar, 255),
					new SqlParameter("@PageSize", SqlDbType.Int),
					new SqlParameter("@PageIndex", SqlDbType.Int),
					new SqlParameter("@IsReCount", SqlDbType.Boolean),
					new SqlParameter("@OrderType", SqlDbType.Boolean),
					new SqlParameter("@strWhere", SqlDbType.NVarChar,1000),
					};
			parameters[0].Value = "" + databaseprefix + "job_apply";
			parameters[1].Value = "ID";
			parameters[2].Value = PageSize;
			parameters[3].Value = PageIndex;
			parameters[4].Value = 0;
			parameters[5].Value = 0;
			parameters[6].Value = strWhere;	
			return DbHelperSQL.RunProcedure("UP_GetRecordByPage",parameters,"ds");
		}*/

		#endregion  Method
	}
}

