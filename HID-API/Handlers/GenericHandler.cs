namespace HID_API.Handlers;

public class GenericHandler
{
    public HidHandler HidHandler { get; set; }
    public FileStream DeviceStream { get; set; }
    public string DevicePath { get; set; }
    public bool Active { get; set; }
}