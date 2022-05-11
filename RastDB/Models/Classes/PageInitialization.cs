using static RastDB.Specification.TableHeader;
using static RastDB.Specification.FieldDescription;
using static RastDB.Specification.RecordDescription;
using RastDB.Exceptions;
using RastDB.Extensions;

namespace RastDB;

public partial record Page
{
    private int RECORDS_OFFSET;
    private int RECORDS_SIZE;
    private bool OverwritePage()
    {
        _memoryAccessor.Fill(0);
        _memoryAccessor.Write(MAGIC_NUMBER_OFFSET, MAGIC_NUMBER);
        _memoryAccessor.Write(FIELD_DESCRIPTION_OFFSET, FIELD_DESCRIPTION_TERMINATOR);
        return true;
    }
    private bool CreatePage()
    {
        if (_memoryAccessor.ReadUInt16(MAGIC_NUMBER_OFFSET) == MAGIC_NUMBER) throw new PageAlreadyExists();

        OverwritePage();
        return true;
    }
    private bool OpenOrCreatePage()
    {
        try
        {
            OpenPage();
        }
        catch (NotAPageException e)
        {
            CreatePage();
        }
        return true;

    }
    private bool OpenPage()
    {
        if (_memoryAccessor.ReadUInt16(MAGIC_NUMBER_OFFSET) != MAGIC_NUMBER) throw new NotAPageException();
        int OFFSET ;
        for (OFFSET = 0; _memoryAccessor.ReadByte(OFFSET + FIELD_DESCRIPTION_OFFSET) != FIELD_DESCRIPTION_TERMINATOR; OFFSET += FIELD_DESCRIPTION_SIZE)
        {
            FieldDescriptions.Add(new(_memoryAccessor, OFFSET));
        }
        RECORDS_OFFSET = OFFSET + FIELD_DESCRIPTION_TERMINATOR_SIZE;
        RECORDS_SIZE = RECORD_VALUES_OFFSET + FieldDescriptions.Aggregate(0, (x, y) => x + y.FieldLength);
        return true;

    }
}
