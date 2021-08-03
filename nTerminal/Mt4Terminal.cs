using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
namespace Mt4
{
    public enum TerminalRole
    {
        Trader = 0,
        Master = 1,
        Both = 2
    }
    public class Mt4Terminal
    {
        private readonly string Broker;
        private readonly string Account;
        private readonly string Password;
        private readonly string Server;
        private int Gwt;
        private string AccountName;
        private string AccountType;
        private string AccountCurrency;
        private double AccountBalance;
        private int AccountLeverage;
        private TerminalRole Role;
        private NetTool.NetInfo NI;
        private ClientWebSocket WS;
        private int RecvFails;
        private int SendFails;
        private string jE;//, ln, Zk, CC, jr, iS;
        private string keyString;
        private string tokenString;
        private byte[] Ao;
        private byte[] Key;
        private byte[] Token;
        private int DH;
        private ulong Step; 
        private bool Debug;
        private bool NoCookie;
        private bool NoServer;
        private bool Started;
        private bool KeepAlive;
        private bool LoginIncomplete;
        private Dictionary<string, int> SymbolIndex;
        private Dictionary<string, string> SymbolMapping;
        private Dictionary<int,OrderStruct> Positions;
        private Dictionary<int,OrderStruct> Records;
        private Queue<OrderRequest> OrderRequestQueue;
        private Queue<int> MonitorWaitingQueue;
        private Dictionary<int, long> MonitorWaitingTimestamp;
        private object OrderReqQueueLocker;
        private object StartedLocker;
        private object LogLocker;
        private object PositionsLocker;
        private Task ConnectTask;
        private Task MonitorTask;
 
