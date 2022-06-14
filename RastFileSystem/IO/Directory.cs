using RastFileSystem.Core;
using RastFileSystem.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem.IO
{
    public class Directory : FileSystemEntity
    {
        private IEnumerable<IntPtr> DeletedEntries => MemorySequence.GetPointerEnumerator<FileDescriptor>().Where(IsDeleted);
        public unsafe IEnumerable<FileSystemEntity> EnumerateAll => MemorySequence.GetPointerEnumerator<FileDescriptor>().Where(x => !IsDeleted(x)).Select(x => FileSystemManager.GetFSEntity((FileDescriptor*)x));
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
        private (string currentDir, string nextDirPath) RecursiveSubfolder(string path)
        {
            int separateIndex = path.IndexOf('/');
            if (separateIndex > 0)
            {
                return (path.Substring(0, separateIndex), path.Substring(separateIndex + 1));
            }
            return (path, null);
        }
        private (string FileName, string FileExtension) SeparateFileExtension(string fileName)
        {
            int fileExtensionSeparatorIndex = fileName.LastIndexOf(".");
            if (fileExtensionSeparatorIndex > 0)
            {
                return (fileName.Substring(0, fileExtensionSeparatorIndex), fileName.Substring(fileExtensionSeparatorIndex + 1));
            }
            return (fileName, string.Empty);

        }
        public unsafe Directory GetDirectory(string directoryPath, bool createIfNotExist = false)
        {
            (string currentDir, string nextDirPath) = RecursiveSubfolder(directoryPath);
            Directory dir = EnumerateDirectories.FirstOrDefault(x => x.Name == currentDir);
            if (dir == null && createIfNotExist)
            {
                FileSystemManager.CreateDirectory((FileDescriptor*)FindSpace(), currentDir);
                dir = EnumerateDirectories.First(x => x.Name == currentDir);
            }
            else if (dir == null && !createIfNotExist)
            {
                throw new DirectoryNotFoundException();
            }
            if (nextDirPath != null)
            {
                return dir.GetDirectory(nextDirPath);
            }
            return dir;
        }


        public unsafe bool TryGetDirectory(string directoryPath, out Directory directory)
        {
            try
            {
                directory = GetDirectory(directoryPath);
                return true;
            }
            catch (Exception)
            {
                directory = null;
                return false;
            }

        }
        public unsafe bool TryGetFile(string filePath, out File file)
        {
            try
            {
                file = GetFile(filePath);
                return true;
            }
            catch (Exception e)
            {
                file = null;
                return false;
            }

        }


        public unsafe File GetFile(string filePath, bool createIfNotExist = false)
        {

            (string currentDir, string nextDirPath) = RecursiveSubfolder(filePath);

            if (nextDirPath != null)
            {
                Directory dir = EnumerateDirectories.FirstOrDefault(x => x.Name == currentDir);
                if (dir == null && createIfNotExist)
                {
                    FileSystemManager.CreateDirectory((FileDescriptor*)FindSpace(), currentDir);
                    dir = EnumerateDirectories.First(x => x.Name == currentDir);
                }
                else if (dir == null && !createIfNotExist)
                {
                    throw new DirectoryNotFoundException();
                }
                return dir.GetFile(nextDirPath);
            }
            else
            {
                (string fileName, string fileExtension) = SeparateFileExtension(currentDir);
                File file = EnumerateFiles.FirstOrDefault(x => x.Name == fileName && x.Extension == fileExtension);
                if (file == null && createIfNotExist)
                {
                    FileSystemManager.CreateFile((FileDescriptor*)FindSpace(), fileName, fileExtension);
                    file = EnumerateFiles.First(x => x.Name == currentDir);
                }
                else if (file == null && !createIfNotExist)
                {
                    throw new FileNotFoundException();
                }
                return file;
            }
        }


        public unsafe FileSystemEntity Get(string path)
        {
            (string currentDir, string nextDirPath) = RecursiveSubfolder(path);

            if (nextDirPath != null)
            {
                Directory dir = EnumerateDirectories.FirstOrDefault(x => x.Name == currentDir);

                if (dir == null)
                {
                    throw new DirectoryNotFoundException(currentDir);
                }
                return dir.Get(nextDirPath);
            }
            else
            {
                (string fileName, string fileExtension) = SeparateFileExtension(currentDir);
                FileSystemEntity file = EnumerateAll.FirstOrDefault(x => x.Name == fileName && x.Extension == fileExtension);

                if (file == null)
                {
                    throw new FileNotFoundException(currentDir);
                }
                return file;
            }
        }
        private unsafe bool IsDeleted(IntPtr fileDescriptor)
        {
            return FileSystemManager.IsDeleted((FileDescriptor*)fileDescriptor);
        }
        private unsafe void TrimDirectorySize()
        {

            bool needToTrim = MemorySequence.GetPointerEnumerator<FileDescriptor>().TakeLast((int)FileSystemManager.ClusterSize / Unsafe.SizeOf<FileDescriptor>()).All(IsDeleted);
            if (needToTrim)
            {
                FileSystemManager.TrimDirectoryCapacity(FileDescriptor);
            }
        }
        private unsafe IntPtr FindSpace()
        {
            IntPtr freeSpace = DeletedEntries.FirstOrDefault();
            if (freeSpace == default)
            {
                FileSystemManager.GrowDirectoryCapacity(FileDescriptor);
                freeSpace = DeletedEntries.FirstOrDefault();
            }
            return freeSpace;
        }
        public void DeleteEntry(string entryName)
        {
            Get(entryName).Delete();
        }
        public void DeleteEntryFile(string filePath)
        {
            GetFile(filePath).Delete();
        }
        public void DeleteEntryDirectory(string directoryPath)
        {
            GetDirectory(directoryPath).Delete();
        }
        public unsafe Directory CreateDirectory(string directoryPath)
        {
            (string currentDir, string nextDirPath) = RecursiveSubfolder(directoryPath);
            Directory dir;
            TryGetDirectory(currentDir, out dir);
            if (dir != null && nextDirPath == null)
            {
                throw new DirectoryExistException(currentDir);
            }
            if (dir == null)
            {
                FileSystemManager.CreateDirectory((FileDescriptor*)FindSpace(), currentDir);
                dir = GetDirectory(currentDir);
            }
            if (nextDirPath != null)
            {
                return dir.CreateDirectory(nextDirPath);
            }
            return dir;
        }
        public unsafe File CreateFile(string filePath)
        {
            (string currentDir, string nextDirPath) = RecursiveSubfolder(filePath);

            if (nextDirPath != null)
            {
                Directory dir;
                TryGetDirectory(currentDir, out dir);
                if (dir == null)
                {
                    FileSystemManager.CreateDirectory((FileDescriptor*)FindSpace(), currentDir);
                    dir = GetDirectory(currentDir);
                }
                return dir.CreateFile(nextDirPath);
            }
            else
            {
                File file;
                TryGetFile(currentDir, out file);
                if (file == null)
                {
                    (string fileName, string fileExtension) = SeparateFileExtension(currentDir);
                    FileSystemManager.CreateFile((FileDescriptor*)FindSpace(), fileName, fileExtension);
                    return GetFile(currentDir);
                }
                else
                {
                    throw new FileExistException(currentDir);
                }

            }
        }

        public unsafe override void Delete()
        {
            foreach (var entry in EnumerateAll)
            {
                entry.Delete();
            }
            FileSystemManager.Delete(FileDescriptor);
        }
        public override string ToString()
        {
            return $"Directory name: {Name} Directory contents:\n{EnumerateAll.Aggregate($"\t", (x, y) => x + y.ToString() + "\n")}";
        }
    }
}
