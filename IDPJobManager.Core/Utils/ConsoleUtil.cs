using System;

namespace IDPJobManager.Core.Utils
{
    /// <summary>
    /// Console Util class 
    /// </summary>
    public class ConsoleUtil
    {
        public static event ConsoleCtrlDelegate OnClose;

        public static void DisableCloseButton(string consoleName)
        {

            IntPtr windowHandle = NativeWindowApiUtil.FindWindow(null, consoleName);
            IntPtr closeMenu = NativeWindowApiUtil.GetSystemMenu(windowHandle, IntPtr.Zero);
            uint scClose = 0xF060;
            NativeWindowApiUtil.RemoveMenu(closeMenu, scClose, 0x0);
        }

        public static void DisableCloseButton()
        {
            DisableCloseButton(Console.Title);
        }

        public static void RegisterCloseConsoleHandle()
        {
            NativeWindowApiUtil.SetConsoleCtrlHandler(ct => {
                if (OnClose != null)
                    return OnClose(ct);
                return false;
            }, true);
        }
    }

    public class ConsoleCloseEventArgs : EventArgs
    {
        public CloseConsoleType CloseType { get; set; }
        public bool IsCancel { get; set; }
    }
}
