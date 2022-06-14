using RastDB.Extensions;
using System.IO.MemoryMappedFiles;
using RastDB.Utils;
using static RastDB.Specification.FieldDescription;

namespace RastDB;

public record FieldDescription
{
    private readonly StreamAccessor _streamAccessor;
    private readonly int _offset;

    public FieldDescription(StreamAccessor streamAccessor, int offset)
    {
        _streamAccessor = streamAccessor;
        _offset = offset;
    }

    public string FieldName
    {
        get => _streamAccessor.ReadString(_offset + FIELD_NAME_OFFSET,FIELD_NAME_LENGTH);
        set => _streamAccessor.Write(_offset + FIELD_NAME_OFFSET, value,FIELD_NAME_LENGTH);
    }
    public DataType FieldType
    {
        get => (DataType)_streamAccessor.ReadByte(_offset + FIELD_TYPE_OFFSET);
        set => _streamAccessor.Write(_offset + FIELD_TYPE_OFFSET, (byte)value);
    }
    public byte[] TypeArguments
    {
        get => _streamAccessor.ReadBytes(_offset + TYPE_ARGUMENTS_OFFSET,TYPE_ARGUMENTS_COUNT);
        set => _streamAccessor.Write(_offset + TYPE_ARGUMENTS_OFFSET, value.Length>5?value[0..(TYPE_ARGUMENTS_COUNT-1)]:value);
    }
    public byte FieldLength
    {
        get => _streamAccessor.ReadByte(_offset + FIELD_LENGTH_OFFSET);
        set => _streamAccessor.Write(_offset + FIELD_LENGTH_OFFSET, value);
    }
    public byte FieldOffset
    {
        get => _streamAccessor.ReadByte(_offset + FIELD_OFFSET_OFFSET);
        set => _streamAccessor.Write(_offset + FIELD_OFFSET_OFFSET, value);
    }
    public byte FieldConstraint
    {
        get => _streamAccessor.ReadByte(_offset + FIELD_CONSTRAINT_OFFSET);
        set => _streamAccessor.Write(_offset + FIELD_CONSTRAINT_OFFSET, value);
    }
    public int AutoIncrementValue
    {
        get => _streamAccessor.ReadInt32(_offset + AUTOINCREMENT_VALUE_OFFSET);
        set => _streamAccessor.Write(_offset + AUTOINCREMENT_VALUE_OFFSET, value);
    }
    public int NextAutoIncrement
    {
        get => _streamAccessor.ReadInt32(_offset + NEXT_AUTOINCREMENT_OFFSET);
        set => _streamAccessor.Write(_offset + NEXT_AUTOINCREMENT_OFFSET, value);
    }

}


