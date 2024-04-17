namespace HID_API.Gadget;

public class GadgetHandler
{
    private const string UsbGadget = "/sys/kernel/config/usb_gadget/";
    private const string Device = "strings/0x409";
    private const string HidUsb = "functions/hid.usb";
    private const string Configuration = "configs/c.1/";
    
    private readonly Gadget _gadget;
    
    public GadgetHandler(Gadget gadget)
    {
        _gadget = gadget;
    }
    
    public struct Gadget
    {
        public int IdVendor { get; set; }
        public int IdProduct { get; set; }
        public int BcdDevice { get; set; }
        public int BcdUsb { get; set; } = 0x0200;
        public string Serialnumber { get; set; }
        public string Manufacturer { get; set; }
        public string Product { get; set; }

        // 750 * 2mA
        public int MaxPower { get; set; } = 750;

        public string Descriptor { get; set; } = "\\x05\\x01\\x09\\x02\\xA1\\x01\\x09\\x01\\xA1\\x00\\x85\\x01\\x05\\x09\\x19\\x01\\x29\\x05\\x15\\x00\\x25\\x01\\x95\\x05\\x75\\x01\\x81\\x02\\x95\\x01\\x75\\x03\\x81\\x03\\x05\\x01\\x09\\x30\\x09\\x31\\x16\\x01\\x80\\x26\\xFF\\x7F\\x75\\x10\\x95\\x02\\x81\\x06\\x09\\x38\\x15\\x81\\x25\\x7F\\x75\\x08\\x95\\x01\\x81\\x06\\xC0\\xC0\\x05\\x01\\x09\\x06\\xA1\\x01\\x85\\x02\\x05\\x07\\x19\\xE0\\x29\\xE7\\x15\\x00\\x25\\x01\\x75\\x01\\x95\\x08\\x81\\x02\\x75\\x08\\x95\\x01\\x81\\x01\\x75\\x01\\x95\\x03\\x05\\x08\\x19\\x01\\x29\\x03\\x91\\x02\\x75\\x01\\x95\\x05\\x91\\x01\\x75\\x08\\x95\\x06\\x15\\x00\\x26\\xFF\\x00\\x05\\x07\\x19\\x00\\x2A\\xFF\\x00\\x81\\x00\\xC0";
        public int ReportLength { get; set; } = 64;
    }

    public void InitGadget(int hidInstances)
    {
        if (!Directory.Exists(UsbGadget))
        {
            throw new Exception($"Failed to create /usb_gadget/ directory.. ({UsbGadget})");
        }

        var gadgetDir = Path.Combine(UsbGadget, "gadget");
        var gadgetInfo = Directory.CreateDirectory(gadgetDir);
        
        if (!gadgetInfo.Exists)
        {
            throw new Exception("Failed to create /usb_gadget/gadget/ directory.");
        }

        var idVendor = File.CreateText(Path.Combine(gadgetDir, "idVendor"));
        var idProduct= File.CreateText(Path.Combine(gadgetDir, "idProduct"));
        var bcdDevice= File.CreateText(Path.Combine(gadgetDir, "bcdDevice"));
        var bcdUsb= File.CreateText(Path.Combine(gadgetDir, "bcdUSB"));

        idVendor.Write(_gadget.IdVendor);
        idProduct.Write(_gadget.IdProduct);
        bcdDevice.Write(_gadget.BcdDevice);
        bcdUsb.Write(_gadget.BcdUsb);
        
        var gadgetDevice = Path.Combine(UsbGadget, Device);
        var deviceInfo = Directory.CreateDirectory(gadgetDevice);
        
        if (!deviceInfo.Exists)
        {
            throw new Exception("Failed to create /usb_gadget/strings/0x409/ directory.");
        }
        
        var serialNumber = File.CreateText(Path.Combine(gadgetDevice, "serialnumber"));
        var manufacturer= File.CreateText(Path.Combine(gadgetDevice, "manufacturer"));
        var product= File.CreateText(Path.Combine(gadgetDevice, "product"));
        
        serialNumber.Write(_gadget.Serialnumber);
        manufacturer.Write(_gadget.Manufacturer);
        product.Write(_gadget.Product);
        
        var gadgetConfiguration = Path.Combine(gadgetDevice, Configuration);
        var configInfo = Directory.CreateDirectory(gadgetConfiguration);
        
        if (!configInfo.Exists)
        {
            throw new Exception("Failed to create /usb_gadget/configs/c.1/ directory.");
        }
        
        var maxPower = File.CreateText(Path.Combine(gadgetConfiguration, "MaxPower"));
        maxPower.Write(_gadget.MaxPower);

        for (int i = 0; i < hidInstances; i++)
        {
            var gadgetHid = Path.Combine(gadgetDevice, HidUsb + $"{i}");
            var gadgetHidInfo = Directory.CreateDirectory(gadgetHid);

            if (!gadgetHidInfo.Exists)
            {
                continue;
            }

            var protocol = File.CreateText(Path.Combine(gadgetHid, "protocol"));
            var subclass = File.CreateText(Path.Combine(gadgetHid, "subclass"));
            var reportLength = File.CreateText(Path.Combine(gadgetHid, "report_length"));
            var reportDesc = File.CreateText(Path.Combine(gadgetHid, "report_desc"));
            
            protocol.Write(0);
            subclass.Write(0);
            reportLength.Write(_gadget.ReportLength);
            reportDesc.Write(_gadget.Descriptor);

            File.CreateSymbolicLink(gadgetHid, Path.Combine(gadgetConfiguration, HidUsb + $"{i}"));
        }
    }
}