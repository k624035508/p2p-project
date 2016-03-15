using Agp2p.Core.ActivityLogic;
using Agp2p.Core.AutoLogic;
using Agp2p.Core.InitLogic;
using Agp2p.Core.NotifyLogic;
using Agp2p.Core.PayApiLogic;
using Lip2p.Core.ActivityLogic;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class MessageBus
    {
        public static readonly TinyMessengerHub Main = new TinyMessengerHub();

        static MessageBus()
        {
            // Init
            NewUserInit.DoSubscribe();
            UserLogin.DoSubscribe();

            // Business
            TransactionFacade.DoSubscribe();
            AutoRepay.DoSubscribe();
            CheckOverTimePaid.DoSubscribe();
            FinancingTimeout.DoSubscribe();
            ScheduleAnnounce.DoSubscribe();

            // Notify
            InvestAnnounce.DoSubscribe();
            RepayAnnounce.DoSubscribe();
            BankTransactionNotify.DoSubscribe();
            ManagerNotifier.DoSubscribe();

            // Activity
            TrialActivity.DoSubscribe();
            InviterBonus.DoSubscribe();

            // PayApi
            RequestApiHandle.DoSubscribe();//所有支付请求
            UserHandle.DoSubscribe();//用户接口响应处理
            BankTransHandle.DoSubscribe();//资金账户接口响应
            ProjectTransHandle.DoSubscribe();//项目接口响应
        }
    }
}
