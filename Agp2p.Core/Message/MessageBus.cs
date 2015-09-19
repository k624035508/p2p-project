using Agp2p.Core.ActivityLogic;
using Agp2p.Core.AutoLogic;
using Agp2p.Core.InitLogic;
using Agp2p.Core.NotifyLogic;
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
            FinancingTimeout.DoSubscribe();
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
