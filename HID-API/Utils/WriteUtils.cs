namespace HID_API.Utils;

public static class WriteUtils
{
    public static void WriteKeyboardReport(FileStream stream, Keyboard keyboard)
    {
        byte[] buffer = new byte[9];

        // Report ID
        buffer[0] = 2;

        // Keyboard modifier
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

        stream.WriteAsync(buffer);
        stream.Flush();
    }
    
    public static void WriteMouseReport(FileStream stream, byte reportId, byte bytes, short x, short y, sbyte signedByte)
    {
        byte[] buffer = new byte[7];
        
        buffer[0] = reportId;
        buffer[1] = bytes;
        
        buffer[2] = (byte) (x & 0xff);
        buffer[3] = (byte) ((x >> 8) & 0xff);
        buffer[4] = (byte) (y & 0xff);
        buffer[5] = (byte) ((y >> 8) & 0xff);
        
        buffer[6] = (byte)signedByte;
        
        stream.Write(buffer, 0, 7);
    }
}