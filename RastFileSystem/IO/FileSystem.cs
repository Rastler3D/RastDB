using RastFileSystem.Core;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem.IO
{
    public class FileSystem : IDisposable
    {
        public enum FileSystemMode
        {
            Open,
            Create,
            OpenOrCreate

        }
        private FileSystemManager _manager;

        private FileSystem() { }



        public Directory RootDirectory => _manager.RootDirectory;
        public FileSystem(string FSpath, FileSystemMode mode)
        {
            _manager = mode switch
            {
                FileSystemMode.Open => FileSystemManager.OpenFileSystem(FSpath),
                FileSystemMode.Create => FileSystemManager.CreateFileSystem(FSpath),
                FileSystemMode.OpenOrCreate => FileSystemManager.OpenOrCreateFileSystem(FSpath),
                _ => throw new NotImplementedException(),
            };
        }

        public static FileSystem CreateFromFile(string FSpath)
        {
            FileSystem fs = new()
            {
                _manager = FileSystemManager.CreateFileSystem(FSpath)
            };
            return fs;

        }
        public static FileSystem OpenOrCreateFromFile(string FSpath)
        {
            FileSystem fs = new()
            {
                _manager = FileSystemManager.OpenOrCreateFileSystem(FSpath)
            };
            return fs;

        }

        public static FileSystem OpenFromFile(string FSpath)
        {
            FileSystem fs = new()
            {
                _manager = FileSystemManager.OpenFileSystem(FSpath)
            };
            return fs;
        }

        public unsafe void Copy(string oldPath, string newPath)
        {
            FileSystemEntity oldFile = GetFileOrDirectory(oldPath);
            FileSystemEntity newFile = GetFileOrDirectory(newPath);
            _manager.Copy(oldFile.FileDescriptor, newFile.FileDescriptor);
        }

        public File CreateFile(string path)
        {
            if (path.StartsWith("root:/"))
            {
                return RootDirectory.CreateFile(path.Substring(path.IndexOf('/') + 1));
            }
            throw new IncorrectPathException();
        }
        public Directory CreateDirectory(string path)
        {
            if (path.StartsWith("root:/"))
            {
                return RootDirectory.CreateDirectory(path.Substring(path.IndexOf('/') + 1));
            }
            throw new IncorrectPathException();
        }


        public void Delete(string path)
        {
            if (path.StartsWith("root:/"))
            {
                RootDirectory.DeleteEntry(path.Substring(path.IndexOf('/') + 1));
            }
            throw new IncorrectPathException();
        }

        public unsafe void Move(string oldPath, string newPath)
        {
            FileSystemEntity oldFile = GetFileOrDirectory(oldPath);
            FileSystemEntity newFile = GetFileOrDirectory(newPath);
            _manager.Copy(oldFile.FileDescriptor, newFile.FileDescriptor);
            oldFile.Delete();
        }

        public Stream OpenFile(string path)
        {
            return GetFile(path).Open();
        }

        public FileSystemEntity GetFileOrDirectory(string path)
        {
            if (path.StartsWith("root:/"))
            {
                return RootDirectory.Get(path.Substring(path.IndexOf('/') + 1));
            }
            throw new IncorrectPathException();
        }
        public File GetFile(string path, bool createIfNotExist = false)
        {
            if (path.StartsWith("root:/"))
            {
                return RootDirectory.GetFile(path.Substring(path.IndexOf('/') + 1), createIfNotExist);
            }
            throw new IncorrectPathException();
        }
        public Directory GetDirectory(string path, bool createIfNotExist = false)
        {
            if (path.StartsWith("root:/"))
            {
                return RootDirectory.GetDirectory(path.Substring(path.IndexOf('/') + 1), createIfNotExist);
            }
            throw new IncorrectPathException();
        }
        public void Dispose()
        {
            _manager?.Dispose();
        }
    }



}
