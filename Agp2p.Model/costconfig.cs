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

        private decimal _loan_fee_rate = 0;
        private decimal _loan_fee_rate_bank = 0;
        private decimal _bond_fee_rate = 0;
        private decimal _bond_fee_rate_bank = 0;
        private decimal _earlier_pay = 0;
        private decimal _overtime_pay = 0;
        private decimal _overtime_cost = 0;
        private decimal _overtime_cost2 = 0;
        private decimal _overtime_cost_bank = 0;
        private decimal _withdraw = 0;
        private decimal _static_withdraw = 0;
        private decimal _recharge_lowest = 0;

        public decimal loan_fee_rate
        {
            get
            {
                return _loan_fee_rate;
            }

            set
            {
                _loan_fee_rate = value;
            }
        }

        public decimal loan_fee_rate_bank
        {
            get
            {
                return _loan_fee_rate_bank;
            }

            set
            {
                _loan_fee_rate_bank = value;
            }
        }

        public decimal bond_fee_rate
        {
            get
            {
                return _bond_fee_rate;
            }

            set
            {
                _bond_fee_rate = value;
            }
        }

        public decimal bond_fee_rate_bank
        {
            get
            {
                return _bond_fee_rate_bank;
            }

            set
            {
                _bond_fee_rate_bank = value;
            }
        }

        public decimal earlier_pay
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

        public decimal overtime_pay
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

        public decimal overtime_cost
        {
            get
            {
                return _overtime_cost;
            }

            set
            {
                _overtime_cost = value;
            }
        }

        public decimal overtime_cost2
        {
            get
            {
                return _overtime_cost2;
            }

            set
            {
                _overtime_cost2 = value;
            }
        }

        public decimal overtime_cost_bank
        {
            get
            {
                return _overtime_cost_bank;
            }

            set
            {
                _overtime_cost_bank = value;
            }
        }

        public decimal withdraw
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

        public decimal static_withdraw
        {
            get { return _static_withdraw; }
            set { _static_withdraw = value; }
        }

        public decimal recharge_lowest
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
