﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkitCs.v1_2.Protocol
{
    public class CtkProtocolReceiverCmd : Queue<String>
    {

        public List<Byte> DataBuffer = new List<byte>();
        /// <summary> Default=true : 每次 Receive 後自動 Parse, 減少使用者理解時間 </summary>
        public bool IsAutoParse = true;



        public int ParseMessage()
        {
            var dBuffer = this.DataBuffer;
            var cnt = 0;
            for (cnt = 0; cnt < 99; cnt++)
            {
                var idx = dBuffer.IndexOf(10);//找換行字元
                if (idx < 0) break;

                var msgArray = dBuffer.Take(idx);//不包含換行字元
                var msgString = Encoding.UTF8.GetString(msgArray.ToArray());
                this.Enqueue(msgString);

                dBuffer.RemoveRange(0, idx + 1);//移除 包含換行字元


            }
            return cnt;

        }

        public void Receive(Byte[] buffer)
        {
            lock (this) this.DataBuffer.AddRange(buffer);
            if (this.IsAutoParse) this.ParseMessage();
        }
        public void Receive(Byte[] buffer, int offset, int length)
        {
            var mybuffer = new Byte[length];
            Array.Copy(buffer, offset, mybuffer, 0, length);
            this.Receive(mybuffer);
        }

        public void Receive(CtkProtocolBufferMessage buffer)
        {
            var mybuffer = new Byte[buffer.Length];
            Array.Copy(buffer.Buffer, buffer.Offset, mybuffer, 0, buffer.Length);
            this.Receive(mybuffer);
        }
    }
}
