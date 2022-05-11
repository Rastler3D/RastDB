using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem
{
    public class FileSystem : IFileSystem
    {
        MemoryMappedViewAccessor FAT;
        public static IFileSystem CreateFromFile(string FSpath)
        {
            FileSystem fs = new FileSystem()
            {
                FAT = MemoryMappedFile.CreateFromFile($"{FSpath}:FAT", FileMode.CreateNew).CreateViewAccessor()
            };
            return fs;

        }

        public static IFileSystem OpenFromFile(string FSpath)
        {
            throw new NotImplementedException();
        }

        public void Copy(string oldPath, string newPath)
        {
            throw new NotImplementedException();
        }

        public Stream Create(string path)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path)
        {
            throw new NotImplementedException();
        }

        public void Move(string oldPath, string newPath)
        {
            throw new NotImplementedException();
        }

        public Stream Open(string path)
        {
            throw new NotImplementedException();
        }
    }
}
