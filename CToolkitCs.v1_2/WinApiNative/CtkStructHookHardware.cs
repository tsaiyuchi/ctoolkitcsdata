﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkitCs.v1_2.WinApiNative
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct CtkStructHookHardware
    {
        public Int32 uMsg;
        public Int16 wParamL;
        public Int16 wParamH;
    }

}
