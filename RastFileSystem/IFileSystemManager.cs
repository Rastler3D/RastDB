using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem
{
    public unsafe interface IFileSystemManager
    {
        public MemorySequence GetFileMemorySequence(FileDescriptor* fileDescriptor);

        public void GrowFileCapacity(FileDescriptor* fileDescriptor);

        public void DeleteFile(FileDescriptor* fileDescriptor);

        public void SetFileSize(FileDescriptor* fileDescriptor);

    }
}
