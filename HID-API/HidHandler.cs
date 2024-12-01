using HID_API.Handlers;
using HID_API.Utils;

namespace HID_API;

public class HidHandler
{
    public readonly List<KeyboardHandler> HidKeyboardHandlers = new();
    public readonly List<MouseHandler> HidMouseHandlers = new();

    private readonly string? _hidPath;

    public HidHandler(string[]? mousePaths, string[]? keyboardPaths, string hidPath, bool hotReload = true)
    {
        if (!File.Exists(hidPath))
        {
            return;
        }

        _hidPath = hidPath;

        if (mousePaths != null)
        {
            foreach (var mousePath in mousePaths)
            {
                if (File.Exists(mousePath))
                {
                    var mouseStream = File.Open(mousePath, FileMode.Open, FileAccess.ReadWrite);
                    HidMouseHandlers.Add(new(this, mouseStream, mousePath, hidPath));
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
                    HidKeyboardHandlers.Add(new(this, keyboardStream, keyboardPath, hidPath));
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
                                HidMouseHandlers.Add(new(this, mouseStream, mousePath, hidPath));
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
                                HidKeyboardHandlers.Add(new(this, keyboardStream, keyboardPath, hidPath));
                            }
                        }
                    }

                    Thread.Sleep(5);
                }
            })
            {
                IsBackground = true
            }.Start();
        }
    }

    public void Stop()
    {
        HidMouseHandlers.ForEach(handler => handler.Active = false);
        HidKeyboardHandlers.ForEach(handler => handler.Active = false);

        if (_hidPath != null)
        {
            var hidStream = CreateHidStream(_hidPath);
            WriteMouseReport(new Mouse(), hidStream);
            WriteKeyboardReport(new Keyboard(), hidStream);
        }
    }

    public void WriteMouseReport(Mouse mouse, FileStream fileStream)
    {
        if (!fileStream.CanWrite)
        {
            return;
        }

        byte buttonByte = (byte) ((mouse.LeftButton ? 1 : 0) |
                                  (mouse.RightButton ? 2 : 0) |
                                  (mouse.MiddleButton ? 4 : 0) |
                                  (mouse.FourButton ? 8 : 0) |
                                  (mouse.FiveButton ? 16 : 0));

        sbyte wheel = (sbyte) mouse.Wheel;
        WriteUtils.WriteMouseReport(fileStream, 1, buttonByte, mouse.X, mouse.Y, wheel);
    }

    public void WriteKeyboardReport(Keyboard keyboard, FileStream fileStream)
    {
        if (!fileStream.CanWrite)
        {
            return;
        }

        WriteUtils.WriteKeyboardReport(fileStream, keyboard);
    }

    public FileStream CreateHidStream(string path)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Write, 0, FileOptions.WriteThrough);
    }
}