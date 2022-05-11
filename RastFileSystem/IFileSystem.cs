using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem
{
    public interface IFileSystem
    {
        public abstract static IFileSystem CreateFromFile(string FSpath);

        public abstract static IFileSystem OpenFromFile(string FSpath);

        public Stream Open(string path);
    
        public Stream Create(string path);
        
        public void Delete(string path);

        public void Move (string oldPath, string newPath);

        public void Copy(string oldPath, string newPath);


    }
}
