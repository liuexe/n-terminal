using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Global
{
    public static class Data
    {
        public static Dictionary<string, Mt4.Mt4Terminal> TerminalPool = new Dictionary<string, Mt4.Mt4Terminal>();
        public static Queue<string> OfflineQueue = new Queue<string>();
        public static object OfflineQueueLocker = new object();
        public static List<string> Billing = new List<string>();
        public static Dictionary<int, string> Magics = new Dictionary<int, string>();
        public static bool Debug = false;
        public static Task TaskWatchDog;
        public static void WatchDog()
        {
            while (true)
            {
                lock (OfflineQueueLocker)
                {
                    if (OfflineQueue.Count > 0)
                    {
                        string key = OfflineQueue.Dequeue();
                        if (TerminalPool.ContainsKey(key))
                        {
                            if (TerminalPool[key].IsStopped())
                            {
                                TerminalPool[key].Start();
                            }
                            else
                            {
                                TerminalPool[key].Restart();
                            }
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        public static string GetMaster(int magic)
        {
            if (Magics.ContainsKey(magic))
            {
                return Magics[magic];
            }
            else{
                return null;
            }
        }
    }

    public static class GWT
    {
        public static Dictionary<string, byte> Local = new Dictionary<string, byte>() { { "ams", 0x01 }, { "lon", 0x07 }, { "ny", 0x03 } };
        public static int FAST = 3;
        static int fastIndex = 2;
        static byte[] gwt = new byte[] { 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13 };
        static int[] ping = new int[gwt.Length];   
        public static void Ping(string server)
        {

            int delay = -1;
            Mt4.Mt4Terminal tester = new Mt4.Mt4Terminal("","ping", "", server,"ny" , Mt4.TerminalRole.Master, Global.Data.Debug);
            for (int k = 0; k < 10; k++)
            {
                for (int i = 0; i < gwt.Length; i++)
                {
                    ping[i] = k == 0 ? tester.Ping(gwt[i]) : (ping[i] + tester.Ping(gwt[i])) / 2;
                    ping[i] = tester.Ping(gwt[i]);
                    int d = delay;
                    delay = delay < 0 ? ping[i] < 0 ? delay : ping[i] : ping[i] > 0 && delay > ping[i] ? ping[i] : delay;
                    fastIndex = delay == d ? fastIndex : i;
                }
                Thread.Sleep(3000);
            }
            tester.Stop();
            FAST = gwt[fastIndex];
        }
    }
}
