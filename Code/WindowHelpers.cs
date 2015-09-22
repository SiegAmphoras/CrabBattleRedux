using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class WindowHelpers
{
    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    public static System.IntPtr GetWindowHandle()
    {
        return GetActiveWindow();
    }

    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner
    }

    public static UnityEngine.Rect GetWindowRectangle()
    {
        UnityEngine.Rect myRect = new UnityEngine.Rect();

        RECT rct;

        if (!GetWindowRect(new HandleRef(null, GetWindowHandle()), out rct))
        {
            //MessageBox.Show("ERROR");
            Debug.LogError("error getting window rectangle");
            return myRect;
        }
        //MessageBox.Show(rct.ToString());

        myRect.x = rct.Left;
        myRect.y = rct.Top;
        myRect.width = rct.Right - rct.Left + 1;
        myRect.height = rct.Bottom - rct.Top + 1;

        return myRect;
    }

    public static bool SetCursorPosition(int x, int y) { return SetCursorPos(x, y); }
}
