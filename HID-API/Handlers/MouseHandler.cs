using HID_API.Utils;

namespace HID_API.Handlers;

public class MouseHandler : GenericHandler
{
    public MouseHandler(HidHandler hidHandler, FileStream deviceStream, string path)
    {
        HidHandler = hidHandler;
        DeviceStream = deviceStream;
        DevicePath = path;
        Active = true;
        
        new Thread(() =>
        {
            
        })
        {
            IsBackground = true
        }.Start();
    }

    private void SetMouseRate(int pollRate)
    {
    }
}