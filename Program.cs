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

namespace ColorYoink
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			GlobalHotkey CtrlAltC = new GlobalHotkey(Keys.C, HotkeyModifier.Control | HotkeyModifier.Alt, () => { YoinkColor(); });
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
		public static void SetClipboardToPNG(Bitmap source)
		{
			MemoryStream bitmapDataStream = new MemoryStream();
			source.Save(bitmapDataStream, ImageFormat.Png);
			ClipboardHelper.SetClipboardStream(bitmapDataStream);
			bitmapDataStream.Dispose();
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
	public static class ClipboardHelper
	{
		[DllImport("user32.dll")]
		private static extern bool OpenClipboard(IntPtr hWndNewOwner);
		[DllImport("user32.dll")]
		private static extern bool EmptyClipboard();
		[DllImport("user32.dll")]
		private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseClipboard();
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint RegisterClipboardFormat(string lpszFormat);

		public static void SetClipboardStream(MemoryStream stream)
		{
			if (!OpenClipboard(IntPtr.Zero))
			{
				throw new Exception("Failed to open clipboard");
			}

			EmptyClipboard();

			try
			{
				// Get the bitmap data from the memory stream
				byte[] data = stream.ToArray();

				// Allocate global memory to store the bitmap data
				IntPtr hMem = Marshal.AllocHGlobal(data.Length);

				if (hMem == IntPtr.Zero)
				{
					throw new Exception("Failed to allocate memory for clipboard");
				}

				// Copy the bitmap data to the global memory
				Marshal.Copy(data, 0, hMem, data.Length);

				// Get the format code for the specified string format
				uint formatCode = RegisterClipboardFormat("image/png");

				if (formatCode == 0)
				{
					throw new Exception("Failed to register clipboard format");
				}

				// Set the clipboard data
				SetClipboardData(formatCode, hMem);
			}
			finally
			{
				// Close the clipboard
				if (!CloseClipboard())
				{
					throw new Exception("Failed to close clipboard");
				}
			}
		}
	}

}