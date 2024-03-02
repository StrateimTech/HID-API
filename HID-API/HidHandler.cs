using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using HID_API.Handlers;
using HID_API.Utils;

namespace HID_API;

public class HidHandler
{
    private readonly FileStream? _hidStream;
    private readonly object _streamLock = new();

    public readonly List<KeyboardHandler> HidKeyboardHandlers = new();
    public readonly List<MouseHandler> HidMouseHandlers = new();

    public HidHandler(string[]? mousePaths, string[]? keyboardPaths, string hidPath)
    {
        if (!File.Exists(hidPath))
        {
            return;
        }

        _hidStream = new FileStream(hidPath, FileMode.Open, FileAccess.Write, FileShare.None, 1024, FileOptions.WriteThrough);

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

        if (mousePaths?.Length != HidMouseHandlers.Count || keyboardPaths?.Length != HidKeyboardHandlers.Count)
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (mousePaths != null)
                    {
                        foreach (var mousePath in mousePaths)
                        {
                            if (!HidMouseHandlers.Exists(item => item.Path == mousePath))
                            {
                                if (File.Exists(mousePath))
                                {
                                    var keyboardStream = File.Open(mousePath, FileMode.Open, FileAccess.Read);
                                    HidKeyboardHandlers.Add(new(this, keyboardStream, mousePath));
                                }
                            }
                        }
                    }

                    if (keyboardPaths != null)
                    {
                        foreach (var keyboardPath in keyboardPaths)
                        {
                            if (!HidKeyboardHandlers.Exists(item => item.Path == keyboardPath))
                            {
                                if (File.Exists(keyboardPath))
                                {
                                    var keyboardStream = File.Open(keyboardPath, FileMode.Open, FileAccess.Read);
                                    HidKeyboardHandlers.Add(new(this, keyboardStream, keyboardPath));
                                }
                            }
                        }
                    }

                    if (mousePaths?.Length == HidMouseHandlers.Count &&
                        keyboardPaths?.Length == HidKeyboardHandlers.Count)
                    {
                        break;
                    }

                    Thread.Sleep(1);
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

    public void WriteGenericEvent(GenericEvent @event)
    {
        switch (@event)
        {
            case Mouse mouse:
                WriteMouseReport(mouse);
                break;
            case Keyboard keyboard:
                WriteKeyboardReport(keyboard);
                break;
        }
    }

    private void WriteMouseReport(Mouse mouse)
    {
        lock (_streamLock)
        {
            if (_hidStream == null || !_hidStream.CanWrite)
            {
                return;
            }

            WriteUtils.WriteMouseReport(_hidStream,
                1,
                new[]
                {
                    DataUtils.ToByte(new BitArray(new[]
                    {
                        mouse.LeftButton, mouse.RightButton, mouse.MiddleButton, mouse.FourButton, mouse.FiveButton,
                        false, false, false
                    }))
                },
                new[] {Convert.ToInt16(mouse.X * mouse.SensitivityMultiplier), Convert.ToInt16(mouse.Y * mouse.SensitivityMultiplier)},
                new[] {Convert.ToSByte(mouse.Wheel)});
        }
    }

    private void WriteKeyboardReport(Keyboard keyboard)
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