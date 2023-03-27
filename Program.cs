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
			GlobalHotkey CtrlAltC = new GlobalHotkey(Keys.C, HotkeyModifier.Control | HotkeyModifier.Alt, () => { MessageBox.Show("Balls"); });
			GlobalHotkey CtrlAltD = new GlobalHotkey(Keys.D, HotkeyModifier.Control | HotkeyModifier.Alt, () => { MessageBox.Show("Dick"); });

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
			Clipboard.SetData("PNG", bitmapDataStream);
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
	#region HELP
	public sealed class KeyboardHook : IDisposable
	{
		private WindowsMessagePump _subsystem;
		public KeyboardHook()
		{
			_subsystem = new WindowsMessagePump(MessageHandler);
		}
		public void RegisterHotKey(HotkeyModifier modifier, Keys key)
		{
			if (!RegisterHotKey(_subsystem.Handle, 0, (uint)modifier, (uint)key))
			{
				throw new InvalidOperationException("Couldn’t register the hot key.");
			}
		}
		public HotkeyEvent KeyPressed;
		public void Dispose()
		{
			UnregisterHotKey(_subsystem.Handle, 0);
			_subsystem.Dispose();
		}
		private void MessageHandler(ref Message message)
		{
			if (message.Msg is 0x0312)
			{
				if (!(KeyPressed is null))
				{
					KeyPressed.Invoke();
				}
			}
		}
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
	}
	#endregion

	public sealed class ColorYoinkForm : Form
	{
		//private GlobalHotkey CtrlAltC;
		private NotifyIcon notifyIcon;
		public ColorYoinkForm()
		{
			//CtrlAltC = new GlobalHotkey(this, Keys.C, GlobalHotkeyModifier.Control);

			notifyIcon = new NotifyIcon();
			notifyIcon.Icon = new Icon("icon.ico");
			notifyIcon.Visible = true;

			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.ShowInTaskbar = false;
			this.Load += Form1_Load;
			this.FormClosing += Form1_FormClosing;
			// Create the context menu
			ContextMenuStrip menu = new ContextMenuStrip();
			// Add menu items
			ToolStripMenuItem item1 = new ToolStripMenuItem("Exit ColorYoink");
			item1.Click += new EventHandler(item1_Click);
			menu.Items.Add(item1);

			// Set the context menu
			notifyIcon.ContextMenuStrip = menu;
		}
		/*protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (!(CtrlAltC is null) && CtrlAltC.ProcessMessage(m))
			{
				MessageBox.Show("My balls");
				ColorYoink.YoinkColor();
			}
		}*/
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			//CtrlAltC.Dispose();
			notifyIcon.Dispose();
		}
		private void item1_Click(object sender, EventArgs e)
		{
			this.Close();
		}
		void Form1_Load(object sender, EventArgs e)
		{
			//CtrlAltC.Register();
			this.Size = new Size(0, 0);
		}
	}


	public delegate void WindowsMessageHandler(ref Message message);
	public sealed class WindowsMessagePump : IDisposable
	{
		#region Private Subclasses
		private sealed class WindowsMessagePumpSubsystemWindow : NativeWindow
		{
			public WindowsMessageHandler WindowsMessageHandler;
			protected override void WndProc(ref Message message)
			{
				base.WndProc(ref message);

				if (!(WindowsMessageHandler is null))
				{
					WindowsMessageHandler.Invoke(ref message);
				}
			}
		}
		#endregion
		#region Public Variables
		public WindowsMessageHandler WindowsMessageHandler
		{
			get
			{
				if (_disposed)
				{
					throw new Exception("WindowsMessagePump has been disposed.");
				}

				return _subsystem.WindowsMessageHandler;
			}
			set
			{
				if (_disposed)
				{
					throw new Exception("WindowsMessagePump has been disposed.");
				}

				_subsystem.WindowsMessageHandler = value;
			}
		}
		public IntPtr Handle
		{
			get
			{
				if (_disposed)
				{
					throw new Exception("WindowsMessagePump has been disposed.");
				}

				return _subsystem.Handle;
			}
		}
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}
		#endregion
		#region Private Variables
		private WindowsMessagePumpSubsystemWindow _subsystem = null;
		private bool _disposed = false;
		#endregion
		#region Public Constructors
		public WindowsMessagePump(WindowsMessageHandler windowsMessageHandler)
		{
			_subsystem = new WindowsMessagePumpSubsystemWindow();
			_subsystem.WindowsMessageHandler = windowsMessageHandler;
			_subsystem.CreateHandle(new CreateParams());
		}
		public WindowsMessagePump()
		{
			_subsystem = new WindowsMessagePumpSubsystemWindow();
			_subsystem.WindowsMessageHandler = null;
			_subsystem.CreateHandle(new CreateParams());
		}
		#endregion
		#region Public Methods
		public void Dispose()
		{
			if (_disposed)
			{
				throw new Exception("WindowsMessagePump has been disposed.");
			}

			_subsystem.WindowsMessageHandler = null;
			_subsystem.DestroyHandle();
			_subsystem = null;

			_disposed = true;
		}
		#endregion
	}
	public delegate void HotkeyEvent();
	[Flags]
	public enum HotkeyModifier : uint
	{
		None = 0x0000,
		Alt = 0x0001,
		Control = 0x0002,
		Shift = 0x0004,
		Windows = 0x0008,
		NoRepeat = 0x4000,
	};
	public sealed class GlobalHotkey : IDisposable
	{
		#region Public Variables
		public HotkeyEvent HotkeyEvent
		{
			get
			{
				if (_disposed)
				{
					throw new Exception("GlobalHotkey has been disposed.");
				}

				return _hotkeyEvent;
			}
			set
			{
				if (_disposed)
				{
					throw new Exception("GlobalHotkey has been disposed.");
				}

				_hotkeyEvent = value;
			}
		}
		public Keys Key
		{
			get
			{
				if (_disposed)
				{
					throw new Exception("GlobalHotkey has been disposed.");
				}

				return _key;
			}
		}
		public bool Control
		{
			get
			{
				if (_disposed)
				{
					throw new Exception("GlobalHotkey has been disposed.");
				}

				return _control;
			}
		}
		public bool Shift
		{
			get
			{
				if (_disposed)
				{
					throw new Exception("GlobalHotkey has been disposed.");
				}

				return _shift;
			}
		}
		public bool Alt
		{
			get
			{
				if (_disposed)
				{
					throw new Exception("GlobalHotkey has been disposed.");
				}

				return _alt;
			}
		}
		public bool Windows
		{
			get
			{
				if (_disposed)
				{
					throw new Exception("GlobalHotkey has been disposed.");
				}

				return _windows;
			}
		}
		public bool NoRepeat
		{
			get
			{
				if (_disposed)
				{
					throw new Exception("GlobalHotkey has been disposed.");
				}

				return _noRepeat;
			}
		}
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}
		#endregion
		#region Private Variables
		private HotkeyEvent _hotkeyEvent = null;
		private Keys _key = Keys.F12;
		private bool _control = false;
		private bool _shift = false;
		private bool _alt = false;
		private bool _windows = false;
		private bool _noRepeat = true;
		private bool _disposed = false;
		private WindowsMessagePump _subsystem = null;
		#endregion
		#region Public Constructors
		public GlobalHotkey(Keys key, HotkeyEvent hotkeyEvent)
		{
			_key = key;
			_control = false;
			_shift = false;
			_alt = false;
			_windows = false;
			_noRepeat = true;
			_disposed = false;
			_hotkeyEvent = hotkeyEvent;

			_subsystem = new WindowsMessagePump();
			_subsystem.WindowsMessageHandler = MessageHandler;

			uint modifiersUInt = 0x0000;
			if (_alt)
			{
				modifiersUInt |= 0x0001;
			}
			if (_control)
			{
				modifiersUInt |= 0x0002;
			}
			if (_shift)
			{
				modifiersUInt |= 0x0004;
			}
			if (_windows)
			{
				modifiersUInt |= 0x0008;
			}
			if (!_noRepeat && false)
			{
				modifiersUInt |= 0x4000;
			}

			if (!RegisterHotKey(_subsystem.Handle, 0, modifiersUInt, (uint)_key))
			{
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}
		public GlobalHotkey(Keys key, HotkeyModifier modifiers, HotkeyEvent hotkeyEvent)
		{
			_key = key;
			_control = ((modifiers & HotkeyModifier.Control) is HotkeyModifier.Control);
			_shift = ((modifiers & HotkeyModifier.Shift) is HotkeyModifier.Shift);
			_alt = ((modifiers & HotkeyModifier.Alt) is HotkeyModifier.Alt);
			_windows = ((modifiers & HotkeyModifier.Windows) is HotkeyModifier.Windows);
			_noRepeat = ((modifiers & HotkeyModifier.NoRepeat) is HotkeyModifier.NoRepeat);
			_disposed = false;
			_hotkeyEvent = hotkeyEvent;

			_subsystem = new WindowsMessagePump();
			_subsystem.WindowsMessageHandler = MessageHandler;

			uint modifiersUInt = 0x0000;
			if (_alt)
			{
				modifiersUInt |= 0x0001;
			}
			if (_control)
			{
				modifiersUInt |= 0x0002;
			}
			if (_shift)
			{
				modifiersUInt |= 0x0004;
			}
			if (_windows)
			{
				modifiersUInt |= 0x0008;
			}
			if (!_noRepeat && false)
			{
				modifiersUInt |= 0x4000;
			}

			if (!RegisterHotKey(_subsystem.Handle, 0, modifiersUInt, (uint)_key))
			{
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}
		public GlobalHotkey(Form form, Keys key, bool control, bool shift, bool alt, bool windows, HotkeyEvent hotkeyEvent)
		{
			_key = key;
			_control = control;
			_shift = shift;
			_alt = alt;
			_windows = windows;
			_noRepeat = true;
			_disposed = false;
			_hotkeyEvent = hotkeyEvent;

			_subsystem = new WindowsMessagePump();
			_subsystem.WindowsMessageHandler = MessageHandler;

			uint modifiersUInt = 0x0000;
			if (_alt)
			{
				modifiersUInt |= 0x0001;
			}
			if (_control)
			{
				modifiersUInt |= 0x0002;
			}
			if (_shift)
			{
				modifiersUInt |= 0x0004;
			}
			if (_windows)
			{
				modifiersUInt |= 0x0008;
			}
			if (!_noRepeat && false)
			{
				modifiersUInt |= 0x4000;
			}

			if (!RegisterHotKey(_subsystem.Handle, 0, modifiersUInt, (uint)_key))
			{
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}
		public GlobalHotkey(Form form, Keys key, bool control, bool shift, bool alt, bool windows, bool repeat, HotkeyEvent hotkeyEvent)
		{
			_key = key;
			_control = control;
			_shift = shift;
			_alt = alt;
			_windows = windows;
			_noRepeat = repeat;
			_disposed = false;
			_hotkeyEvent = hotkeyEvent;

			_subsystem = new WindowsMessagePump();
			_subsystem.WindowsMessageHandler = MessageHandler;

			uint modifiersUInt = 0x0000;
			if (_alt)
			{
				modifiersUInt |= 0x0001;
			}
			if (_control)
			{
				modifiersUInt |= 0x0002;
			}
			if (_shift)
			{
				modifiersUInt |= 0x0004;
			}
			if (_windows)
			{
				modifiersUInt |= 0x0008;
			}
			if (!_noRepeat && false)
			{
				modifiersUInt |= 0x4000;
			}

			if (!RegisterHotKey(_subsystem.Handle, 0, modifiersUInt, (uint)_key))
			{
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}
		#endregion
		#region Public Methods
		public void Dispose()
		{
			if (_disposed)
			{
				throw new Exception("GlobalHotkey has already been disposed.");
			}

			UnregisterHotKey(_subsystem.Handle, 0);
			_subsystem.Dispose();
			_subsystem = null;

			_disposed = true;
		}
		#endregion
		#region Private Methods
		private void MessageHandler(ref Message message)
		{
			if (message.Msg is 0x0312)
			{
				if (!(HotkeyEvent is null))
				{
					HotkeyEvent.Invoke();
				}
			}
		}
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		#endregion
	}
}