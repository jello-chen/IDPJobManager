using System;
using System.Runtime.InteropServices;

namespace IDPJobManager.Core.Utils
{

    public delegate bool ConsoleCtrlDelegate(int ctrlType);

    public class NativeWindowApiUtil
    {

        #region API

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        public static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);

        #endregion
    }

    public enum CloseConsoleType
    {
        ///
        /// Ctrl+C
        ///
        CtrlCEvent = 0,

        ///
        /// Ctrl+break
        ///
        CtrlBreakEvent = 1,

        ///
        /// Close manually
        ///
        CloseEvent = 2,
        
        ///
        /// Log off
        ///
        LogoffEvent = 5,
        
        ///
        /// Shutdown system
        ///
        ShutdownEvent = 6,
    }
}
