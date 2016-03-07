using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Core.AutoLogic;

namespace Agp2p.Test
{
    public static class Common
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);
        
        public static void SetSystemTime(DateTime cSharpTime)
        {
            var st = new SYSTEMTIME
            {
                wYear = (short) cSharpTime.Year,
                wMonth = (short) cSharpTime.Month,
                wDay = (short) cSharpTime.Day,
                wHour = (short) cSharpTime.Hour,
                wMinute = (short) cSharpTime.Minute,
                wSecond = (short) cSharpTime.Second,
                wMilliseconds = (short) cSharpTime.Millisecond
            };

            SetSystemTime(ref st); // invoke this method.
        }

        public static void AutoRepaySimulate(DateTime runAt)
        {
            AutoRepay.GenerateHuoqiRepaymentTask(false);
            AutoRepay.DoRepay(false);
            AutoRepay.HuoqiClaimTransferToCompanyWhenNeeded(false);
            AutoRepay.DoHuoqiProjectWithdraw(false, runAt);
        }
    }
}
