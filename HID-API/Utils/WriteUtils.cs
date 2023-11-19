using System.Buffers.Binary;

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

    public static void WriteMouseReport(FileStream stream, byte reportId, byte[] bytes, short[] shorts,
        sbyte[] signedBytes)
    {
        var data = new List<byte[]>
        {
            new[] {reportId},
            bytes
        };

        foreach (var shorty in shorts)
        {
            Span<byte> buffer = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16LittleEndian(buffer, shorty);

            foreach (var shortByte in buffer)
            {
                data.Add(new[] {shortByte});
            }
        }

        data.Add((byte[]) (object) signedBytes);

        var flatArray = data.SelectMany(d => d).ToArray();
        stream.WriteAsync(flatArray);
        stream.Flush();
    }
}