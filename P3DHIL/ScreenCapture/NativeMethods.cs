using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
 
namespace Spazzarama.ScreenCapture
{
    #region Native Win32 Interop
    [SuppressUnmanagedCodeSecurity()]
    internal sealed class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
 
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
 
        /// <summary>
        /// Get a windows client rectangle in a .NET structure
        /// </summary>
        /// <param name="hwnd">The window handle to look up</param>
        /// <returns>The rectangle</returns>
        internal static Rectangle GetClientRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetClientRect(hwnd, out rect);
            return rect.AsRectangle;
        }
 
        /// <summary>
        /// Get a windows rectangle in a .NET structure
        /// </summary>
        /// <param name="hwnd">The window handle to look up</param>
        /// <returns>The rectangle</returns>
        internal static Rectangle GetWindowRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hwnd, out rect);
            return rect.AsRectangle;
        }
 
        internal static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
        {
            Rectangle windowRect = NativeMethods.GetWindowRect(hWnd);
            Rectangle clientRect = NativeMethods.GetClientRect(hWnd);
 
            // This gives us the width of the left, right and bottom chrome - we can then determine the top height
            int chromeWidth = (int)((windowRect.Width - clientRect.Width) / 2);
 
            return new Rectangle(new Point(windowRect.X + chromeWidth, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
        }
    }
    #endregion
}