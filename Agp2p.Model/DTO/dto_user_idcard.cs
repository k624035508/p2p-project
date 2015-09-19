using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agp2p.Model.DTO
{
    public class dto_user_idcard
    {
        private string resultcode;

        public string Resultcode
        {
            get { return resultcode; }
            set { resultcode = value; }
        }
        private string reason;

        public string Reason
        {
            get { return reason; }
            set { reason = value; }
        }

        private result result;
        public result Result
        {
            get { return result; }
            set { result = value; }
        }        
    }

    public class result
    {
        private string area;
        public string Area
        {
            get { return area; }
            set { area = value; }
        }
        private string sex;

        public string Sex
        {
            get { return sex; }
            set { sex = value; }
        }
        private string birthday;

        public string Birthday
        {
            get { return birthday; }
            set { birthday = value; }
        }
    }
}

