using HID_API.Gadget;
using HID_API.Handlers;
using HID_API.Utils;

namespace HID_API;

public class HidHandler
{
    public List<KeyboardHandler> HidKeyboardHandlers = new();
    public List<MouseHandler> HidMouseHandlers = new();

    public HidHandler(List<string>? mousePaths, List<string>? keyboardPaths, GadgetHandler.Gadget gadget)
    {
        if (mousePaths == null && keyboardPaths == null)
        {
            throw new Exception("No device paths were configured.");
        }
    }

    public void Stop()
    {
    }

    public void WriteMouseReport(Mouse mouse, FileStream hidStream)
    {
        if (!hidStream.CanWrite)
        {
            return;
        }
        
        byte buttonByte = (byte) ((mouse.LeftButton ? 1 : 0) |
                                  (mouse.RightButton ? 2 : 0) |
                                  (mouse.MiddleButton ? 4 : 0) |
                                  (mouse.FourButton ? 8 : 0) |
                                  (mouse.FiveButton ? 16 : 0));

        float sensitivityMultiplier = mouse.SensitivityMultiplier;

        short x = (short) (mouse.X * sensitivityMultiplier);
        short y = (short) (mouse.Y * sensitivityMultiplier);
        sbyte wheel = (sbyte) mouse.Wheel;

        WriteUtils.WriteMouseReport(hidStream,
            1,
            buttonByte,
            new[] {x, y},
            wheel);
    }

    public void WriteKeyboardReport(Keyboard keyboard, FileStream hidStream)
    {
        if (!hidStream.CanWrite)
        {
            return;
        }
        
        WriteUtils.WriteKeyboardReport(hidStream, keyboard);
    }
}