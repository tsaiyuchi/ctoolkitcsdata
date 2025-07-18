﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Data;
using System.IO.Compression;
using System.IO;
using CToolkitCs.v1_2.Compress;
using System.Threading.Tasks;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.Net.Cache;
using Newtonsoft.Json;

namespace CToolkitCs.v1_2.Net.HttpTx
{
    public class CtkNetHttpTransaction : IDisposable
    {
        public HttpWebRequest HwRequest { get; protected set; }
        public Encoding HwRequestEncoding = Encoding.UTF8;
        public Encoding HwResponseEncoding = Encoding.UTF8;
        protected string HwRequestData;
        protected HttpWebResponse hwResponse;
        protected String HwResponseData;


        /// <summary> 若還沒交易, 會依現有資料進行交易 </summary>
        public HttpWebResponse GetHwResponse()
        {
            if (this.hwResponse != null) return this.hwResponse;

            var hwreq = this.HwRequest;
            var hwreqData = this.HwRequestData;
            var hwreqEncoding = this.HwRequestEncoding;


            if (string.Compare(hwreq.Method, "POST", true) == 0)
            {
                if (hwreqData == null) hwreqData = "";
                var byteData = hwreqEncoding.GetBytes(hwreqData);
                hwreq.ContentLength = byteData.Length;//未必要設定
                using (var reqstm = hwreq.GetRequestStream())
                    reqstm.Write(byteData, 0, byteData.Length);
            }

            return this.hwResponse = (HttpWebResponse)hwreq.GetResponse();
        }

        /// <summary> 若還沒有Response, 會依現有資料進行交易 並取得Response </summary>
        public String GetHwResponseData()
        {
            if (this.HwResponseData != null) return this.HwResponseData;//只有null, 空字串仍代表已讀過

            var hwresp = this.GetHwResponse();
            var hwrespEncoding = this.HwResponseEncoding;

            if (hwrespEncoding == null && !string.IsNullOrEmpty(hwresp.CharacterSet))
            {
                try { hwrespEncoding = Encoding.GetEncoding(hwresp.CharacterSet); }
                catch (Exception) { }
            }
            if (hwrespEncoding == null) { hwrespEncoding = Encoding.UTF8; }

            using (var stream = hwresp.GetResponseStream())
            using (var reader = new StreamReader(stream, hwrespEncoding))
                return this.HwResponseData = reader.ReadToEnd();
        }

        /// <summary> Response為GZip壓縮 </summary>
        public String GetHwResponseDataGZip()
        {
            if (this.HwResponseData != null) return this.HwResponseData;//只有null, 空字串仍代表已讀過

            var hwresp = this.GetHwResponse();
            var hwrespEncoding = this.HwResponseEncoding;

            if (hwrespEncoding == null && !string.IsNullOrEmpty(hwresp.CharacterSet))
            {
                try { hwrespEncoding = Encoding.GetEncoding(hwresp.CharacterSet); }
                catch (Exception) { }
            }
            if (hwrespEncoding == null) { hwrespEncoding = Encoding.UTF8; }




            using (var wrespStream = hwresp.GetResponseStream())
            using (var memStream = new MemoryStream())
            {
                var buffer = new byte[1024];
                var cnt = 0;
                do
                {
                    cnt = wrespStream.Read(buffer, 0, buffer.Length);
                    if (cnt == 0) break;
                    memStream.Write(buffer, 0, cnt);
                } while (cnt > 0);


                memStream.Position = 0;
                memStream.Read(buffer, 0, 8);
                memStream.Position = 0;



                if (CtkFileFormat.IsGZip(buffer))
                {
                    using (var gzipStream = new GZipStream(memStream, CompressionMode.Decompress))
                    using (var reader = new StreamReader(gzipStream, hwrespEncoding))
                        return this.HwResponseData = reader.ReadToEnd();
                }
                else
                {
                    using (var reader = new StreamReader(memStream, hwrespEncoding))
                        return this.HwResponseData = reader.ReadToEnd();
                }


            }
        }
        /// <summary> Post時使用 </summary>
        public void SetFormData(String formData)
        {
            this.HwRequestData = formData;
        }

