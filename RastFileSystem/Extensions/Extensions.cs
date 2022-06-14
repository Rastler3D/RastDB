using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem.Extensions
{
    public static unsafe class Extensions
    {
        [Flags]
        public enum FreeType
        {
            Decommit = 0x4000,
            Release = 0x8000,
        }
        [Flags]
        public enum FileMapAccessType : uint
        {
            Copy = 0x01,
            Write = 0x02,
            Read = 0x04,
            AllAccess = 0x08,
            Execute = 0x20,
            Reserve = 0x80000000
        }
        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        public static byte* CreateViewPointer(this MemoryMappedFile mappedFile, byte* desiredAddress = null)
        {
            return mappedFile.CreateViewPointer(0, 0, desiredAddress);
        }
        public static byte* CreateViewPointer(this MemoryMappedFile mappedFile, long offset, long size, byte* desiredAddress = null)
        {
            uint* offsetPointer = (uint*)&offset;
            IntPtr fileHandler = mappedFile.SafeMemoryMappedFileHandle.DangerousGetHandle();
            return MapViewOfFileEx(
                fileHandler,
                FileMapAccessType.Write,
                offsetPointer[1],
                offsetPointer[0],
                new UIntPtr((uint)size),
                (IntPtr)desiredAddress
            );
        }
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static unsafe extern bool VirtualFree(IntPtr lpAddress,
   int dwSize, FreeType dwFreeType);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress,
                     IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern byte* MapViewOfFileEx(IntPtr hFileMappingObject,
   FileMapAccessType dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow,
   UIntPtr dwNumberOfBytesToMap, IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile(byte* address);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlushViewOfFile(byte* address, int bytesToFlush);

    }
}
