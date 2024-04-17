using HID_API.Utils;

namespace HID_API.Handlers;

public class MouseHandler
{
    public Mouse Mouse { get; set; } = new();
    public readonly ReaderWriterLockSlim MouseLock = new(LockRecursionPolicy.SupportsRecursion);

    public readonly string Path;
    public readonly FileStream DeviceStream;
    public bool Active = true;

    public MouseHandler(HidHandler hidHandler, FileStream mouseFileStream, string streamPath, string hidPath)
    {
        Path = streamPath;
        DeviceStream = mouseFileStream;
        new Thread(() =>
        {
            var hidStream = hidHandler.CreateHidStream(hidPath);
            
            // https://wiki.osdev.org/PS/2_Mouse
            // Enable Z axis & side buttons (four, five) via magic sample rate
            mouseFileStream.Write(new byte[] {0xf3, 200, 0xf3, 200, 0xf3, 80});
            mouseFileStream.Flush();

            var skip = true;
            while (Active)
            {
                sbyte[]? mouseSbyteArray = DataUtils.ReadSByteFromStream(mouseFileStream);
                if (mouseSbyteArray == null)
                {
                    continue;
                }

                if (mouseSbyteArray.Length > 0)
                {
                    if (skip)
                    {
                        skip = false;
                        continue;
                    }

                    var fourButton = false;
                    var fiveButton = false;
                    int wheel;

                    bool invertX;
                    bool invertY;
                    bool invertWheel;

                    MouseLock.EnterReadLock();
                    try
                    {
                        invertX = Mouse.InvertMouseX;
                        invertY = Mouse.InvertMouseY;
                        invertWheel = Mouse.InvertMouseWheel;
                    }
                    finally
                    {
                        MouseLock.ExitReadLock();
                    }
                    
                    mouseSbyteArray[1] = invertX ? Convert.ToSByte(Convert.ToInt32(mouseSbyteArray[1]) * -1) : mouseSbyteArray[1];
                    mouseSbyteArray[2] = invertY ? mouseSbyteArray[2] : Convert.ToSByte(Convert.ToInt32(mouseSbyteArray[2]) * -1);
                    
                    if (mouseSbyteArray.Length != 4)
                    {
                        mouseSbyteArray[3] = invertWheel ? mouseSbyteArray[3] : Convert.ToSByte(Convert.ToInt32(mouseSbyteArray[3]) * -1);
                        wheel = Convert.ToInt32(mouseSbyteArray[3]);
                    }
                    else
                    {
                        fourButton = (mouseSbyteArray[3] & 0x10) > 0;
                        fiveButton = (mouseSbyteArray[3] & 0x20) > 0;
                    
                        int z = (mouseSbyteArray[3] & 0xF) > 7 ? (mouseSbyteArray[3] & 0xF) - 16 : (mouseSbyteArray[3] & 0xF);
                    
                        wheel = invertWheel ? z : z * -1;
                    }
                    
                    var localMouse = new Mouse
                    {
                        LeftButton = (mouseSbyteArray[0] & 0x1) > 0,
                        RightButton = (mouseSbyteArray[0] & 0x2) > 0,
                        MiddleButton = (mouseSbyteArray[0] & 0x4) > 0,
                        FourButton = fourButton,
                        FiveButton = fiveButton,
                        X = Convert.ToInt32(mouseSbyteArray[1]),
                        Y = Convert.ToInt32(mouseSbyteArray[2]),
                        Wheel = wheel
                    };
                    
                    MouseLock.EnterWriteLock();
                    try
                    {
                        Mouse = localMouse;
                    }
                    finally
                    {
                        MouseLock.ExitWriteLock();
                    }

                    hidHandler.WriteMouseReport(localMouse, hidStream);
                }
            }
        }).Start();
    }
}