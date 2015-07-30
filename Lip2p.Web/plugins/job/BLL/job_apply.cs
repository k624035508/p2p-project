using System;
using System.Data;
using System.Collections.Generic;
namespace Lip2p.Web.Plugin.Job.BLL
{
	/// <summary>
	/// job_apply
	/// </summary>
	public partial class job_apply
	{
        //private readonly DAL.job_apply dal = new DAL.job_apply();
        //public job_apply()
        //{}

        private readonly Lip2p.Model.siteconfig siteConfig = new Lip2p.BLL.siteconfig().loadConfig(); //获得站点配置信息
        private readonly DAL.job_apply dal;

        public job_apply()
        {
            dal = new DAL.job_apply(siteConfig.sysdatabaseprefix);
        }

		#region  Method

		/// <summary>
		/// 得到最大ID
		/// </summary>
		public int GetMaxId()
		{
			return dal.GetMaxId();
		}

		/// <summary>
		/// 是否存在该记录
		/// </summary>
		public bool Exists(int ID)
		{
			return dal.Exists(ID);
		}

		/// <summary>
		/// 增加一条数据
		/// </summary>
		public bool Add(Model.job_apply model)
		{
			return dal.Add(model);
		}

        /// <summary>
        /// 修改一列数据
        /// </summary>
        public void UpdateField(int id, string strValue)
        {
            dal.UpdateField(id, strValue);
        }

		/// <summary>
		/// 更新一条数据
		/// </summary>
		public bool Update(Model.job_apply model)
		{
			return dal.Update(model);
		}

		/// <summary>
		/// 删除一条数据
		/// </summary>
		public bool Delete(int ID)
		{
			
			return dal.Delete(ID);
		}
		/// <summary>
		/// 删除一条数据
		/// </summary>
		public bool DeleteList(string IDlist )
		{
			return dal.DeleteList(IDlist );
		}

		/// <summary>
		/// 得到一个对象实体
		/// </summary>
		public Model.job_apply GetModel(int ID)
		{
			
			return dal.GetModel(ID);
		}

