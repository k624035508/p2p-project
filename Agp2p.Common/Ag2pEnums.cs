using System;
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
            [Description("推荐")]
            Recommend = 2,
            [Description("信用保障")]
            CreditGuarantee = 4,
            [Description("火爆")]
            Hot = 8,
            [Description("新手体验标")]
            Trial = 16,
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
            [Description("作废")]
            FinancingApplicationCancel = 0,
            // 发标前 (前台不可见)
            /// <summary>
            /// 借款申请待提交
            /// </summary>
            [Description("待提交")]
            FinancingApplicationUncommitted = 1,
            /// <summary>
            /// 借款申请审核中
            /// </summary>
            [Description("审核中")]
            FinancingApplicationChecking = 2,
            /// <summary>
            /// 借款申请审核不通过
            /// </summary>
            [Description("审核失败")]
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
            ProjectRepaying = 30, // 根据 还款计划 判断是否逾期未还，再决定是否垫付
            /// <summary>
            /// 逾期的借款最后完成都视为坏账
            /// </summary>
            [Description("坏账")]
            BadDebt = 31,

            // 还款完成
            /// <summary>
            /// 按时还款完成
            /// </summary>
            [Description("已完成")]
            RepayCompleteIntime = 40,
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
            [Description("天")]
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
            DengEr = 20,
            /// <summary>
            /// 到期还本付息 
            /// </summary>
            [Description("到期还本付息")]
            DaoQi = 30,
            [Description("每日收益")]
            HuoQi = 40
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

            // 借款人相关
            [Description("向借款人放款")]
            LoanerMakeLoan = 10,
            [Description("收取借款人还款")]
            GainLoanerRepay = 11,
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
            [Description("新手体验券")]
            Trial = 2,
            [Description("被推荐人首次投资奖励推荐人")]
            RefereeFirstTimeProfitBonus = 3,
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
            // 投资者 相关
            [Description("投标")]
            Invest = 1,
            [Description("返还给投资者")]
            RepayToInvestor = 2,
            [Description("返还逾期罚息")]
            RepayOverdueFine = 3,
            [Description("活期项目提现")]
            HuoqiProjectWithdraw = 4,
            [Description("中间人垫付利息")]
            AgentPaidInterest = 5,
            [Description("中间人收回垫付利息")]
            AgentGainPaidInterest = 6,
            [Description("中间人收回活期债权")]
            AgentRecaptureHuoqiClaims = 7,
            [Description("自动续投")]
            AutoInvest = 8,
            [Description("债权转出")]
            ClaimTransferredOut = 9,
            [Description("债权买入")]
            ClaimTransferredIn = 10,
        }

        /// <summary>
        /// 项目交易状态
        /// </summary>
        public enum ProjectTransactionStatusEnum
        {
            [Description("进行中")]
            Pending = 1,
            [Description("成功")]
            Success = 2,
            [Description("撤销")]
            Rollback = 3,
        }

        /// <summary>
        /// 还款计划状态
        /// </summary>
        public enum RepaymentStatusEnum
        {
            [Description("待还款")]
            Unpaid = 1,
            [Description("作废")] // 提前还款后，某些还款计划可能作废
            Invalid = 2,
            [Description("逾期")]
            OverTime = 3,
            [Description("已手动还款")]
            ManualPaid = 10,
            [Description("已自动还款")]
            AutoPaid = 11,
            [Description("提前还款")]
            EarlierPaid = 20,
            [Description("逾期已还")]
            OverTimePaid = 30
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
            [Description("投资撤销")]
            InvestorRefund = 12,
            [Description("活期推迟投资成功")]
            DelayInvestSuccess = 13,

            [Description("利息收益")]
            RepaidInterest = 20,
            [Description("返还本金")]
            RepaidPrincipal = 21,
            [Description("返还本息")]
            RepaidPrincipalAndInterest = 22,
            [Description("返还逾期罚息")]
            RepaidOverdueFine = 23,
            [Description("活期项目提现")]
            HuoqiProjectWithdrawSuccess = 24,
            [Description("中间人垫付利息")]
            AgentPaidInterest = 25,
            [Description("中间人收回垫付利息")]
            AgentGainPaidInterest = 26,
            [Description("中间人收回活期债权")]
            AgentRecaptureHuoqiClaims = 27,
            [Description("自动续投")]
            AutoInvest = 28,
            [Description("债权转出")]
            ClaimTransferredOut = 29,
            [Description("债权买入")]
            ClaimTransferredIn = 30,
            [Description("债权买入成功")] /* 指的是定期债权 */
            ClaimTransferredInSuccess = 31,
            [Description("债权买入失败")] /* 指的是定期债权 */
            ClaimTransferredInFail = 32,

            [Description("获得金钱待确认")]
            Gaining = 40,
            [Description("扣取金钱待确认")]
            Losting = 41,
            [Description("获得金钱")]
            GainConfirm = 42,
            [Description("扣取金钱")]
            LostConfirm = 43,
            [Description("取消获得金钱")]
            GainCancel = 44,
            [Description("取消扣取金钱")]
            LostCancel = 45,

            // 借款人相关
            [Description("向借款人放款成功")]
            LoanerMakeLoanSuccess = 50,
            [Description("收取借款人还款成功")]
            LoanerRepaySuccess = 51,
        }

        public enum OfflineTransactionTypeEnum
        {
            // 预计收入
            [Description("预计平台服务费")]
            SumManagementFeeOfLoanning = 1,
            [Description("预计风险保证金")]
            SumBondFee = 2,

            // 收入
            [Description("平台服务费")]
            ManagementFeeOfLoanning = 10,
            [Description("逾期管理费")]
            ManagementFeeOfOverTime = 11,
            [Description("风险保证金")]
            BondFee = 12,
            [Description("债权转让管理费")]
            StaticClaimTransfer = 13,

            // 支出
            [Description("充值手续费")]
            ReChangeFee = 20
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
            [Description("汇潮网银支付")]
            Ecpss = 4,
            [Description("汇潮快捷支付")]
            EcpssQ = 5,
            [Description("丰付网银支付")]
            Sumapay = 6,
            [Description("丰付一键支付")]
            SumapayQ = 7,
        }

        /// <summary>
        /// 项目状态查询枚举
        /// </summary>
        public enum ProjectStatusQueryTypeEnum
        {
            [Description("全部")]
            All = 0,
            [Description("可投标")]
            Financing = 1,
            [Description("已满标")]
            FinancingSuccess = 2,
            [Description("还款中")]
            ProjectRepaying = 3,
            [Description("还款完成")]
            ProjectRepayComplete = 4,
        }

        /// <summary>
        /// 年化利率筛选枚举
        /// </summary>
        public enum InterestRateTypeEnum
        {
            [Description("全部")]
            All = 0,
            [Description("6%以下")]
            LessThanSix = 1,
            [Description("6-10%")]
            SixToTen = 2,
            [Description("10-15%")]
            TenToFifteen = 3,
        }

        /// <summary>
        /// 借款期限筛选枚举
        /// </summary>
        public enum RepaymentTermEnum
        {
            [Description("全部")]
            All = 0,
            [Description("1个月以下")]
            LessThanOneMonth = 1,
            [Description("1-3个月")]
            OneToThreeMonth = 2,
            [Description("3-6个月")]
            ThreeToSixMonth = 3,
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
            [Description("回款中")]
            Repaying = 2,
            [Description("已完成")]
            RepayComplete = 3,
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
        /// 我的投资 图表查询类型
        /// </summary>
        public enum ChartQueryEnum
        {
            [Description("累计投资")]
            TotalInvestment = 0,
            [Description("在投本金")]
            InvestingMoney = 1,
            [Description("已收本金")]
            RepaidInvestment = 2,
            [Description("累计收益")]
            AccumulativeProfit = 3,
            [Description("待收益")]
            ProfitingMoney = 4,
            [Description("已收益")]
            TotalProfit = 5,
            
        }

        /// <summary>
        /// 回款计划查询状态枚举
        /// </summary>
        public enum MyRepaymentQueryTypeEnum
        {
            [Description("未回款")]
            Unpaid = 1,
            [Description("已回款")]
            Paid = 2,
        }

        /// <summary>
        /// 用户消息状态筛选枚举
        /// </summary>
        public enum UserMessageReadStatusEnum
        {
            [Description("未读消息")]
            Unread = 0,
            [Description("已读消息")]
            Read = 1,
        }

        /// <summary>
        /// 已禁用的提醒的类别，数据库没有数据设置时代表需要发送提醒
        /// </summary>
        public enum DisabledNotificationTypeEnum
        {
            [Description("充值成功-站内消息")]
            ChargeSuccessForUserMsg = 1,
            [Description("提现申请-站内消息")]
            WithdrawApplyForUserMsg = 10,
            [Description("项目回款-站内消息")]
            ProjectRepaidForUserMsg = 20,
            [Description("投资成功-站内消息")]
            InvestSuccessForUserMsg = 30,
            [Description("奖券过期提醒-站内消息")]
            LotteryWillExpireForUserMsg = 40,

            [Description("充值成功-短信")]
            ChargeSuccessForSms = 1001,
            [Description("提现申请-短信")]
            WithdrawApplyForSms = 1010,
            [Description("项目回款-短信")]
            ProjectRepaidForSms = 1020,
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
        /// 丰付支付银行代码枚举
        /// </summary>
        public enum SumapayBankCodeEnum
        {
            [Description("中国工商银行")]
            icbc = 1,
            [Description("中国建设银行")]
            ccb = 2,
            [Description("中国农业银行")]
            abc = 3,
            [Description("招商银行")]
            cmb = 4,
            [Description("交通银行")]
            comm = 5,
            [Description("中国银行")]
            boc = 6,
            [Description("中国光大银行")]
            ceb = 7,
            [Description("中国民生银行")]
            cmsb = 8,
            [Description("兴业银行")]
            cib = 9,
            [Description("中信银行")]
            cncb = 10,
            [Description("广发银行")]
            cgb = 11,
            [Description("浦发银行")]
            spdb = 12,
            [Description("平安银行")]
            pab = 13,
            [Description("华夏银行")]
            hxb = 14,
            [Description("中国邮政储蓄银行")]
            psbc = 15
        }

        /// <summary>
        /// 管理员消息来源枚举
        /// </summary>
        public enum ManagerMessageSourceEnum
        {
            [Description("全部")]
            All = 0,
            [Description("新用户注册")]
            NewUserRegisted = 1,
            [Description("用户充值成功")]
            UserRechargeSuccess = 2,
            [Description("用户提现申请")]
            UserWithdrawApply = 3,
            [Description("用户投标")]
            UserInvested = 4,
            [Description("项目满标")]
            ProjectFinancingSuccess = 5,
            [Description("项目超时")]
            ProjectFinancingTimeout = 6,
            [Description("项目截标")]
            ProjectFinancingSuccessEvenTimeout = 7,
            [Description("项目流标")]
            ProjectFinancingFail = 8,
            [Description("项目回款")]
            ProjectRepaid = 9
        }

        public enum ClaimStatusEnum
        {
            [Description("未转让")]
            Nontransferable = 1,
            [Description("可转让")] /* 被中间人持有，可以被活期买入 */
            Transferable = 2,
            [Description("需要转让")] // 标记为提现中的债权
            NeedTransfer = 3,
            [Description("完成")]
            Completed = 10,
            [Description("已完成未回款")] // 提现 T + 1，债权在提现后被完成，则设置为这个状态（现在没有使用）
            CompletedUnpaid = 11,
            [Description("已转让")]
            Transferred = 20,
            [Description("已转让未回款")] // 提现 T + 1，债权在提现后被转让，则设置为这个状态
            TransferredUnpaid = 21,
            [Description("失效")] // 项目流标 / 投资退款 / 债权拆分（提现时发生，旧债权的本金不变，标记为失效，创建一个新的债权，债权编号不变）
            Invalid = 30,
        }

        public enum LoanerStatusEnum
        {
            [Description("正常")]
            Normal = 1,
            [Description("待审核")]
            Pending = 10,
            [Description("审核不通过")]
            PendingFail = 11,
            [Description("禁用")]
            Disable = 20
        }

        public enum SumapayRequestEnum
        {
            [Description("等待响应")]
            Waiting = 1,
            [Description("已失败")]
            Fail = 2,
            [Description("已完成")]
            Complete = 3
        }

        public enum SumapayResponseEnum
        {
            [Description("已返回")]
            Return = 1,
            [Description("无效")]
            Invalid = 2,
            [Description("已完成")]
            Complete = 3
        }

        public enum SumapayApiEnum
        {
            [Description("实名开户")] URegi = 1,
            [Description("实名认证")] UAuth = 2,
            [Description("用户激活")] Activ = 3,
            [Description("账户管理")] Accou = 4,
            [Description("自动投标续约")] AtBid = 5,
            [Description("取消自动投标")] ClBid = 6,
            [Description("开通存管账户自动还款")] AcReO = 7,
            [Description("开通银行账户自动还款")] AbReO = 8,
            [Description("关闭用户自动还款")] ClRep = 9,
            [Description("网银充值")] WeRec = 10,
            [Description("一键充值")] WhRec = 11,
            [Description("投标普通项目")] MaBid = 12,
            [Description("投标集合项目")] McBid = 13,
            [Description("自动投标普通项目")] AmBid = 14,
            [Description("自动投标集合项目")] AcBid = 15,
            [Description("撤标普通项目")] CaPro = 16,
            [Description("撤标集合项目")] CoPro = 17,
            [Description("流标普通项目")] RePro = 18,
            [Description("普通项目放款")] ALoan = 19,
            [Description("集合项目放款")] CLoan = 20,
            [Description("用户提现")] Wdraw = 21,
            [Description("存管账户还款(普通项目)")] MaRep = 22,
            [Description("存管账户还款(集合项目)")] McRep = 23,
            [Description("银行账户还款(普通项目)")] BaRep = 24,
            [Description("银行账户还款(集合项目)")] BcRep = 25,
            [Description("自动还款(普通项目)")] AcRep = 26,
            [Description("自动还款(集合项目)")] AbRep = 27,
            [Description("本息到账(普通项目)")] RetPt = 28,
            [Description("本息到账(集合项目)")] RetCo = 29,
            [Description("债权转让")] CreAs = 30,
            [Description("单笔付款至个人")] TranU = 31,
            [Description("用户签约银行卡查询")] QuBan = 32,
            [Description("商户项目查询")] QuPro = 33,

            [Description("移动端实名开户")]
            URegM = 101,
            [Description("移动端账户管理")]
            AccoM = 104,
            [Description("移动端移动端充值")]
            WhReM = 111,
            [Description("移动端提现")]
            WdraM = 121,
            [Description("移动端投标普通项目")]
            MaBiM = 112,
            [Description("移动端投标集合项目")]
            McBiM = 113,
            [Description("移动端债权转让")]
            CreAM = 130
        }

        public enum StaticClaimQueryEnum
        {
            [Description("可转让")]
            Profiting = 1,
            [Description("转让中")]
            Transfering = 2,
            [Description("已转让")]
            Transferred = 3,
        }

        public enum HuoqiClaimQueryEnum
        {
            [Description("全部")]
            All = 0,
            [Description("收益中")]
            Profiting = 1,
            [Description("转让中")]
            Transfering = 2,
            [Description("已结束")]
            Completed = 3,
        }

        public enum HuoqiTransactionQueryEnum
        {
            [Description("全部")]
            All = 0,
            [Description("收益")]
            Profiting = 1,
            [Description("买入")]
            BuyIn = 2,
            [Description("转出")]
            TransferOut = 3,
        }
        public enum MyLoanQueryTypeEnum
        {
            [Description("申请中")]
            Applying = 0,
            [Description("借款中")]
            Loaning = 1,
            [Description("还款中")]
            Repaying = 2,
            [Description("已还款")]
            Repaid = 3,
        }
    }
}
