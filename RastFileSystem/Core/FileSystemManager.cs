using RastFileSystem.Core;
using RastFileSystem.Extensions;
using RastFileSystem.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Directory = RastFileSystem.IO.Directory;
using File = RastFileSystem.IO.File;

namespace RastFileSystem.Core
{

    public delegate void EventHandler<Sender, EventArgs>(Sender sender, EventArgs eventArgs);
    public delegate void EventHandler<Sender>(Sender sender);
    public unsafe class FileSystemManager : IDisposable
    {
        public Directory RootDirectory => GetDirectory(&FSBR->RootDirectory);
        private const int FileSignature = 0x1eaf;
        public long ClusterCount => FSBR->ClusterCount;
        public long ClusterSize => FSBR->ClusterSize;
        public FSBR* FSBR;
        private FATEntry* FAT;
        private FileStream FATStream;

        public static FileSystemManager OpenOrCreateFileSystem(string filePath, int clusterSize = 1 * 1024, int initialClusterCount = 64, int growClusterCount = 32)
        {
            FileSystemManager fileSystem = new FileSystemManager(filePath, FileMode.OpenOrCreate);
            if (fileSystem.FSBR->FileSignature != FileSignature)
            {
                fileSystem.FSBR->FileSignature = FileSignature;
                fileSystem.FSBR->ClusterSize = clusterSize;
                fileSystem.FSBR->InitialClusterCount = initialClusterCount;
                fileSystem.FSBR->GrowClusterCount = growClusterCount;
                fileSystem.FSBR->FirstFreeCluster = 0;
                fileSystem.FSBR->LastFreeCluster = 0;
                fileSystem.AddCluster(initialClusterCount);
                fileSystem.CreateDirectory(&fileSystem.FSBR->RootDirectory, "root");
            }
            return fileSystem;
        }


        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                FATStream?.Dispose();
                DATA?.Dispose();
                DATAStream?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FileSystemManager()
        {
            Dispose(false);
        }

        public MemoryMappedFile DATA;
        private FileStream DATAStream;
        public static FileSystemManager OpenFileSystem(string filePath)
        {
            FileSystemManager fileSystem = new FileSystemManager(filePath, FileMode.Open);
            if (fileSystem.FSBR->FileSignature != FileSignature)
            {
                throw new FileSystemNotExistException();
            }
            return fileSystem;
        }
        public static FileSystemManager CreateFileSystem(string filePath, int clusterSize = 64 * 1024, int initialClusterCount = 64, int growClusterCount = 32)
        {
            FileSystemManager fileSystem = new FileSystemManager(filePath, FileMode.Create, initialClusterCount, clusterSize);
            fileSystem.FSBR->FileSignature = FileSignature;
            fileSystem.FSBR->ClusterSize = clusterSize;
            fileSystem.FSBR->InitialClusterCount = initialClusterCount;
            fileSystem.FSBR->GrowClusterCount = growClusterCount;
            fileSystem.FSBR->FirstFreeCluster = 0;
            fileSystem.FSBR->LastFreeCluster = 0;
            fileSystem.AddCluster(initialClusterCount);
            fileSystem.CreateDirectory(&fileSystem.FSBR->RootDirectory, "root");

            return fileSystem;
        }

        private FileSystemManager(string filePath, FileMode fileMode, int initialClusterCount = 64, int clusterSize = 64 * 1024)
        {
            DATAStream = new FileStream($"{filePath}::$DATA", fileMode);
            if (DATAStream.Length == 0) DATAStream.SetLength(initialClusterCount * clusterSize);
            DATA = MemoryMappedFile.CreateFromFile(
                DATAStream,
                null,
                DATAStream.Length,
                MemoryMappedFileAccess.ReadWrite,
                HandleInheritability.None,
                false
            );
            FileStream FSBRStream = new FileStream($"{filePath}:FSBR", fileMode);
            if (FSBRStream.Length == 0) FSBRStream.SetLength(sizeof(FSBR));
            FSBR = (FSBR*)MemoryMappedFile.CreateFromFile(
                FSBRStream,
                null,
                FSBRStream.Length,
                MemoryMappedFileAccess.ReadWrite,
                HandleInheritability.None,
                false).CreateViewPointer();
            FATStream = new FileStream($"{filePath}:FAT", fileMode);
            if (FATStream.Length == 0) FATStream.SetLength(initialClusterCount * sizeof(FATEntry));
            FAT = (FATEntry*)MemoryMappedFile.CreateFromFile(
                FATStream,
                null,
                FATStream.Length,
                MemoryMappedFileAccess.ReadWrite,
                HandleInheritability.None,
                false
            ).CreateViewPointer();

        }

