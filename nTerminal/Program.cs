using System;
using System.Text;
using System.Threading.Tasks;
using Mt4;
using System.IO;

namespace nTerminal
{
    class Program
    {
        static void Main(string[] args)
        {

            //Global.GWT.Ping(Global.Data.Config.PingServer);

            string[] accounts = File.ReadAllLines("accounts.txt");
            //account.txt
            //one account per line:
            //account,password,ICMarketsSC-Live06,master/slave
            foreach (string a in accounts)
            {
                if (a.Length > 10)
                {
                    string[] acc = a.Split(',');
                    TerminalRole role;
                    if (acc[5] == "master")
                    {
                        role = TerminalRole.Master;
                    }
                    else
                    {
                        role = TerminalRole.Trader;
                    }
                    Mt4Terminal client = new Mt4Terminal(acc[0], acc[1], acc[2], acc[3], acc[4], role, Global.Data.Debug);
                    Global.Data.TerminalPool.Add(acc[1], client);
                }

            }

            foreach (Mt4Terminal client in Global.Data.TerminalPool.Values)
            {
                if (client.IsStopped())
                {
                    client.Start();
                    System.Threading.Thread.Sleep(100);
                }
            }

            while (true)
            {
                if (Console.ReadKey().KeyChar.Equals('c'))
                {
                    Console.WriteLine("\nCommand:");
                    string line = Console.ReadLine();
                    if (line == "quit")
                    {
                        break;
                    }
                }
            }
            Console.WriteLine("bye");
        }       
    }
}