        public void SetHeaders(String headers)
        {
            var lines = headers.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var dict = new Dictionary<String, String>();

            foreach (var line in lines)
            {
                var myline = line.Trim();
                if (String.IsNullOrEmpty(myline)) continue;
                if (myline.StartsWith("//")) continue;//註解的
                var idx = myline.IndexOf(":");
                var key = myline.Substring(0, idx).Trim();
                var value = myline.Substring(idx + 1).Trim();
                dict[key] = value;
            }


            var hwreq = this.HwRequest;
            foreach (var kv in dict)
            {
                var key = kv.Key;
                var value = kv.Value;

                if (String.Compare(key, "Accept", true) == 0)
                    hwreq.Accept = value;
                else if (String.Compare(key, "Connection", true) == 0)
                {
                    if (String.Compare(value, "keep-alive", true) == 0)
                        hwreq.KeepAlive = true;
                    else
                        hwreq.Headers[key] = value;
                }
                else if (String.Compare(key, "Content-Length", true) == 0)
                    continue;
                else if (String.Compare(key, "Content-Type", true) == 0)
                    hwreq.ContentType = value;
                else if (String.Compare(key, "Host", true) == 0)
                    hwreq.Host = value;
                else if (String.Compare(key, "Referer", true) == 0)
                    hwreq.Referer = value;
                else if (String.Compare(key, "User-Agent", true) == 0)
                    hwreq.UserAgent = value;
                else
                    hwreq.Headers[key] = value;


            }



        }










        #region IDisposable
        // Flag: Has Dispose already been called?
        protected bool disposed = false;
        ~CtkNetHttpTransaction() { this.Dispose(false); }
        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // Protected implementation of Dispose pattern.
        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //

            this.DisposeClose();

            disposed = true;
        }
        void DisposeClose()
        {
            try
            {
                if (this.hwResponse != null)
                {
                    try
                    {
                        this.hwResponse.Close();
                        this.hwResponse.Dispose();
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex) { CtkLog.Write(ex); }
            //斷線不用清除Event, 但Dispsoe需要, 因為即使斷線此物件仍存活著
            CtkEventUtil.RemoveSubscriberOfObjectByFilter(this, (dlgt) => true);
        }

        #endregion







        #region === Static === === ===

        public static CtkNetHttpTransaction Create(String url)
        {
            var rs = new CtkNetHttpTransaction();
            rs.HwRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            return rs;
        }
        public static CtkNetHttpTransaction Create(String url, RequestCacheLevel cachePolicy, String httpMethod = "GET")
        {
            var rs = new CtkNetHttpTransaction();
            var req = rs.HwRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            req.CachePolicy = new System.Net.Cache.RequestCachePolicy(cachePolicy);
            req.Method = httpMethod;

            return rs;
        }

        #endregion


        #region === Static - Direct Return === === ===


        public static String HttpGet(string uri, RequestCacheLevel cachePolicy) { return HttpGet(new Uri(uri), cachePolicy); }
        public static String HttpGet(Uri uri, RequestCacheLevel cachePolicy)
        {
            WebRequest wreq = WebRequest.Create(uri);
            wreq.CachePolicy = new RequestCachePolicy(cachePolicy);
            using (var wresp = wreq.GetResponse())
            using (var wrespStream = wresp.GetResponseStream())
            using (var reader = new System.IO.StreamReader(wrespStream))
                return reader.ReadToEnd();
        }
        public static String HttpGet(string uri, Encoding encodingResp = null) { return HttpGet(new Uri(uri), encodingResp); }
        public static String HttpGet(Uri uri, Encoding encodingResp = null)
        {
            if (encodingResp == null) encodingResp = Encoding.UTF8;
            var wreq = WebRequest.Create(uri);
            using (var wresp = wreq.GetResponse())
            using (var wrespStream = wresp.GetResponseStream())
            using (var reader = new StreamReader(wrespStream, encodingResp))
                return reader.ReadToEnd();

        }


        public static String HttpPost(String uri, String contentType, String post, Encoding encodingReq = null)
        {
            if (encodingReq == null) encodingReq = Encoding.UTF8;
            byte[] byteData = encodingReq.GetBytes(post);

            HttpWebRequest wreq = null;
            Stream reqstm = null;
            HttpWebResponse wresp = null;
            StreamReader reader = null;
            try
            {
                wreq = (HttpWebRequest)WebRequest.Create(uri);
                wreq.Method = "POST";
                wreq.ContentType = contentType;
                wreq.ContentLength = byteData.Length;
                //request.Credentials = new NetworkCredential("xx", "xx"); 
                reqstm = wreq.GetRequestStream();
                reqstm.Write(byteData, 0, byteData.Length);
                wresp = (HttpWebResponse)wreq.GetResponse();
                //string responseStatus = response.StatusDescription;

                using (var wrespStream = wresp.GetResponseStream())
                {
                    reader = new System.IO.StreamReader(wrespStream);
                    return reader.ReadToEnd();
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); reader.Dispose(); }
                if (wresp != null) { wresp.Close(); }
                if (reqstm != null) { reqstm.Close(); }
            }
        }
        public static String HttpPost(String uri, String contentType, Stream stream, Encoding encodingReq = null)
        {
            if (encodingReq == null) encodingReq = Encoding.UTF8;

            HttpWebRequest wreq = null;
            Stream reqstm = null;
            HttpWebResponse wresp = null;
            StreamReader reader = null;
            try
            {
                wreq = (HttpWebRequest)WebRequest.Create(uri);
                wreq.Method = "POST";
                wreq.ContentType = contentType;
                //wreq.ContentLength = byteData.Length;
                //request.Credentials = new NetworkCredential("xx", "xx"); 
                reqstm = wreq.GetRequestStream();


                var buffer = new byte[1024];
                var count = stream.Read(buffer, 0, buffer.Length);
                while (count > 0)
                {
                    reqstm.Write(buffer, 0, buffer.Length);
                    count = stream.Read(buffer, 0, buffer.Length);
                }

                wresp = (HttpWebResponse)wreq.GetResponse();
                //string responseStatus = response.StatusDescription;

                using (var wrespStream = wresp.GetResponseStream())
                {
                    reader = new StreamReader(wrespStream);
                    return reader.ReadToEnd();
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); reader.Dispose(); }
                if (wresp != null) { wresp.Close(); }
                if (reqstm != null) { reqstm.Close(); }
            }
        }


