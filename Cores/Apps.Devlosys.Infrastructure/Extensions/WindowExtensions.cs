using System;
using System.Runtime.InteropServices;

namespace System.Windows
{
    public static class WindowExtensions
    {
        #region Win32 API functions

        //private const int SW_SHOW_NORMAL = 1;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        #endregion

        public static void ActivateWindow(this IntPtr @this)
        {
            if (IsIconic(@this))
            {
                _ = ShowWindowAsync(@this, SW_RESTORE);
            }

            //ShowWindowAsync(@this, IsIconic(@this) ? SW_RESTORE : SW_SHOW_NORMAL);

            _ = SetForegroundWindow(@this);
            _ = FlashWindow(@this, true);
        }
    }
}
