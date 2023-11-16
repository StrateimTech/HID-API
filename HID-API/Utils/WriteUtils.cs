using System.Buffers.Binary;
using System.Text;

namespace HID_API.Utils;

public static class WriteUtils
{
    public static void WriteReport(FileStream fileStream, byte[] bytes, bool leaveOpen = true)
    {
        using BinaryWriter binaryWriter = new(fileStream, Encoding.Default, leaveOpen);
        binaryWriter.Write(bytes);
        binaryWriter.Flush();
    }

    public static void WriteReport(BinaryWriter binaryWriter, byte[] bytes)
    {
        binaryWriter.Write(bytes);
        binaryWriter.Flush();
    }

    public static void WriteReport(BinaryWriter binaryWriter, byte reportId, byte[] bytes, short[] shorts,
        sbyte[] signedBytes)
    {
        var data = new List<byte[]>
        {
            new []{reportId},
            bytes
        };
        
        foreach (var shorty in shorts)
        {
            Span<byte> buffer = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16LittleEndian(buffer, shorty);
            
            foreach (var shortByte in buffer)
            {
                data.Add(new []{shortByte});
            }
        }

        data.Add((byte[]) (object) signedBytes);

        binaryWriter.Write(data.SelectMany(d => d).ToArray());
        binaryWriter.Flush();
    }
}