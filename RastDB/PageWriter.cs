using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastDB
{
    //internal class PageWriter
    //{
    //    public static void Write(Page page,Stream stream)
    //    {
    //        stream.Position = 0;
    //        BinaryWriter writer = new(stream);
    //        writer.Write((byte)page.PageHeader.PageType);
    //        writer.Write((byte)page.PageHeader.PageFlag);
    //        writer.Write(page.PageHeader.PreviousPage);
    //        writer.Write(page.PageHeader.NextPage);
    //        writer.Write(page.PageHeader.RecordsCount);
    //        writer.Write(page.PageHeader.RecordSize);
    //        writer.Write(page.PageHeader.FreeSpace);
    //        writer.Write(page.PageHeader.CreationDate.ToBinary());
    //        writer.Write(page.PageHeader.LastModifiedDate.ToBinary());
            
    //        foreach(var fieldDescription in page.FieldDescriptions)
    //        {
    //            writer.Write(fieldDescription.FieldName);
    //            writer.Write((byte)fieldDescription.FieldType);
    //            foreach(var fieldAttribute in fieldDescription.FieldAttributes)
    //            {
    //                writer.Write(fieldAttribute);
    //                writer.Write(fieldAttribute);
    //            }
    //            writer.Write(fieldDescription.FieldLength);
    //            writer.Write(fieldDescription.FieldOffset);
    //            writer.Write(fieldDescription.FieldIndex);
    //            writer.Write(fieldDescription.FieldConstrain);
    //            writer.Write(fieldDescription.NextAutoIncrement);
    //        }

    //        foreach(var record in page.Records)
    //        {
    //            writer.Write(record.IsFree);
    //            writer.Write(record.NextFree);
    //            writer.Write(record.Value);
    //        }

    //        writer.Close();

    //    }
    //}
}
