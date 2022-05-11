using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem
{
    
    public unsafe class ClusterChain : List<IntPtr>
    {
        public event EventHandler<ClusterChain> AddingCluster;
        public void Add()
        {
            AddingCluster.Invoke(this);
        }
        public void Attach(FATEntry* entry)
        { 
            FATEntry* lastEntry = (FATEntry*)this.Last();
            lastEntry->ClusterStatus ^= ClusterStatus.EOC;
            lastEntry->Value = entry->ClusterOffset;
            Add((IntPtr)entry);
        }

        public void Add(FATEntry* cluster)
        {
            Add((IntPtr)cluster);
        }
    }
}
