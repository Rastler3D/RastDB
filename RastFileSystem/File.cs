using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem
{
    public unsafe class File
    {
        internal FileDescriptor* FileDescriptor;
        internal MemorySequence MemorySequence;
        internal FileSystemManager FileSystemManager;
        public long Size
        {
            get => FileDescriptor->Size;
            set => FileSystemManager.SetFileSize(FileDescriptor, value);
        }

        public Stream Open()
        {
            if (MemorySequence == null)
            {
                FileSystemManager.GetFileMemorySequence(FileDescriptor);
            }
            return new Rfs
        }
    }
}