        public static String HttpPostForm(String uri, List<KeyValuePair<string, object>> postData, Encoding encodingReq = null)
        {
            var list = new List<string>();
            foreach (var kv in postData)
            {
                var param = string.Format("{0}={1}", kv.Key, Uri.EscapeDataString(Convert.ToString(kv.Value)));
                list.Add(param);
            }
            var post = string.Join("&", list.ToArray());
            return HttpPostForm(uri, post, encodingReq);

        }
        public static String HttpPostForm(String uri, String post, Encoding encodingReq = null) { return HttpPost(uri, CtkNetHttpContentType.AppForm, post, encodingReq); }
        public static String HttpPostForm(String uri, Stream stream, Encoding encodingReq = null) { return HttpPost(uri, CtkNetHttpContentType.AppForm, stream, encodingReq); }
        public static String HttpPostJson(String uri, String post, Encoding encodingReq = null) { return HttpPost(uri, CtkNetHttpContentType.AppJson, post, encodingReq); }
        public static String HttpPostJson(String uri, Stream stream, Encoding encodingReq = null) { return HttpPost(uri, CtkNetHttpContentType.AppJson, stream, encodingReq); }

        public static String HttpRequest(HttpWebRequest wreq, string dataReq = null, Encoding encodingReq = null, Encoding encodingResp = null)
        {
            if (encodingReq == null) encodingReq = Encoding.UTF8;


            if (string.Compare(wreq.Method, "POST", true) == 0)
            {
                if (dataReq == null) dataReq = "";
                var byteData = encodingReq.GetBytes(dataReq);
                wreq.ContentLength = byteData.Length;
                using (var reqstm = wreq.GetRequestStream())
                    reqstm.Write(byteData, 0, byteData.Length);
            }



            using (var wresp = (HttpWebResponse)wreq.GetResponse())
            {
                if (encodingResp == null && !string.IsNullOrEmpty(wresp.CharacterSet))
                {
                    try { encodingResp = Encoding.GetEncoding(wresp.CharacterSet); }
                    catch (Exception) { }
                }
                if (encodingResp == null) { encodingResp = Encoding.UTF8; }

                using (var wrespStream = wresp.GetResponseStream())
                using (var reader = new System.IO.StreamReader(wrespStream, encodingResp))
                    return reader.ReadToEnd();
            }
        }





