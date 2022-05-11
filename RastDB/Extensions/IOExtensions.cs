using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

namespace RastDB.Extensions;

public static class IOExtensions
{
    public static unsafe byte[] ReadBytes(this MemoryMappedViewAccessor accessor, int offset, int number)
    {
        byte[] arr = new byte[number];
        byte* ptr = (byte*)0;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        Marshal.Copy(IntPtr.Add(new IntPtr(ptr), offset), arr, 0, number);
        accessor.SafeMemoryMappedViewHandle.ReleasePointer();
        return arr;
    }

    public static unsafe void WriteBytes(this MemoryMappedViewAccessor accessor, int offset, byte[] data)
    {
        byte* ptr = (byte*)0;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        Marshal.Copy(data, 0, IntPtr.Add(new IntPtr(ptr), offset), data.Length);
        accessor.SafeMemoryMappedViewHandle.ReleasePointer();
    }
    public static string ReadString(this MemoryMappedViewAccessor accessor, int offset, int length)
    {
        byte[] arr = ReadBytes(accessor, offset, length);
        return Encoding.UTF8.GetString(arr);
    }
    public static void WriteString(this MemoryMappedViewAccessor accessor, int offset, string str, int maxLength)
    {
        byte[] arr = Encoding.UTF8.GetBytes(str, 0, maxLength>str.Length?str.Length:maxLength);
        WriteBytes(accessor, offset, arr);
        
    }
    public static unsafe void Fill(this MemoryMappedViewAccessor accessor, int value, int offset = 0)
    {
        byte[] data = new byte[accessor.Capacity - offset];
        Array.Fill(data, (byte)value);
        WriteBytes(accessor, offset, data);
        
    }

}