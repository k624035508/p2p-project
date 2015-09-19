﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Agp2p.Common
{
    public class Agp2pEnums
    {       
        /// <summary>
        /// 项目标识
        /// </summary>
        public enum ProjectTagEnum
        {
            [Description("特约标")]
            Ordered = 1,
            [Description("火爆")]
            Hot = 2,
            [Description("新手体验标")]
            Trial = 3,
            [Description("天标")]
            DailyProject = 4
        }

        public enum LoanTypeEnum
        {
            [Description("企业")]
            Company = 10,
            [Description("个人")]
            Personal = 20,
            [Description("债权")]
            Creditor = 30
        }

        public enum GuarantorTypeEnum
        {
            [Description("小额贷款公司")]
            MicrofinanceCompany = 1,
            [Description("担保公司")]
            GuaranteeCompany = 2,
        }

        /// <summary>
        /// 流程状态
        /// </summary>
        public enum ProjectStatusEnum
        {
            // 发标前 (前台不可见)
            /// <summary>
            /// 借款申请待提交
            /// </summary>
            [Description("待提交")]
            FinancingApplicationUncommitted = 1,
            /// <summary>
            /// 借款申请审核中
            /// </summary>
            [Description("初审中")]
            FinancingApplicationChecking = 2,
            /// <summary>
            /// 借款申请审核不通过
            /// </summary>
            [Description("初审失败")]
            FinancingApplicationFail = 3,
            /// <summary>
            /// 借款申请审核通过
            /// </summary>
            [Description("待发标")]
            FinancingApplicationSuccess = 4,
            /// <summary>
            /// 融资失败
            /// </summary>
            [Description("流标")]
            FinancingFail = 5,

            // 上线募集
            [Description("定时发标")]
            FinancingAtTime = 10,
            /// <summary>
            /// 融资中
            /// </summary>
            [Description("借款中")]
            Financing = 11,

            // 募集结束
            /// <summary>
            /// 融资超时 等待决定是由平台满标还是流标
            /// </summary>
            [Description("已过期")] 
            FinancingTimeout = 20,
            /// <summary>
            /// 复审，准备放款给融资者
            /// </summary>
            [Description("满标")]
            FinancingSuccess = 21,

            // 还款中
            [Description("还款中")]
            ProjectRepaying = 30,
            /// <summary>
            /// 到期未还款完成 等待决定是否垫付
            /// </summary>
            [Description("逾期未还")]
            NotRepayCompleteIntime = 31,
            /// <summary>
            /// 逾期的借款最后完成都视为坏账
            /// </summary>
            [Description("坏账")]
            BadDebt = 32,

            // 还款完成
            /// <summary>
            /// 按时还款完成
            /// </summary>
            [Description("已完成")]
            RepayCompleteIntime = 40,
            /// <summary>
            /// 提前还款完成
            /// </summary>
            [Description("提前还款")]
            RepayCompleteEarlier = 41,
            [Description("到期未还款已垫付")]
            AdvancePayForFinancer = 42,
            [Description("垫付后还款完成")]
            RepayCompleteDelay = 43,
            [Description("坏账后还款完成")]
            BadDebtRepayComplete = 44,
        }

        /// <summary>
        /// 人员性别
        /// </summary>
        public enum PersonSexTypeEnum
        {
            /// <summary>
            /// 保密
            /// </summary>
            [Description("保密")]
            Secrecy = 1,
            /// <summary>
            /// 男
            /// </summary>
            [Description("男")]
            Male = 2,
            /// <summary>
            /// 女
            /// </summary>
            [Description("女")]
            Female = 3
        }

        /// <summary>
        /// 婚姻状况
        /// </summary>
        public enum MaritalStatusEnum
        {
            [Description("未婚")]
            Unmarried = 1,
            [Description("已婚")]
            Married = 2,
            [Description("离婚")]
            Divorce = 3,
            [Description("丧偶")]
            Widowed = 4
        }

        /// <summary>
        /// 标的物使用状态
        /// </summary>
        public enum MortgageStatusEnum
        {
            [Description("已抵押")]
            Mortgaged = 1,
            [Description("可抵押")]
            Mortgageable = 2,
        }

        /// <summary>
        /// 还款期限单位
        /// </summary>
        public enum ProjectRepaymentTermSpanEnum
        {
            /// <summary>
            /// 年
            /// </summary>
            [Description("年")]
            Year = 10,
            /// <summary>
            /// 月
            /// </summary>
            [Description("个月")]
            Month = 20,
            /// <summary>
            /// 日
            /// </summary>
            [Description("日")]
            Day = 30
        }

        /// <summary>
        /// 还款方式
        /// </summary>
        public enum ProjectRepaymentTypeEnum
        {
            /// <summary>
            /// 先息后本
            /// </summary>
            [Description("先息后本")]
            XianXi = 10,
            /// <summary>
            /// 等额本息
            /// </summary>
            [Description("等额本息")]
            DengEr = 20
        }

        /// <summary>
        /// 银行交易类型
        /// </summary>
        public enum BankTransactionTypeEnum
        {
            [Description("充值")]
            Charge = 1,
            [Description("提现")]
            Withdraw = 2,
        }

        /// <summary>
        /// 银行交易状态
        /// </summary>
        public enum BankTransactionStatusEnum
        {
            [Description("进行中")]
            Acting = 1,
            [Description("成功")]
            Confirm = 2,
            [Description("取消")]
            Cancel = 3,
        }

        /// <summary>
        /// 系统活动交易类型
        /// </summary>
        public enum ActivityTransactionTypeEnum
        {
            [Description("获得金钱")]
            Gain = 1,
            [Description("扣取金钱")]
            Lost = 2,
        }

        /// <summary>
        /// 系统活动交易状态
        /// </summary>
        public enum ActivityTransactionStatusEnum
        {
            [Description("进行中")]
            Acting = 1,
            [Description("成功")]
            Confirm = 2,
            [Description("取消")]
            Cancel = 3,
        }

        /// <summary>
        /// 系统活动的活动类型
        /// </summary>
        public enum ActivityTransactionActivityTypeEnum
        {
            [Description("手工操作/冲正")]
            ManualOperation = 1,
            [Description("投资激活红包")]
            HongBaoActivation = 2,
            [Description("被推荐人首次投资奖励推荐人")]
            RefereeFirstTimeProfitBonus = 3,
            [Description("投资红包奖励")]
            InvestBonus = 4,
            [Description("新手体验券")]
            Trial = 5,
            [Description("天标券")]
            DailyProject = 6,
        }

        /// <summary>
        /// 银行交易手续费类型
        /// </summary>
        public enum BankTransactionHandlingFeeTypeEnum
        {
            [Description("无需手续费")]
            NoHandlingFee = 1,
            [Description("提现手续费")]
            WithdrawHandlingFee = 2,
            [Description("提现/防套现手续费")]
            WithdrawUnusedMoneyHandlingFee = 3,
        }

        /// <summary>
        /// 项目交易类型
        /// </summary>
        public enum ProjectTransactionTypeEnum
        {
            [Description("投标")]
            Invest = 1,
            [Description("利息收益")]
            RepaidInterest = 2,
            [Description("返还本金")]
            RepaidPrincipal = 3,
            [Description("返还本息")]
            RepaidPrincipalAndInterest = 4,
        }

        /// <summary>
        /// 项目交易状态
        /// </summary>
        public enum ProjectTransactionStatusEnum
        {
            [Description("成功")]
            Success = 1,
            [Description("撤销")]
            Rollback = 2,
        }

        /// <summary>
        /// 还款计划状态
        /// </summary>
        public enum RepaymentStatusEnum
        {
            [Description("待还款")]
            Unpaid = 1,
            [Description("已手动还款")]
            ManualPaid = 2,
            [Description("已自动还款")]
            AutoPaid = 3,
        }

        /// <summary>
        /// 钱包日志类型
        /// </summary>
        public enum WalletHistoryTypeEnum
        {
            [Description("充值待确认")]
            Charging = 1,
            [Description("提现待执行")]
            Withdrawing = 2,
            [Description("充值成功")]
            ChargeConfirm = 3,
            [Description("提现成功")]
            WithdrawConfirm = 4,
            [Description("充值取消")]
            ChargeCancel = 5,
            [Description("提现取消")]
            WithdrawCancel = 6,

            [Description("投资")]
            Invest = 10,
            [Description("项目满标")] // 开始计算待收益金额
            InvestSuccess = 11,
            [Description("申请退款")]
            InvestorRefund = 12,

            [Description("利息收益")]
            RepaidInterest = 20,
            [Description("返还本金")]
            RepaidPrincipal = 21,
            [Description("返还本息")]
            RepaidPrincipalAndInterest = 22,

            [Description("获得金钱待确认")]
            Gaining = 30,
            [Description("扣取金钱待确认")]
            Losting = 31,
            [Description("获得金钱")]
            GainConfirm = 32,
            [Description("扣取金钱")]
            LostConfirm = 33,
            [Description("取消获得金钱")]
            GainCancel = 34,
            [Description("取消扣取金钱")]
            LostCancel = 35,
        }

        /// <summary>
        /// 上传的图片的类型
        /// </summary>
        public enum AlbumTypeEnum
        {
            [Description("照片")]
            Pictures = 1,
            [Description("身份证")]
            IdCard = 2,
            [Description("产权证")]
            PropertyCertificate = 3,
            [Description("他项证")]
            LienCertificate = 4,
            [Description("借款协议")]
            LoanAgreement = 5,
            [Description("抵押合同")]
            MortgageContract = 6,
        }

        /// <summary>
        /// 第三方支付接口枚举
        /// </summary>
        public enum PayApiTypeEnum
        {
            [Description("手工添加")]
            ManualAppend = 1,
            [Description("连连支付")]
            Lianlianpay = 2,
            [Description("宝付")]
            Baofoo = 3,
            [Description("汇潮支付")]
            Ecpss = 4
        }

        /// <summary>
        /// 年化利率筛选枚举
        /// </summary>
        public enum InterestRateTypeEnum
        {
            [Description("全部")]
            all = 0,
            [Description("15%")]
            one = 1,
            [Description("16%")]
            two = 2,
            [Description("17%")]
            three = 3,
            [Description("18%")]
            four = 4
        }

        /// <summary>
        /// 借款期限筛选枚举
        /// </summary>
        public enum RepaymentTermEnum
        {
            [Description("全部")]
            all = 0,
            [Description("1个月")]
            one = 1,
            [Description("2个月")]
            two = 2,
            [Description("3个月")]
            three = 3
        }

        /// <summary>
        /// 金额筛选枚举
        /// </summary>
        public enum AmountTypeEnum
        {
            [Description("全部")]
            all = 0,
            [Description("1-10万")]
            one = 1,
            [Description("10-25万")]
            two = 2,
            [Description("25-50万")]
            three = 3,
            [Description("50万以上")]
            four = 4
        }

        /// <summary>
        /// 交易明细下拉框枚举
        /// </summary>
        public enum TransactionDetailsDropDownListEnum
        {
            [Description("全部")]
            All = 0,
            [Description("充值")]
            Charge = 1,
            [Description("提现")]
            Withdraw = 2,
            [Description("投资")]
            Invest = 3,
            [Description("回款")]
            Repay = 4,
            [Description("其他")]
            Others = 5,
        }

        /// <summary>
        /// 投资列表选择枚举
        /// </summary>
        public enum MyInvestRadioBtnTypeEnum
        {
            [Description("全部")]
            All = 0,
            [Description("投资中")]
            Investing = 1,
            [Description("投资结束")]
            InvestEndding = 2,
            [Description("还款中")]
            Repaying = 3,
            [Description("已完成")]
            RepayComplete = 4,
        }

        /// <summary>
        /// 我的奖券页呈现状态枚举
        /// </summary>
        public enum MyLotteryRadioBtnTypeEnum
        {
            [Description("全部")]
            All = 0,
            [Description("未使用")]
            Unused = 1,
            [Description("已使用")]
            Used = 2,
            [Description("已失效")]
            Invalid = 3,
        }

        /// <summary>
        /// 连连支付银行代码枚举
        /// </summary>
        public enum LianlianpayBankCodeEnum
        {
            [Description("工商银行")]
            ICBC = 1020000,
            [Description("建设银行")]
            CCB = 1050000,
            [Description("农业银行")]
            ABC = 1030000,
            [Description("招商银行")]
            CMB = 3080000,
            [Description("交通银行")]
            BCM = 3010000,
            [Description("中国银行")]
            BOC = 1040000,
            [Description("光大银行")]
            CEB = 3030000,
            [Description("民生银行")]
            CMBC = 3050000,
            [Description("兴业银行")]
            CIB = 3090000,
            [Description("中信银行")]
            CNCB = 3020000,
            [Description("广发银行")]
            GDB = 3060000,
            [Description("浦发银行")]
            SPDB = 3100000,
            [Description("平安银行")]
            PAB = 3070000,
            [Description("华夏银行")]
            HXB = 3040000,
            [Description("中国邮政银行")]
            PSBC = 1000000
        }

        /// <summary>
        /// 汇潮支付银行代码枚举
        /// </summary>
        public enum EcpssBankCodeEnum
        {
            [Description("工商银行")]
            ICBC = 1,
            [Description("建设银行")]
            CCB = 2,
            [Description("农业银行")]
            ABC = 3,
            [Description("招商银行")]
            CMB = 4,
            [Description("交通银行")]
            BOCOM = 5,
            [Description("中国银行")]
            BOCSH = 6,
            [Description("光大银行")]
            CEB = 7,
            [Description("民生银行")]
            CMBC = 8,
            [Description("兴业银行")]
            CIB = 9,
            [Description("中信银行")]
            CNCB = 10,
            [Description("广发银行")]
            GDB = 11,
            [Description("浦发银行")]
            SPDB = 12,
            [Description("平安银行")]
            PAB = 13,
            [Description("华夏银行")]
            HXB = 14,
            [Description("中国邮政银行")]
            PSBC = 15
        }
    }
}