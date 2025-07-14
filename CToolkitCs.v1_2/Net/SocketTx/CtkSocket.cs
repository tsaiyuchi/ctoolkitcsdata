﻿using CToolkitCs.v1_2.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CToolkitCs.v1_2.Net.SocketTx
{
    public abstract class CtkSocket : ICtkProtocolNonStopConnect, IDisposable
    {

        /// <summary> Client? or Listener </summary>
        public bool IsClient = false;
        /// <summary> 自動聆聽開啟時, 會在每次收到連線後 再繼續聆聽下一個 </summary>
        public bool IsAsynAutoListen = true;
        /// <summary>
        /// 若開啟自動讀取,
        /// 在連線完成 及 讀取完成 時, 會自動開始下一次的讀取.
        /// 這不適用在Sync作業, 因為Sync讀完會需要使用者處理後續.
        /// 因此只有非同步類的允許自動讀取
        /// 預設=true, 邏輯上來說, 使用者不希望太複雜的控制.
        /// </summary>
        public bool IsAsynAutoReceive = true;
        public Uri LocalUri;
        public String Name;
        /// <summary> 
        /// TCP/Listen 不需指定 ;
        /// TCP/Client 需指定 ;
        /// UDP/Listen 指定接受來源, 可指定 0.0.0.0:0 = IPAddress.Any ;
        /// UDP/Client 若指定表示預設發送對象 ;
        /// </summary>
        public Uri RemoteUri;

        public ConcurrentQueue<EndPoint> TargetEndPoints = new ConcurrentQueue<EndPoint>();
        public ConcurrentQueue<Socket> TargetSockets = new ConcurrentQueue<Socket>();

        bool m_isReceiveLoop = false;
        /// <summary>
        /// 不能用 AutoResetEvent.
        /// WaitOne 會自動 Set.
        /// 但有時WaintOne只是拿來判斷是否在忙.
        /// </summary>
        ManualResetEvent mreIsConnecting = new ManualResetEvent(true);
        ManualResetEvent mreIsReceiving = new ManualResetEvent(true);

        ~CtkSocket() { this.Dispose(false); }

        /// <summary> 同步時的重複接收旗標 </summary>
        public bool IsReceiveLoop { get { return m_isReceiveLoop; } private set { lock (this) m_isReceiveLoop = value; } }

        public bool IsWaitReceive { get { return this.mreIsReceiving.WaitOne(10); } }



        public Socket SocketConn { get; protected set; }
        /// <summary>
        /// 開始讀取Socket資料, Begin 代表非同步.
        /// 用於 1. IsAsynAutoReceive, 每次讀取需自行執行;
        ///     2. 若連線還在, 但讀取異常中止, 可以再度開始;
        /// </summary>
        public void BeginReceive()
        {
            var myea = new CtkNetStateEventArgs();

            //採用現正操作中的Socket進行接收
            var target = this.ActiveTarget as Socket;
            myea.Sender = this;
            myea.TargetSocket = target;
            var trxBuffer = myea.TrxBuffer;
            target.BeginReceive(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length, SocketFlags.None, new AsyncCallback(EndReceiveCallback), myea);

            //EndReceive 是由 BeginReceive 回呼的函式中去執行
            //也就是收到後, 通知結束工作的函式
            //你無法在其它地方呼叫, 因為你沒有 IAsyncResult 的物件
        }

        public void CloseIfUnReadable()
        {
            if (this.IsOpenConnecting) return;//開啟中 不執行
            if (this.SocketConn == null) return;//沒連線 不執行
            if (this.TargetSockets.Count == 0) return;//沒連線 不執行
            if (this.TargetTestReadableAll() > 0) return;//有可抵達的連線 不執行

            this.Disconnect();//先斷線
        }

        /// <summary> 指定的 Socket or Last </summary>
        public int ReceiveLoop()
        {
            try
            {
                this.IsReceiveLoop = true;
                while (this.IsReceiveLoop && !this.disposed)
                {
                    this.ReceiveOnce();
                }
            }
            catch (Exception ex)
            {
                this.IsReceiveLoop = false;
                throw ex;//同步型作業, 直接拋出例外, 不用寫Log
            }
            return 0;
        }
        public void ReceiveLoopCancel()
        {
            this.IsReceiveLoop = false;
        }
        public int ReceiveOnce()
        {
            var mySocketActive = this.TargetGetSocket();
            try
            {
                if (!Monitor.TryEnter(this, 1000)) return -1;//進不去先離開
                if (!this.mreIsReceiving.WaitOne(10)) return 0;//接收中先離開
                this.mreIsReceiving.Reset();//先卡住, 不讓後面的再次進行

                var ea = new CtkProtocolEventArgs()
                {
                    Sender = this,
                };

                ea.TrxMessage = new CtkProtocolBufferMessage(1024);
                var trxBuffer = ea.TrxMessage.TxBuffer();

                trxBuffer.Length = mySocketActive.Receive(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length, SocketFlags.None);
                if (trxBuffer.Length == 0) return -1;
                this.OnDataReceive(ea);
            }
            catch (Exception ex)
            {
                this.OnErrorReceive(new CtkProtocolEventArgs() { Message = "Read Fail" });
                //當 this.ConnSocket == this.WorkSocket 時, 代表這是 client 端
                this.Disconnect();
                if (this.SocketConn != mySocketActive)
                    CtkNetUtil.DisposeSocketTry(mySocketActive);//執行出現例外, 先釋放Socket
                throw ex;//同步型作業, 直接拋出例外, 不用寫Log
            }
            finally
            {
                try { this.mreIsReceiving.Set(); /*同步型的, 結束就可以Set*/ }
                catch (ObjectDisposedException) { }
                if (Monitor.IsEntered(this)) Monitor.Exit(this);
            }
            return 0;
        }



        public void WriteMsgTo(CtkProtocolTrxMessage msg, Uri uri)
        {
            var ep = CtkNetUtil.ToIPEndPoint(uri);
            this.WriteMsgTo(msg, ep);
        }
        public void WriteMsgTo(CtkProtocolTrxMessage msg, EndPoint ep)
        {
            if (this.SocketConn.ProtocolType == ProtocolType.Tcp)
            {
                var socket = this.TargetGetSocketActiveOrLast(ep);
                this.WriteMsgTo_Socket(msg, socket);
            }
            else if (this.SocketConn.ProtocolType == ProtocolType.Udp)
            {
                this.WriteMsgTo_Ep(msg, ep);
            }
        }
        protected void WriteMsgTo_Ep(CtkProtocolTrxMessage msg, EndPoint ep)
        {
            try
            {
                //寫入不卡Monitor, 並不會造成impact

                if (this.SocketConn.ProtocolType != ProtocolType.Udp)
                    throw new CtkSocketException($"Only UDP can send by EndPoint");

                var buffer = msg.TxBuffer();
                this.SocketConn.SendTo(buffer.Buffer, buffer.Offset, buffer.Length, SocketFlags.None, ep);
                return;
            }
            catch (Exception ex)
            {
                this.Disconnect();//寫入失敗就斷線
                CtkLog.WarnAn(this, ex);
                throw ex;//就例外就拋出, 不吃掉
            }
        }
        protected void WriteMsgTo_Socket(CtkProtocolTrxMessage msg, Socket socket)
        {
            try
            {
                //寫入不卡Monitor, 並不會造成impact
                //但如果卡了Monitor, 你無法同時 等待Receive 和 要求Send

                //其它作業可以卡 Monitor.TryEnter
                //此物件會同時進行的只有 Receive 和 Send
                //所以其它作業卡同一個沒問題: Monitor.TryEnter(this, 1000)
                if (this.SocketConn.ProtocolType != ProtocolType.Tcp)
                    throw new CtkSocketException($"Only TCP can send by Socket");
                var buffer = msg.TxBuffer();
                socket.Send(buffer.Buffer, buffer.Offset, buffer.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                this.Disconnect();//寫入失敗就斷線
                CtkLog.WarnAn(this, ex);
                throw ex;//就例外就拋出, 不吃掉
            }
        }




        #region Target Socket Function


        public bool TargetCleanInvalid()
        {
            try
            {
                if (!Monitor.TryEnter(this.TargetSockets, 1000)) return false;
                var list = new List<Socket>();
                Socket client = null;
                while (!this.TargetSockets.IsEmpty)
                {
                    if (!this.TargetSockets.TryDequeue(out client)) break;
                    if (client != null && client.Connected)
                    {
                        list.Add(client);
                    }
                    else
                    {
                        this.TargetClose(client);
                    }
                }

                foreach (var tc in list)
                    this.TargetSockets.Enqueue(tc);

                return true;
            }
            catch (Exception ex)
            {
                CtkLog.Write(ex);
                return false;
            }
            finally { Monitor.Exit(this.TargetSockets); }
        }



        /// <summary> 關閉 預設 對象Socket </summary>
        public bool TargetClose()
        {
            var socket = this.TargetGetSocket();
            return this.TargetClose(socket);
        }
        /// <summary> 關閉 指定 對象Socket </summary>
        public bool TargetClose(int index)
        {
            var socket = this.TargetSockets.ElementAt(index);
            return this.TargetClose(socket);
        }
        /// <summary> 關閉 指定 對象Socket </summary>
        public bool TargetClose(Uri uri)
        {
            var socket = this.TargetGetSocket(uri);
            return this.TargetClose(socket);
        }
        /// <summary> 關閉 指定 對象Socket </summary>
        public bool TargetClose(EndPoint ep)
        {
            var socket = this.TargetGetSocket(ep);
            return this.TargetClose(socket);
        }
        /// <summary> 關閉 全部 對象Socket </summary>
        public int TargetCloseAll()
        {
            var count = 0;
            var list = this.TargetSockets.ToList();
            foreach (var socket in list)
                count += this.TargetClose(socket) ? 1 : 0;
            return count;
        }



        /// <summary> 取得 預設 對象Socket </summary>
        public Socket TargetGetSocket()
        {
            var rtn = this.ActiveTarget as Socket;
            if (rtn != null) return rtn;

            return this.TargetSockets.LastOrDefault();//最後取得的
        }
        /// <summary> 取得 指定 對象Socket </summary>
        public Socket TargetGetSocket(EndPoint ep)
        {
            if (ep == null) return null;
            var list = this.TargetSockets.ToList();
            foreach (var socket in list)
            {
                if (socket == null) continue;
                if (!socket.Connected) continue;
                if (ep.Equals(socket.RemoteEndPoint)) return socket;
            }
            return null;
        }
        /// <summary> 取得 指定 對象Socket </summary>
        public Socket TargetGetSocket(Uri uri)
        {
            if (uri == null) return null;
            var ep = CtkNetUtil.ToIPEndPoint(uri);
            return this.TargetGetSocket(ep);
        }



        /// <summary> 取得 指定 或 預設 對象Socket </summary>
        public Socket TargetGetSocketActiveOrLast(EndPoint ep)
        {
            var rtn = this.TargetGetSocket(ep);
            if (rtn != null) return rtn;

            return this.TargetGetSocket();
        }
        public Socket TargetGetSocketActiveOrLast(Uri uri)
        {
            var rtn = this.TargetGetSocket(uri);
            if (rtn != null) return rtn;

            return this.TargetGetSocket();
        }

        /// <summary> 測試 預設 對象 </summary>
        public bool TargetTestReadable()
        {
            var socket = this.TargetGetSocket();
            return this.TargetTestReadable(socket);
        }
        /// <summary> 測試 指定 對象 </summary>
        public bool TargetTestReadable(Uri uri)
        {
            var socket = this.TargetGetSocket(uri);
            return this.TargetTestReadable(socket);
        }
        /// <summary> 測試 指定 對象 </summary>
        public bool TargetTestReadable(EndPoint ep)
        {
            var socket = this.TargetGetSocket(ep);
            return this.TargetTestReadable(socket);
        }
        /// <summary> 測試 全部 對象 </summary>
        public int TargetTestReadableAll()
        {
            var count = 0;
            var list = this.TargetSockets.ToList();
            foreach (var socket in list)
                count += this.TargetTestReadable(socket) ? 1 : 0;
            return count;
        }


        protected bool TargetClose(Socket socket)
        {
            if (socket == null) return false;
            try
            {
                CtkNetUtil.DisposeSocketTry(socket);
                return true;
            }
            catch { return false; }
        }
        protected bool TargetTestReadable(Socket socket)
        {
            if (socket == null) return false;
            if (!socket.Connected) return false;//沒有連線不用測連線
            return !(socket.Poll(1000, SelectMode.SelectRead) && (socket.Available == 0));
        }

        #endregion



        #region ICtkProtocolConnect

        public event EventHandler<CtkProtocolEventArgs> EhDataReceive;
        public event EventHandler<CtkProtocolEventArgs> EhDisconnect;
        public event EventHandler<CtkProtocolEventArgs> EhErrorReceive;
        public event EventHandler<CtkProtocolEventArgs> EhFailConnect;
        public event EventHandler<CtkProtocolEventArgs> EhFirstConnect;

        /// <summary> 設定主要通訊對象 </summary>
        public object ActiveTarget { get; set; }

        /// <summary> Local 設置好+Bound 就算完成 </summary>
        public bool IsLocalPrepared { get { return this.SocketConn != null && this.SocketConn.IsBound; } }
        /// <summary> 嘗試建立連線都算, 包含聆聽者等待連線以及開始下次聆聽中 </summary>
        public bool IsOpenConnecting { get { return !this.mreIsConnecting.WaitOne(10); } }
        /// <summary> 確實有建立連線的對象 </summary>
        public bool IsRemoteConnected { get { this.TargetCleanInvalid(); return this.TargetSockets.Count > 0 || this.TargetEndPoints.Count > 0; } }





        /// <summary> 暫不支援同步型, 很少使用 </summary>
        public int ConnectTry() { throw new NotImplementedException(); }
        public int ConnectTryStart()
        {
            /*Client: 
             * 連線中=嘗試不需重啟; 
             * 已連線=已可通訊不需重啟; 
             * 若不在連線中也沒對象=會嘗試重啟連線.
             * Local 準備好不代表連線成功.
             */
            /*Listener: 
             * 連線中=包含重複聆聽, 不需重啟. 若非連線中=不再Accept; 
             * 已連線=已有建立通訊的對象;
             * 若不在連線中也沒對象=會嘗試重啟連線
             * Local 準備好不代表連線成功 也不代表聆聽中
             */
            if (this.IsOpenConnecting || this.IsRemoteConnected) return 0;


            try
            {
                if (!Monitor.TryEnter(this, 1000)) return -1;//進不去先離開
                this.TargetCleanInvalid();
                if (!this.mreIsConnecting.WaitOne(10)) return 0;//連線中先離開 = IsOpenRequesting
                this.mreIsConnecting.Reset();//先卡住, 不讓後面的再次進行



                //若連線不曾建立, 或聆聽/連線被關閉
                if (this.SocketConn == null || !this.SocketConn.Connected)
                {
                    CtkNetUtil.DisposeSocketTry(this.SocketConn);//Dispose舊的
                    this.SocketConn = this.CreateSocket();//建立新的
                }


                var myea = new CtkNetStateEventArgs();
                myea.Sender = this;

                if (this.IsClient)
                {//Client + TCP/UDP
                    if (this.LocalUri != null && !this.SocketConn.IsBound)
                        this.SocketConn.Bind(CtkNetUtil.ToIPEndPoint(this.LocalUri));
                    if (this.RemoteUri == null)
                        throw new CtkSocketException("remote field can not be null");

                    if (this.SocketConn.ProtocolType == ProtocolType.Tcp
                        || this.SocketConn.ProtocolType == ProtocolType.Udp)
                    {
                        myea.TargetReceiveEndPoint = CtkNetUtil.ToIPEndPoint(this.RemoteUri);//主動連線必須指定 IP:Port;
                        this.SocketConn.BeginConnect(myea.TargetReceiveEndPoint, new AsyncCallback(EndConnectCallback), myea);
                    }
                    else { throw new CtkSocketException("Not support protocol type"); }
                }
                else
                {//Listener + TCP/UDP
                    if (this.LocalUri == null)
                        throw new Exception("local field can not be null");
                    if (!this.SocketConn.IsBound)
                        this.SocketConn.Bind(CtkNetUtil.ToIPEndPoint(this.LocalUri));

                    if (this.SocketConn.ProtocolType == ProtocolType.Tcp)
                    { /* TCP 需要 Handshake = Accept 連線*/
                        this.SocketConn.Listen(100);
                        this.SocketConn.BeginAccept(new AsyncCallback(EndAcceptCallback), myea);
                    }
                    else if (this.SocketConn.ProtocolType == ProtocolType.Udp)
                    { /* UDP 不需 Handshake = 直接開始接收*/

                        myea.TargetSocket = this.SocketConn;
                        this.TargetSockets.Enqueue(myea.TargetSocket);
                        var trxmBuffer = myea.TrxBuffer;

                        this.SocketConn.BeginReceiveFrom(trxmBuffer.Buffer, 0, trxmBuffer.Buffer.Length, SocketFlags.None, ref myea.TargetReceiveEndPoint, new AsyncCallback(EndReceiveFromCallback), myea);
                    }
                    else { throw new CtkSocketException("Not support protocol type"); }
                }

                return 0;
            }
            catch (Exception ex)
            {
                //一旦聆聽/連線失敗, 直接關閉所有Socket, 重新來過
                this.Disconnect();
                this.OnFailConnect(new CtkProtocolEventArgs() { Message = "Connect Fail" });
                throw ex;//同步型作業, 直接拋出例外, 不用寫Log
            }
            finally
            {
                if (Monitor.IsEntered(this)) Monitor.Exit(this);
            }
        }

        public void Disconnect()
        {
            try { this.mreIsReceiving.Set();/*僅Set不釋放, 可能還會使用*/ } catch (ObjectDisposedException) { }
            try { this.mreIsConnecting.Set();/*僅Set不釋放, 可能還會使用*/ } catch (ObjectDisposedException) { }

            // Socket斷開後, 此物件無法再使用 等同 Released, 所以直接 Dispose, 若仍需要 請另開
            foreach (var socket in this.TargetSockets) CtkNetUtil.DisposeSocketTry(socket);
            CtkNetUtil.DisposeSocketTry(this.SocketConn);
            this.OnDisconnect(new CtkProtocolEventArgs() { Message = "Disconnect method is executed" });
        }



        public void WriteMsg(CtkProtocolTrxMessage msg)
        {
            if (this.SocketConn.ProtocolType == ProtocolType.Udp)
                throw new CtkSocketException("UDP can not use this method, it must assign IP:Port");
            var socket = this.TargetGetSocket();
            this.WriteMsgTo_Socket(msg, socket);
        }


        #endregion



        #region ICtkProtocolNonStopConnect

        public int NonStopIntervalTimeOfConnectCheck { get; set; }
        public bool IsNonStopRunning { get; set; }
        public void NonStopRunStart()
        {
            throw new NotImplementedException();
        }
        public void NonStopRunStop()
        {
            throw new NotImplementedException();
        }

        #endregion



        #region Callback

        /// <summary>
        /// Server Accept End
        /// </summary>
        /// <param name="ar"></param>
        void EndAcceptCallback(IAsyncResult ar)
        {
            var nextState = new CtkNetStateEventArgs(); //每個新連線都有新的 EventArgs
            var trxmBuffer = nextState.TrxBuffer;
            Socket target = null;
            try
            {
                Monitor.Enter(this);//一定要等到進去
                var state = (CtkNetStateEventArgs)ar.AsyncState;
                var ctk = state.Sender as CtkSocket;
                target = ctk.SocketConn.EndAccept(ar);
                ctk.TargetSockets.Enqueue(target);

                nextState.Sender = ctk;
                nextState.TargetSocket = target; //觸發EndAccept的Socket未必是同一個, 因此註明 TargetSocket

                if (!ar.IsCompleted || target == null || !target.Connected)
                    throw new CtkSocketException("Connection Fail");


                //呼叫他人不應影響自己運作, catch起來
                try { this.OnFirstConnect(nextState); }
                catch (Exception ex) { CtkLog.WarnAn(this, ex); }


                if (this.IsAsynAutoListen)
                {
                    try { ctk.SocketConn.BeginAccept(new AsyncCallback(EndAcceptCallback), nextState); }
                    catch (Exception) { this.mreIsConnecting.Set(); /*例外導致不再Listen的話 = 非連線中*/ }
                }
                else { this.mreIsConnecting.Set(); /*不再Listen的話 = 非連線中*/ }

                if (this.IsAsynAutoReceive)
                    target.BeginReceive(trxmBuffer.Buffer, 0, trxmBuffer.Buffer.Length, SocketFlags.None, new AsyncCallback(EndReceiveCallback), nextState);
            }
            catch (Exception ex)
            {
                CtkNetUtil.DisposeSocketTry(target);//失敗清除, Listener不用
                nextState.Message = ex.Message;
                nextState.Exception = ex;
                this.OnFailConnect(nextState);
                CtkLog.WarnAn(this, ex);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        /// <summary>
        /// Clinet Connect End
        /// </summary>
        /// <param name="ar"></param>
        void EndConnectCallback(IAsyncResult ar)
        {
            var nextState = new CtkNetStateEventArgs(); //每個新連線都有新的 EventArgs
            var trxBuffer = nextState.TrxBuffer;
            Socket target = null;
            try
            {
                Monitor.Enter(this);//一定要等到進去
                var state = (CtkNetStateEventArgs)ar.AsyncState;
                var ctk = state.Sender as CtkSocket;

                ctk.SocketConn.EndConnect(ar);
                target = ctk.SocketConn; //作為Client時, Target = Conn
                ctk.TargetSockets.Enqueue(target);//TCP/UDP Client時, 都是放自身Socket

                nextState.Sender = ctk;
                nextState.TargetSocket = target;

                if (!ar.IsCompleted || target == null || !target.Connected)
                    throw new CtkSocketException("Connection Fail");


                //呼叫他人不應影響自己運作, catch起來
                try { this.OnFirstConnect(nextState); }
                catch (Exception ex) { CtkLog.WarnAn(this, ex); }


                if (this.IsAsynAutoReceive)
                {
                    if (ctk.SocketConn.ProtocolType == ProtocolType.Tcp)
                    {
                        target.BeginReceive(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length, SocketFlags.None, new AsyncCallback(EndReceiveCallback), nextState);
                    }
                    else if (ctk.SocketConn.ProtocolType == ProtocolType.Udp)
                    {
                        target.BeginReceiveFrom(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length, SocketFlags.None, ref nextState.TargetReceiveEndPoint, new AsyncCallback(EndReceiveFromCallback), nextState);
                    }
                    else { throw new CtkSocketException("Not support protocol type"); }
                }
            }
            catch (Exception ex)
            {
                //失敗就中斷連線, 清除
                CtkNetUtil.DisposeSocketTry(target);
                nextState.Message = ex.Message;
                nextState.Exception = ex;
                this.OnFailConnect(nextState);
                CtkLog.WarnAn(this, ex);
            }
            finally
            {
                try { this.mreIsConnecting.Set(); /*同步型的, 結束就可以Set*/ }
                catch (ObjectDisposedException) { }
                Monitor.Exit(this);
            }
        }
        /// <summary>
        /// Client/Server Receive End
        /// </summary>
        /// <param name="ar"></param>
        void EndReceiveCallback(IAsyncResult ar)
        {
            var state = (CtkNetStateEventArgs)ar.AsyncState;
            var ctk = state.Sender as CtkSocket;
            var targetSocket = state.TargetSocket;
            var trxBuffer = state.TrxBuffer;
            try
            {

                if (!ar.IsCompleted || targetSocket == null)
                    throw new CtkSocketException("Read Fail");
                if (ctk.SocketConn.ProtocolType == ProtocolType.Tcp && !targetSocket.Connected)
                    throw new CtkSocketException("Read Fail");

                var bytesRead = targetSocket.EndReceive(ar);
                trxBuffer.Length = bytesRead;


                //呼叫他人不應影響自己運作, catch起來
                try { this.OnDataReceive(state); }
                catch (Exception ex) { CtkLog.Write(ex); }

                if (this.IsAsynAutoReceive)
                    targetSocket.BeginReceive(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length, SocketFlags.None, new AsyncCallback(EndReceiveCallback), state);
            }
            catch (Exception ex)
            {
                CtkNetUtil.DisposeSocketTry(targetSocket);//僅關閉讀取失敗的連線
                state.Message = ex.Message;
                state.Exception = ex;
                this.OnErrorReceive(state);//但要呼叫 OnErrorReceive
                CtkLog.WarnAn(this, ex);
            }
            finally
            {
                //會有多個Socket進入 Receive Callback, 所以不用 Reset Event
            }

        }
        void EndReceiveFromCallback(IAsyncResult ar)
        {
            var state = (CtkNetStateEventArgs)ar.AsyncState;
            var ctk = state.Sender as CtkSocket;
            var targetSocket = state.TargetSocket;
            var trxBuffer = state.TrxBuffer;
            try
            {

                if (!ar.IsCompleted || targetSocket == null)
                    throw new CtkSocketException("Read Fail");

                var bytesRead = targetSocket.EndReceiveFrom(ar, ref state.TargetReceiveEndPoint);
                trxBuffer.Length = bytesRead;

                //儲存曾接收資料的對象
                var targetEndPoint = state.TargetReceiveEndPoint;
                if (!this.TargetEndPoints.Contains(targetEndPoint))
                {/* Linq 的 Contains 採用 Equals, 所以內容相同就算是 Contain */
                    this.TargetEndPoints.Enqueue(targetEndPoint);
                }

                //呼叫他人不應影響自己運作, catch起來
                try { this.OnDataReceive(state); }
                catch (Exception ex) { CtkLog.Write(ex); }


                if (this.IsAsynAutoReceive)
                    targetSocket.BeginReceiveFrom(trxBuffer.Buffer, 0, trxBuffer.Buffer.Length, SocketFlags.None, ref state.TargetReceiveEndPoint, new AsyncCallback(EndReceiveFromCallback), state);
            }
            catch (Exception ex)
            {
                CtkNetUtil.DisposeSocketTry(targetSocket);//僅關閉讀取失敗的連線
                state.Message = ex.Message;
                state.Exception = ex;
                this.OnErrorReceive(state);//但要呼叫 OnErrorReceive
                CtkLog.WarnAn(this, ex);
            }
            finally
            {
                //會有多個Socket進入 Receive Callback, 所以不用 Reset Event
            }

        }


        #endregion


        #region 連線模式 Method

        public abstract Socket CreateSocket();

        #endregion




        #region Event

        protected void OnDataReceive(CtkProtocolEventArgs ea)
        {
            if (this.EhDataReceive == null) return;
            this.EhDataReceive(this, ea);
        }
        protected void OnDisconnect(CtkProtocolEventArgs ea)
        {
            if (this.EhDisconnect == null) return;
            this.EhDisconnect(this, ea);
        }
        protected void OnErrorReceive(CtkProtocolEventArgs ea)
        {
            if (this.EhErrorReceive == null) return;
            this.EhErrorReceive(this, ea);
        }
        protected void OnFailConnect(CtkProtocolEventArgs ea)
        {
            if (this.EhFailConnect == null) return;
            this.EhFailConnect(this, ea);
        }
        protected void OnFirstConnect(CtkProtocolEventArgs ea)
        {
            if (this.EhFirstConnect == null) return;
            this.EhFirstConnect(this, ea);
        }

        #endregion






        #region Dispose

        bool disposed = false;
        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }
        public void DisposeClose()
        {
            this.Disconnect();
            CtkUtil.DisposeObjTry(this.mreIsConnecting);
            CtkUtil.DisposeObjTry(this.mreIsReceiving);
            CtkEventUtil.RemoveSubscriberOfObjectByFilter(this, (dlgt) => true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                // Free any managed objects here.
            }
            // Free any unmanaged objects here.
            //
            this.DisposeClose();
            disposed = true;
        }

        #endregion





    }
}
