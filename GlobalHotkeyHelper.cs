using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalHotkeyHelper
{
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
	}
	public sealed class HotkeyBinding
	{
		#region Public Variables
		public event HotkeyEvent HotkeyEvent
		{
			add
			{
				if (_disposed)
				{
					throw new Exception("GlobalHotkey has been disposed.");
				}

				for (int i = 0; i < _hotkeyEvent.Count; i++)
				{
					if (_hotkeyEvent[i] == value)
					{
						return;
					}
				}

				_hotkeyEvent.Add(value);
			}
			remove
			{
				if (_disposed)
				{
					throw new Exception("GlobalHotkey has been disposed.");
				}

				_hotkeyEvent.Remove(value);
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
		private List<HotkeyEvent> _hotkeyEvent = new List<HotkeyEvent>();
		private Keys _key = Keys.F12;
		private bool _control = false;
		private bool _shift = false;
		private bool _alt = false;
		private bool _windows = false;
		private bool _noRepeat = true;
		private bool _disposed = false;


		#endregion
		#region Public Constructors
		public HotkeyBinding(Keys key, HotkeyEvent hotkeyEvent)
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

			if (!RegisterHotKey(_subsystem.Handle, GetHashCode(), modifiersUInt, (uint)_key))
			{
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}
		public HotkeyBinding(Keys key, HotkeyModifier modifiers, HotkeyEvent hotkeyEvent)
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

			if (!RegisterHotKey(_subsystem.Handle, GetHashCode(), modifiersUInt, (uint)_key))
			{
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}
		public HotkeyBinding(Keys key, bool control, bool shift, bool alt, bool windows, HotkeyEvent hotkeyEvent)
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

			if (!RegisterHotKey(_subsystem.Handle, GetHashCode(), modifiersUInt, (uint)_key))
			{
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}
		public HotkeyBinding(Keys key, bool control, bool shift, bool alt, bool windows, bool repeat, HotkeyEvent hotkeyEvent)
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

			if (!RegisterHotKey(_subsystem.Handle, GetHashCode(), modifiersUInt, (uint)_key))
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

			UnregisterHotKey(_subsystem.Handle, GetHashCode());
			_subsystem.Dispose();
			_subsystem = null;

			_disposed = true;
		}
		#endregion
		#region Private Methods
		private void AsyncInvokeHotkeyEvent()
		{
			for (int i = 0; i < _hotkeyEvent.Count; i++)
			{
				ThreadPool.QueueUserWorkItem((object state) => { _hotkeyEvent[i].Invoke(); }, null);
			}
		}
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		#endregion
	}
	public enum EventPumpState : byte
	{
		Stopped = 0,
		Stopping = 1,
		Running = 2,
		Starting = 3,
	}
	public static class HotkeyEventPump
	{
		#region Private Subclasses
		private sealed class SubsystemWindow : NativeWindow
		{
			protected override void WndProc(ref Message message)
			{
				base.WndProc(ref message);

				MessageHandler(ref message);
			}
		}
		#endregion
		#region Public Static Variables
		public static EventPumpState State { get; private set; } = EventPumpState.Stopped;
		#endregion
		private List<HotkeyBinding> _registeredHotKeys = new List<HotkeyBinding>();
		private static Thread _pumpThread = null;
		private static readonly object _stateLock = new object();
		internal static void Start()
		{
			lock (_stateLock)
			{
				if (State is EventPumpState.Stopped || State is EventPumpState.Stopping)
				{
					State = EventPumpState.Starting;
				}
			}
		}
		internal static void Stop()
		{

		}
		private void LaunchPumpThread()
		{
			_pumpThread
			while (true)
			{
				Application.DoEvents();
			}
		}
		private static void RunPump()
		{

		}
		private static void MessageHandler(ref Message message)
		{
			if (message.Msg is 0x0312)
			{
				Keys key = (Keys)(((int)message.LParam >> 16) & 0xFFFF);
				HotkeyModifier modifier = (HotkeyModifier)((int)message.LParam & 0xFFFF);

				if (!(HotkeyEvent is null))
				{
					HotkeyEvent.Invoke();
				}
			}
		}
	}
}
