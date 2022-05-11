using RastDB.Extensions;
using System.IO.MemoryMappedFiles;
using static RastDB.Specification.FieldDescription;

namespace RastDB;

public record FieldDescription
{
    private MemoryMappedViewAccessor _memoryAccessor;
    private int _offset;

    public FieldDescription(MemoryMappedViewAccessor memoryAccessor, int offset)
    {
        _memoryAccessor = memoryAccessor;
        _offset = offset;
    }

    public string FieldName
    {
        get => _memoryAccessor.ReadString(_offset + FIELD_NAME_OFFSET,FIELD_NAME_SIZE);
        set => _memoryAccessor.WriteString(_offset + FIELD_NAME_OFFSET, value,FIELD_NAME_SIZE);
    }
    public DataType FieldType
    {
        get => (DataType)_memoryAccessor.ReadByte(_offset + FIELD_TYPE_OFFSET);
        set => _memoryAccessor.Write(_offset + FIELD_TYPE_OFFSET, (byte)value);
    }
    public byte FieldLength
    {
        get => _memoryAccessor.ReadByte(_offset + FIELD_LENGTH_OFFSET);
        set => _memoryAccessor.Write(_offset + FIELD_LENGTH_OFFSET, value);
    }
    public byte FieldOffset
    {
        get => _memoryAccessor.ReadByte(_offset + FIELD_OFFSET_OFFSET);
        set => _memoryAccessor.Write(_offset + FIELD_OFFSET_OFFSET, value);
    }
    public byte FieldIndex
    {
        get => _memoryAccessor.ReadByte(_offset + FIELD_INDEX_OFFSET);
        set => _memoryAccessor.Write(_offset + FIELD_INDEX_OFFSET, value);
    }
    public byte FieldConstraint
    {
        get => _memoryAccessor.ReadByte(_offset + FIELD_CONSTRAINT_OFFSET);
        set => _memoryAccessor.Write(_offset + FIELD_CONSTRAINT_OFFSET, value);
    }
    public ulong NextAutoIncrement
    {
        get => _memoryAccessor.ReadUInt64(_offset + NEXT_AUTOINCREMENT_OFFSET);
        set => _memoryAccessor.Write(_offset + NEXT_AUTOINCREMENT_OFFSET, value);
    }
}


