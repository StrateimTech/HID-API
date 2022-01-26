using HID_API.Utils;

namespace HID_API.Handlers;

public class HidMouseHandler
{
    public Mouse Mouse { get; private set; } = new();
        
    public readonly string Path;
    public readonly FileStream DeviceStream;
    public bool Active = true;

    public HidMouseHandler(HidHandler hidHandler, FileStream mouseFileStream, string streamPath)
    {
        Path = streamPath;
        DeviceStream = mouseFileStream;
        new Thread(() =>
        {
            WriteUtils.WriteReport(mouseFileStream, new byte[] {0xf3, 200, 0xf3, 100, 0xf3, 80});
            var skip = true;
            while (Active)
            {
                var mouseSbyteArray = DataUtils.ReadSByteFromStream(mouseFileStream);
                if (mouseSbyteArray.Length > 0)
                {
                    if (skip)
                    {
                        skip = false;
                        continue;
                    }

                    mouseSbyteArray[1] = Mouse.InvertMouseX ? Convert.ToSByte(Convert.ToInt32(mouseSbyteArray[1]) * -1) : mouseSbyteArray[1];
                    mouseSbyteArray[2] = Mouse.InvertMouseY ? mouseSbyteArray[2] : Convert.ToSByte(Convert.ToInt32(mouseSbyteArray[2]) * -1);
                    mouseSbyteArray[3] = Mouse.InvertMouseWheel ? mouseSbyteArray[3] : Convert.ToSByte(Convert.ToInt32(mouseSbyteArray[3]) * -1);
                    Mouse = new Mouse                         
                    {
                        LeftButton = (mouseSbyteArray[0] & 0x1) > 0,
                        RightButton = (mouseSbyteArray[0] & 0x2) > 0,
                        MiddleButton = (mouseSbyteArray[0] & 0x4) > 0,
                        X = Convert.ToInt32(mouseSbyteArray[1]),
                        Y = Convert.ToInt32(mouseSbyteArray[2]),
                        Wheel = Convert.ToInt32(mouseSbyteArray[3])
                    };
                    hidHandler.WriteMouseReport(Mouse);
                }
            }
        }).Start();
    }
}