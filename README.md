# HID-API
.NET library leveraging Man-in-the-middle (MITM) for seamless mouse & keyboard passthrough, while allowing for data interception and injection to an external computer.

## Pi setup
Assuming your using a Rpi 4b, Zero, or newer Rpi 5
1. dwc2 driver must be loaded on your pi you can follow this [guide](https://gist.github.com/gbaman/975e2db164b3ca2b51ae11e45e8fd40a?permalink_comment_id=2970837) or [isticktoit "Step 1"](https://www.isticktoit.net/?p=1383)
2. Create a custom gadget here's an [example](./examples/custom_gadget.sh) using the compatible report descriptor. Make sure to run it with sudo, since it makes a directory in ``/sys/kernel/config/usb_gadget/``.
```
sudo ./custom_gadget.sh
sudo restart now
```
3. Now this library will be able to function

## Setup on other devices
- Dedicated [salve](https://en.wikipedia.org/wiki/Master/slave_(technology)) computer must support [USB OTG](https://en.wikipedia.org/wiki/USB_On-The-Go)
- Linux installed on slave computer
- Follow [pi setup](#pi-setup) to get gadget's working or follow [your own guide](https://google.com) _``modprobe gadgetfs``_

## Examples
Start handling inputs from ``/dev/input/mice`` (mouse device path) and output them to ``/dev/hidg0`` (**_the external computer_**)
```c#
var hidThread = new Thread(() => hidHandler = new HidHandler(new[]
    {
        "/dev/input/mice"
    },
    null!, 
    "/dev/hidg0")
)
{
    IsBackground = true
};
hidThread.Start();
```

### Extract state from device (Mouse)
Grabs data out of mouse 0 (*_/dev/input/mice_*), HidMouseHandlers is just a list make sure it's not empty. 
```c#
bool left;
bool right;
hidHandler.HidMouseHandlers[0].MouseLock.EnterReadLock();
try
{
    left = hidHandler.HidMouseHandlers[0].Mouse.LeftButton;
    right = hidHandler.HidMouseHandlers[0].Mouse.RightButton;
}
finally
{
    hidHandler.HidMouseHandlers[0].MouseLock.ExitReadLock();
}
```

### Injecting mouse events through the stream
Moves the mouse down 5 
```c#
hidHandler.HidMouseHandlers[0].MouseLock.EnterReadLock();
try
{
    hidHandler.WriteGenericEvent(hidHandler.HidMouseHandlers[0].Mouse with
    {
        X = 0,
        Y = 5,
        Wheel = 0
    });
}
finally
{
    hidHandler.HidMouseHandlers[0].MouseLock.ExitReadLock();
}
```

## Features
- Hot reloading devices
- Multithreading capable

## Report descriptor used by this library
Parsed output from [eleccelerator](https://eleccelerator.com/usbdescreqparser).
```
0x05, 0x01,        // Usage Page (Generic Desktop Ctrls)
0x09, 0x02,        // Usage (Mouse)
0xA1, 0x01,        // Collection (Application)
0x09, 0x01,        //   Usage (Pointer)
0xA1, 0x00,        //   Collection (Physical)
0x85, 0x01,        //     Report ID (1)
0x05, 0x09,        //     Usage Page (Button)
0x19, 0x01,        //     Usage Minimum (0x01)
0x29, 0x03,        //     Usage Maximum (0x03)
0x15, 0x00,        //     Logical Minimum (0)
0x25, 0x01,        //     Logical Maximum (1)
0x95, 0x03,        //     Report Count (3)
0x75, 0x01,        //     Report Size (1)
0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
0x95, 0x01,        //     Report Count (1)
0x75, 0x05,        //     Report Size (5)
0x81, 0x03,        //     Input (Const,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
0x05, 0x01,        //     Usage Page (Generic Desktop Ctrls)
0x09, 0x30,        //     Usage (X)
0x09, 0x31,        //     Usage (Y)
0x16, 0x01, 0x80,  //     Logical Minimum (-32767)
0x26, 0xFF, 0x7F,  //     Logical Maximum (32767)
0x75, 0x10,        //     Report Size (16)
0x95, 0x02,        //     Report Count (2)
0x81, 0x06,        //     Input (Data,Var,Rel,No Wrap,Linear,Preferred State,No Null Position)
0x09, 0x38,        //     Usage (Wheel)
0x15, 0x81,        //     Logical Minimum (-127)
0x25, 0x7F,        //     Logical Maximum (127)
0x75, 0x08,        //     Report Size (8)
0x95, 0x01,        //     Report Count (1)
0x81, 0x06,        //     Input (Data,Var,Rel,No Wrap,Linear,Preferred State,No Null Position)
0xC0,              //   End Collection
0xC0,              // End Collection
0x05, 0x01,        // Usage Page (Generic Desktop Ctrls)
0x09, 0x06,        // Usage (Keyboard)
0xA1, 0x01,        // Collection (Application)
0x85, 0x02,        //   Report ID (2)
0x05, 0x07,        //   Usage Page (Kbrd/Keypad)
0x19, 0xE0,        //   Usage Minimum (0xE0)
0x29, 0xE7,        //   Usage Maximum (0xE7)
0x15, 0x00,        //   Logical Minimum (0)
0x25, 0x01,        //   Logical Maximum (1)
0x75, 0x01,        //   Report Size (1)
0x95, 0x08,        //   Report Count (8)
0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
0x75, 0x08,        //   Report Size (8)
0x95, 0x01,        //   Report Count (1)
0x81, 0x01,        //   Input (Const,Array,Abs,No Wrap,Linear,Preferred State,No Null Position)
0x75, 0x01,        //   Report Size (1)
0x95, 0x03,        //   Report Count (3)
0x05, 0x08,        //   Usage Page (LEDs)
0x19, 0x01,        //   Usage Minimum (Num Lock)
0x29, 0x03,        //   Usage Maximum (Scroll Lock)
0x91, 0x02,        //   Output (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position,Non-volatile)
0x75, 0x01,        //   Report Size (1)
0x95, 0x05,        //   Report Count (5)
0x91, 0x01,        //   Output (Const,Array,Abs,No Wrap,Linear,Preferred State,No Null Position,Non-volatile)
0x75, 0x08,        //   Report Size (8)
0x95, 0x06,        //   Report Count (6)
0x15, 0x00,        //   Logical Minimum (0)
0x26, 0xFF, 0x00,  //   Logical Maximum (255)
0x05, 0x07,        //   Usage Page (Kbrd/Keypad)
0x19, 0x00,        //   Usage Minimum (0x00)
0x2A, 0xFF, 0x00,  //   Usage Maximum (0xFF)
0x81, 0x00,        //   Input (Data,Array,Abs,No Wrap,Linear,Preferred State,No Null Position)
0xC0,              // End Collection

// 133 bytes
```

### HEX
```
0x05 0x01 0x09 0x02 0xA1 0x01 0x09 0x01 0xA1 0x00 0x85 0x01 0x05 0x09 0x19 0x01 0x29 0x03 0x15 0x00 0x25 0x01 0x95 0x03 0x75 0x01 0x81 0x02 0x95 0x01 0x75 0x05 0x81 0x03 0x05 0x01 0x09 0x30 0x09 0x31 0x16 0x01 0x80 0x26 0xFF 0x7F 0x75 0x10 0x95 0x02 0x81 0x06 0x09 0x38 0x15 0x81 0x25 0x7F 0x75 0x08 0x95 0x01 0x81 0x06 0xC0 0xC0 0x05 0x01 0x09 0x06 0xA1 0x01 0x85 0x02 0x05 0x07 0x19 0xe0 0x29 0xe7 0x15 0x00 0x25 0x01 0x75 0x01 0x95 0x08 0x81 0x02 0x75 0x08 0x95 0x01 0x81 0x01 0x75 0x01 0x95 0x03 0x05 0x08 0x19 0x01 0x29 0x03 0x91 0x02 0x75 0x01 0x95 0x05 0x91 0x01 0x75 0x08 0x95 0x06 0x15 0x00 0x26 0xff 0x00 0x05 0x07 0x19 0x00 0x2a 0xff 0x00 0x81 0x00 
```