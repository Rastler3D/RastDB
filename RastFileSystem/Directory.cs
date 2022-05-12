using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem
{
    public class Directory : FileSystemEntity
    {
        public unsafe IEnumerable<FileSystemEntity> EnumerateAll =>  MemorySequence.GetPointerEnumerator<FileDescriptor>().Select(x => FileSystemManager.GetFile((FileDescriptor*)x));
        public IEnumerable<Directory> EnumerateDirectories => EnumerateAll.Where(x => x.EntityType == EntityType.Directory).Cast<Directory>();
        public IEnumerable<File> EnumerateFiles => EnumerateAll.Where(x => x.EntityType == EntityType.File).Cast<File>();
       
        public FileSystemEntity this[string fileName, EntityType entityType = EntityType.Any]
        {
            get => entityType switch
            {
                EntityType.Directory => GetDirectory(fileName),
                EntityType.File => GetFile(fileName),
                EntityType.Any or _ => Get(fileName)
            };

        }
        public Directory GetDirectory(string directoryName)
        {
            return EnumerateDirectories.First(x=>x.Name == directoryName);
        }
        public File GetFile(string fileName)
        {
            return EnumerateFiles.First(x => x.Name == fileName);
        }
        public FileSystemEntity Get(string Name)
        {
            return EnumerateAll.First(x => x.Name == Name);
        }



    }
}
