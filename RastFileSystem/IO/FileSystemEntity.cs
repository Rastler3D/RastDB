using System.Runtime.InteropServices;
using System.Text;
using RastFileSystem.Core;

namespace RastFileSystem.IO;

public enum EntityType
{
    Directory,
    File,
    Any
}

public abstract unsafe class FileSystemEntity
{
    private readonly Lazy<MemorySequence> _memorySequence;
    internal FileDescriptor* FileDescriptor;

    internal FileSystemManager FileSystemManager;

    protected FileSystemEntity()
    {
        _memorySequence = new Lazy<MemorySequence>(() => FileSystemManager.GetFileMemorySequence(FileDescriptor));
    }

    internal MemorySequence MemorySequence => _memorySequence.Value;
    internal bool IsMemorySequenceInitialized => _memorySequence.IsValueCreated;

    public long Size
    {
        get => FileDescriptor->Size;
        set => FileSystemManager.SetFileSize(FileDescriptor, value);
    }

    public EntityType EntityType => (FileDescriptor->Attribute & FileAttribute.IsDirectory) > 0
        ? EntityType.Directory
        : EntityType.File;

    public string Name
    {
        get => Marshal.PtrToStringUTF8((IntPtr) FileDescriptor->Name, 28).Trim('\0');
        set => Marshal.Copy(Encoding.ASCII.GetBytes(value), 0, (IntPtr) FileDescriptor->Name,
            value.Length > 28 ? 28 : value.Length);
    }

    public string Extension
    {
        get => Marshal.PtrToStringAnsi((IntPtr) FileDescriptor->Extension, 3).Trim('\0');
        set => Marshal.Copy(Encoding.ASCII.GetBytes(value), 0, (IntPtr) FileDescriptor->Extension, 3);
    }

    public abstract void Delete();
}