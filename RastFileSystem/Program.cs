using RastFileSystem;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using static RastFileSystem.MemoryMappedFileExtensions;

unsafe
{
    new DirectoryInfo("/rdb.rdb").
    FileStream fs = new FileStream($"RastFS.txt", FileMode.OpenOrCreate);
    fs.SetLength(20000);
    fs.Flush();
    var str = MemoryMappedFile.CreateFromFile(fs, null,fs.Length, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
    int page_size = 4096;
    DirectoryInfo

    IntPtr handle = str.SafeMemoryMappedFileHandle.DangerousGetHandle();

    byte* memory = (byte*)VirtualAlloc(IntPtr.Zero, (IntPtr)(page_size * 2), AllocationType.Reserve, MemoryProtection.ReadWrite);
    byte* memory3 = (byte*)VirtualAlloc((IntPtr)memory, (IntPtr)(page_size * 2), AllocationType.Commit, MemoryProtection.ReadWrite);
   
    byte* memory2 = (byte*)VirtualAlloc((IntPtr)memory, (IntPtr)(page_size * 2), AllocationType.Commit, MemoryProtection.ReadWrite);

    var vr = MapViewOfFileEx(handle, FileMapAccessType.Read | FileMapAccessType.Write | FileMapAccessType.Reserve, 0, 0, (UIntPtr)(page_size * 2), (IntPtr)memory);
    *(vr) = 157;

    for (int i = 0; i < 8000; i++)
    {
        *(vr+i) = 157;
    }
    FlushViewOfFile(vr, 8192);

    var a = str.CreateViewAccessor();
    for(int i = 0; i < 8192; i++)
    {
        Console.WriteLine(a.ReadByte(i));
    }



}

