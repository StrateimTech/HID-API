using System.Collections;
using System.Text;
using HID_API.Handlers;
using HID_API.Utils;

namespace HID_API;

public class HidHandler
{
    private readonly BinaryWriter? _hidBinaryWriter;

    private readonly ReaderWriterLockSlim _hidWriteLock = new();

    public readonly List<KeyboardHandler> HidKeyboardHandlers = new();
    public readonly List<MouseHandler> HidMouseHandlers = new();

    public HidHandler(string[] mousePaths, string[] keyboardPaths, string hidPath)
    {
        if (!File.Exists(hidPath))
        {
            return;
        }

        var hidFileStream = File.Open(hidPath, FileMode.Open, FileAccess.Write);
        _hidBinaryWriter = new BinaryWriter(hidFileStream, Encoding.Default, true);

        foreach (var mousePath in mousePaths)
        {
            if (File.Exists(mousePath))
            {
                var mouseStream = File.Open(mousePath, FileMode.Open, FileAccess.ReadWrite);
                HidMouseHandlers.Add(new(this, mouseStream, mousePath));
            }
        }

        foreach (var keyboardPath in keyboardPaths)
        {
            if (File.Exists(keyboardPath))
            {
                var keyboardStream = File.Open(keyboardPath, FileMode.Open, FileAccess.Read);
                HidKeyboardHandlers.Add(new(this, keyboardStream, keyboardPath));
            }
        }

        if (mousePaths.Length != HidMouseHandlers.Count || keyboardPaths.Length != HidKeyboardHandlers.Count)
        {
            new Thread(() =>
            {
                while (true)
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

                    if (mousePaths.Length == HidMouseHandlers.Count &&
                        keyboardPaths.Length == HidKeyboardHandlers.Count)
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

        if (_hidBinaryWriter is not null)
        {
            WriteMouseReport(new Mouse());
            WriteKeyboardReport(new Keyboard());

            _hidBinaryWriter.Close();
        }
    }

    public void WriteMouseReport(Mouse mouse)
    {
        if (_hidBinaryWriter is null)
        {
            return;
        }

        _hidWriteLock.EnterWriteLock();
        try
        {
            WriteUtils.WriteReport(_hidBinaryWriter,
                1,
                new[]
                {
                    DataUtils.ToByte(new BitArray(new[]
                    {
                        mouse.LeftButton, mouse.RightButton, mouse.MiddleButton,
                        false, false, false, false, false
                    }))
                },
                new[] {Convert.ToInt16(mouse.X * mouse.SensitivityMultiplier), Convert.ToInt16(mouse.Y * mouse.SensitivityMultiplier)},
                new[] {Convert.ToSByte(mouse.Wheel)});
        }
        finally
        {
            _hidWriteLock.ExitWriteLock();
        }
    }

    public void WriteKeyboardReport(Keyboard keyboard)
    {
        if (_hidBinaryWriter is null)
        {
            return;
        }

        _hidWriteLock.EnterWriteLock();
        try
        {
            byte[] buffer = new byte[9];
            buffer[0] = 2;
            if (keyboard.Modifier != null)
            {
                buffer[1] = keyboard.Modifier.Value;
            }

            if (keyboard.KeyCode != null)
            {
                buffer[3] = keyboard.KeyCode.Value;
            }

            for (int i = 4; i < 8; i++)
            {
                buffer[i] = keyboard.ExtraKeys[i - 4];
            }

            WriteUtils.WriteReport(_hidBinaryWriter, buffer);
        }
        finally
        {
            _hidWriteLock.ExitWriteLock();
        }
    }
}