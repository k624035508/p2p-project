using System;
using System.Data;
using System.Collections.Generic;

namespace Lip2p.Web.Plugin.Job.BLL
{
	/// <summary>
	/// Job
	/// </summary>
	public partial class job
	{
      

           private readonly Lip2p.Model.siteconfig siteConfig = new Lip2p.BLL.siteconfig().loadConfig(); //获得站点配置信息
        private readonly DAL.job dal;

        public job()
        {
            dal = new DAL.job(siteConfig.sysdatabaseprefix);
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
		public bool Exists(int id)
		{
			return dal.Exists(id);
		}

		/// <summary>
		/// 增加一条数据
		/// </summary>
        public bool Add(Model.job model)
		{
			return dal.Add(model);
		}

        /// <summary>
        /// 修改一列数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="strValue"></param>
        public void UpdateField(int id, string strValue)
        {
            dal.UpdateField(id, strValue);
        }

		/// <summary>
		/// 更新一条数据
		/// </summary>
        public bool Update(Model.job model)
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
		/// 删除一条数据
		/// </summary>
		public bool DeleteList(string idlist )
		{
			return dal.DeleteList(idlist );
		}

		/// <summary>
		/// 得到一个对象实体
		/// </summary>
		public Model.job GetModel(int id)
		{
			
			return dal.GetModel(id);
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
		public List<Model.job> GetModelList(string strWhere)
		{
			DataSet ds = dal.GetList(strWhere);
			return DataTableToList(ds.Tables[0]);
		}
		/// <summary>
		/// 获得数据列表
		/// </summary>
		public List<Model.job> DataTableToList(DataTable dt)
		{
			List<Model.job> modelList = new List<Model.job>();
			int rowsCount = dt.Rows.Count;
			if (rowsCount > 0)
			{
				Model.job model;
				for (int n = 0; n < rowsCount; n++)
				{
					model = new Model.job();
					if(dt.Rows[n]["id"]!=null && dt.Rows[n]["id"].ToString()!="")
					{
						model.id=int.Parse(dt.Rows[n]["id"].ToString());
					}
					if(dt.Rows[n]["title"]!=null && dt.Rows[n]["title"].ToString()!="")
					{
					model.title=dt.Rows[n]["title"].ToString();
					}
					if(dt.Rows[n]["depart"]!=null && dt.Rows[n]["depart"].ToString()!="")
					{
					model.depart=dt.Rows[n]["depart"].ToString();
					}
					if(dt.Rows[n]["number"]!=null && dt.Rows[n]["number"].ToString()!="")
					{
					//model.number=dt.Rows[n]["number"].ToString();
					}
					if(dt.Rows[n]["sex"]!=null && dt.Rows[n]["sex"].ToString()!="")
					{
					model.sex=dt.Rows[n]["sex"].ToString();
					}
					if(dt.Rows[n]["experience"]!=null && dt.Rows[n]["experience"].ToString()!="")
					{
					model.experience=dt.Rows[n]["experience"].ToString();
					}
					if(dt.Rows[n]["education"]!=null && dt.Rows[n]["education"].ToString()!="")
					{
					model.education=dt.Rows[n]["education"].ToString();
					}
					if(dt.Rows[n]["age"]!=null && dt.Rows[n]["age"].ToString()!="")
					{
					model.age=dt.Rows[n]["age"].ToString();
					}
					if(dt.Rows[n]["profession"]!=null && dt.Rows[n]["profession"].ToString()!="")
					{
					model.profession=dt.Rows[n]["profession"].ToString();
					}
					if(dt.Rows[n]["work_area"]!=null && dt.Rows[n]["work_area"].ToString()!="")
					{
					model.work_area=dt.Rows[n]["work_area"].ToString();
					}
					if(dt.Rows[n]["start_time"]!=null && dt.Rows[n]["start_time"].ToString()!="")
					{
						model.start_time=dt.Rows[n]["start_time"].ToString();
					}
					if(dt.Rows[n]["end_time"]!=null && dt.Rows[n]["end_time"].ToString()!="")
					{
						model.end_time=dt.Rows[n]["end_time"].ToString();
					}
					if(dt.Rows[n]["Content"]!=null && dt.Rows[n]["Content"].ToString()!="")
					{
					model.Content=dt.Rows[n]["Content"].ToString();
					}
					if(dt.Rows[n]["ContactName"]!=null && dt.Rows[n]["ContactName"].ToString()!="")
					{
					model.ContactName=dt.Rows[n]["ContactName"].ToString();
					}
					if(dt.Rows[n]["Phone"]!=null && dt.Rows[n]["Phone"].ToString()!="")
					{
					model.Phone=dt.Rows[n]["Phone"].ToString();
					}
					if(dt.Rows[n]["Fax"]!=null && dt.Rows[n]["Fax"].ToString()!="")
					{
					model.Fax=dt.Rows[n]["Fax"].ToString();
					}
					if(dt.Rows[n]["Email"]!=null && dt.Rows[n]["Email"].ToString()!="")
					{
					model.Email=dt.Rows[n]["Email"].ToString();
					}
					if(dt.Rows[n]["Address"]!=null && dt.Rows[n]["Address"].ToString()!="")
					{
					model.Address=dt.Rows[n]["Address"].ToString();
					}
					if(dt.Rows[n]["click"]!=null && dt.Rows[n]["click"].ToString()!="")
					{
						model.click=int.Parse(dt.Rows[n]["click"].ToString());
					}
					if(dt.Rows[n]["sort_id"]!=null && dt.Rows[n]["sort_id"].ToString()!="")
					{
						model.sort_id=int.Parse(dt.Rows[n]["sort_id"].ToString());
					}
					if(dt.Rows[n]["is_lock"]!=null && dt.Rows[n]["is_lock"].ToString()!="")
					{
						model.is_lock=int.Parse(dt.Rows[n]["is_lock"].ToString());
					}
					if(dt.Rows[n]["add_time"]!=null && dt.Rows[n]["add_time"].ToString()!="")
					{
						model.add_time=DateTime.Parse(dt.Rows[n]["add_time"].ToString());
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

        public DataSet GetList(int Top, string strWhere, string filedOrder)
        {
            return dal.GetList(Top, strWhere, filedOrder);
        }

        /// <summary>
        /// 获得查询分页数据
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strWhere"></param>
        /// <param name="filedOrder"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public DataSet GetList(int pageSize, int pageIndex, string strWhere, string filedOrder, out int recordCount)
        {
            return dal.GetList(pageSize, pageIndex, strWhere, filedOrder, out  recordCount);
        }

        /// <summary>
        /// 获得查询分页数据
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="currentPage"></param>
        /// <param name="strWhere"></param>
        /// <param name="filedOrder"></param>
        /// <returns></returns>
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

