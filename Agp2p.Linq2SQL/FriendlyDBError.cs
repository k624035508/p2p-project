using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Agp2p.Linq2SQL
{
    public class FriendlyDBError
    {
        private static Dictionary<string, string> translate = new Dictionary<string, string>()
        {
            {"risks", "风控信息表"},
            {"mortgages", "抵押信息表"},
            {"loaner", "借贷人"},
            {"owner", "抵押者"},
            {"creditor", "债权人"}
        }; 
        public static string HandleDeleteError(Exception ex)
        {
            var msg = ex.Message;
            var regex = new Regex(@"DELETE.+FK.+dbo.li_(\w+).+column '(.+)'");
            var matcher = regex.Match(msg);
            string table = "", column = "";
            translate.TryGetValue(matcher.Groups[1].Value, out table); 
            translate.TryGetValue(matcher.Groups[2].Value, out column);
            return table + " 依赖其中一个 " + column;
        }
    }
}
