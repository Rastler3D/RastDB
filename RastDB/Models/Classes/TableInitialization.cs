using static RastDB.Specification.TableHeader;
using static RastDB.Specification.FieldDescription;
using static RastDB.Specification.RecordDescription;
using RastDB.Exceptions;
using RastDB.Extensions;
using RastDB.Models.Classes;
using RastDB.Utils;

namespace RastDB;

public partial record Table
{

    public Table(StreamAccessor streamAccessor)
    {
        _streamAccessor = streamAccessor;
        TableHeader = new TableHeader(_streamAccessor);
        FieldDescriptions = new List<FieldDescription>();
    }

    public static Table CreateTable(StreamAccessor streamAccessor, string tableName, IEnumerable<ColumnDefinition> columnDefinitions)
    {
        streamAccessor.Write(SIGNATURE_NUMBER_OFFSET, SIGNATURE_NUMBER);
        Table table = new(streamAccessor);
        table.TableHeader.TableName = tableName;
        table.TableHeader.TableType = TableType.Data;
        table.TableHeader.CreationDate = DateTime.UtcNow;
        table.TableHeader.LastModifiedDate = DateTime.UtcNow;
        byte fieldOffset = 0;
        int descriptionOffset = FIELD_DESCRIPTION_OFFSET;
        foreach (var definition in columnDefinitions)
        {
            table.FieldDescriptions.Add(new(streamAccessor,descriptionOffset)
            {
                FieldName = definition.Name,
                TypeArguments = definition.DataType.Arguments.ToArray(),
                FieldLength = (byte)definition.DataType.Length,
                FieldType = definition.DataType.Type,
                FieldOffset = fieldOffset,
                AutoIncrementValue = definition.Identity.Increment,
                NextAutoIncrement = definition.Identity.Seed

            });
            fieldOffset += (byte) definition.DataType.Length;
            descriptionOffset += FIELD_DESCRIPTION_SIZE;
        }
        table._streamAccessor.Write(descriptionOffset,FIELD_DESCRIPTION_TERMINATOR);
        ushort recordOffset = (ushort)(descriptionOffset + FIELD_DESCRIPTION_TERMINATOR_SIZE);
        table.TableHeader.RecordSize = fieldOffset;
        table.TableHeader.RecordsCount = 0;
        table.TableHeader.RecordsOffset = recordOffset;
        table.TableHeader.FirstFree = recordOffset;
        table.TableHeader.FreeSpace = recordOffset;
        table.Records = new RecordCollection(streamAccessor, table.TableHeader, table.FieldDescriptions);
        table._streamAccessor.Flush();
        return table;
    }
    
    public static Table OpenTable(StreamAccessor streamAccessor)
    {
        //if (streamAccessor.ReadUInt16(SIGNATURE_NUMBER_OFFSET) != SIGNATURE_NUMBER) throw new NotATableException();
        Table table = new(streamAccessor);
        int descriptionOffset = FIELD_DESCRIPTION_OFFSET;
        for (;streamAccessor.ReadByte(descriptionOffset) != FIELD_DESCRIPTION_TERMINATOR; descriptionOffset += FIELD_DESCRIPTION_SIZE)
        {
            table.FieldDescriptions.Add(new(streamAccessor, descriptionOffset));
        }
        table.Records = new RecordCollection(streamAccessor, table.TableHeader, table.FieldDescriptions);
        return table;

    }
}
