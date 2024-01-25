using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System;
using System.Threading;

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
public sealed class GlobalHotkey
{
	#region Private Subclasses
	private sealed class SubsystemWindow : NativeWindow
	{
		protected override void WndProc(ref Message message)
		{
			base.WndProc(ref message);

			if (message.Msg is 0x0312)
			{
				Keys key = (Keys)(((int)message.LParam >> 16) & 0xFFFF);
				HotkeyModifier modifiers = (HotkeyModifier)((uint)message.LParam & 0xFFFF);

				lock (_registryLock)
				{
					foreach (GlobalHotkey globalHotkey in _registery)
					{
						if (globalHotkey._key == key && globalHotkey._modifiers == modifiers)
						{
							globalHotkey._hotkeyEvent.Invoke();
						}
					}
				}
			}
		}
	}
	#endregion
	#region Private Static Variables
	private static object _registryLock = new object();
	private static List<GlobalHotkey> _registery = new List<GlobalHotkey>();
	private static List<GlobalHotkey> _registerQue = new List<GlobalHotkey>();
	private static List<GlobalHotkey> _unregisterQue = new List<GlobalHotkey>();

	private static object _nextIDLock = new object();
	private static int _nextID = 0;

	private static object _subsystemLock = new object();
	private static bool _subsystemRunning = false;
	private static SubsystemWindow _subsystemWindow = null;
	#endregion
	#region Private Static Methods
	private static void StartSubsystemIfNeeded()
	{
		lock (_subsystemLock)
		{
			if (!_subsystemRunning)
			{
				Thread _subsystemThread = new Thread(() =>
				{
					lock (_registryLock)
					{
						_subsystemWindow = new SubsystemWindow();
						_subsystemWindow.CreateHandle(new CreateParams());
					}

					_subsystemRunning = true;

					while (true)
					{
						lock (_registryLock)
						{
							while (_registerQue.Count > 0)
							{
								RegisterHotKey(_subsystemWindow.Handle, _registerQue[0]._hotkeyID, (uint)_registerQue[0]._modifiers, (uint)_registerQue[0]._key);
								_registery.Add(_registerQue[0]);
								_registerQue.RemoveAt(0);
							}
						}

						Application.DoEvents();
					}
				});

				_subsystemThread.Name = "GlobalHotkey Message Pump";
				_subsystemThread.Priority = ThreadPriority.Normal;
				_subsystemThread.IsBackground = true;
				_subsystemThread.Start();

				while (!_subsystemRunning)
				{

				}
			}
		}
	}
	[DllImport("user32.dll")]
	private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
	[DllImport("user32.dll")]
	private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
	#endregion
	#region Public Variables
	public HotkeyEvent HotkeyEvent
	{
		get
		{
			return _hotkeyEvent;
		}
	}
	public Keys Key
	{
		get
		{
			return _key;
		}
	}
	public HotkeyModifier Modifiers
	{
		get
		{
			return _modifiers;
		}
	}
	public bool Control
	{
		get
		{
			if ((_modifiers & HotkeyModifier.Control) == HotkeyModifier.Control)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
	public bool Shift
	{
		get
		{
			if ((_modifiers & HotkeyModifier.Shift) == HotkeyModifier.Shift)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
	public bool Alt
	{
		get
		{
			if ((_modifiers & HotkeyModifier.Alt) == HotkeyModifier.Alt)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
	public bool Windows
	{
		get
		{
			if ((_modifiers & HotkeyModifier.Windows) == HotkeyModifier.Windows)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
	public bool NoRepeat
	{
		get
		{
			if ((_modifiers & HotkeyModifier.NoRepeat) == HotkeyModifier.NoRepeat)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
	public int HotkeyID
	{
		get
		{
			return _hotkeyID;
		}
	}
	public bool Registered
	{
		get
		{
			lock (_localRegistryLock)
			{
				return _registered;
			}
		}
	}
	#endregion
	#region Private Variables
	private HotkeyEvent _hotkeyEvent = null;
	private Keys _key = Keys.None;
	private HotkeyModifier _modifiers = HotkeyModifier.None;
	private int _hotkeyID = 0;
	private object _localRegistryLock = new object();
	private bool _registered = false;
	#endregion
	#region Public Constructors
	public GlobalHotkey(Keys key, HotkeyEvent hotkeyEvent)
	{
		if (hotkeyEvent is null)
		{
			throw new Exception("hotkeyEvent cannot be null.");
		}
		_hotkeyEvent = hotkeyEvent;
		_key = key;
		_modifiers = HotkeyModifier.None;

		lock (_nextIDLock)
		{
			_hotkeyID = _nextID;
			_nextID++;
		}

		_registered = false;
	}
	public GlobalHotkey(Keys key, HotkeyModifier modifiers, HotkeyEvent hotkeyEvent)
	{
		if (hotkeyEvent is null)
		{
			throw new Exception("hotkeyEvent cannot be null.");
		}
		_hotkeyEvent = hotkeyEvent;
		_key = key;
		_modifiers = modifiers;

		lock (_nextIDLock)
		{
			_hotkeyID = _nextID;
			_nextID++;
		}

		_registered = false;
	}
	public GlobalHotkey(Keys key, bool control, bool shift, bool alt, bool windows, HotkeyEvent hotkeyEvent)
	{
		if (hotkeyEvent is null)
		{
			throw new Exception("hotkeyEvent cannot be null.");
		}
		_hotkeyEvent = hotkeyEvent;
		_key = key;

		if (control)
		{
			_modifiers |= HotkeyModifier.Control;
		}
		if (shift)
		{
			_modifiers |= HotkeyModifier.Shift;
		}
		if (alt)
		{
			_modifiers |= HotkeyModifier.Alt;
		}
		if (windows)
		{
			_modifiers |= HotkeyModifier.Windows;
		}

		lock (_nextIDLock)
		{
			_hotkeyID = _nextID;
			_nextID++;
		}

		_registered = false;
	}
	public GlobalHotkey(Keys key, bool control, bool shift, bool alt, bool windows, bool noRepeat, HotkeyEvent hotkeyEvent)
	{
		if (hotkeyEvent is null)
		{
			throw new Exception("hotkeyEvent cannot be null.");
		}
		_hotkeyEvent = hotkeyEvent;
		_key = key;

		if (control)
		{
			_modifiers |= HotkeyModifier.Control;
		}
		if (shift)
		{
			_modifiers |= HotkeyModifier.Shift;
		}
		if (alt)
		{
			_modifiers |= HotkeyModifier.Alt;
		}
		if (windows)
		{
			_modifiers |= HotkeyModifier.Windows;
		}
		if (noRepeat)
		{
			_modifiers |= HotkeyModifier.NoRepeat;
		}

		lock (_nextIDLock)
		{
			_hotkeyID = _nextID;
			_nextID++;
		}

		_registered = false;
	}
	#endregion
	#region Public Methods
	public void Register()
	{
		StartSubsystemIfNeeded();

		lock (_localRegistryLock)
		{
			if (_registered)
			{
				throw new Exception("GlobalHotkey has already been registered.");
			}

			lock (_registryLock)
			{
				_registerQue.Add(this);
				_registered = true;
			}
		}
	}
	public void Unregister()
	{
		lock (_localRegistryLock)
		{
			if (!_registered)
			{
				throw new Exception("GlobalHotkey has already been unregistered.");
			}

			lock (_registryLock)
			{
				_registery.Remove(this);
				UnregisterHotKey(_subsystemWindow.Handle, _hotkeyID);
				_registered = false;
			}
		}
	}
	#endregion
}