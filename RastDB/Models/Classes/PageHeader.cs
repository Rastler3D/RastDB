using RastDB.Models.Classes;
using System.IO.MemoryMappedFiles;
using static RastDB.Specification.TableHeader;

namespace RastDB;

public record PageHeader
{
    private MemoryMappedViewAccessor _memoryAccessor;
    public PageHeader(MemoryMappedViewAccessor memoryAccessor)
    {
        _memoryAccessor = memoryAccessor;
        var a = new RecordCollection();
    }

    public PageType PageType 
    {
        get => (PageType)_memoryAccessor.ReadByte(PAGETYPE_OFFSET);
        set => _memoryAccessor.Write(PAGETYPE_OFFSET, (byte)value);
    }
    public PageFlag PageFlag
    {
        get => (PageFlag)_memoryAccessor.ReadByte(PAGEFLAG_OFFSET);
        set => _memoryAccessor.Write(PAGEFLAG_OFFSET, (byte)value);
    }
    public ushort PreviousPage
    {
        get => _memoryAccessor.ReadUInt16(PREVIOUS_PAGE_OFFSET);
        set => _memoryAccessor.Write(PREVIOUS_PAGE_OFFSET,value);
    }
    public ushort NextPage
    {
        get => _memoryAccessor.ReadUInt16(NEXT_PAGE_OFFSET);
        set => _memoryAccessor.Write(NEXT_PAGE_OFFSET, value);
    }
    public ushort RecordsCount
    {
        get => _memoryAccessor.ReadUInt16(RECORDS_COUNT_OFFSET);
        set => _memoryAccessor.Write(RECORDS_COUNT_OFFSET, value);
    }
    public ushort RecordSize
    {
        get => _memoryAccessor.ReadUInt16(RECORD_SIZE_OFFSET);
        set => _memoryAccessor.Write(RECORD_SIZE_OFFSET, value);
    }
    public ushort FreeSpace
    {
        get => _memoryAccessor.ReadUInt16(FREE_SPACE_OFFSET);
        set => _memoryAccessor.Write(FREE_SPACE_OFFSET, value);
    }
    public DateTime CreationDate
    {
        get => DateTime.FromBinary(_memoryAccessor.ReadUInt16(CREATION_DATE_OFFSET));
        set => _memoryAccessor.Write(CREATION_DATE_OFFSET, value.ToBinary());
    }
    public DateTime LastModifiedDate
    {
        get => DateTime.FromBinary(_memoryAccessor.ReadUInt16(LAST_MODIFIED_OFFSET));
        set => _memoryAccessor.Write(LAST_MODIFIED_OFFSET, value.ToBinary());
    }




}


