using System.Data.SqlTypes;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using RastDB.Exceptions;
using RastDB.Utils;

namespace RastDB.Extensions;

public static class Extensions
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

    public static StreamAccessor ToAccessor(this Stream stream)
    {
        return new(stream);
    }

    public static string ReadDbField(this StreamAccessor streamAccessor, int recordOffset, FieldDescription fieldDescription)
    {
        var position = fieldDescription.FieldOffset + recordOffset;
        return fieldDescription.FieldType switch
        {
            DataType.INT => streamAccessor.ReadDbInt(position),
            DataType.DECIMAL => streamAccessor.ReadDbDecimal(position,fieldDescription.FieldLength),
            DataType.NVARCHAR => streamAccessor.ReadDbNvarchar(position, fieldDescription.FieldLength),
            _ => throw new UnknownTypeException()
        };

    }
    public static void WriteDbField(this StreamAccessor streamAccessor, int recordOffset, FieldDescription fieldDescription,string value)
    {
        var position = fieldDescription.FieldOffset + recordOffset;
        switch (fieldDescription.FieldType)
        {
            case DataType.INT: streamAccessor.WriteDbInt(position, value); return;

            case DataType.DECIMAL:  streamAccessor. WriteDbDecimal(position, value, fieldDescription.FieldLength, fieldDescription.TypeArguments); return;

            case DataType.NVARCHAR: streamAccessor.WriteDbNvarchar(position, value, fieldDescription.TypeArguments); return;

            default:
                throw new UnknownTypeException();
        }

    }

    public static string ReadDbInt(this StreamAccessor streamAccessor, int position)
    {
        var value = streamAccessor.ReadInt32(position);
        return value.ToString().Trim('\u0001').Trim('\0');
    }
    public static string ReadDbNvarchar(this StreamAccessor streamAccessor, int position, int length)
    {
        var value = streamAccessor.ReadString(position,length);
        return value.Trim('\u0001').Trim('\0');
    }
    public static string ReadDbDecimal(this StreamAccessor streamAccessor, int position, int length)
    {
        var value = streamAccessor.ReadString(position,length);
        return value.ToString().Trim('\u0001').Trim('\0');
    }
    public static void WriteDbInt(this StreamAccessor streamAccessor, int position, string value)
    {
        if (int.TryParse(value, out var parsedValue))
        {
            streamAccessor.Write(position,parsedValue);
            return;
        }

        throw new IncorrectValueException();
    }
    public static void WriteDbNvarchar(this StreamAccessor streamAccessor, int position,string value, byte[] arguments)
    {
        streamAccessor.Write(position, value, arguments[0]);
    }
    public static void WriteDbDecimal(this StreamAccessor streamAccessor, int position, string value, int length, byte[] arguments)
    {

        if (decimal.TryParse(value.Replace(".",","), out var parsedValue))
        {
            parsedValue %= (decimal) Math.Pow(10, arguments[0] - arguments[1]);
            parsedValue = Math.Round(parsedValue, arguments[1]);
            streamAccessor.Write(position, parsedValue.ToString(),length);
            return;
        }

        throw new IncorrectValueException();
    }

}