using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem.Exceptions
{
    public class FileSystemNotExistException : Exception
    {
        public FileSystemNotExistException()
        {
        }

        public FileSystemNotExistException(string message) : base(message)
        {
        }
    }
    public class FileSystemException : Exception
    {
        public FileSystemException() { }
        public FileSystemException(string message) : base(message) { }
    }
    public class IncorrectPathException : FileSystemException { }
    public class DirectoryExistException : FileSystemException
    {
        public DirectoryExistException(string message) : base(message) { }
    }
    public class FileExistException : FileSystemException
    {
        public FileExistException(string message) : base(message) { }
    }

    public class FileNotFoundException : FileSystemException
    {
        public FileNotFoundException()
        {
        }

        public FileNotFoundException(string message) : base(message)
        {
        }
    }

    public class DirectoryNotFoundException : FileSystemException
    {
        public DirectoryNotFoundException()
        {
        }

        public DirectoryNotFoundException(string message) : base(message)
        {
        }
    }
}
