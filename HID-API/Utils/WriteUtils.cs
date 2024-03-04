using System.Buffers.Binary;
using System.Diagnostics;

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

    public static void WriteMouseReport(FileStream stream, byte reportId, byte bytes, short[] shorts, sbyte signedByte)
    {
        int totalSize = sizeof(byte) + sizeof(byte) + (sizeof(short) * shorts.Length) + sizeof(sbyte);
        
        using var memoryStream = new MemoryStream(totalSize);
        memoryStream.WriteByte(reportId);
        memoryStream.WriteByte(bytes);
    
        byte[] shortBytes = new byte[shorts.Length * sizeof(short)];
        Buffer.BlockCopy(shorts, 0, shortBytes, 0, shortBytes.Length);
        memoryStream.Write(shortBytes, 0, shortBytes.Length);
        
        memoryStream.WriteByte((byte)signedByte);
    
        byte[] buffer = memoryStream.ToArray();
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }
}