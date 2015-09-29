using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agp2p.Model
{
    /// <summary>
    /// 平台费用配置
    /// </summary>
    [Serializable]
    public class costconfig
    {
        public costconfig()
        { }
        private float _earlier_pay = 0;
        private float _overtime_pay = 0;
        private float _withdraw = 0;
        private float _recharge_lowest = 0;

        public float earlier_pay
        {
            get
            {
                return _earlier_pay;
            }

            set
            {
                _earlier_pay = value;
            }
        }

        public float overtime_pay
        {
            get
            {
                return _overtime_pay;
            }

            set
            {
                _overtime_pay = value;
            }
        }

        public float withdraw
        {
            get
            {
                return _withdraw;
            }

            set
            {
                _withdraw = value;
            }
        }

        public float recharge_lowest
        {
            get
            {
                return _recharge_lowest;
            }

            set
            {
                _recharge_lowest = value;
            }
        }
    }
}