        public long[] AddClusterToChain(long startClusterOffset, int addClusterCount)
        {
            long[] addedClusters = new long[addClusterCount];
            FATEntry* lastCluster = GetCluster(GetClusterChain(startClusterOffset).Last());
            for (int i = 0; i < addClusterCount; i++)
            {
                long newClusterOffset = ClaimFirstFreeCluster();
                addedClusters[i] = newClusterOffset;
                FATEntry* newCluster = GetCluster(newClusterOffset);
                lastCluster->ClusterStatus ^= ClusterStatus.EOC;
                lastCluster->Value = newClusterOffset;
                lastCluster = newCluster;
            }

            return addedClusters;

        }

        public void FreeClusterChain(long startClusterOffset)
        {
            FATEntry* lastFreeCluster = GetCluster(FSBR->LastFreeCluster);
            lastFreeCluster->ClusterStatus ^= ClusterStatus.EOC;
            lastFreeCluster->Value = startClusterOffset;

            long nextCluster = startClusterOffset;
            while (true)
            {
                FATEntry* cluster = GetCluster(nextCluster);
                cluster->ClusterStatus |= ClusterStatus.Free;
                if ((cluster->ClusterStatus & ClusterStatus.EOC) == ClusterStatus.EOC)
                {
                    FSBR->LastFreeCluster = nextCluster;
                    break;
                }
                nextCluster = cluster->Value;
            }
        }
        public long ClaimFirstFreeCluster()
        {
            long clusterOffset = FSBR->FirstFreeCluster;
            FATEntry* cluster = GetCluster(clusterOffset);
            if ((cluster->ClusterStatus & ClusterStatus.EOC) == ClusterStatus.EOC)
            {
                AddCluster(FSBR->GrowClusterCount);
            }
            FSBR->FirstFreeCluster = cluster->Value;
            cluster->ClusterStatus = ClusterStatus.Claimed | ClusterStatus.EOC;
            return clusterOffset;
        }
        public long[] GetClusterChain(long startClusterOffset)
        {
            List<long> clusterChain = new();
            clusterChain.Add(startClusterOffset);
            long nextCluster = startClusterOffset;
            while (true)
            {
                FATEntry* cluster = GetCluster(nextCluster);
                if ((cluster->ClusterStatus & ClusterStatus.EOC) != 0) break;
                nextCluster = cluster->Value;
                clusterChain.Add(nextCluster);
            }

            return clusterChain.ToArray();
        }
        public MemoryMappedViewAccessor OpenCluster(long clusterOffset)
        {
            return DATA.CreateViewAccessor(clusterOffset * ClusterSize, ClusterSize);
        }
        public FATEntry* GetCluster(long clusterOffset)
        {
            if (clusterOffset < 0 || clusterOffset > FSBR->ClusterCount)
            {
                throw new IndexOutOfRangeException();
            }
            return FAT + clusterOffset;
        }

        public void AddCluster(long clusterCountToAdd = 1)
        {

            if (ClusterCount + clusterCountToAdd > FATStream.Length / sizeof(FATEntry))
            {
                long addedLength = (clusterCountToAdd / (FSBR->GrowClusterCount + 1) + 1) * FSBR->GrowClusterCount * sizeof(FATEntry);
                FATStream.SetLength(FATStream.Length + addedLength);
                FAT = (FATEntry*)MemoryMappedFile.CreateFromFile(
                   FATStream,
                   null,
                   FATStream.Length,
                   MemoryMappedFileAccess.ReadWrite,
                   HandleInheritability.None,
                   false
               ).CreateViewPointer();
            };
            if (ClusterCount + clusterCountToAdd > DATAStream.Length / ClusterSize)
            {
                long addedLength = (clusterCountToAdd / FSBR->GrowClusterCount + 1) * FSBR->GrowClusterCount * ClusterSize;
                DATAStream.SetLength(DATAStream.Length + addedLength);
                DATA = MemoryMappedFile.CreateFromFile(
                   DATAStream,
                   null,
                   DATAStream.Length,
                   MemoryMappedFileAccess.ReadWrite,
                   HandleInheritability.None,
                   false
               );
            };

            FATEntry* lastFreeCluster = GetCluster(FSBR->LastFreeCluster);
            lastFreeCluster->ClusterStatus ^= ClusterStatus.EOC;
            lastFreeCluster->Value = FSBR->ClusterCount;
            FATEntry* newCluster;
            for (int i = 0; i < clusterCountToAdd; i++)
            {
                newCluster = GetCluster(FSBR->ClusterCount++);
                newCluster->ClusterStatus = ClusterStatus.Free;
                newCluster->Value = FSBR->ClusterCount;
            }
            newCluster = GetCluster(FSBR->ClusterCount - 1);
            newCluster->ClusterStatus |= ClusterStatus.EOC;
            newCluster->Value = -1;
            FSBR->LastFreeCluster = FSBR->ClusterCount - 1;
        }

