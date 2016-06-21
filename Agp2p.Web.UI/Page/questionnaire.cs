using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Web;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Linq;
using System.Net;
using System.Web.Script.Services;
using System.Web.Services;
using Agp2p.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Agp2p.Web.UI.Page
{
     public partial class questionnaire :Web.UI.BasePage
    {
        private static decimal SumOfScore(Agp2pEnums.QuestionnaireEnum questionnaire, List<string> results)
        {
            switch (questionnaire)
            {
                case Agp2pEnums.QuestionnaireEnum.LenderRiskAssessmentTest:
                    return results.Select(s => s.ToUpper()).Aggregate(0m, (sum, str) =>
                    {
                        switch (str)
                        {
                            case "A": return sum + 1;
                            case "B": return sum + 2;
                            case "C": return sum + 3;
                            case "D": return sum + 4;
                            default:
                                throw new NotImplementedException();
                        }
                    });
                default:
                    throw new NotImplementedException();
            }
        }

        protected string GetQuestionnaireResult(int questionnaireId)
         {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            var score = 0;
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            // 返回的格式：{"answer": ["A", "A&B", ...], "score": 999}

            var answers = userInfo.li_questionnaire_results.Where(q => q.questionnaireId == questionnaireId)
                .OrderBy(q => q.questionId).Select(q => q.answer).ToList();

            score = Convert.ToInt32(SumOfScore((Agp2pEnums.QuestionnaireEnum) questionnaireId, answers));
            return score.ToString();
         
        }
    }
}
