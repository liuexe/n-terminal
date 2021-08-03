using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace NetTool
{
    public class NetInfo
    {
        public string UserAgent;
        public string Proxy;
        public string ProxyType;
        public string ProxyPass;
        public string Logfile;
        public CookieContainer cookieContainer = new CookieContainer();
        public Dictionary<string, string> headerlist = new Dictionary<string, string>();
        public NetInfo(string _host, string _cookie, string _useragent, string _proxy, string _proxyType, string _proxyPass)
        {
            this.UserAgent = _useragent;
            this.Proxy = _proxy;
            this.ProxyType = _proxyType;
            this.ProxyPass = _proxyPass;

        }
        public NetInfo(string _host, string logfile, string _useragent)
        {
            this.UserAgent = _useragent;
            this.Proxy = "";
            this.ProxyType = "";
            this.ProxyPass = "";
            this.Logfile = logfile;

        }
        public NetInfo(string _host, string _cookie)
        {
            this.UserAgent = "";
            this.Proxy = "";
            this.ProxyType = "";
            this.ProxyPass = "";

        }
        public NetInfo(string _cookie)
        {
            this.UserAgent = "";
            this.Proxy = "";
            this.ProxyType = "";
            this.ProxyPass = "";

        }
        public NetInfo()
        {
            this.UserAgent = "";
            this.Proxy = "";
            this.ProxyType = "";
            this.ProxyPass = "";

        }
        public void AddHeader(string name, string value)
        {
            if (this.headerlist.ContainsKey(name))
            {
                this.headerlist[name] = value;
            }
            else
            {
                this.headerlist.Add(name, value);
            }
        }
    }
    public class HttpsRequest
    {

        private NetInfo NI;
        private int count;
        private WebHeaderCollection webHeader;
        public HttpsRequest(NetInfo ni)
        {
            this.NI = ni;
            this.count = 0;
        }
        public string Perform(string url, string referer, string postString, int timeout, bool header, NetInfo ni)
        {
            try
            {
                if (ni != null)
                {
                    this.NI = ni;
                }
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                if (referer != null)
                {
                    request.Referer = referer;
                }
                request.Method = postString == null ? WebRequestMethods.Http.Get : WebRequestMethods.Http.Post;
                request.UserAgent = this.NI.UserAgent;
                request.AutomaticDecompression = DecompressionMethods.All;
                request.CookieContainer = NI.cookieContainer;
                request.KeepAlive = true;
                request.Timeout = timeout;

                foreach (string key in this.NI.headerlist.Keys)
                {
                    request.Headers.Set(key, this.NI.headerlist[key]);
                }
                if (postString == null? false : postString.Length > 0)
                {
                    request.ContentType = "application/json";
                    byte[] postData = Encoding.ASCII.GetBytes(postString);
                    System.IO.Stream outputStream = request.GetRequestStream();
                    outputStream.Write(postData, 0, postData.Length);
                    outputStream.Close();
                }
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string srcString = reader.ReadToEnd();
                    this.count++;
                    this.webHeader = response.Headers;
                    response.Close();
                    request.Abort();
                    return srcString;
                }
                else
                {
                    string ret = response.StatusDescription;
                    response.Close();
                    request.Abort();
                    return ret;
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
        public string GetHeaderValue(string name)
        {      
            if (count == 0) return null;
            string[] values = this.webHeader.GetValues(name);
            return values == null ? null : values.Length < 1 ? null : this.BuildString(values);
        }
        private string BuildString(string[] sa)
        {
            StringBuilder sb = new StringBuilder();
            foreach(string s in sa)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}
