using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Media;
using System.Reflection.Emit;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Runtime.Remoting.Contexts;
using System.Xml.Linq;

namespace ColorYoink
{
    public static class Program
    {
        public struct FormatData
        {
            public uint format;
            public string name;
            public int length;
            public byte[] payload;
            public string text;
        }
        [STAThread]
        public static void Main(string[] args)
        {
            /*List<FormatData> formats = new List<FormatData>();

            PInvoke.OpenClipboard(IntPtr.Zero);
            uint formatInt = 0;
            while ((formatInt = PInvoke.EnumClipboardFormats(formatInt)) != 0)
            {
                FormatData format = new FormatData();
                format.format = formatInt;
                StringBuilder formatName = new StringBuilder(256);
                PInvoke.GetClipboardFormatName(formatInt, formatName, formatName.Capacity);
                format.name = formatName.ToString();
                IntPtr hGlobal = PInvoke.GetClipboardData(formatInt);
                if (hGlobal == IntPtr.Zero)
                {
                    format.text = "";
                    format.payload = null;
                }
                else
                {
                    IntPtr clipboardContentsPointer = PInvoke.GlobalLock(hGlobal);
                    if (clipboardContentsPointer == IntPtr.Zero)
                    {
                        format.text = "";
                        format.payload = null;
                    }
                    else
                    {
                        format.text = Marshal.PtrToStringAuto(clipboardContentsPointer);
                        int size = PInvoke.GlobalSize(hGlobal);
                        format.payload = new byte[size];
                        Marshal.Copy(clipboardContentsPointer, format.payload, 0, size);
                        PInvoke.GlobalUnlock(hGlobal);
                    }
                }
                formats.Add(format);
            }

            // Close the clipboard
            PInvoke.CloseClipboard();

            Console.ReadLine();
            return;*/

            GlobalHotkey CtrlAltC = new GlobalHotkey(Keys.C, HotkeyModifier.Control | HotkeyModifier.Alt | HotkeyModifier.NoRepeat, YoinkColor);
            CtrlAltC.Register();

            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("icon.ico");
            notifyIcon.Visible = true;

            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit ColorYoink");
            exitMenuItem.Click += new EventHandler((object sender, EventArgs e) =>
                {
                    CtrlAltC.Unregister();
                    notifyIcon.Dispose();
                    Environment.Exit(0);
                });
            menu.Items.Add(exitMenuItem);

            notifyIcon.ContextMenuStrip = menu;

            while (true)
            {
                Application.DoEvents();
            }
        }
        public static void YoinkColor()
        {
            try
            {
                ColorYoink.SetClipboardToPNG(ColorYoink.ColorToBitmap(ColorYoink.GetColorFromScreen(ColorYoink.GetCursorPosition()), 16, 16));
                SystemSounds.Asterisk.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    public static class ColorYoink
    {
        #region Public Methods
        public static void SetClipboardToString(string source)
        {
            Clipboard.SetText(source);
        }
        public static void SetClipboardToImage(Bitmap source)
        {
            Clipboard.SetImage(source);
        }
        public static Bitmap clipboardContents = null;
        public static void SetClipboardToPNG(Bitmap source)
        {
            // Open the clipboard
            if (!PInvoke.OpenClipboard(IntPtr.Zero))
            {
                throw new Exception("Failed to open clipboard.");
            }

            // Empty the clipboard
            if (!PInvoke.EmptyClipboard())
            {
                PInvoke.CloseClipboard();
                throw new Exception("Failed to empty clipboard.");
            }

            // Set the clipboard data to the bitmap
            IntPtr hBitmap = source.GetHbitmap();
            IntPtr result = PInvoke.SetClipboardData(2 /* CF_BITMAP */, hBitmap);
            clipboardContents = source;

            // Check if setting the clipboard data was successful
            if (result == IntPtr.Zero)
            {
                PInvoke.CloseClipboard();
                throw new Exception("Failed to set clipboard data.");
            }

            // Close the clipboard
            if (!PInvoke.CloseClipboard())
            {
                throw new Exception("Failed to close clipboard.");
            }

            while (true)
            {

            }
        }
        public static Color GetColorFromScreen(Point point)
        {
            Bitmap bitmap = new Bitmap(1, 1);

            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(point.X, point.Y, 0, 0, new Size(1, 1));

            graphics.Dispose();

            Color output = bitmap.GetPixel(0, 0);

            bitmap.Dispose();

            return output;
        }
        public static Color GetColorFromScreen(int x, int y)
        {
            Bitmap bitmap = new Bitmap(1, 1);

            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(x, y, 0, 0, new Size(1, 1));

            graphics.Dispose();

            Color output = bitmap.GetPixel(0, 0);

            bitmap.Dispose();

            return output;
        }
        public static Bitmap TakeScreenshot()
        {
            Rectangle bounds = Screen.AllScreens[0].Bounds;

            for (int i = 1; i < Screen.AllScreens.Length; i++)
            {
                if (Screen.AllScreens[i].Bounds.X < bounds.X)
                {
                    bounds.X = Screen.AllScreens[i].Bounds.X;
                }
                if (Screen.AllScreens[i].Bounds.Y < bounds.Y)
                {
                    bounds.Y = Screen.AllScreens[i].Bounds.Y;
                }
                if (Screen.AllScreens[i].Bounds.Right > bounds.Right)
                {
                    bounds.Width = Screen.AllScreens[i].Bounds.Right - bounds.X;
                }
                if (Screen.AllScreens[i].Bounds.Bottom > bounds.Bottom)
                {
                    bounds.Height = Screen.AllScreens[i].Bounds.Bottom - bounds.Y;
                }
            }

            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.Clear(Color.Transparent);

            foreach (Screen screen in Screen.AllScreens)
            {
                graphics.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.X - bounds.X, screen.Bounds.Y - bounds.Y, screen.Bounds.Size);
            }

            graphics.Dispose();

            bitmap.Save("D:\\HI.png");

            return bitmap;
        }
        public static Bitmap TakeScreenshot(int targetScreenIndex)
        {
            if (targetScreenIndex < 0 || targetScreenIndex >= Screen.AllScreens.Length)
            {
                throw new Exception("targetScreenIndex must be within the bounds of Screen.AllScreens.");
            }

            Rectangle bounds = Screen.AllScreens[targetScreenIndex].Bounds;

            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);

            graphics.Dispose();

            return bitmap;
        }
        public static Bitmap TakeScreenshot(Screen targetScreen)
        {
            Rectangle bounds = targetScreen.Bounds;

            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);

            graphics.Dispose();

            return bitmap;
        }
        public static Bitmap ColorToBitmap(Color color)
        {
            Bitmap output = new Bitmap(1, 1);
            output.SetPixel(0, 0, color);
            return output;
        }
        public static Bitmap ColorToBitmap(Color color, Size size)
        {
            Bitmap output = new Bitmap(size.Width, size.Height);

            Graphics graphics = Graphics.FromImage(output);

            graphics.Clear(color);

            graphics.Dispose();

            return output;
        }
        public static Bitmap ColorToBitmap(Color color, int Width, int Height)
        {
            Bitmap output = new Bitmap(Width, Height);

            Graphics graphics = Graphics.FromImage(output);

            graphics.Clear(color);

            graphics.Dispose();

            return output;
        }
        public static Color HexToColor(string hex)
        {
            if (hex is null)
            {
                throw new Exception("hex cannot be null.");
            }
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }
            if (hex.Length == 6)
            {
                try
                {
                    return Color.FromArgb(255, Convert.ToByte(hex.Substring(0, 2), 16), Convert.ToByte(hex.Substring(2, 2), 16), Convert.ToByte(hex.Substring(4, 2), 16));
                }
                catch
                {
                    throw new Exception("Invalid hex color.");
                }
            }
            else if (hex.Length == 8)
            {
                try
                {
                    return Color.FromArgb(Convert.ToByte(hex.Substring(6, 2), 16), Convert.ToByte(hex.Substring(0, 2), 16), Convert.ToByte(hex.Substring(2, 2), 16), Convert.ToByte(hex.Substring(4, 2), 16));
                }
                catch
                {
                    throw new Exception("Invalid hex color.");
                }
            }
            else
            {
                throw new Exception("Invalid hex color.");
            }
        }
        public static string ColorToHex(Color color)
        {
            return $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}";
        }
        public static Point GetCursorPosition()
        {
            Point output;
            if (!GetCursorPos(out output))
            {
                throw Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());
            }
            return output;
        }
        #endregion
        #region Private Methods
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);
        #endregion
    }
    public static class PInvoke
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseClipboard();


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern IntPtr GetOpenClipboardWindow();


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EmptyClipboard();


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern uint RegisterClipboardFormat(string lpszFormat);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int GetClipboardFormatName(uint format, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder lpszFormatName, int cchMaxCount);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern IntPtr GetClipboardData(uint uFormat);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern uint EnumClipboardFormats(uint format);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern IntPtr GlobalLock(IntPtr hMem);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalUnlock(IntPtr hMem);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int GlobalSize(IntPtr hMem);
    }
    public static class ClipboardHelper
    {
        private static NativeWindow _subsystemWindow = new Func<NativeWindow>(() => { NativeWindow subsystemWindow = new NativeWindow(); subsystemWindow.CreateHandle(new CreateParams()); return subsystemWindow; }).Invoke();

        public static void OpenClipboard()
        {
            if (!PInvoke.OpenClipboard(_subsystemWindow.Handle))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
        }
        public static void CloseClipboard()
        {
            if (!PInvoke.CloseClipboard())
            {
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
        }
        public static bool ClipboardOpen()
        {
            return PInvoke.GetOpenClipboardWindow() != IntPtr.Zero;
        }
        public static bool ClipboardOpenByMe()
        {
            return PInvoke.GetOpenClipboardWindow() != _subsystemWindow.Handle;
        }
        public static bool ClipboardOpenByOther()
        {
            IntPtr owner = PInvoke.GetOpenClipboardWindow();
            return owner != _subsystemWindow.Handle && owner != IntPtr.Zero;
        }
        public enum ClipboardOpenState : byte { Closed = 0, OpenByMe = 1, OpenByOther = 2 }
        public static ClipboardOpenState GetClipboardOpenState()
        {
            IntPtr owner = PInvoke.GetOpenClipboardWindow();
            if (owner == IntPtr.Zero)
            {
                return ClipboardOpenState.Closed;
            }
            else if (owner == _subsystemWindow.Handle)
            {
                return ClipboardOpenState.OpenByMe;
            }
            else
            {
                return ClipboardOpenState.OpenByOther;
            }
        }
        public static void ClearClipboard()
        {
            if (!PInvoke.EmptyClipboard())
            {
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
        }
        public static void SetClipboardBitmap(Bitmap data)
        {
            IntPtr hBitmap = data.GetHbitmap();

            PInvoke.SetClipboardData(2 /* CF_BITMAP */, hBitmap);
        }
        public static void SetClipboardData(byte[] data, uint format)
        {
            IntPtr hMem = Marshal.AllocHGlobal(data.Length);

            if (hMem == IntPtr.Zero)
            {
                throw new Exception("Failed to allocate memory for clipboard");
            }

            // Copy the bitmap data to the global memory
            Marshal.Copy(data, 0, hMem, data.Length);

            // Set the clipboard data
            PInvoke.SetClipboardData(format, hMem);
        }
        public static uint GetFormatByName(string name)
        {
            name = name.ToLower();

            for (uint i = 0; i < 1000000; i++)
            {
                StringBuilder formatName = new StringBuilder(255);
                int result = PInvoke.GetClipboardFormatName(i, formatName, formatName.Capacity);
                if (!(result is 0) && formatName.ToString().ToLower() == name)
                {
                    return i;
                }
            }

            throw new Exception("Unnable to locate clipboard format with the specified name.");
        }
    }

}