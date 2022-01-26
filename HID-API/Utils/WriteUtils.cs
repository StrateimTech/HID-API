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
        binaryWriter.Write(reportId);
        binaryWriter.Write(bytes);
        foreach (var signedUShort in shorts)
        {
            binaryWriter.Write(signedUShort);
        }

        binaryWriter.Write((byte[]) (object) signedBytes);
        binaryWriter.Flush();
    }
}