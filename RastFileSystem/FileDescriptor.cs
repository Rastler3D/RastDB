using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
[Flags]
public enum FileAttribute : byte
{
	EmptyEntry = 0,
	ReadAndWrite = 1,
	ReadOnly = 2,
	Hidden = 4,
	System = 8,
	IsDirectory = 16,
	IsFile = 32,
	IsDeleted = 64
}
namespace RastFileSystem
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct FileDescriptor
	{
		public fixed byte Name[28];

		public fixed byte Extension[3];

		public FileAttribute Attribute;

		public DateTime CreateDateTime;

		public DateTime ModifyDateTime;

		public Int64 FirstClusterIndex;

		public Int64 Size;


	}
}
