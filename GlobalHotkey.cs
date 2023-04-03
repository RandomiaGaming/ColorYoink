using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
public static class GlobalHotkey
{
    #region Public Static Methods
    public static void Register(Keys key, bool control, bool shift, bool alt, bool windows, bool noRepeat, HotkeyEvent hotkeyEvent)
    {
        lock (lockObj)
        {
            uint modifiersUInt = 0x0000;
            if (alt)
            {
                modifiersUInt |= 0x0001;
            }
            if (control)
            {
                modifiersUInt |= 0x0002;
            }
            if (shift)
            {
                modifiersUInt |= 0x0004;
            }
            if (windows)
            {
                modifiersUInt |= 0x0008;
            }
            if (!noRepeat)
            {
                modifiersUInt |= 0x4000;
            }

            HotkeyRegistryEntry entry = new HotkeyRegistryEntry();
            entry.key = key;
            entry.modifiers = (HotkeyModifier)modifiersUInt;
            entry.hotkeyEvent = hotkeyEvent;
            entry.id = nextFreeID;
            nextFreeID++;

            if (!RegisterHotKey(subsystem.Handle, entry.id, (uint)entry.modifiers, (uint)entry.key))
            {
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            hotkeyRegistry.Add(entry);
        }
    }
    public static void Register(Keys key, HotkeyModifier modifiers, HotkeyEvent hotkeyEvent)
    {
        lock (lockObj)
        {
            for (int i = 0; i < hotkeyRegistry.Count; i++)
            {
                if (hotkeyRegistry[i].hotkeyEvent == hotkeyEvent)
                {
                    throw new Exception("");
                }
            }

            HotkeyRegistryEntry entry = new HotkeyRegistryEntry();
            entry.key = key;
            entry.modifiers = modifiers;
            entry.hotkeyEvent = hotkeyEvent;
            entry.id = nextFreeID;
            nextFreeID++;

            if (!RegisterHotKey(subsystem.Handle, entry.id, (uint)entry.modifiers, (uint)entry.key))
            {
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            hotkeyRegistry.Add(entry);
        }
    }
    public static void Unregister(HotkeyEvent hotkeyEvent)
    {
        lock (lockObj)
        {
            
        }
    }
    public static void UnregisterAll(Keys key, HotkeyModifier modifiers)
    {

    }
    public static void UnregisterAll()
    {

    }
    #endregion
    #region Private Static Methods
    #endregion
    #region Private Static Variables
    private static List<HotkeyRegistryEntry> hotkeyRegistry = new List<HotkeyRegistryEntry>();
    private static object lockObj = new object();
    private static int nextFreeID = 0;
    private static SubsystemWindow subsystem = null;
    private static bool running = false;
    private static bool startRequested = false;
    private static bool stopRequested = false;
    #endregion
    #region Private Subclasses
    private sealed class HotkeyRegistryEntry
    {
        public Keys key = Keys.None;
        public HotkeyModifier modifiers = HotkeyModifier.None;
        public HotkeyEvent hotkeyEvent = null;
        public int id = 0;
    }
    private sealed class SubsystemWindow : NativeWindow
    {
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);

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
    #endregion
    #region Private PInvoke Bindings
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    #endregion
    //Old Code
    public sealed class HotkeyBinding
    {
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
        #endregion
    }
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
}