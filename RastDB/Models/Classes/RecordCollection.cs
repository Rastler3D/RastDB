using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastDB.Models.Classes
{
    public class RecordCollection : IEnumerable<Record>
    {
        MemoryMappedViewAccessor _memoryAccessor;

        public IEnumerator<Record> GetEnumerator()
        {
            yield return new Record();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            RecordCollection list = new RecordCollection();
            
            throw new NotImplementedException();
            
        }
    }
}
