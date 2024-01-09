using System;
using System.Collections.Generic;
using System.Text;

namespace NetTubeClean.Sample01.Operator
{
    public enum ENetTubeCleanCommand
    {
        None,

        /// <summary> Read Dxxxx Hold Register </summary>
        ReadData,
        /// <summary> Write Dxxxx Hold Register </summary>
        WriteData,

        /// <summary> Read Yxxx Coil </summary>
        ReadCoil,
        /// <summary> Write Yxxx Coil </summary>
        WriteCoil,


    }
}
