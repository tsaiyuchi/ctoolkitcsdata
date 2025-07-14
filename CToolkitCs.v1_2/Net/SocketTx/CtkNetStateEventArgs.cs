using CToolkitCs.v1_2.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CToolkitCs.v1_2.Net.SocketTx
{
    public class CtkNetStateEventArgs : CtkProtocolEventArgs
    {
        public Object Target;
        public CtkProtocolBufferMessage TrxBuffer
        {
            get
            {
                if (this.TrxMessage == null) this.TrxMessage = new CtkProtocolBufferMessage();
                if (!this.TrxMessage.Is<CtkProtocolBufferMessage>()) throw new InvalidOperationException("TrxMessage is not Buffer");
                return this.TrxMessage.As<CtkProtocolBufferMessage>();
            }
            set { this.TrxMessage = value; }
        }

        /// <summary> �����@�~��H </summary>
        public Socket TargetSocket { get { return this.Target as Socket; } set { this.Target = value; } }
        public TcpClient TargetTcpClient { get { return this.Target as TcpClient; } set { this.Target = value; } }

        /// <summary> ����������ƪ����ݦ�m, �q�`��UDP�ϥ� </summary>
        public EndPoint TargetReceiveEndPoint = new IPEndPoint(IPAddress.Any, 0);

        /// <summary> �㦳���w��H, �ҥH�i�H�����g�J </summary>
        public void WriteMsg(byte[] buff, int offset, int length)
        {
            if (this.TargetSocket != null)
            {
                if (!this.TargetSocket.Connected) return;
                this.TargetSocket.Send(buff, offset, length, SocketFlags.None);
            }
            else if (this.TargetTcpClient != null)
            {
                if (!this.TargetTcpClient.Connected) return;
                var stm = this.TargetTcpClient.GetStream();
                stm.Write(buff, offset, length);
            }

        }
        public void WriteMsg(byte[] buff, int length) { this.WriteMsg(buff, 0, length); }
        public void WriteMsg(String msg)
        {
            var buff = Encoding.UTF8.GetBytes(msg);
            this.WriteMsg(buff, 0, buff.Length);
        }
    }
}
