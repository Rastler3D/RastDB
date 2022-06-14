using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Text;
using static RastFileSystem.Extensions.Extensions;
using RastFileSystem.IO;

FileSystem fs = FileSystem.OpenOrCreateFromFile("Rast.fs");
RastFileSystem.IO.File file = fs.GetFile("root:/File1.txt", true);
Stream str = file.Open();
StreamWriter sw = new(str);
StreamReader sr = new(str);
StringBuilder builder = new();
for (int i = 0; i< 1024; i++)
{
    builder.Append(i + "|");
}
sw.WriteLine(builder.ToString());
sw.Flush();
sr.BaseStream.Position = 0;
Console.WriteLine(sr.ReadToEnd());
sr.Close();
str.Close();



    







