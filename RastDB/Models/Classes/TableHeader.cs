using RastDB.Models.Classes;
using System.IO.MemoryMappedFiles;
using RastDB.Utils;
using static RastDB.Specification.TableHeader;

namespace RastDB;

public record TableHeader
{
    private readonly StreamAccessor _streamAccessor;
    public TableHeader(StreamAccessor streamAccessor)
    {
        _streamAccessor = streamAccessor;
    }

    public string TableName
    {
        get => _streamAccessor.ReadString(TABLE_NAME_OFFSET, TABLE_NAME_LENGTH);
        set => _streamAccessor.Write(TABLE_NAME_OFFSET, value, TABLE_NAME_LENGTH);
    }
    public TableType TableType 
    {
        get => (TableType)_streamAccessor.ReadByte(TABLETYPE_OFFSET);
        set => _streamAccessor.Write(TABLETYPE_OFFSET, (byte)value);
    }
   

    public ushort RecordsCount
    {
        get => _streamAccessor.ReadUInt16(RECORDS_COUNT_OFFSET);
        set => _streamAccessor.Write(RECORDS_COUNT_OFFSET, value);
    }
    public ushort RecordSize
    {
        get => _streamAccessor.ReadUInt16(RECORD_SIZE_OFFSET);
        set => _streamAccessor.Write(RECORD_SIZE_OFFSET, value);
    }
    public ushort RecordsOffset
    {
        get => _streamAccessor.ReadUInt16(RECORDS_START_OFFSET);
        set => _streamAccessor.Write(RECORDS_START_OFFSET, value);
    }
    public ushort FreeSpace
    {
        get => _streamAccessor.ReadUInt16(FREE_SPACE_OFFSET);
        set => _streamAccessor.Write(FREE_SPACE_OFFSET, value);
    }

    public ushort FirstFree
    {
        get=> _streamAccessor.ReadUInt16(FIRST_FREE_OFFSET);
        set=> _streamAccessor.Write(FIRST_FREE_OFFSET, value);
    }
    public DateTime CreationDate
    {
        get => DateTime.FromBinary(_streamAccessor.ReadUInt16(CREATION_DATE_OFFSET));
        set => _streamAccessor.Write(CREATION_DATE_OFFSET, value.ToBinary());
    }
    public DateTime LastModifiedDate
    {
        get => DateTime.FromBinary(_streamAccessor.ReadUInt16(LAST_MODIFIED_OFFSET));
        set => _streamAccessor.Write(LAST_MODIFIED_OFFSET, value.ToBinary());
    }




}


