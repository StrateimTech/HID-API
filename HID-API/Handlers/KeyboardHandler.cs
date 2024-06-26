﻿namespace HID_API.Handlers;

public class KeyboardHandler
{
    private int? _keyCodeModifier;

    private readonly List<int> _keysDown = new();

    public readonly string Path;
    public bool Active = true;

    public KeyboardHandler(HidHandler hidHandler, FileStream keyboardFileStream, string streamPath, string hidPath)
    {
        // TODO: This does not handle special keys correctly
        // TODO: Numpad works but does not work when Numpad is supposed to be enabled
        // TODO: All F1-F12 keys do not work or rather (They output their fn alternatives) 
        
        // TODO: Multithreaded keyboard keys have to be globally handled since keydowns will be overwritten and such 
        Path = streamPath;
        new Thread(() =>
        {
            var hidStream = hidHandler.CreateHidStream(hidPath);
            
            while (Active)
            {
                byte[] buffer = new byte[24];
                keyboardFileStream.Read(buffer, 0, buffer.Length);

                // offset 8 bytes ignoring time values
                var offset = 8;
                short type = BitConverter.ToInt16(new[] {buffer[offset], buffer[++offset]}, 0);
                short code = BitConverter.ToInt16(new[] {buffer[++offset], buffer[++offset]}, 0);
                int value = BitConverter.ToInt32(
                    new[] {buffer[++offset], buffer[++offset], buffer[++offset], buffer[++offset]}, 0);

                var eventType = (Keyboard.EventType) type;
                var keyCode = (Keyboard.LinuxKeyCode) code;
                var keyState = (Keyboard.KeyState) value;

                switch (eventType)
                {
                    case Keyboard.EventType.EV_KEY:
                    {
                        switch (keyState)
                        {
                            case Keyboard.KeyState.KeyHold:
                            case Keyboard.KeyState.KeyDown:
                            {
                                if (Enum.IsDefined(typeof(Keyboard.UsbKeyCodeModifiers), keyCode.ToString()))
                                {
                                    _keyCodeModifier = (int) Enum.Parse(typeof(Keyboard.UsbKeyCodeModifiers),
                                        keyCode.ToString());
                                }

                                if (!Enum.IsDefined(typeof(Keyboard.UsbKeyCodeModifiers), keyCode.ToString()) &&
                                    !Enum.IsDefined(typeof(Keyboard.UsbKeyCode), keyCode.ToString()))
                                {
                                    break;
                                }

                                int? usbKeyCode = null;
                                if (Enum.IsDefined(typeof(Keyboard.UsbKeyCode), keyCode.ToString()))
                                {
                                    usbKeyCode = (int) Enum.Parse(typeof(Keyboard.UsbKeyCode), keyCode.ToString());
                                }

                                var keyboard = new Keyboard
                                {
                                    KeyCode = usbKeyCode != null ? Convert.ToByte(usbKeyCode) : null
                                };

                                Keyboard.UsbKeyCodeModifiers? localModifier = _keyCodeModifier != null
                                    ? (Keyboard.UsbKeyCodeModifiers) _keyCodeModifier
                                    : null;

                                for (int i = 0; i < _keysDown.Count; i++)
                                {
                                    if (i <= 5)
                                    {
                                        if (Enum.IsDefined(typeof(Keyboard.UsbKeyCodeModifiers),
                                                ((Keyboard.LinuxKeyCode) _keysDown[i]).ToString()))
                                        {
                                            if (localModifier == null)
                                            {
                                                localModifier =
                                                    (Keyboard.UsbKeyCodeModifiers) Enum.Parse(
                                                        typeof(Keyboard.UsbKeyCodeModifiers),
                                                        ((Keyboard.LinuxKeyCode) _keysDown[i]).ToString());
                                            }
                                            else
                                            {
                                                localModifier |= (Keyboard.UsbKeyCodeModifiers) Enum.Parse(
                                                    typeof(Keyboard.UsbKeyCodeModifiers),
                                                    ((Keyboard.LinuxKeyCode) _keysDown[i]).ToString());
                                            }
                                        }
                                        else
                                        {
                                            keyboard.ExtraKeys[i] =
                                                Convert.ToByte(Enum.Parse(typeof(Keyboard.UsbKeyCode),
                                                    ((Keyboard.LinuxKeyCode) _keysDown[i]).ToString()));
                                        }
                                    }
                                }

                                keyboard.Modifier = localModifier != null ? Convert.ToByte(localModifier) : null;

                                hidHandler.WriteKeyboardReport(keyboard, hidStream);
                                if (!_keysDown.Contains(code))
                                    _keysDown.Add(code);
                                break;
                            }
                            case Keyboard.KeyState.KeyUp:
                            {
                                if (_keysDown.Contains(code))
                                    _keysDown.Remove(code);
                                if (Enum.IsDefined(typeof(Keyboard.UsbKeyCodeModifiers), keyCode.ToString()))
                                {
                                    _keyCodeModifier = null;
                                }

                                var keyboard = new Keyboard();

                                Keyboard.UsbKeyCodeModifiers? localModifier = _keyCodeModifier != null
                                    ? (Keyboard.UsbKeyCodeModifiers) _keyCodeModifier
                                    : null;

                                for (int i = 0; i < _keysDown.Count; i++)
                                {
                                    if (i <= 5)
                                    {
                                        if (Enum.IsDefined(typeof(Keyboard.UsbKeyCodeModifiers),
                                                ((Keyboard.LinuxKeyCode) _keysDown[i]).ToString()))
                                        {
                                            if (localModifier == null)
                                            {
                                                localModifier =
                                                    (Keyboard.UsbKeyCodeModifiers) Enum.Parse(
                                                        typeof(Keyboard.UsbKeyCodeModifiers),
                                                        ((Keyboard.LinuxKeyCode) _keysDown[i]).ToString());
                                            }
                                            else
                                            {
                                                localModifier |= (Keyboard.UsbKeyCodeModifiers) Enum.Parse(
                                                    typeof(Keyboard.UsbKeyCodeModifiers),
                                                    ((Keyboard.LinuxKeyCode) _keysDown[i]).ToString());
                                            }
                                        }
                                        else
                                        {
                                            keyboard.ExtraKeys[i] =
                                                Convert.ToByte(Enum.Parse(typeof(Keyboard.UsbKeyCode),
                                                    ((Keyboard.LinuxKeyCode) _keysDown[i]).ToString()));
                                        }
                                    }
                                }

                                keyboard.Modifier = localModifier != null ? Convert.ToByte(localModifier) : null;

                                hidHandler.WriteKeyboardReport(keyboard, hidStream);
                                break;
                            }
                        }

                        break;
                    }
                }
            }
        }).Start();
    }

    public bool IsKeyDown(Keyboard.LinuxKeyCode code)
    {
        return _keysDown.Contains((int) code);
    }
}