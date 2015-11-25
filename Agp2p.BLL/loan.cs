using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.BLL
{
    public class loan
    {
        protected Agp2pDataContext Context;

        public loan(Agp2pDataContext context)
        {
            Context = context;
        }

        public class MortgageItem
        {
            public int id { get; set; }
            public string name { get; set; }
            public string typeName { get; set; }
            public decimal valuation { get; set; }
            public byte status { get; set; }
            public bool check { get; set; }
            public bool enable { get; set; }
        }

        public string QueryUsingProject(int mortgageId)
        {
            var mortgage = Context.li_mortgages.Single(m => m.id == mortgageId);
            var projs = mortgage.li_risk_mortgage.Select(rm => rm.li_risks)
                .SelectMany(r => r.li_projects.Where(p => p.status < (int)Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime)).ToList();
            var projectNames = projs.Select(p => p.title).ToList();
            var riskCount = projs.GroupBy(p => p.risk_id).Count();
            return string.Join(",", projectNames) + (riskCount <= 1 ? "" : " 警告：此抵押物被多个风控信息关联");
        }

        public IQueryable<MortgageItem> LoadMortgageList(int loaner_id, int risk_id, bool can_check=true)
        {
            // status: 抵押物是否被其他风控信息使用, check: 抵押物是否被当前风控信息使用
            // 未关联风控信息的抵押物
            var allMortgages =
                from m in Context.li_mortgages
                where m.owner == loaner_id
                orderby m.last_update_time descending
                select new MortgageItem
                {
                    id = m.id,
                    name = m.name,
                    typeName = m.li_mortgage_types.name,
                    valuation = m.valuation,
                    status = (byte)Agp2pEnums.MortgageStatusEnum.Mortgageable,
                    check = false,
                    enable = true
                };
            // 已关联风控信息的抵押物，如果是别的风控信息，禁用；否则可设置关联
            var mortgageInUse =
                (from m in Context.li_mortgages
                 from rm in Context.li_risk_mortgage
                 from r in Context.li_risks
                 where
                     loaner_id == m.owner && m.id == rm.mortgage && rm.risk == r.id  && r.li_projects.Any(
                                            p => p.status >= (int)Agp2pEnums.ProjectStatusEnum.Financing &&
                                            p.status <= (int) Agp2pEnums.ProjectStatusEnum.ProjectRepaying) // 有项目未完成，其他项目就不可以用此项目正在使用的抵押物
                 select new MortgageItem
                 {
                     id = m.id,
                     name = m.name,
                     typeName = m.li_mortgage_types.name,
                     valuation = m.valuation,
                     status = (byte)Agp2pEnums.MortgageStatusEnum.Mortgaged,
                     check = r.id == risk_id,
                     enable = r.id == risk_id
                 }).GroupBy(m => m.id).ToDictionary(m => m.Key, m => m.First()); // 旧数据中可能会有一个抵押物多次绑定多个未完成的风控信息的情况，这里只取第一个

            return !can_check
                ? mortgageInUse.Values.Where(m => m.check).AsQueryable()
                : allMortgages.Select(m => mortgageInUse.ContainsKey(m.id) ? mortgageInUse[m.id] : m);

        }

        /// <summary>
        /// 加载相册
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <param name="splittedIndex"></param>
        /// <param name="request"></param>
        public void LoadAlbum(li_risks model, Agp2pEnums.AlbumTypeEnum type, int splittedIndex, HttpRequest request)
        {
            string[] albumArr = GetSplittedFormValue("hid_photo_name", splittedIndex, request).ToArray();
            string[] remarkArr = GetSplittedFormValue("hid_photo_remark", splittedIndex, request).ToArray();
            Context.li_albums.DeleteAllOnSubmit(Context.li_albums.Where(a => a.risk == model.id && a.type == (int)type));
            if (albumArr != null)
            {
                var preAdd = albumArr.Zip(remarkArr, (album, remark) =>
                {
                    var albumSplit = album.Split('|');
                    return new li_albums
                    {
                        original_path = albumSplit[1],
                        thumb_path = albumSplit[2],
                        remark = remark,
                        add_time = DateTime.Now,
                        li_risks = model,
                        type = (byte)type
                    };
                });
                Context.li_albums.InsertAllOnSubmit(preAdd);
            }
        }

        /// <summary>
        /// 拆分相册，由于从 Request.Form 读到的内容是全部相册合在一起的，
        /// 所以需要加个隐藏字段将他们隔开，再在后台拆分开
        /// </summary>
        /// <param name="inputName"></param>
        /// <param name="splittedIndex"></param>
        /// <returns></returns>
        private IEnumerable<string> GetSplittedFormValue(string inputName, int splittedIndex, HttpRequest request)
        {
            var strings = request.Form.GetValues(inputName);
            var currentRangeIndex = 0;
            foreach (var t in strings) // 有 splitter 的存在，不为 null
            {
                if (t == "splitter")
                {
                    if (currentRangeIndex < splittedIndex)
                    {
                        currentRangeIndex += 1;
                    }
                    else
                    {
                        yield break;
                    }
                }
                else if (currentRangeIndex == splittedIndex)
                {
                    yield return t;
                }
            }
        }
    }
}
