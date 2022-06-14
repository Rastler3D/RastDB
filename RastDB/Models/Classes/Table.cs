using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RastDB.Models.Classes;
using RastDB.Utils;

namespace RastDB;

public partial record Table
{
    private readonly StreamAccessor _streamAccessor;

    public TableHeader TableHeader { get; set; }
    public List<FieldDescription> FieldDescriptions { get; set; }
    public RecordCollection Records { get; set; }

   
}