		/// <summary>
		/// 获得数据列表
		/// </summary>
		public DataSet GetList(string strWhere)
		{
			return dal.GetList(strWhere);
		}
		/// <summary>
		/// 获得数据列表
		/// </summary>
		public List<Model.job_apply> GetModelList(string strWhere)
		{
			DataSet ds = dal.GetList(strWhere);
			return DataTableToList(ds.Tables[0]);
		}
		/// <summary>
		/// 获得数据列表
		/// </summary>
		public List<Model.job_apply> DataTableToList(DataTable dt)
		{
			List<Model.job_apply> modelList = new List<Model.job_apply>();
			int rowsCount = dt.Rows.Count;
            if (rowsCount > 0)
            {
                Model.job_apply model;
                for (int n = 0; n < rowsCount; n++)
                {
                    model = new Model.job_apply();
                    if (dt.Rows[n]["ID"] != null && dt.Rows[n]["ID"].ToString() != "")
                    {
                        model.ID = int.Parse(dt.Rows[n]["ID"].ToString());
                    }
                    if (dt.Rows[n]["job_id"] != null && dt.Rows[n]["job_id"].ToString() != "")
                    {
                        model.job_id = int.Parse(dt.Rows[n]["job_id"].ToString());
                    }
                    if (dt.Rows[n]["RealName"] != null && dt.Rows[n]["RealName"].ToString() != "")
                    {
                        model.RealName = dt.Rows[n]["RealName"].ToString();
                    }
                    if (dt.Rows[n]["Sex"] != null && dt.Rows[n]["Sex"].ToString() != "")
                    {
                        model.Sex = dt.Rows[n]["Sex"].ToString();
                    }
                    if (dt.Rows[n]["Birth"] != null && dt.Rows[n]["Birth"].ToString() != "")
                    {
                        model.Birth = dt.Rows[n]["Birth"].ToString();
                    }
                    if (dt.Rows[n]["Marital"] != null && dt.Rows[n]["Marital"].ToString() != "")
                    {
                        model.Marital = dt.Rows[n]["Marital"].ToString();
                    }
                    if (dt.Rows[n]["Origin"] != null && dt.Rows[n]["Origin"].ToString() != "")
                    {
                        model.Origin = dt.Rows[n]["Origin"].ToString();
                    }
                    if (dt.Rows[n]["Hobby"] != null && dt.Rows[n]["Hobby"].ToString() != "")
                    {
                        model.Hobby = dt.Rows[n]["Hobby"].ToString();
                    }
                    if (dt.Rows[n]["School"] != null && dt.Rows[n]["School"].ToString() != "")
                    {
                        model.School = dt.Rows[n]["School"].ToString();
                    }
                    if (dt.Rows[n]["Degree"] != null && dt.Rows[n]["Degree"].ToString() != "")
                    {
                        model.Degree = dt.Rows[n]["Degree"].ToString();
                    }
                    if (dt.Rows[n]["Profess"] != null && dt.Rows[n]["Profess"].ToString() != "")
                    {
                        model.Profess = dt.Rows[n]["Profess"].ToString();
                    }
                    if (dt.Rows[n]["IDNum"] != null && dt.Rows[n]["IDNum"].ToString() != "")
                    {
                        model.IDNum = dt.Rows[n]["IDNum"].ToString();
                    }
                    if (dt.Rows[n]["Address"] != null && dt.Rows[n]["Address"].ToString() != "")
                    {
                        model.Address = dt.Rows[n]["Address"].ToString();
                    }
                    if (dt.Rows[n]["Tel"] != null && dt.Rows[n]["Tel"].ToString() != "")
                    {
                        model.Tel = dt.Rows[n]["Tel"].ToString();
                    }
                    if (dt.Rows[n]["Email"] != null && dt.Rows[n]["Email"].ToString() != "")
                    {
                        model.Email = dt.Rows[n]["Email"].ToString();
                    }
                    if (dt.Rows[n]["Salary"] != null && dt.Rows[n]["Salary"].ToString() != "")
                    {
                        model.Salary = dt.Rows[n]["Salary"].ToString();
                    }
                    if (dt.Rows[n]["EducationExperience"] != null && dt.Rows[n]["EducationExperience"].ToString() != "")
                    {
                        model.EducationExperience = dt.Rows[n]["EducationExperience"].ToString();
                    }
                    if (dt.Rows[n]["WorkExperience"] != null && dt.Rows[n]["WorkExperience"].ToString() != "")
                    {
                        model.WorkExperience = dt.Rows[n]["WorkExperience"].ToString();
                    }
                    if (dt.Rows[n]["SelfContent"] != null && dt.Rows[n]["SelfContent"].ToString() != "")
                    {
                        model.SelfContent = dt.Rows[n]["SelfContent"].ToString();
                    }
                    if (dt.Rows[n]["is_see"] != null && dt.Rows[n]["is_see"].ToString() != "")
                    {
                        model.is_see = int.Parse(dt.Rows[n]["is_see"].ToString());
                    }
                    if (dt.Rows[n]["sort_id"] != null && dt.Rows[n]["sort_id"].ToString() != "")
                    {
                        model.sort_id = int.Parse(dt.Rows[n]["sort_id"].ToString());
                    }
                    if (dt.Rows[n]["add_time"] != null && dt.Rows[n]["add_time"].ToString() != "")
                    {
                        model.add_time = DateTime.Parse(dt.Rows[n]["add_time"].ToString());
                    }
                    modelList.Add(model);
                }
            }
			return modelList;
		}

		/// <summary>
		/// 获得数据列表
		/// </summary>
		public DataSet GetAllList()
		{
			return GetList("");
		}

        /// <summary>
        /// 获得前几行数据
        /// </summary>
        public DataSet GetList(int Top, string strWhere, string filedOrder)
        {
            return dal.GetList(Top, strWhere, filedOrder);
        }

        /// <summary>
        /// 获得查询分页数据
        /// </summary>
        public DataSet GetList(int pageSize, int pageIndex, string strWhere, string filedOrder, out int recordCount)
        {
            return dal.GetList(pageSize, pageIndex, strWhere, filedOrder, out  recordCount);
        }

        /// <summary>
        /// 获得查询分页数据
        /// </summary>
        public DataSet GetPageList(int pageSize, int currentPage, string strWhere, string filedOrder)
        {
            return dal.GetPageList(pageSize, currentPage, strWhere, filedOrder);
        }

		/// <summary>
		/// 分页获取数据列表
		/// </summary>
		public int GetRecordCount(string strWhere)
		{
			return dal.GetRecordCount(strWhere);
		}
		/// <summary>
		/// 分页获取数据列表
		/// </summary>
		public DataSet GetListByPage(string strWhere, string orderby, int startIndex, int endIndex)
		{
			return dal.GetListByPage( strWhere,  orderby,  startIndex,  endIndex);
		}
		/// <summary>
		/// 分页获取数据列表
		/// </summary>
		//public DataSet GetList(int PageSize,int PageIndex,string strWhere)
		//{
			//return dal.GetList(PageSize,PageIndex,strWhere);
		//}

		#endregion  Method
	}
}

