using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem.IO
{
    public unsafe class File : FileSystemEntity
    {
        public override void Delete()
        {
            FileSystemManager.Delete(FileDescriptor);
        }

        public Stream Open()
        {

            return new RfsStream(this);
        }
        public override string ToString()
        {
            return $"File name: {Name}, File extension: {Extension}, File size: {Size}";
        }
    }
}
