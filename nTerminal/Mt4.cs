using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Mt4
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HistoryRequest
    {
        byte Rnd1;
        byte Rnd2;
        short Packet;
        int Start;//p.setInt32(0, Math.floor((g.Zh || 0) / 1E3), !0);
        int End;//p.setInt32(4, Math.floor((g.RH || 0) / 1E3), !0);
        public HistoryRequest(int days = -30)
        {
            Rnd1 = (byte)new Random().Next(0, 255);
            Rnd2 = (byte)new Random().Next(0, 255);
            Packet = (byte)PacketType.History;
            if (days == 0)
            {
                Start = 0;
            }
            else
            {
                Start = BitConverter.ToInt32(Mt4.hG(days),0);
            }
            End = 0;
        }
        public byte[] GetBytes() {
            return ByteTool.Struct.StructToBytes(this);
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    
    public struct OrderRequest
    {
        byte Rnd1;
        byte Rnd2;
        short Packet;
        public byte Action;//k.setUint8(p, g.type || 0, !0);/Mt4js_b.Mq
        short OP;//k.setInt16(p += 1, g.D || 0, !0);
        public int Ticket;//k.setInt32(p += 2, g.T || 0, !0);  open:0;close:ticket
        public int Magic;//k.setInt32(p += 4, g.$i || 0, !0);  $i||0
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        byte[] Symbol;//k.G(p += 4, g.I || "");
        int Volume;//k.setInt32(p += 12, g.ua || 0, !0);
        double Price;//k.setFloat64(p += 4, g.Pd || 0, !0);
        double TP;//k.setFloat64(p += 8, g.Aa || 0, !0);
        double SL;//k.setFloat64(p += 8, g.Ba || 0, !0);
        int Dev;//k.setInt32(p += 8, g.Yi || 0, !0); deviation
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        byte[] Comment;//k.Ah(p += 4, g.wc && g.wc.substr(0, 32) || "");
        int Bc;//k.setInt32(p += 32, g.Bc ? g.Bc : 0, !0);
        public int jj; //k.setInt32(p + 4, g.jj || 0, !0); H.DH++ 1000++
        public OrderRequest(TradeOP op, string symbol, int volume, TradeAction action, int ticket, int magic, string comment = null, double price = 0.0, double targetProfit = 0.0, double stopLoss = 0.0, int dev = 0)
        {
            Rnd1 = (byte)new Random().Next(0, 255);
            Rnd2 = (byte)new Random().Next(0, 255);
            Packet = (short)PacketType.OrderReq;
            Action = (byte)action;
            OP = (short)op;
            Ticket = ticket;
            Magic = magic;
            Symbol = new byte[12];
            Array.Copy(Encoding.ASCII.GetBytes(symbol), 0, Symbol, 0, symbol.Length);
            Volume = volume;
            Price = price;
            TP = targetProfit;
            SL = stopLoss;
            Dev = dev;
            Comment = new byte[32];
            if (comment != null)
            {
                if (comment.Length > 0 && comment.Length < 32)
                {
                    Encoding.ASCII.GetBytes(comment).CopyTo(Comment, 0);
                }
            }
            Bc = 0;
            jj = 0;
        }
        public OrderRequest(TradeOP op, string symbol, double volume, TradeAction action, int ticket, int magic, string comment = null, double price = 0.0, double targetProfit = 0.0, double stopLoss = 0.0, int dev = 0)
        {
            Rnd1 = (byte)new Random().Next(0, 255);
            Rnd2 = (byte)new Random().Next(0, 255);
            Packet = (short)PacketType.OrderReq;
            Action = (byte)action;
            OP = (short)op;
            Ticket = ticket;
            Magic = magic;
            Symbol = new byte[12];
            Array.Copy(Encoding.ASCII.GetBytes(symbol), 0, Symbol, 0, symbol.Length);
            Volume = Convert.ToInt32(volume / 100.0);
            Price = price;
            TP = targetProfit;
            SL = stopLoss;
            Dev = dev;
            Comment = new byte[32];
            if (comment != null)
            { 
                if (comment.Length > 0 && comment.Length < 32)
                {
                    Encoding.ASCII.GetBytes(comment).CopyTo(Comment,0);
                }
            }
            Bc = 0;
            jj = 0;
        }
        public string GetSymbol()
        {
            return Encoding.ASCII.GetString(Symbol).Replace("\0", "");
        }
        public void SetSymbol(string symbol)
        {
            Encoding.ASCII.GetBytes(symbol).CopyTo(Symbol, 0);
        }
        public string GetComment()
        {
            return Encoding.ASCII.GetString(Comment).Replace("\0", "").Replace("#","");
        }
        public void CleanComment()
        {
            Comment = new byte[32];
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OrderReceivedhead
    {
        readonly byte Rnd1;
        readonly byte Rnd2;
        readonly short Packet;
        readonly byte Js_P;//js_P
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OrderReceivedData
    {
        //h(a, c) -> q.sC -> x(g,q)
        readonly uint SU;//k.$U
        public readonly int XH;//k.XH = {lu: 0,yw: 1,ZF: 2,Bu: 3 }
        public readonly double Balance;//k.rf//余额
        readonly double Xh;//k.Xh
        public readonly OrderStruct Order;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OrderStruct
    {
        public readonly int Ticket;//k.T = p.getInt32(q, !0);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        readonly byte[] Symbol;// k.I = p.Yb(q + 4, 12);
        int Digits; //unknown,security precision???????? k.S = p.getInt32(q + 16, !0);
        public readonly int OP;//k.D = p.getInt32(q + 20, !0);
        public readonly int Volume;//k.ua = p.getInt32(q + 24, !0);
        public readonly int OpenTime;//k.zo = p.getInt32(q + 28, !0);
        readonly int State;//unknown ??????k.WH = p.getInt32(q + 32, !0); 0 or not 0
        public readonly double Price;//k.Ra = p.getFloat64(q + 36, !0);
        public readonly double TP;//k.Aa = p.getFloat64(q + 44, !0);
        public readonly double SL;//k.Ba = p.getFloat64(q + 52, !0);
        public readonly int CloseTime;//k.Qi = p.getInt32(q + 60, !0);
        readonly int Expiration;//k.Bc = 1E3 * p.getInt32(q + 64, !0);
        readonly byte Reason;//k.ZU = p.getInt8(q + 68, !0);
        public readonly double Commission;//k.Il = p.getFloat64(q + 69, !0);
        readonly double FeeAgent;//k.nU = p.getFloat64(q + 77, !0);
        readonly double Swap;//k.im = p.getFloat64(q + 85, !0);
        public readonly double ClosePrice;//k.Sc = p.getFloat64(q + 93, !0);
        public readonly double Profit;//k.Tc = p.getFloat64(q + 101, !0);
        readonly double Taxes;//k.nm = p.getFloat64(q + 109, !0);
        public readonly int Magic;//k.FU = p.getInt32(q + 117, !0);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        readonly byte[] Comment;//k.wc = p.vg(q + 121, 32);
        public readonly double MarginRate;//k.wo = p.getFloat64(q + 153, !0);
        public string GetSymbol()
        {
            return Encoding.ASCII.GetString(Symbol).Replace("\0", "");
        }
        public string GetComment()
        {
            string ret = Encoding.ASCII.GetString(Comment).Replace("\0", "").Replace("#", "");
            if (ret.Length > 4)
            {
                return ret;
            }
            else
            {
                return null;
            }
        }
        public string ToJson()
        {
            string tpl = "{{\"ticket\":{0},\"symbol\":\"{1}\",\"digits\":{2},\"op\":{3},\"volume\":{4},\"openTime\":{5},\"state\":{6},\"price\":{7},\"tp\":{8},\"sl\":{9},\"closeTime\":{10},\"expiration\":{11},\"reason\":{12},\"commission\":{13},\"feeAgent\":{14},\"swap\":{15},\"closePrice\":{16},\"profit\":{17},\"taxes\":{18},\"magic\":{19},\"comment\":\"{20}\",\"marginRate\":{21}}}";
            string json = string.Format(tpl, this.Ticket.ToString(), this.GetSymbol(), this.Digits.ToString(), this.OP.ToString(), this.Volume.ToString(), this.OpenTime.ToString(), this.State.ToString(), this.Price.ToString(), this.TP.ToString(), this.SL.ToString(), this.CloseTime.ToString(), this.Expiration.ToString(), this.Reason.ToString(), this.Commission.ToString(), this.FeeAgent.ToString(), this.Swap.ToString(), this.ClosePrice.ToString(), this.Profit.ToString(), this.Taxes.ToString(), this.Magic.ToString(), this.GetComment(), this.MarginRate.ToString());
            return json;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tick //byte[15]
    {
        public short symbol;//l.mg = d.getInt16(k, !0);
        public int time;//l.tg = 1E3 * d.getInt32(k += 2, !0);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] ask;//l.gb
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] bid;//l.vb

                          //0 === l.tg ? (l.gb = d.getInt32(k += 4, !0),
                          //l.vb = d.getInt32(k + 4, !0)) : (l.gb = d.getFloat32(k += 4, !0),
                          //l.vb = d.getFloat32(k + 4, !0));
        override public string ToString()
        {
            string tpl = "Tick:{0} time:{1} ask:{2} bid:{3}";
            if (time == 0)
            {
                return string.Format(tpl, symbol, time, BitConverter.ToInt32(ask,0), BitConverter.ToInt32(bid,0)); 
            }
            else
            {
                return string.Format(tpl, symbol, time, BitConverter.ToSingle(ask,0), BitConverter.ToSingle(bid,0));
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SymbolStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] Symbol;//l.I = d.Yb(k, 12);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        byte[] Description;//l.Yh = d.Yb(k += 12, 64);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        byte[] Currency;//l.gj = d.Yb(k += 64, 12);
        int Type;//l.Ie = d.getInt32(k += 12, !0);
        int Digits;//l.S = d.getInt32(k += 4, !0);
        int TradeMode;//l.Fh = d.getInt32(k += 4, !0);
        uint HexColor;//l.qt = d.kk(k += 4); //#FFFFFF webcolor
        public short index;//l.mg = d.getInt16(k += 4, !0);
        short WatchIndex;//l.Ti = d.getInt16(k += 2, !0);
        int Starting;//l.au = 1E3 * d.getInt32(k += 2, !0);
        int Expiration;//l.Bc = 1E3 * d.getInt32(k += 4, !0);
        int hi;//l.hi = d.getInt32(k += 4, !0);
        int MH;//l.MH = d.getInt32(k += 4, !0);
        int Vi;//l.Vi = d.getInt32(k += 4, !0);
        int NH;//l.NH = d.getInt32(k += 4, !0);
        int lm;//l.lm = d.getInt32(k += 4, !0);
        double sk;//l.sk = d.getFloat64(k += 4, !0);
        double tk;//l.tk = d.getFloat64(k += 8, !0);
        int Eo;//l.Eo = d.getInt32(k += 8, !0);
        double Zc;//l.Zc = d.getFloat64(k += 4, !0);
        double Zg;//l.Zg = d.getFloat64(k += 8, !0);
        double gf;//l.gf = d.getFloat64(k += 8, !0);
        int ji;//l.ji = d.getInt32(k += 8, !0);
        int Ot;//l.Ot = d.getInt32(k += 4, !0);
        double nd;//l.nd = d.getFloat64(k += 4, !0);
        double lk;//l.lk = d.getFloat64(k += 8, !0);
        double Wg;//l.Wg = d.getFloat64(k += 8, !0);
        double Rb;//l.Rb = d.getFloat64(k += 8, !0);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        byte[] MarginCurrency;//l.Nt = d.Yb(k += 8, 12);
        int GU;//l.GU = d.getInt32(k += 12, !0);
        double gi;//l.gi = d.getFloat64(k += 4, !0);
        double OH;//l.OH = d.getFloat64(k += 8, !0);
        int EU;//l.EU = d.getInt32(k += 8, !0);
        int so;//l.so = d.getInt32(k + 4, !0);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AccountBase// offset 5
    {
        byte Valid;//???? k.Of = q.getUint8(p, !0);
        public double Balance;//k.rf = q.getFloat64(p += 1, !0);
        double Xh;//k.Xh = q.getFloat64(p += 8, !0);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Currency;//k.Nf = q.td(p += 8, 32);
        public ushort Leverage;//k.tf = q.getUint16(p += 32, !0);
        int hj;//k.hj = q.getInt32(p += 2, !0);
        sbyte ck;//k.ck = q.getInt8(p += 4, !0);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] SignalServer;//k.ze = q.td(p += 1, 128);
        ushort Version;//k.HH = q.getUint16(p += 128, !0);
        sbyte Ft;//k.Ft = q.getInt8(p += 2, !0);
        sbyte bH;//k.bH = q.getInt8(p += 1, !0);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] Name;//k.Pg = q.vg(p += 1, 64);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32 * 28)]
        byte[] unknown;//k.Pg = q.vg(p += 1, 64);
        int ik;// k.ik = q.getInt32(p += 64 + 32 * g.F.YD, !0); //g.F.YD = 28
        //k.Wi = null;
        //p += 4;
        //u.byteLength >= p + 4 && (k.Wi = q.getInt32(p, !0));
        //k.mk = null;
        //p += 4;
        //u.byteLength >= p + 4 && 0 < q.getInt32(p, !0) && (k.mk = q.getInt32(p, !0));
        //p = p + 4 + 4;
        //p += 128 * g.F.Hx; //g.F.Hx = 36
        //u.byteLength >= p + 128 && (k.Kl = q.Yb(p, 128),
        //p += 128);
        //u.byteLength >= p + 4 && (k.Vt = q.getInt32(p, !0),
        //p += 4);
        //u.byteLength >= p + 4 && (k.gk = q.getInt32(p, !0));
    }
    public class AccountExtra
    {
        int Wi;
        int mk;
        public string AccountType;
        int Vt;
        int Gk;
        public bool GetStruct(byte[] data)
        {
            int offset = Marshal.SizeOf(typeof(AccountBase));
            if(data.Length >= offset + 4 + 5)
            {
                Wi = BitConverter.ToInt32(data,0);
                offset += 4;
            }
            else
            {
                return false;
            }
            if(data.Length >= offset + 4)
            {
                mk = BitConverter.ToInt32(data, offset);
                offset = offset + 4 + 4;
                offset += 128 * 36;
            }
            if(data.Length >= offset + 128)
            {
                byte[] kl = new byte[128];
                Array.Copy(data, offset, kl, 0, 128);
                AccountType = Encoding.UTF8.GetString(kl).Replace("\0","");
                offset += 128;
            }
            if(data.Length >= offset + 4)
            {
                Vt = BitConverter.ToInt32(data, offset);
                offset += 4;
            }
            if(data.Length >= offset + 4)
            {
                Gk = BitConverter.ToInt32(data, offset);
            }
            return true;
        }
    }
    public static class Mt4
    {
        public static string Wf(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] xx = Encoding.ASCII.GetBytes(str);
            foreach (byte x in xx)
            {
                if (28 == x)
                {
                    sb.Append('&');
                }
                else if (23 == x)
                {
                    sb.Append('!');
                }
                else
                {
                    sb.Append((char)(x - 1)); //CharCodeAt(src_char) - 1
                }
                string tmp = sb.ToString();
            }
            return sb.ToString();
        }
        public static byte[] iG(string pwd, byte[] ao)// data 80byte 64byte pwd + 16byte ao, 64byte password  new char order = 2*index, other fill by '\0';ao:MD5 value in byte[16]. 
        {
            if (pwd.Length < 5 || ao.Length != 16)
            {
                return null;
            }
            List<byte> sb = new List<byte>();
            byte[] p = Encoding.ASCII.GetBytes(pwd);
            for (int i = 0; i < p.Length; i++)
            {
                sb.Add(p[i]);
                sb.Add(0);
            }
            for (int j = 0; j < 64 - p.Length * 2; j++)
            {
                sb.Add(0);
            }
            sb.AddRange(ao);
            if (sb.Count == 80)
            {
                return sb.ToArray();
            }
            else
            {
                return null;
            }
        }
        public static byte[] Se(byte[] data, PacketType code)//add head 32bit: 8bit rnd + 8bit rnd +  16bit Terminal.b code
        {
            int dataLength;
            if (data == null)
            {
                dataLength = 0;
            }
            else
            {
                dataLength = data.Length;
            }
            byte[] rnd = new byte[4];
            new Random().NextBytes(rnd);
            BitConverter.GetBytes((ushort)code).CopyTo(rnd, 2);
            byte[] d = new byte[dataLength + rnd.Length];
            rnd.CopyTo(d, 0);
            if (data == null)
            {
                return d;
            }
            else
            {
                data.CopyTo(d, 4);
                return d;
            }
        }
        public static byte[] hG(int days)
        {
            TimeSpan ts = DateTime.Now.AddDays(days) - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            int tsp = Convert.ToInt32(ts.TotalSeconds);
            byte[] x = BitConverter.GetBytes(tsp);
            return x;
        }
        public static byte[] Pack(byte[] plainText, uint flag, byte[] secret)
        {
            byte[] cipherText = CipherTool.AES256_CBC_PKCS7.EncryptBytes(plainText, null, secret);
            byte[] head = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            uint length = (uint)plainText.Length;
            BitConverter.GetBytes(length).CopyTo(head, 0);
            BitConverter.GetBytes(flag).CopyTo(head, 4);
            byte[] packet = new byte[head.Length + cipherText.Length];
            head.CopyTo(packet, 0);
            cipherText.CopyTo(packet, head.Length);
            return packet;
        }
        public static byte[] Unpack(byte[] packet, byte[] secret) 
        {
            uint length = BitConverter.ToUInt32(packet, 0);
            byte[] cipherText = new byte[length];
            Array.Copy(packet, 8, cipherText, 0, length);
            byte[] plainText = CipherTool.AES256_CBC_PKCS7.DecryptBytes(cipherText, null, secret);
            return plainText;
        }
    }
    public enum PacketType//js_b
    {
        Token = 0,//send token b.Sk=0
        Password = 1,//send password b.Qk=1
        Mm = 2,
        AccountInfo = 3,//b.Jq
        Position = 4,//b.Nm=4
        History = 5,//b.Lm=5
        Symbols = 6,//b.Pm
        TickReq = 7,//b.cB
        TickRcv = 8,//tick b.Lq=8
        SymbolGroup = 9,//b.bB
        OrderRcv = 10,//b.dB
        Chart = 11,//b.Rk
        OrderReq = 12,//b.Mq
        Qm = 13,
        aB = 14,
        bB = 15,
        Pk = 16,
        GO = 21,
        Kq = 26,
        Pq = 0,
        Oq = 2,
        HO = 3,
        MO = 4,
        KO = 5,
        UO = 6,
        SO = 18,
        TO = 9,
        PO = 7,
        hB = 8,
        YO = 10,
        NO = 11,
        WO = 12,
        VO = 13,
        IO = 14,
        JO = 15,
        gB = 17,
        XO = 19,
        RO = 20,
        Rm = 21,
        QO = 22,
        eB = 23,
        LO = 24,
        OO = 25,
        Sm = 27,
        Tk = 30,
        fB = 28,
        Nq = 29,
        Quiz = 50,//check is OK by compute rnd Ok=50
        Ping = 51
    }
    public enum js_P
    {
        gh = 0,
        pg = 1,
        mr = 2,
        or = 3,
        ur = 4,
        rr = 5,
        an = 6,
        qr = 7,
        vr = 8,
        pr = 9,
        nr = 10,
        tr = 11,
        kr = 64,
        lr = 65,
        sr = 66,
        en = 128,
        wr = 129,
        xr = 130,
        yr = 131,
        Er = 132,
        cn = 133,
        Gr = 134,
        Jr = 135,
        Hr = 136,
        zr = 137,
        Dj = 138,
        Ir = 139,
        Dr = 140,
        Lr = 141,
        bn = 142,
        dn = 143,
        Mr = 144,
        Fr = 145,
        Ar = 146,
        Br = 147,
        fn = 148,
        Cr = 149,
        Kr = 150
    }
    public enum TradeAction//js_I
    {
        fl = 0,
        UT = 1,
        InstantOrder = 64,//dl = 64,0x40
        OrderByRequest = 65,//Bn = 65,0x41
        OPEN = 66,//Ij = 66,0x42
        PedingOrder = 67,//el = 67,0x43
        Gi = 68,//Gi = 68,0x44
        An = 69, //An 0x45
        CLOSE = 70,//zn 0x46
        ModifyPendingOrder = 71,//Jj = 71,0x47
        DeletePendingOrder = 72,//yn = 72,0x48
        Closed = 73,//xn
        MultipeClose = 74//
    }
    public enum OrderResponseAction
    {
        PositionOpen,   //开仓
        PositionClose,  //平仓
        PositionModify, //修改
        PendingOpen,    //挂单
        PendingDelete,  //删除挂单
        PendingModify,  //挂单修改
        PendingFill,    //挂单触发
        Balance,        //余额
        Credit          //信用
    }
    public enum TradeOP {
        /*
             R = {
                yb: 0,
                Ma: 1,
                Rd: 2,
                Me: 3,
                We: 4,
                he: 5,
                yg: 6,
                nj: 7
            }
         */
        BUY = 0,
        SELL = 1, 
        BUY_LIMIT = 2,//Rd 
        SELL_LIMIT = 3,//Me
        BUY_STOP = 4,//We
        SELL_STOP = 5,//He 
        BALANCE = 6, 
        CREDIT = 7
    }
    public enum ReceivedXH
    {
        /*
         L = {
            lu: 0,
            yw: 1,
            ZF: 2,
            Bu: 3
        }
        */
        Opened = 0,
        Closed_Deleted_Exp = 1,
        Modified_Filled = 2,
        Balance_Credit = 3
    }
}
