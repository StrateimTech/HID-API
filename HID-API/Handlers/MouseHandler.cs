
namespace HID_API.Handlers;

public class MouseHandler
{
    public MouseSettings Settings { get; } = new();
    private Mouse _mouse = new();

    public readonly string Path;
    public bool Active = true;

    public MouseHandler(HidHandler hidHandler, FileStream mouseFileStream, string streamPath, string hidPath)
    {
        Path = streamPath;
        new Thread(() =>
        {
            var hidStream = hidHandler.CreateHidStream(hidPath);

            // https://wiki.osdev.org/PS/2_Mouse
            // Enable Z axis & side buttons (four, five) via magic sample rate
            mouseFileStream.Write(new byte[] {0xf3, 200, 0xf3, 200, 0xf3, 80});
            mouseFileStream.Flush();

            byte[] buffer = GC.AllocateArray<byte>(4, true);

            var skip = true;
            while (Active)
            {
                var dataAvailable = mouseFileStream.Read(buffer);
                if (dataAvailable == 0)
                {
                    continue;
                }

                var mouseSbyteArray = (sbyte[]) (Array) buffer;

                if (skip)
                {
                    skip = false;
                    continue;
                }

                var fourButton = false;
                var fiveButton = false;
                int wheel;

                mouseSbyteArray[1] = Settings.InvertMouseX ? Convert.ToSByte(Convert.ToInt32(mouseSbyteArray[1]) * -1) : mouseSbyteArray[1];
                mouseSbyteArray[2] = Settings.InvertMouseY ? mouseSbyteArray[2] : Convert.ToSByte(Convert.ToInt32(mouseSbyteArray[2]) * -1);

                if (mouseSbyteArray.Length != 4)
                {
                    mouseSbyteArray[3] = Settings.InvertMouseWheel ? mouseSbyteArray[3] : Convert.ToSByte(Convert.ToInt32(mouseSbyteArray[3]) * -1);
                    wheel = Convert.ToInt32(mouseSbyteArray[3]);
                }
                else
                {
                    fourButton = (mouseSbyteArray[3] & 0x10) > 0;
                    fiveButton = (mouseSbyteArray[3] & 0x20) > 0;

                    int z = (mouseSbyteArray[3] & 0xF) > 7 ? (mouseSbyteArray[3] & 0xF) - 16 : (mouseSbyteArray[3] & 0xF);

                    wheel = Settings.InvertMouseWheel ? z : z * -1;
                }

                var x = mouseSbyteArray[1] * Settings.SensitivityMultiplier.x;
                var y = mouseSbyteArray[2] * Settings.SensitivityMultiplier.y;

                var localMouse = new Mouse
                {
                    LeftButton = (mouseSbyteArray[0] & 0x1) > 0,
                    RightButton = (mouseSbyteArray[0] & 0x2) > 0,
                    MiddleButton = (mouseSbyteArray[0] & 0x4) > 0,
                    FourButton = fourButton,
                    FiveButton = fiveButton,
                    X = (short) x,
                    Y = (short) y,
                    Wheel = wheel
                };

                _mouse = localMouse;

                hidHandler.WriteMouseReport(localMouse, hidStream);
            }
        })
        {
            IsBackground = true
        }.Start();
    }

    public Mouse GetMouseState()
    {
        return _mouse;
    }
}