        public static Regex RegexUrl() { return new Regex(@"^(?<proto>\w+)://[^/]+?(?<port>:\d+)?/", RegexOptions.Compiled); }


        #endregion



        #region Static - Selenium


        public static async Task<CtkNetHttpGetRtn<IWebDriver>> SeleniumChromeHttpGetAsyn(String uri,
            Func<IWebDriver, bool> callback = null,
            int timeout = 30 * 1000, int delayBrowserOpen = 5 * 1000)
        {
            var rtn = new CtkNetHttpGetRtn<IWebDriver>();

            var start = DateTime.Now;

            ChromeOptions options = new ChromeOptions();
            options.AddArgument(string.Format("user-agent={0}", CtkNetUserAgents.Random().UserAgent));
            using (var driver = rtn.Driver = new ChromeDriver(options))
            {
                //開啟網頁
                driver.Navigate().GoToUrl(uri);
                //隱式等待 - 直到畫面跑出資料才往下執行, 只需宣告一次, 之後找元件都等待同樣秒數.
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(delayBrowserOpen);





                rtn.Html = await Task.Run<string>(() =>
                {
                    var interval = 500;

                    for (int idx = 0; (DateTime.Now - start).TotalMilliseconds < timeout; idx++)
                    {
                        if (string.IsNullOrEmpty(driver.PageSource))
                        {//等頁面載入完成
                            Thread.Sleep(interval);
                            continue;
                        }
                        if (callback != null && !callback(driver))
                        {//有callback 要等callback完成
                            Thread.Sleep(interval);
                            continue;
                        }

                        return driver.PageSource;
                    }
                    return null;
                });



                driver.Close();
                driver.Quit();
            }
            return rtn;


            ////輸入帳號
            //IWebElement inputAccount = driver.FindElement(By.Name("Account"));
            //Thread.Sleep(2000);
            ////清除按鈕
            //inputAccount.Clear();
            //Thread.Sleep(2000);
            //inputAccount.SendKeys("20180513");
            //Thread.Sleep(2000);

            ////輸入密碼
            //IWebElement inputPassword = driver.FindElement(By.Name("Passwrod"));

            //inputPassword.Clear();
            //Thread.Sleep(2000);
            //inputPassword.SendKeys("123456");
            //Thread.Sleep(2000);

            ////點擊執行
            //IWebElement submitButton = driver.FindElement(By.XPath("/html/body/div[2]/form/table/tbody/tr[4]/td[2]/input"));
            //Thread.Sleep(2000);
            //submitButton.Click();
            //Thread.Sleep(2000);


        }


        public static async Task<CtkNetHttpGetRtn<IWebDriver>> SeleniumRemoteChromeHttpGetAsyn(string seleniumRemoteUri, String reqUri,
            Func<IWebDriver, bool> callback = null, int timeout = 30 * 1000, int delayBrowserOpen = 5 * 1000)
        {
            var rtn = new CtkNetHttpGetRtn<IWebDriver>();
            var start = DateTime.Now;

            ChromeOptions options = new ChromeOptions();
            options.AddArgument(string.Format("user-agent={0}", CtkNetUserAgents.Random().UserAgent));
            var remote = new Uri(seleniumRemoteUri); // e.q.: http://nas002:49157/wd/hub

            using (var driver = rtn.Driver = new RemoteWebDriver(remote, options))
            {


                //開啟網頁
                driver.Navigate().GoToUrl(reqUri);
                //隱式等待 - 直到畫面跑出資料才往下執行, 只需宣告一次, 之後找元件都等待同樣秒數.
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(delayBrowserOpen);





                rtn.Html = await Task.Run<string>(() =>
                {
                    var interval = 500;

                    for (int idx = 0; (DateTime.Now - start).TotalMilliseconds < timeout; idx++)
                    {
                        if (string.IsNullOrEmpty(driver.PageSource))
                        {//等頁面載入完成
                            Thread.Sleep(interval);
                            continue;
                        }
                        if (callback != null && !callback(driver))
                        {//有callback 要等callback完成
                            Thread.Sleep(interval);
                            continue;
                        }

                        return driver.PageSource;
                    }
                    return null;
                });




                driver.Close();
                driver.Quit();
            }
            return rtn;



        }



        #endregion


    }
}
