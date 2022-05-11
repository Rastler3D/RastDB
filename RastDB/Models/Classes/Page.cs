using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastDB;

public partial record Page
{
    private MemoryMappedViewAccessor _memoryAccessor;
    public Page(MemoryMappedViewAccessor memoryAccessor,PageMode pageMode)
    {
        _memoryAccessor = memoryAccessor;
        _ = pageMode switch
        {
            PageMode.Open => OpenPage(),
            PageMode.Create => CreatePage(),
            PageMode.Overwrite => OverwritePage(),
            PageMode.OpenOrCreate or _ => OpenOrCreatePage(),     
        };

       
        
       
    }

    public PageHeader PageHeader { get; set; }
    public List<FieldDescription> FieldDescriptions { get; set; }
    public List<Record> Records { get; set; }

   
}



