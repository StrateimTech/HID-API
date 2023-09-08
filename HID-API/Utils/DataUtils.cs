using System.Collections;

namespace HID_API.Utils;

public static class DataUtils
{
    public static byte ToByte(BitArray bits)
    {
        byte[] bytes = new byte[1];
        bits.CopyTo(bytes, 0);
        return bytes[0];
    }

    public static sbyte[]? ReadSByteFromStream(FileStream fileStream, int length = 4)
    {
        var byteArray = new byte[length];
        var dataAvailable = fileStream.Read(byteArray, 0, byteArray.Length);
        if (dataAvailable == 0)
        {
            return null;
        }

        var sbyteArray = new sbyte[byteArray.Length];
        Buffer.BlockCopy(byteArray, 0, sbyteArray, 0, byteArray.Length);
        return sbyteArray;
    }
}