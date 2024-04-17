using HID_API.Handlers;
using HID_API.Utils;

namespace HID_API;

public class HidHandler
{
    private readonly FileStream? _hidStream;
    private readonly object _streamLock = new();

    public readonly List<KeyboardHandler> HidKeyboardHandlers = new();
    public readonly List<MouseHandler> HidMouseHandlers = new();

    public HidHandler(string[]? mousePaths, string[]? keyboardPaths, string hidPath, bool hotReload = true)
    {
        if (!File.Exists(hidPath))
        {
            return;
        }

        _hidStream = new FileStream(hidPath, FileMode.Open, FileAccess.Write, FileShare.None, 0, FileOptions.WriteThrough);

        if (mousePaths != null)
        {
            foreach (var mousePath in mousePaths)
            {
                if (File.Exists(mousePath))
                {
                    var mouseStream = File.Open(mousePath, FileMode.Open, FileAccess.ReadWrite);
                    HidMouseHandlers.Add(new(this, mouseStream, mousePath));
                }
            }
        }

        if (keyboardPaths != null)
        {
            foreach (var keyboardPath in keyboardPaths)
            {
                if (File.Exists(keyboardPath))
                {
                    var keyboardStream = File.Open(keyboardPath, FileMode.Open, FileAccess.Read);
                    HidKeyboardHandlers.Add(new(this, keyboardStream, keyboardPath));
                }
            }
        }

        if (!hotReload)
        {
            return;
        }

        if (mousePaths?.Length != HidMouseHandlers.Count || keyboardPaths?.Length != HidKeyboardHandlers.Count)
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (mousePaths != null && HidMouseHandlers.Count != mousePaths.Length)
                    {
                        foreach (var mousePath in mousePaths)
                        {
                            if (!File.Exists(mousePath))
                            {
                                continue;
                            }
                            
                            if (HidMouseHandlers.All(mouse => mouse.Path != mousePath))
                            {
                                var mouseStream = File.Open(mousePath, FileMode.Open, FileAccess.Read);
                                HidMouseHandlers.Add(new(this, mouseStream, mousePath));
                            }
                        }
                    }

                    if (keyboardPaths != null && HidKeyboardHandlers.Count != keyboardPaths.Length)
                    {
                        foreach (var keyboardPath in keyboardPaths)
                        {
                            if (!File.Exists(keyboardPath))
                            {
                                continue;
                            }

                            if (HidKeyboardHandlers.Any(keyboard => keyboard.Path != keyboardPath))
                            {
                                var keyboardStream = File.Open(keyboardPath, FileMode.Open, FileAccess.Read);
                                HidKeyboardHandlers.Add(new(this, keyboardStream, keyboardPath));
                            }
                        }
                    }

                    Thread.Sleep(5);
                }
            }).Start();
        }
    }

    public void Stop()
    {
        foreach (var mouseHandler in HidMouseHandlers)
        {
            mouseHandler.DeviceStream.Close();
            mouseHandler.Active = false;
        }

        foreach (var keyboardHandler in HidKeyboardHandlers)
        {
            keyboardHandler.DeviceStream.Close();
            keyboardHandler.Active = false;
        }

        if (_hidStream != null)
        {
            WriteMouseReport(new Mouse());
            WriteKeyboardReport(new Keyboard());

            _hidStream.Close();
        }
    }

    public void WriteMouseReport(Mouse mouse)
    {
        byte buttonByte = (byte) ((mouse.LeftButton ? 1 : 0) |
                                  (mouse.RightButton ? 2 : 0) |
                                  (mouse.MiddleButton ? 4 : 0) |
                                  (mouse.FourButton ? 8 : 0) |
                                  (mouse.FiveButton ? 16 : 0));

        float sensitivityMultiplier = mouse.SensitivityMultiplier;
        
        short x = (short) (mouse.X * sensitivityMultiplier);
        short y = (short) (mouse.Y * sensitivityMultiplier);
        sbyte wheel = (sbyte) mouse.Wheel;
        
        lock (_streamLock)
        {
            if (_hidStream == null || !_hidStream.CanWrite)
            {
                return;
            }

            WriteUtils.WriteMouseReport(_hidStream,
                1,
                buttonByte,
                new[] {x, y},
                wheel);
        }
    }

    public void WriteKeyboardReport(Keyboard keyboard)
    {
        lock (_streamLock)
        {
            if (_hidStream == null || !_hidStream.CanWrite)
            {
                return;
            }

            WriteUtils.WriteKeyboardReport(_hidStream, keyboard);
        }
    }
}