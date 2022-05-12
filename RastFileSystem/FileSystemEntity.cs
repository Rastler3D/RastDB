using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem
{
    public enum EntityType
    {
        Directory,
        File,
        Any
    }
    public unsafe abstract class FileSystemEntity
    {
        internal FileDescriptor* FileDescriptor;
        internal MemorySequence MemorySequence;
        internal FileSystemManager FileSystemManager;

        public EntityType EntityType => (FileDescriptor->Attribute & FileAttribute.IsDirectory) > 0? EntityType.Directory : EntityType.File;
        public string Name 
        { 
            get=> Marshal.PtrToStringAnsi((IntPtr) FileDescriptor->Name,28);
            set => Marshal.Copy(Encoding.ASCII.GetBytes(value), 0, (IntPtr)FileDescriptor->Name, 28);
        }
        public string Extension
        {
            get => Marshal.PtrToStringAnsi((IntPtr)FileDescriptor->Extension, 3);
            set => Marshal.Copy(Encoding.ASCII.GetBytes(value), 0, (IntPtr)FileDescriptor->Extension, 3);
        }


    }
}
