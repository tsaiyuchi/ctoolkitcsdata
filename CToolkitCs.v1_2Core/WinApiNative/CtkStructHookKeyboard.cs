﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkitCs.v1_2Core.WinApiNative
{



    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct CtkStructHookKeyboard
    {
        public Int16 wVk; // VirtualKeyCode
        public Int16 wScan;
        public CtkEnumKeyboardFlag dwFlags;
        public Int32 time;
        public IntPtr dwExtraInfo;
    }
}
