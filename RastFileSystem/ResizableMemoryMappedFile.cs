using RastFileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace RastFileSystem
{

    public delegate void EventHandler<Sender, EventArgs>(Sender sender, EventArgs eventArgs);
    public delegate void EventHandler<Sender>(Sender sender);
    public unsafe class FileSystemManager : IFileSystemManager
    {
        private long ClusterCount => FSBR->ClusterCount;
        private long ClusterSize => FSBR->ClusterSize;
        public FSBR* FSBR;
        private FATEntry* FAT;
        private FileStream FATStream;
        public MemoryMappedFile DATA;
        private FileStream DATAStream;


        public FileSystemManager(string filePath)
        {
            FSBR = (FSBR*)MemoryMappedFile.CreateFromFile($"{filePath}:FSBR").CreateViewPointer();
            FATStream = new FileStream($"{filePath}:FAT", FileMode.OpenOrCreate);
            FAT = (FATEntry*)MemoryMappedFile.CreateFromFile(
                FATStream,
                null,
                FATStream.Length,
                MemoryMappedFileAccess.ReadWrite,
                HandleInheritability.None,
                false
            ).CreateViewPointer();
            DATAStream = new FileStream(filePath, FileMode.OpenOrCreate);
            DATA = MemoryMappedFile.CreateFromFile(
                DATAStream,
                null,
                DATAStream.Length,
                MemoryMappedFileAccess.ReadWrite,
                HandleInheritability.None,
                false
            );


        }

        public FATEntry*[] AddClusterToChain(long startClusterOffset, int addClusterCount)
        {
            FATEntry*[] addedClusters = new FATEntry*[addClusterCount];
            FATEntry* lastCluster = (FATEntry*)GetClusterChain(startClusterOffset).Last();
            for (int i = 0; i < addClusterCount; i++)
            {
                FATEntry* newCluster = ClaimFirstFreeCluster();
                lastCluster->ClusterStatus ^= ClusterStatus.EOC;
                lastCluster->Value = newCluster->ClusterOffset;
                addedClusters[i] = newCluster;
                lastCluster = newCluster;
            }

            return addedClusters;

        }

        public void FreeClusterChain(long startClusterOffset)
        {
            FATEntry* cluster = GetCluster(startClusterOffset);
            FSBR->LastFreeCluster = cluster->ClusterOffset;
            cluster->ClusterStatus = ClusterStatus.Free;
            long nextClustel = cluster->Value;
            while (true)
            {
                cluster = GetCluster(nextClustel);
                cluster->ClusterStatus |= ClusterStatus.Free;
                cluster->ClusterOffset = startClusterOffset;
                if ((cluster->ClusterStatus & ClusterStatus.EOC) == ClusterStatus.EOC) break;
                nextClustel = cluster->Value;
            }
        }
        public FATEntry* ClaimFirstFreeCluster()
        {
            FATEntry* entry = GetCluster(FSBR->FirstFreeCluster);
            FSBR->FirstFreeCluster = entry->Value;
            entry->ClusterStatus = ClusterStatus.Claimed | ClusterStatus.EOC;
            return entry;
        }
        public IntPtr[] GetClusterChain(long startClusterOffset)
        {
            var clusterChain = Enumerable.Empty<IntPtr>();
            long nextCluster = startClusterOffset;
            while (true)
            {
                FATEntry* cluster = GetCluster(nextCluster);
                clusterChain.Append((IntPtr)cluster);
                if ((cluster->ClusterStatus & ClusterStatus.EOC) != 0) break;
                nextCluster = cluster->Value;
            }

            return clusterChain.ToArray();
        }

        public FATEntry* GetCluster(long clusterOffset)
        {
            long recordIndex = clusterOffset / ClusterSize;
            if (recordIndex < 0 || recordIndex > FSBR->ClusterCount)
            {
                throw new IndexOutOfRangeException();
            }
            return FAT + recordIndex;
        }

        public void AddCluster()
        {

            if (ClusterCount == FATStream.Length / sizeof(FATEntry))
            {
                FATStream.SetLength(FATStream.Length + FSBR->GrowClusterCount * sizeof(FATEntry));
                FAT = (FATEntry*)MemoryMappedFile.CreateFromFile(
                   FATStream,
                   null,
                   FATStream.Length,
                   MemoryMappedFileAccess.ReadWrite,
                   HandleInheritability.None,
                   false
               ).CreateViewPointer();
            };
            if (ClusterCount == DATAStream.Length / ClusterSize)
            {
                DATAStream.SetLength(DATAStream.Length + FSBR->GrowClusterCount * ClusterSize);
                DATA = MemoryMappedFile.CreateFromFile(
                   DATAStream,
                   null,
                   DATAStream.Length,
                   MemoryMappedFileAccess.ReadWrite,
                   HandleInheritability.None,
                   false
               );
            };
            long newClusterOffset = FAT[ClusterCount - 1].ClusterOffset + ClusterSize;
            FAT[FSBR->ClusterCount] = new(newClusterOffset, ClusterStatus.Free | ClusterStatus.EOC, -1);
            FATEntry* lastFreeCluster = GetCluster(FSBR->LastFreeCluster);
            lastFreeCluster->ClusterOffset = newClusterOffset;
            lastFreeCluster->ClusterStatus ^= ClusterStatus.EOC;
            FSBR->LastFreeCluster = newClusterOffset;
            FSBR->ClusterCount++;


        }

        public Dictionary<IntPtr, MemorySequence> OpenedFile = new();

        public MemorySequence GetFileMemorySequence(FileDescriptor* fileDescriptor)
        {
            var memoryBlocks = GetClusterChain(fileDescriptor->FirstClusterIndex).Select(x => DATA.CreateViewAccessor(((FATEntry*)x)->ClusterOffset * ClusterSize, ClusterSize)).ToArray();
            MemorySequence memorySequence = new MemorySequence(memoryBlocks);
            OpenedFile[(IntPtr)fileDescriptor] = memorySequence;
            return memorySequence;

        }

        public void GrowFileCapacity(FileDescriptor* fileDescriptor)
        {
            IntPtr[] clusters = GetClusterChain(fileDescriptor->FirstClusterIndex);
            int currentClusterCount = clusters.Length;
            int targetClusterCount = (int)(fileDescriptor->Size / ClusterSize);
            int needToAddCount = currentClusterCount - targetClusterCount;
            FATEntry*[] newClusters = AddClusterToChain(fileDescriptor->FirstClusterIndex, needToAddCount);
            MemorySequence memorySequence = OpenedFile[(IntPtr)fileDescriptor];
            if (memorySequence != null)
            {
                foreach (FATEntry* cluster in newClusters)
                {
                    OpenedFile[(IntPtr)fileDescriptor]?.Add(DATA.CreateViewAccessor(cluster->ClusterOffset * ClusterSize, ClusterSize));

                }

            }

        }
        public void TrimFileCapacity(FileDescriptor* fileDescriptor)
        {
            IntPtr[] clusters = GetClusterChain(fileDescriptor->FirstClusterIndex);
            int currentClusterCount = clusters.Length;
            int targetClusterCount = (int)(fileDescriptor->Size / ClusterSize);
            int needToTrimCount = currentClusterCount - targetClusterCount;
            if (needToTrimCount > 0)
            {
                ((FATEntry*)clusters[targetClusterCount - 1])->ClusterStatus = ClusterStatus.EOC;
                FreeClusterChain(((FATEntry*)clusters[targetClusterCount])->ClusterOffset);
                OpenedFile[(IntPtr)fileDescriptor]?.Trim(targetClusterCount);
            }

        }
        public FileSystemEntity GetFSEntity(FileDescriptor* fileDescriptor)
        {
            if((fileDescriptor->Attribute & FileAttribute.IsDirectory) > 0)
            {
                return GetDirectory(fileDescriptor);
            }
            return GetFile(fileDescriptor);
        }
        public File GetFile(FileDescriptor* fileDescriptor)
        {
            return new File { FileDescriptor = fileDescriptor, FileSystemManager = this };
        }
        public Directory GetDirectory(FileDescriptor* fileDescriptor)
        {
            return new Directory { FileDescriptor = fileDescriptor, FileSystemManager = this };
        }
        public void DeleteFile(FileDescriptor* fileDescriptor)
        {
            throw new NotImplementedException();
        }

        public void SetFileSize(FileDescriptor* fileDescriptor, long size)
        {
            long oldSize = fileDescriptor->Size;
            long oldClusterCount = oldSize / ClusterSize;
            long newClusterCount = size / ClusterSize;
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
    public long ClusterCount;
    public long ClusterSize;
    public short FileNameSize;
    public long InitialClusterCount;
    public long GrowClusterCount;
    public long FirstFreeCluster;
    public long LastFreeCluster;
    public FileDescriptor RootDirectory;
}