        public Dictionary<IntPtr, WeakReference<FileSystemEntity>> OpenedFile = new();

        public MemorySequence GetFileMemorySequence(FileDescriptor* fileDescriptor)
        {
            var clusters = GetClusterChain(fileDescriptor->FirstClusterIndex);
            var memoryBlocks = clusters.Select(x => OpenCluster(x)).ToArray();
            return new MemorySequence(memoryBlocks); ;

        }
        public unsafe void CreateDirectory(FileDescriptor* directoryPointer, string directoryName)
        {
            directoryPointer->CreateDateTime = DateTime.Now;
            directoryPointer->ModifyDateTime = DateTime.Now;
            directoryPointer->FirstClusterIndex = ClaimFirstFreeCluster();
            directoryPointer->Size = ClusterSize;
            directoryPointer->Attribute = FileAttribute.IsDirectory | FileAttribute.ReadAndWrite;
            Marshal.Copy(Encoding.ASCII.GetBytes(directoryName), 0, (IntPtr)directoryPointer->Name, directoryName.Length > 28 ? 28 : directoryName.Length);
            Marshal.Copy(Encoding.ASCII.GetBytes(string.Empty), 0, (IntPtr)directoryPointer->Extension, string.Empty.Length);
            string str = Marshal.PtrToStringAnsi((IntPtr)directoryPointer->Name, 28);
        }
        public unsafe void CreateFile(FileDescriptor* filePointer, string fileName, string fileExtension = null)
        {
            filePointer->CreateDateTime = DateTime.Now;
            filePointer->ModifyDateTime = DateTime.Now;
            filePointer->FirstClusterIndex = ClaimFirstFreeCluster();
            filePointer->Size = 0;
            filePointer->Attribute = FileAttribute.IsFile | FileAttribute.ReadAndWrite;
            Marshal.Copy(Encoding.ASCII.GetBytes(fileName), 0, (IntPtr)filePointer->Name, fileName.Length > 28 ? 28 : fileName.Length);
            Marshal.Copy(Encoding.ASCII.GetBytes(fileExtension ?? string.Empty), 0, (IntPtr)filePointer->Extension, (fileExtension ?? string.Empty).Length > 3 ? 3 : fileExtension.Length);
        }
        public void GrowDirectoryCapacity(FileDescriptor* fileDescriptor, int clusterToAdd = 1)
        {
            fileDescriptor->Size += ClusterSize * clusterToAdd;
            GrowFileCapacity(fileDescriptor);
        }
        public void TrimDirectoryCapacity(FileDescriptor* fileDescriptor, int clusterToTrim = 1)
        {
            fileDescriptor->Size -= ClusterSize * clusterToTrim;
            TrimFileCapacity(fileDescriptor);
        }
        public void GrowFileCapacity(FileDescriptor* fileDescriptor)
        {
            long[] clusters = GetClusterChain(fileDescriptor->FirstClusterIndex);
            int currentClusterCount = clusters.Length;
            int targetClusterCount = (int)(fileDescriptor->Size / ClusterSize + 1);
            int needToAddCount = targetClusterCount - currentClusterCount;
            long[] newClusters = AddClusterToChain(fileDescriptor->FirstClusterIndex, needToAddCount);
            WeakReference<FileSystemEntity> weakRefOpennedFile;
            FileSystemEntity opennedFile;

            if (OpenedFile.TryGetValue((IntPtr)fileDescriptor, out weakRefOpennedFile) && weakRefOpennedFile.TryGetTarget(out opennedFile) && opennedFile.IsMemorySequenceInitialized)
            {
                foreach (long cluster in newClusters)
                {

                    opennedFile.MemorySequence.Add(OpenCluster(cluster));

                }
                opennedFile.Size = fileDescriptor->Size;
            }
        }
        public void TrimFileCapacity(FileDescriptor* fileDescriptor)
        {
            long[] clusters = GetClusterChain(fileDescriptor->FirstClusterIndex);
            int currentClusterCount = clusters.Length;
            int targetClusterCount = (int)(fileDescriptor->Size / ClusterSize + 1);
            int needToTrimCount = currentClusterCount - targetClusterCount;
            if (needToTrimCount > 0)
            {
                GetCluster(clusters[targetClusterCount - 1])->ClusterStatus = ClusterStatus.EOC;
                FreeClusterChain(clusters[targetClusterCount]);
                FileSystemEntity opennedFile;
                WeakReference<FileSystemEntity> weakRefOpennedFile;
                if (OpenedFile.TryGetValue((IntPtr)fileDescriptor, out weakRefOpennedFile) && weakRefOpennedFile.TryGetTarget(out opennedFile) && opennedFile.IsMemorySequenceInitialized)
                {
                    opennedFile.MemorySequence.Trim(targetClusterCount);
                    opennedFile.MemorySequence.Size = fileDescriptor->Size;

                }
            }

        }
        public unsafe void Copy(FileDescriptor* fromFileDescriptor, FileDescriptor* toFileDescriptor)
        {
            *toFileDescriptor = *fromFileDescriptor;
        }
        public unsafe bool IsDeleted(FileDescriptor* fileDescriptor)
        {
            return *(char*)fileDescriptor == '\0';
        }
        public FileSystemEntity GetFSEntity(FileDescriptor* fileDescriptor)
        {
            if ((fileDescriptor->Attribute & FileAttribute.IsDirectory) > 0)
            {
                return GetDirectory(fileDescriptor);
            }
            return GetFile(fileDescriptor);
        }
        public File GetFile(FileDescriptor* fileDescriptor)
        {
            FileSystemEntity file;
            WeakReference<FileSystemEntity> reference;
            if (OpenedFile.TryGetValue((IntPtr)fileDescriptor, out reference))
            {
                if (reference.TryGetTarget(out file))
                {
                    return (File)file;
                }
                OpenedFile.Remove((IntPtr)fileDescriptor);
            }
            file = new File { FileDescriptor = fileDescriptor, FileSystemManager = this };
            reference = new(file);
            OpenedFile.Add((IntPtr)fileDescriptor, reference);
            return (File)file;
        }
        public Directory GetDirectory(FileDescriptor* fileDescriptor)
        {
            FileSystemEntity directory;
            WeakReference<FileSystemEntity> reference;
            if (OpenedFile.TryGetValue((IntPtr)fileDescriptor, out reference))
            {
                if (reference.TryGetTarget(out directory))
                {
                    return (Directory)directory;
                }
                OpenedFile.Remove((IntPtr)fileDescriptor);
            }
            directory = new Directory { FileDescriptor = fileDescriptor, FileSystemManager = this };
            reference = new(directory);
            OpenedFile.Add((IntPtr)fileDescriptor, reference);
            return (Directory)directory;

        }
        public void Delete(FileDescriptor* fileDescriptor)
        {
            FreeClusterChain(fileDescriptor->FirstClusterIndex);
            *(char*)fileDescriptor = '\0';
        }


