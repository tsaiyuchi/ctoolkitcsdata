﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CToolkitCs.v1_2Core.WinApiNative
{
    public class CtkKernel32Lib
    {

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [DllImport("kernel32.dll", EntryPoint= "SetSystemTime", SetLastError = true)]
        public static extern bool SetSystemTime(ref CtkStructSystemTime st);

    }
}