        public Mt4Terminal(string broker,string account, string password, string server, string gwt,TerminalRole role,bool debug)
        {
            this.Broker = broker;
            this.Account = account;
            this.Password = password;
            this.Server = server;
            this.Gwt = Global.GWT.Local.ContainsKey(gwt) ? Global.GWT.Local[gwt] : Global.GWT.FAST;

            this.jE = "13ef13b2b76dd8:5795gdcfb2fdc1ge85bf768f54773d22fff996e3ge75g5:75";
            //this.ln = ":e4dd535gf:ddg7361613d6885fc6841ffd:4g:g498g8266dg:eff33886f738c";
            //this.Zk = "bfddfd:b:c5b5bdd976fbc::86dec9b:bfbc:685cgc7115389";
            //this.CC = "987264ef98b:159fe89dd9bf986fc97ggcd7:27dg95dd28173b45f48b:d8e397";
            //this.jr = "4234g3e3deb5474fg5c62fbd81f8g64e68c7:b:4379g448d797d8g4fdd54de33";
            //this.iS = "f113:g215496df3g477f2g1b3bbg2:82";

            this.NI = new NetTool.NetInfo("*", account+".log", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0");
            this.NI.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            this.NI.AddHeader("Accept-Language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2");
            this.NI.AddHeader("Upgrade-Insecure-Requests", "1");

            this.RecvFails = -1;
            this.SendFails = -1;
            this.WS = new ClientWebSocket();

            this.DH = 1000;
            this.NoCookie = true;
            this.NoServer = true;
            this.LoginIncomplete = true;
            this.Started = false;
            this.KeepAlive = true;
            this.SymbolIndex = new Dictionary<string, int>();
            this.SymbolMapping = new Dictionary<string, string>();
            this.Positions = new Dictionary<int, OrderStruct>();
            this.Records = new Dictionary<int, OrderStruct>();
            this.OrderRequestQueue = new Queue<OrderRequest>();
            this.MonitorWaitingQueue = new Queue<int>();
            this.OrderReqQueueLocker = new object();
            this.LogLocker = new object();
            this.StartedLocker = new object();
            this.PositionsLocker = new object();
            this.Role = role;
            this.MonitorWaitingTimestamp = new Dictionary<int, long>();
            this.SymbolIndex = new Dictionary<string, int>();
            this.Debug = debug;
            this.Step = 0L;
        }
        public string ListAccountInfo()
        {
            string line = string.Format("{0}:{1}@{2} {3}, {4}[{5}] {6} ${7}\r\n", this.Role.ToString(), this.Account, this.Server, this.AccountName, this.AccountType, this.AccountLeverage, this.AccountCurrency, this.AccountBalance);
            return line;
        }
        public void Start()
        {
            try
            {
                this.ConnectTask = Task.Run(new Action(this.Connect));
                this.MonitorTask = Task.Run(new Action(this.PositionMonitor));
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), true);
            }
        }
        public void Restart()
        {
            this.Reset();
            this.Start();
        }
        public void Stop()
        {
            try
            {
                this.KeepAlive = false;
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace, this.Debug);
            }
            this.WsClose();
        }
        public void Reset()
        {
            this.WsClose();
            try
            {
                this.KeepAlive = false;
                while (true)
                {
                    bool w = this.ConnectTask.Status == TaskStatus.RanToCompletion || this.ConnectTask.Status == TaskStatus.Faulted;
                    bool m = this.MonitorTask.Status == TaskStatus.RanToCompletion || this.MonitorTask.Status == TaskStatus.Faulted;
                    if (w && m)
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                this.KeepAlive = true;
                this.WS = new ClientWebSocket();
                lock (this.StartedLocker)
                {
                    this.Started = false;
                }
                this.NoServer = true;
                this.LoginIncomplete = true;
                this.DH = 1000;
                this.RecvFails = 0;
                this.SendFails = 0;
                this.Step = 0L;
                this.Log("Reseted>>>>>>>>>>>>>>>>>", this.Debug);
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), true);
            }
        }
        public bool IsStopped()
        {
            bool flag;
            lock (this.StartedLocker)
            {
                flag = !this.Started;
            }
            return flag;
        }
        public int Ping(byte i)
        {
            try
            {
                if (NoCookie)
                {
                    if (!this.InitCookie())
                    {
                        return -1;
                    }
                }
                ulong t1;
                ulong t2;
                if (this.WsConnect(i))
                {
                    int ret = -1;
                    byte[] pingkey = ByteTool.Hex.ConvertHexStringToBytes(Mt4.Wf(this.jE));
                    byte[] sp = Mt4.Se(null, PacketType.Ping);
                    byte[] pkPing = Mt4.Pack(sp, 1, pingkey);
                    t1 = Convert.ToUInt64((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
                    this.WsSend(pkPing);
                    while (true)
                    {
                        byte[] buffer = new byte[64];
                        if (this.WsReceive(ref buffer))
                        {
                            t2 = Convert.ToUInt64((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
                            uint x = BitConverter.ToUInt32(buffer, 0) + 8;
                            if (x > 23)
                            {
                                byte[] packet = new byte[x + 8];
                                Array.Copy(buffer, 0, packet, 0, 24);
                                byte[] response = Mt4.Unpack(packet, pingkey);
                                if (BitConverter.ToUInt16(response, 2) == (ushort)PacketType.Ping)
                                {
                                    ret = Convert.ToInt32(t2 - t1);
                                    break;
                                }
                            }
                        }
                    }
                    return ret;
                }
                return -1;
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                return -2;
            }
            
        }
        public string PositionsToJson()
        {
            List<object> json = new List<object>();
            lock (this.PositionsLocker)
            {
                    foreach(int key in this.Positions.Keys)
                    {
                        json.Add(JsonConvert.DeserializeObject(this.Positions[key].ToJson()));
                    }
            }
            return JsonConvert.SerializeObject(json);
        }
        public bool ExistPositionLocal(int ticket)
        {
            if (this.LoginIncomplete)
            {
                return true;
            }
            else
            {
                return this.Positions.ContainsKey(ticket);
            }
        }
        public void OrderSendQueue(string srcBroker, OrderRequest req) 
        {
            try
            {
                lock (this.OrderReqQueueLocker)
                {
                    if(req.Action == (ushort)TradeAction.CLOSE)
                    {
                        this.MonitorWaitingQueue.Enqueue(req.Ticket);
                    }
                    if (!this.Broker.ToUpper().Equals(srcBroker.ToUpper()))
                    {
                        req.SetSymbol(this.SymbolMapping[req.GetSymbol()]);
                    }
                    this.OrderRequestQueue.Enqueue(req);
                }
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(),this.Debug);
            }
        }
        private void Connect()
        {
            try
            {

                if (this.Started)
                {
                    this.Log("Duplicate Starting!!!");
                    return;
                }
                else
                {
                    lock (this.StartedLocker)
                    {
                        this.Started = true;
                    }
                }
                
                if (NoCookie)
                {
                    if (!this.InitCookie())
                    {
                        lock (Global.Data.OfflineQueueLocker)
                        {
                            Global.Data.OfflineQueue.Enqueue(this.Account);
                        }
                        return;
                    }
                }
                if (NoServer)
                {
                    if (!this.InitServer())
                    {
                        lock (Global.Data.OfflineQueueLocker)
                        {
                            Global.Data.OfflineQueue.Enqueue(this.Account);
                        }
                        return;
                    }
                }
                if (!this.WsConnect())
                {
                    lock (Global.Data.OfflineQueueLocker)
                    {
                        Global.Data.OfflineQueue.Enqueue(this.Account);
                    }
                    return;
                }
                this.SendTokenAndPassword();
                byte[] buffer = new byte[1024 * 1024];
                ulong currentLoop = 0;
                while (this.KeepAlive)
                {
                    bool received;
                    if(this.WsReceive(ref buffer))
                    {
                        received = true;
                    }
                    else
                    {
                        received = false;
                    }
                    if (received)
                    {
                        uint length = BitConverter.ToUInt32(buffer, 0);
                        if (length > 0)
                        {
                            currentLoop++;
                            byte[] packet = new byte[length + 8];
                            Array.Copy(buffer, 0, packet, 0, length + 8);
                            byte[] plainText = Mt4.Unpack(packet, this.Key);
                            ushort packetType = BitConverter.ToUInt16(plainText, 2);
                            if (packetType != (ushort)PacketType.TickRcv || this.Debug)
                            {
                                this.Log(((PacketType)packetType).ToString() + ":" + ByteTool.Hex.ConvertBytesToHexString(plainText), this.Debug);
                            }
                            else {
                                if (RecvFails > 0)
                                {
                                    this.RecvFails = 0;
                                    this.Log(this.Account + " resumed", this.Debug);
                                }
                            }

                            //Reply quiz
                            if (packetType == (ushort)PacketType.Quiz)
                            {
                                Task.Run(new Action(() => this.ReplyQuiz(plainText, packetType)));
                            }

                            //Process position
                            if (packetType == (ushort)PacketType.Position)
                            {
                                Task.Run(new Action(() => this.ProcessPosition(plainText, packetType)));
                            }
                            
                            //Process login
                            if (this.LoginIncomplete)
                            {
                                this.ProcessLogin(plainText, packetType);
                            }

                            //Process order received
                            if (packetType == (ushort)PacketType.OrderRcv)
                            {
                                Task.Run(new Action(() => this.ProcessOrderReceived(plainText, packetType)));
                            }
                        }
                        this.Step++;
                    }
                }
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
            }
        }   
        private void PositionMonitor()
        {
            if(this.Role == TerminalRole.Master)
            {
                return;
            }
            while (this.KeepAlive)
            {
                try
                {
                    //Process follow order
                    this.ProcessOrderQueue();

                    bool qp = false;
                    int WaitingOrder = 0;
                    lock (this.OrderReqQueueLocker)
                    {
                        WaitingOrder = this.MonitorWaitingQueue.Count > 0 ? this.MonitorWaitingQueue.Dequeue() : -1;
                        if (WaitingOrder > 0)
                        {
                            if (this.MonitorWaitingTimestamp.ContainsKey(WaitingOrder))
                            {
                                if (this.MonitorWaitingTimestamp[WaitingOrder] > JSTool.Timestamp.GetTimeStamp(true) - 5000)
                                {
                                    this.MonitorWaitingQueue.Enqueue(WaitingOrder);
                                }
                                else if (this.MonitorWaitingTimestamp[WaitingOrder] < JSTool.Timestamp.GetTimeStamp(true) - 15000)
                                {
                                    this.Log("Close fail,Query position: "+ WaitingOrder.ToString());
                                    qp = true;
                                }
                            }
                            else
                            {
                                this.MonitorWaitingTimestamp.Add(WaitingOrder, JSTool.Timestamp.GetTimeStamp(true));
                                this.MonitorWaitingQueue.Enqueue(WaitingOrder); 
                            }
                        }
                    }
                    if (qp)
                    {
                        this.QueryPosition();
                        Thread.Sleep(5000);
                        this.MonitorWaitingTimestamp.Clear();
                        this.MonitorWaitingQueue.Clear();
                    }
                    else
                    {
                        bool flag = false;
                        OrderStruct order = new OrderStruct();
                        lock (this.PositionsLocker)
                        {
                            foreach (int key in this.Positions.Keys)
                            {
                                order = this.Positions[key];
                                if ((order.Ticket == WaitingOrder && this.MonitorWaitingTimestamp[WaitingOrder] < JSTool.Timestamp.GetTimeStamp(true) - 5000) || (this.MonitorWaitingQueue.Count == 0 && this.MonitorWaitingTimestamp.Count == 0))
                                {
                                    //string comment = order.GetComment();
                                    //if (comment != null)
                                    if (order.Magic > 0)
                                    {
                                        //int master = ByteTool.Hex.EFI(comment);
                                        //int ticket = ByteTool.Hex.DFI(comment);
                                        //if (!Global.Data.TerminalPool[master.ToString()].ExistPositionLocal(symbol, ticket))
                                        string master = Global.Data.GetMaster(order.Magic);
                                        if (master != null)
                                        {
                                            if (!Global.Data.TerminalPool[master].ExistPositionLocal(order.Magic))
                                            {
                                                flag = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (flag)
                        {
                            //close order
                            OrderRequest req = new OrderRequest((TradeOP)order.OP, order.GetSymbol(), order.Volume, TradeAction.CLOSE, order.Ticket, order.Magic);
                            this.OrderSendQueue(this.Broker, req);
                            this.Log("Monitor CLOSE:" + req.Ticket.ToString(), true);
                        }


                    }
                    Thread.Sleep(15);
                }
                catch (Exception ex)
                {
                    this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                }
            }
        }   
        private void OrderSendCommit(OrderRequest req)
        {
            try
            {
                this.DH++;
                req.jj = this.DH;
                byte[] reqBytes = ByteTool.Struct.StructToBytes(req);
                this.WsSend(Mt4.Pack(reqBytes, 1, this.Key));
                this.Log("Req: " + ByteTool.Hex.ConvertBytesToHexString(reqBytes), this.Debug);
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
            }
        }
        private void SendTokenAndPassword()
        {
            try
            {
                //send Token aes by jE.Wf()
                byte[] kt = ByteTool.Hex.ConvertHexStringToBytes(Mt4.Wf(this.jE));
                byte[] st = Mt4.Se(Token, PacketType.Token);
                byte[] pkdToken = Mt4.Pack(st, 1, kt);
                this.WsSend(pkdToken);
                //send password
                byte[] sp = Mt4.Se(Mt4.iG(this.Password, this.Ao), PacketType.Password);
                byte[] pkdPwd = Mt4.Pack(sp, 1, this.Key);
                this.WsSend(pkdPwd);
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                this.Log("key:" + ByteTool.Hex.ConvertBytesToHexString(this.Key), this.Debug);
            }
        }
        private void ReplyQuiz(byte[] plainText,ushort packetType)
        {
            try
            {
                if (packetType == (ushort)PacketType.Quiz)//first call is loop 4
                {
                    //reply quiz
                    byte[] jsb = new byte[plainText.Length - 4];
                    Array.Copy(plainText, 4, jsb, 0, jsb.Length);
                    string js = Encoding.ASCII.GetString(jsb).Replace("\0", "");
                    string Rep = JSTool.ChakraCore.ExecuteScript(js);
                    byte[] sRep = Mt4.Se(Encoding.ASCII.GetBytes(Rep), PacketType.Quiz);
                    byte[] pkRep = Mt4.Pack(sRep, 1, this.Key);
                    this.WsSend(pkRep);
                    this.Log("repJs: " + js, this.Debug);
                    this.Log("rep: " + Rep, this.Debug);
                }
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                this.Log("Quiz:" + plainText, this.Debug);
            }
        }
        private void QueryAccountInfo()
        {
            try
            {
                byte[] sJq = Mt4.Se(null, PacketType.AccountInfo);
                byte[] pkdJq = Mt4.Pack(sJq, 1, this.Key);
                this.WsSend(pkdJq);
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
            }
        }
        private void QueryPosition()
        {
            try
            {
                //query position
                byte[] sNm = Mt4.Se(null, PacketType.Position);
                byte[] pkdNm = Mt4.Pack(sNm, 1, this.Key);
                this.WsSend(pkdNm);
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
            }
        }
        private void QueryHistory(int days)
        {
            try
            {
                //query history
                byte[] hLm = new byte[8];
                Mt4.hG(days).CopyTo(hLm, 0);
                byte[] sLm = Mt4.Se(hLm, PacketType.History);
                byte[] pkdLm = Mt4.Pack(sLm, 1, this.Key);
                this.WsSend(pkdLm);
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
            }
        }
        private void QueryTick()
        {
            try
            {
                byte[] cB = new byte[] { 0x01, 0x00, 0x53, 0x00 };
                byte[] scB = Mt4.Se(cB, PacketType.TickReq);
                byte[] pkdcB = Mt4.Pack(scB, 1, this.Key);
                this.WsSend(pkdcB);
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
            }
        }
        private void QueryChart(byte[] st)
        {
            try
            {
                byte[] Rk = new byte[] { 0x47, 0x42, 0x50, 0x4A, 0x50, 0x59, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                byte[] ed = Mt4.hG(0);
                if (st == null)
                {
                    ed.CopyTo(Rk, Rk.Length - 8);
                    ed.CopyTo(Rk, Rk.Length - 4);
                }
                else
                {
                    st.CopyTo(Rk, Rk.Length - 8);
                    ed.CopyTo(Rk, Rk.Length - 4);
                }
                byte[] sRk = Mt4.Se(Rk, PacketType.Chart);
                byte[] pkdRk = Mt4.Pack(sRk, 1, this.Key);
                this.WsSend(pkdRk);
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
            }
        }
        private void QuerySymbols()
        {
            try
            {
                byte[] sPm = Mt4.Se(null, PacketType.Symbols);
                byte[] pkdsPm = Mt4.Pack(sPm, 1, this.Key);
                this.WsSend(pkdsPm);
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
            }
        }
        private void QuerySymbolGroup()
        {
            try
            {
                byte[] sOm = Mt4.Se(null, PacketType.SymbolGroup);
                byte[] pkdOm = Mt4.Pack(sOm, 1, this.Key);
                this.WsSend(pkdOm);
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
            }
        }
        private void ProcessOrderQueue()
        {
            if (this.Role == TerminalRole.Trader)
            {
                while (true)
                {
                    OrderRequest req;
                    lock (this.OrderReqQueueLocker)
                    {
                        if (this.OrderRequestQueue.Count > 0)
                        {
                            req = this.OrderRequestQueue.Dequeue();
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    try
                    {
                        string symbol = req.GetSymbol();
                        //string comment = req.GetComment();
                        //int ticket = req.Ticket;
                        
                        int magic = req.Magic;
                        bool flag = false;
                        //if (comment != null)
                        if(magic > 0)
                        {
                            if (req.Action == (byte)TradeAction.CLOSE)
                            {
                                foreach (OrderStruct order in Positions.Values)
                                {
                                    //if (comment.Equals(order.GetComment()))
                                    if (magic == order.Magic)
                                    {
                                        req.Ticket = order.Ticket;
                                        //req.CleanComment();
                                        flag = true;
                                    }
                                }
                            }
                            else if (req.Action == (byte)TradeAction.OPEN)
                            {
                                //if (!Positions[symbol].ContainsKey(ticket))
                                if (!Positions.ContainsKey(magic))
                                {
                                    flag = true;
                                }
                            }

                            if (flag)
                            {
                                Task.Run(new Action(() => this.OrderSendCommit(req)));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                    }
                }
            }
        }
        private void ProcessOrderReceived(byte[] plainText, ushort packetType)
        {

            try
            {
                if (packetType == (ushort)PacketType.OrderRcv)//
                {
                    byte js_P = plainText[4];
                    //if (js_P == ) { }
                    //////////////////////////
                    int size = ByteTool.Struct.GetByteLength(typeof(OrderReceivedData));
                    Double count = (plainText.Length - 5) / size;
                    int offset;
                    for (int i = 0; i < (int)count; i++)
                    {
                        offset = i * size + 5;
                        OrderReceivedData received = (OrderReceivedData)ByteTool.Struct.BytesToStruct(plainText, typeof(OrderReceivedData), offset);
                        TradeOP op = (TradeOP)received.Order.OP; ;
                        TradeAction action = received.Order.CloseTime == 0 ? TradeAction.OPEN : TradeAction.CLOSE;
                        string strAction = action == TradeAction.OPEN ? "OPEN" : "CLOSE";
                        string symbol = received.Order.GetSymbol();
                        if (this.Role == TerminalRole.Master)
                        {
                            if (received.XH == (int)ReceivedXH.Opened || received.XH == (int)ReceivedXH.Modified_Filled || received.XH == (int)ReceivedXH.Closed_Deleted_Exp)
                            {
                                if (received.Order.OP == (int)TradeOP.BUY || received.Order.OP == (int)TradeOP.SELL)
                                {
                                    //查position
                                    bool position;
                                    bool noRecord;
                                    lock (this.PositionsLocker)
                                    {
                                        if (this.Positions.ContainsKey(received.Order.Ticket))
                                        {
                                            OrderStruct positionOrder = this.Positions[received.Order.Ticket];
                                            position = true;
                                            if (!received.Order.SL.ToString().Equals(positionOrder.SL.ToString()) || !received.Order.TP.ToString().Equals(positionOrder.TP.ToString()))
                                            {
                                                if (received.XH == (int)ReceivedXH.Modified_Filled)
                                                {
                                                    action = TradeAction.ModifyPendingOrder;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            position = false;
                                        }
                                        noRecord = !this.Records.ContainsKey(received.Order.Ticket);
                                        if (action == TradeAction.OPEN && position == false && noRecord)
                                        {
                                            if (!this.Positions.ContainsKey(received.Order.Ticket))
                                            {
                                                this.Positions.Add(received.Order.Ticket, received.Order);
                                            }
                                        }
                                    }
                                    bool isClose = position && action == TradeAction.CLOSE;
                                    bool isNewOrFilled = !position || (position && received.XH == (int)ReceivedXH.Modified_Filled);
                                    bool isOpen = action == TradeAction.OPEN && noRecord;
                                    if (isClose || (isNewOrFilled && isOpen))
                                    {
                                        //带单comment
                                        //string comment;
                                        //string str = "##" + ByteTool.Hex.GFI(this.Account, received.Order.Ticket);
                                        //if (str.Length > 32)
                                        //{
                                        //    comment = str.Substring(0, 32);
                                        //}
                                        //else
                                        //{
                                        //    comment = str;
                                        //}
                                        //OrderRequest orderRequest = new OrderRequest(op, received.Order.GetSymbol(), received.Order.Volume, action, comment);
                                        
                                        //带单magic
                                        OrderRequest orderRequest = new OrderRequest(op, received.Order.GetSymbol(), received.Order.Volume, action, 0, received.Order.Ticket);
                                        foreach (Mt4Terminal client in Global.Data.TerminalPool.Values)
                                        {
                                            if (client.Role.Equals(TerminalRole.Trader) && !client.IsStopped())
                                            {
                                                client.OrderSendQueue(this.Broker,orderRequest);
                                            }
                                        }
                                        if(isOpen)
                                        {
                                            Global.Data.Magics.Add(received.Order.Ticket, this.Account);
                                        }
                                        if (isClose)
                                        {
                                            if (this.Positions.ContainsKey(received.Order.Ticket))
                                            {
                                                lock (this.PositionsLocker)
                                                {
                                                    this.Positions.Remove(received.Order.Ticket);
                                                    this.Records.Add(received.Order.Ticket, received.Order);
                                                }
                                            }
                                        }
                                        double price;
                                        if (action == TradeAction.OPEN)
                                        {
                                            price = received.Order.Price;
                                        }
                                        else
                                        {
                                            price = received.Order.ClosePrice;
                                        }
                                        this.Log(string.Format("Master: {0} {1} {2} {3}@{4} {5} AC:{6}", received.Order.Ticket.ToString(), received.Order.GetSymbol(), op.ToString(), ((double)received.Order.Volume / 100.0).ToString("F2"), price.ToString(), strAction, this.Account), true);
                                    }
                                }
                            }
                        }
                        if (this.Role == TerminalRole.Trader)
                        {
                            if (received.XH == (int)ReceivedXH.Opened || received.XH == (int)ReceivedXH.Closed_Deleted_Exp)
                            {
                                if (received.Order.OP == (int)TradeOP.BUY || received.Order.OP == (int)TradeOP.SELL)
                                {
                                    lock (this.PositionsLocker)
                                    {
                                        double price;
                                        if (action == TradeAction.OPEN)
                                        {
                                            if (!this.Positions.ContainsKey(received.Order.Ticket))
                                            {
                                                bool isEA = false;
                                                //string cmt = received.Order.GetComment();
                                                int magic = received.Order.Magic;
                                                //if (cmt != null)
                                                if(magic > 0)
                                                {
                                                    //int src = ByteTool.Hex.EFI(cmt);
                                                    string src = Global.Data.GetMaster(magic);
                                                    //if (src > 0)
                                                    if(src != null)
                                                    {
                                                        //if (Global.Data.TerminalPool.ContainsKey(src.ToString()))
                                                        if (Global.Data.TerminalPool.ContainsKey(src))
                                                        {
                                                            //if (Global.Data.TerminalPool[src.ToString()].Role == TerminalRole.Master)
                                                            if (Global.Data.TerminalPool[src].Role == TerminalRole.Master)
                                                            {
                                                                isEA = true;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (isEA)
                                                {
                                                    this.Positions.Add(received.Order.Ticket, received.Order);
                                                }
                                            }
                                            price = received.Order.Price;
                                        }
                                        else
                                        {
                                            if (this.Positions.ContainsKey(received.Order.Ticket))
                                            {
                                                this.Positions.Remove(received.Order.Ticket);
                                                if (received.XH == (int)ReceivedXH.Closed_Deleted_Exp)
                                                {
                                                    this.AccountBalance = received.Balance;
                                                }
                                            }
                                            price = received.Order.ClosePrice;
                                        }
                                    }
                                    this.Log(string.Format("Trader: {0} {1} {2} {3}@{4} {5} AC:{6} {7}${8}", received.Order.Ticket.ToString(), received.Order.GetSymbol(), op.ToString(), ((double)received.Order.Volume / 100.0).ToString("F2"), received.Order.ClosePrice.ToString(), strAction, this.Account, this.AccountCurrency, received.Balance.ToString("F2")), true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                this.Log("Order:" + ByteTool.Hex.ConvertBytesToHexString(plainText), this.Debug);
            }
        }
        private void ProcessAccountInfo(byte[] plainText,ushort packetType)
        {
            try
            {
                //Process account info
                if (packetType == (ushort)PacketType.AccountInfo)
                {
                    AccountBase baseInfo = (AccountBase)ByteTool.Struct.BytesToStruct(plainText, typeof(AccountBase), 5);
                    AccountExtra extraInfo = new AccountExtra();
                    extraInfo.GetStruct(plainText);

                    this.AccountType = extraInfo.AccountType;
                    this.AccountName = Encoding.ASCII.GetString(baseInfo.Name).Replace("\0", "");
                    this.AccountCurrency = Encoding.ASCII.GetString(baseInfo.Currency).Replace("\0", "");
                    this.AccountBalance = baseInfo.Balance;
                    this.AccountLeverage = baseInfo.Leverage;
                    string line = string.Format("{0}:{1}@{2} {3},{4}[{5}] {6}${7}", this.Role.ToString(), this.Account, this.Server, this.AccountName, this.AccountType, this.AccountLeverage, this.AccountCurrency, this.AccountBalance);
                    this.Log(line, true);
                }
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                this.Log("Account:" + ByteTool.Hex.ConvertBytesToHexString(plainText), this.Debug);
            }
        }
        private void ProcessSymbols(byte[] plainText, ushort packetType)
        {
            try
            { if (packetType == (ushort)PacketType.Symbols)
                {

                    if (this.SymbolIndex.Count < 1)
                    {
                        int size = ByteTool.Struct.GetByteLength(typeof(SymbolStruct));
                        Double count = (plainText.Length - 5) / size;
                        int offset;
                        for (int i = 0; i < count; i++)
                        {
                            offset = i * size + 5;
                            SymbolStruct symbol = (SymbolStruct)ByteTool.Struct.BytesToStruct(plainText, typeof(SymbolStruct), offset);
                            string key = Encoding.ASCII.GetString(symbol.Symbol).Replace("\0", "");
                            this.SymbolIndex.Add(key, symbol.index);
                        }
                    }
                } }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                this.Log("Symbols:" + ByteTool.Hex.ConvertBytesToHexString(plainText), this.Debug);
            }
        }
        private void ProcessPosition(byte[] plainText, ushort packetType)
        {
            try
            {
                if (packetType == (ushort)PacketType.Position)
                {
                    lock (this.PositionsLocker)
                    {
                        this.Positions.Clear();
                        int size = ByteTool.Struct.GetByteLength(typeof(OrderStruct));
                        Double count = (plainText.Length - 5) / size;
                        int offset;

                        for (int i = 0; i < (int)count; i++)
                        {
                            offset = i * size + 5;
                            OrderStruct position = (OrderStruct)ByteTool.Struct.BytesToStruct(plainText, typeof(OrderStruct), offset);
                            
                            string symbol = position.GetSymbol();
                            if (!this.Positions.ContainsKey(position.Ticket))
                            {
                                this.Positions.Add(position.Ticket, position);
                            }
                            Task.Run(new Action(() => this.Log(string.Format(this.Role.ToString() + ": {0} {1} {2}@{3} AC:{4} {5}", position.Ticket.ToString(), position.GetSymbol(), ((double)position.Volume / 100.0).ToString("F2"), position.ClosePrice.ToString(), this.Account, position.Magic.ToString()), true)));
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                this.Log("Position:" + ByteTool.Hex.ConvertBytesToHexString(plainText), this.Debug);
            }
        }
        private void ProcessHistory(byte[] plainText, ushort packetType)
        {
            try
            {
                if (packetType == (ushort)PacketType.History)
                {
                    int size = ByteTool.Struct.GetByteLength(typeof(OrderStruct));
                    Double count = (plainText.Length - 5) / size;
                    int offset;
                    List<OrderStruct> h = new List<OrderStruct>();
                    for (int i = 0; i < (int)count; i++)
                    {
                        offset = i * size + 5;
                        OrderStruct order = (OrderStruct)ByteTool.Struct.BytesToStruct(plainText, typeof(OrderStruct), offset);
                        h.Add(order);
                        if (order.OP == (int)TradeOP.BALANCE && order.Profit < 0)
                        {
                            string line = string.Format("{0},{1} 出金:{2}{3} {4} {5}", this.Account, this.AccountName, this.AccountCurrency, Math.Abs(order.Profit).ToString("F2"), order.GetComment(),JSTool.Timestamp.GetTime(order.OpenTime).ToLongDateString());
                            Global.Data.Billing.Add(line);
                            this.Log(line, true);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                this.Log("History:" + ByteTool.Hex.ConvertBytesToHexString(plainText), this.Debug);
            }
        }
        private void ProcessChart(byte[] plainText, ushort packetType)
        {

        }
        private void ProcessLogin(byte[] plainText, ushort packetType)
        {
            if (!this.LoginIncomplete)
            {
                return;
            }
            //token response
            if (packetType == (ushort)PacketType.Token)
            {

            }
            //password response
            if (packetType == (ushort)PacketType.Password)
            {

            }
            if (packetType == (ushort)PacketType.bB)
            {
                //query AccountInfo
                this.QueryAccountInfo();
            }

            if (packetType == (ushort)PacketType.AccountInfo)
            {
                //query Symbols
                this.QuerySymbols();
                //query SymbolGroup
                this.QuerySymbolGroup();
            }

            //Process account info
            this.ProcessAccountInfo(plainText, packetType);
            //Process Symbol
            this.ProcessSymbols(plainText, packetType);

            if (packetType == (ushort)PacketType.SymbolGroup)
            {
                //query position
                this.QueryPosition();
                //query history
                this.QueryHistory(-1);
                //query chart
                this.QueryChart(null);
            }
            //Process position
            //this.ProcessPosition(plainText, packetType);
            //Process history
            this.ProcessHistory(plainText, packetType);
            if (packetType == (ushort)PacketType.Chart && this.LoginIncomplete)
            {
                this.LoginIncomplete = false;
                this.Log("Login Completed", this.Debug);
                try
                {
                    int res = BitConverter.ToInt32(plainText, 5);
                    byte[] st = BitConverter.GetBytes(res - 806400);
                    //query chart
                    this.QueryChart(st);
                }
                catch(Exception ex) 
                {
                    this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                }
                //query tick
                this.QueryTick();
            }
        }
        private bool InitCookie()
        {
            try
            {
                NetTool.HttpsRequest request = new NetTool.HttpsRequest(this.NI);
                string url = "https://www.mql5.com/en/trading";
                string refer = "";
                string src = request.Perform(url, refer, null, 30000, true, this.NI);
                string cookie = request.GetHeaderValue("Set-Cookie");
                int index = cookie == null && cookie.Contains("uniq=") ? -1 : cookie.IndexOf("uniq=");
                if (index < 0 || src.Length - index - 6 < 12)
                {
                    this.Log("Init cookie fail.");
                    return false;
                }
                string uf = cookie.Substring(index + 5, 12);
                string ao_src = uf + ";Win32;Gecko;0;zh-CN;1920x1080";
                this.Ao = CipherTool.Hash.GenerateMD5bytes(ao_src);
                this.NoCookie = false;
                return true;
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace, this.Debug);
                return false;
            }
        }
        private bool InitServer()
        {
            try
            {
                NetTool.HttpsRequest request = new NetTool.HttpsRequest(this.NI);
                string tpl = "https://trade.mql5.com/trade/json?gwt={0}&login={1}&trade_server={2}";
                string url = string.Format(tpl, this.Gwt, this.Account, this.Server);
                string refer = "https://trade.mql5.com/trade?callback&switch_platform=1&border=0&startup_version=5&demo_all_servers=1&user_token=0&startup_mode=create_demo";
                this.NI.AddHeader("Accept", "*/*");
                string src = request.Perform(url, refer, null, 30000, false, this.NI);
                int index = src.Contains("\"key\"=") ? -1 : src.IndexOf("key");
                if (index < 0 || src.Length - index - 6 < 64)
                {
                    this.Log("Init server fail.");
                    return false;
                }
                this.keyString = src.Substring(index + 6, 64);
                this.Key = ByteTool.Hex.ConvertHexStringToBytes(keyString);
                index = src.IndexOf("token");
                this.tokenString = src.Substring(index + 8, 64);
                this.Token = Encoding.ASCII.GetBytes(tokenString);
                this.Log("key: " + this.keyString, this.Debug);
                this.Log("token: " + this.tokenString, this.Debug);
                NoCookie = false;
                return true;
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace, this.Debug);
                return false;
            }
        }
        private bool WsConnect()
        {
            return this.WsConnect((byte)this.Gwt);
        }
        private bool WsConnect(byte gwt)
        {
            Uri uri = new Uri(string.Format("wss://gwt{0}.mql5.com/", gwt));
            try
            {
                if (this.WS.State == WebSocketState.None)
                {
                    this.WS.Options.SetRequestHeader("User-Agent", NI.UserAgent);
                    this.WS.Options.Cookies = NI.cookieContainer;
                    this.WS.ConnectAsync(uri, CancellationToken.None).Wait(5000);
                }
                while (this.WS.State == WebSocketState.Connecting)
                {
                    this.Log("Connecting......", true);
                    Thread.Sleep(200);
                }
                if (this.WS.State == WebSocketState.Open)
                {
                    this.RecvFails = 0;
                    this.Log(uri.ToString() + " connected", this.Debug);
                    return true;
                }
                else
                {
                    this.Log("WS State: " + this.WS.State.ToString(),true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), true);
                return false;
            }
        }
        private bool WsSend(byte[] data)
        {
            try
            {
                this.WS.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                if(this.SendFails > 3)
                {
                    lock (Global.Data.OfflineQueueLocker)
                    {
                        Global.Data.OfflineQueue.Enqueue(this.Account);
                    }
                }
                this.SendFails++;
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                this.Log(this.Account.ToString() + " send fail " + this.SendFails, true);
                return false;
            }
        }
        private bool WsReceive(ref byte[] buffer)
        {
            try
            {

                this.WS.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Wait();
                return true;
            }
            catch (Exception ex)
            {
                if (this.RecvFails > 5 || this.WS.State == WebSocketState.Aborted)
                {
                    lock (Global.Data.OfflineQueueLocker)
                    {
                        Global.Data.OfflineQueue.Enqueue(this.Account);
                    }
                }
                this.RecvFails++;
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
                this.Log(this.Account.ToString() + " receive fail " + this.RecvFails, true);                
                return false;
            }
        }
        private void WsClose()
        {
            try
            {
                WS.Dispose();
            }
            catch(Exception ex)
            {
                this.Log(ex.Message + ex.StackTrace.ToString(), this.Debug);
            }
        }
        private void Log(string text, bool console = false)
        {
            if (console)
            {
                Console.WriteLine(text);
            }
            lock (this.LogLocker)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    File.AppendAllText(Directory.GetCurrentDirectory() + "\\logs\\" + this.NI.Logfile, text + "\r\n", Encoding.UTF8);
                }
                else
                {
                    File.AppendAllText(Directory.GetCurrentDirectory() + "/logs/" + this.NI.Logfile, text + "\r\n", Encoding.UTF8);
                }
                
            }

        }
        private void PositionReport()
        {

        }
    }
}