        public void SetFileSize(FileDescriptor* fileDescriptor, long size)
        {
            long oldSize = fileDescriptor->Size;
            long oldClusterCount = oldSize / ClusterSize + 1;
            long newClusterCount = size / ClusterSize + 1;
            fileDescriptor->Size = size;
            if (newClusterCount > oldClusterCount) GrowFileCapacity(fileDescriptor);
            if (oldClusterCount > newClusterCount) TrimFileCapacity(fileDescriptor);



        }
    }
}
public enum ClusterStatus : byte
{
    Claimed = 0b00001000,
    Free = 0b00001001,
    EOC = 0b00000100
}

[StructLayout(LayoutKind.Sequential)]
public struct FATEntry
{
    public const long EOC = -1;
    public long ClusterOffset;
    public ClusterStatus ClusterStatus;
    public long Value;

    public FATEntry(long clusterOffset, ClusterStatus clusterStatus, long value)
    {
        ClusterOffset = clusterOffset;
        ClusterStatus = clusterStatus;
        Value = value;
    }
}
[StructLayout(LayoutKind.Sequential)]
public struct FSBR
{
    public int FileSignature;
    public long ClusterSize;
    public long InitialClusterCount;
    public long GrowClusterCount;
    public long ClusterCount;
    public long FirstFreeCluster;
    public long LastFreeCluster;
    public FileDescriptor RootDirectory;
}
