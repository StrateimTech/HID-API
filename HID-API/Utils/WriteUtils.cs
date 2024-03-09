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

    private static byte[] _mouseBuffer = GC.AllocateArray<byte>(7, true);
    
    public static void WriteMouseReport(FileStream stream, byte reportId, byte bytes, short[] shorts, sbyte signedByte)
    {
        _mouseBuffer[0] = reportId;
        _mouseBuffer[1] = bytes;
        
        Buffer.BlockCopy(shorts, 0, _mouseBuffer, 2, 4);
        _mouseBuffer[6] = (byte)signedByte;
        
        stream.Write(_mouseBuffer, 0, 7);
        // stream.Flush();
    }
}