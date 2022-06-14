using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
[Flags]
public enum FileAttribute : byte
{
    EmptyEntry = 0,
    ReadAndWrite = 1,
    ReadOnly = 2,
    Hidden = 4,
    System = 8,
    IsDirectory = 16,
    IsFile = 32,
    IsDeleted = 64
}
namespace RastFileSystem.Core
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct FileDescriptor
    {
        [FieldOffset(0)]
        public DateTime CreateDateTime;
        [FieldOffset(8)]
        public fixed byte Name[28];
        [FieldOffset(36)]
        public fixed byte Extension[3];
        [FieldOffset(39)]
        public FileAttribute Attribute;
        [FieldOffset(40)]
        public long FirstClusterIndex;
        [FieldOffset(48)]
        public DateTime ModifyDateTime;
        [FieldOffset(56)]
        public long Size;



    }
}
