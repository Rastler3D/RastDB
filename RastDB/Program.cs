using RastDB;
using RastDB.Extensions;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

int pageSize = 1024;
string fileName = "File.rdb";
MemoryMappedFile dbFile = MemoryMappedFile.CreateFromFile(fileName, FileMode.OpenOrCreate, "Db", 1_000);













//Page page = new Page
//{
//    PageHeader = new()
//    {
//        CreationDate = DateTime.Now,
//        FreeSpace = 10,
//        LastModifiedDate = DateTime.Now,
//        NextPage = 100,
//        PageFlag = PageFlag.FreeSpace,
//        PageType = PageType.Data,
//        PreviousPage = 10,
//        RecordsCount = 10,
//        RecordSize = 10,

//    },
//    FieldDescriptions = new() {
//        new()
//        {
//            FieldAttributes = new string[0],
//            FieldConstrain = 1,
//            FieldIndex = 0,
//            FieldLength = 0,
//            FieldName = "Ale",
//            FieldOffset = 0,
//            FieldType = DataType.INT,
//            NextAutoIncrement = "1"

//        }

//    },
//    Records = new() {
//        new()
//        {
//             IsFree=true,
//             NextFree=1,
//             Value="10"
//        }
//    }


//};
//PageWriter.Write(page,stream);