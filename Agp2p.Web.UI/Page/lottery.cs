using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;

namespace Agp2p.Web.UI.Page
{
    public partial class lottery: Web.UI.BasePage
    {
        protected List<dt_user_point_log> JifenRecord()
        {
            var context = new Agp2pDataContext();
            return context.dt_user_point_log.Where(l => l.type==(int)Agp2pEnums.PointEnum.LotteryGet).OrderByDescending(l => l.add_time).Take(9).ToList();
        }

        static Random rmd = new Random(); 
        //奖品实体类
        public class JiangPin
        {
            public int id;//奖品ID
            public string Name;//奖品名字
            public int gailv;//奖品概率
        }

        [WebMethod]
        public static string ZhuanPan()
        {
            int result;
            //定义奖品和概率
            var award = new List<JiangPin>
            {
                new JiangPin {id = 0, Name = "恭喜您，抽中88积分", gailv = 45},
                new JiangPin {id = 1, Name = "恭喜您，抽中688积分", gailv = 5},
                new JiangPin {id = 2, Name = "恭喜您，抽中18积分", gailv = 20},
                new JiangPin {id = 3, Name = "恭喜您，抽中68积分", gailv = 10},
                new JiangPin {id = 4, Name = "恭喜您，抽中188积分", gailv = 8},
                new JiangPin {id = 5, Name = "恭喜您，抽中888积分", gailv = 6},
                new JiangPin {id = 6, Name = "恭喜您，抽中288积分", gailv = 6},
                new JiangPin {id = 7, Name = "很遗憾，本次抽奖没有获得积分", gailv = 0},
            };
            int id = chouJiang(award).id;
            result = award[id].id;
            string name = award[id].Name;
            return JsonConvert.SerializeObject(new {name = name, result = result.ToString()});
        }

        private static JiangPin chouJiang(List<JiangPin> szZp)
        {
            return (from x in Enumerable.Range(0, 1000000)
                //模拟抽奖
                let sjcp = szZp[rmd.Next(szZp.Count())]
                let zgz = rmd.Next(0, 1000)
                where zgz < sjcp.gailv
                select sjcp).First();
        }
    }
}
