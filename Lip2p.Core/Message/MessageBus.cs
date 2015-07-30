using Lip2p.Core.ActivityLogic;
using Lip2p.Core.AutoLogic;
using Lip2p.Core.InitLogic;
using Lip2p.Core.NotifyLogic;
using TinyMessenger;

namespace Lip2p.Core.Message
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
            ScheduleAnnounce.DoSubscribe();

            // Activity
            RegHongBao.DoSubscribe();
            InviterBonus.DoSubscribe();
            TrialActivity.DoSubscribe();
            DailyProjectActivity.DoSubscribe();

            // Notify
            InvestAnnounce.DoSubscribe();
            RepayAnnounce.DoSubscribe();
        }
    }
}
