using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastDB
{
   
    
    public class DbFileReader
    {
        int pageNumber;
        const int pageSize = 1024;

        public void Read(string fileName)
        {
            MemoryMappedFile dbFile = MemoryMappedFile.CreateFromFile(fileName);
            dbFile.CreateViewStream(0, pageSize);


        }
    }
}
