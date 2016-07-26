using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services;
using Newtonsoft.Json;

namespace Agp2p.Web.UI.Page
{
    public partial class lottery: Web.UI.BasePage
    {
        protected void Page_load(object sender, EventArgs e)
        {
            
        }
        static Random rnd = new Random(); 
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
            var szZp = new List<JiangPin>
            {
                new JiangPin {id = 0, Name = "88", gailv = 0},
                new JiangPin {id = 1, Name = "1688", gailv = 0},
                new JiangPin {id = 2, Name = "18", gailv = 0},
                new JiangPin {id = 3, Name = "88888", gailv = 0},
                new JiangPin {id = 4, Name = "恭喜您，抽中1积分", gailv = 100},
                new JiangPin {id = 5, Name = "16888", gailv = 0},
                new JiangPin {id = 6, Name = "8", gailv = 0},
                new JiangPin {id = 7, Name = "8888", gailv = 0},
                new JiangPin {id = 8, Name = "188", gailv = 0},
                new JiangPin {id = 9, Name = "888", gailv = 0},
                new JiangPin {id = 10, Name = "2888", gailv = 0},
                new JiangPin {id = 11, Name = "0", gailv = 0},
            };
            int id = chouJiang(szZp).id;
            result = szZp[id].id;
            string name = szZp[id].Name;
            return JsonConvert.SerializeObject(new {name = name, result = result.ToString()});
        }

        private static JiangPin chouJiang(List<JiangPin> szZp)
        {
            return (from x in Enumerable.Range(0, 1000000)
                //模拟抽奖
                let sjcp = szZp[rnd.Next(szZp.Count())]
                let zgz = rnd.Next(0, 1000)
                where zgz < sjcp.gailv
                select sjcp).First();
        }
    }
}
