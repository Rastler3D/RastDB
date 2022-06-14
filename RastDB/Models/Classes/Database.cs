using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RastDB.Exceptions;
using RastDB.Extensions;
using RastDB.Utils;
using Directory = RastFileSystem.IO.Directory;
using File = RastFileSystem.IO.File;
using static RastDB.Specification.Database;
using RastFileSystem.IO;

namespace RastDB.Models.Classes
{
    internal class Database : IDisposable
    {
        public Table HeaderTable { get; set; }
        public Table PointerTable { get; set; }
        public List<Table> DataTables { get; set; } 

        

        private FileSystem _fileSystem { get; set; }

        private Database()
        {
            DataTables = new();
        }

        public static Database OpenDatabase(string dbFilePath)
        {
            Database database = new()
            {
                _fileSystem = FileSystem.OpenFromFile(dbFilePath)
            };
           // try
           // {
            
                File headerFile = database._fileSystem.RootDirectory.GetFile(HEADER_PATH);
                Console.WriteLine(headerFile.Size);
                database.HeaderTable = Table.OpenTable(headerFile.Open().ToAccessor());
                Record pointerTableRecord = database.HeaderTable.Records.Get(x => x["key"] == "HeaderPath");
                File pointerTableFile = database._fileSystem.RootDirectory.GetFile(pointerTableRecord["value"]);
                database.PointerTable = Table.OpenTable(pointerTableFile.Open().ToAccessor());

                foreach (Record record in database.PointerTable.Records)
                {
                    File tableFile = database._fileSystem.RootDirectory.GetFile(record["filePath"]);
                    Table table = Table.OpenTable(tableFile.Open().ToAccessor());
                    database.DataTables.Add(table);
                }

                return database;
            //}
          
        }
        public static Database CreateDatabase(string dbFilePath,string dbName)
        {
            Database database = new()
            {
                _fileSystem = FileSystem.CreateFromFile(dbFilePath)
            };
            try
            {
                Directory systemDirectory = database._fileSystem.RootDirectory.CreateDirectory("SystemTables");
                File headerFile = systemDirectory.CreateFile("HeaderTable.dbh");
                database.HeaderTable = InitializeHeader(headerFile.Open().ToAccessor());
                File pointerTableFile = systemDirectory.CreateFile("PointerTable.dbp");
                database.PointerTable = InitializeTablePointer(pointerTableFile.Open().ToAccessor());
                database.HeaderTable.Records.Add(new()
                {
                    {"key", "HeaderPath"},
                    {"value", "SystemTables/PointerTable.dbp"}
                });
                database.HeaderTable.Records.Add(new()
                {
                    {"key", "TableName"},
                    {"value", dbName}
                });
                database.PointerTable.Records.Flush();
                database.HeaderTable.Records.Flush();
                return database;
            }
            catch (FileSystemException e)
            {
                database.Dispose();
                throw new DatabaseAlreadyExistsException();
            }
           
        }

        private static Table InitializeTablePointer(StreamAccessor streamAccessor)
        {
            Table pointerTable = Table.CreateTable(streamAccessor, "header", new[]
            {
                new ColumnDefinition
                {
                    Name = "table",
                    DataType = TypeDefinition.NVARCHAR(20)
                },
                new ColumnDefinition
                {
                    Name = "filePath",
                    DataType = TypeDefinition.NVARCHAR(50)
                }
            });

            return pointerTable;
        }

        private static Table InitializeHeader(StreamAccessor streamAccessor)
        {
            Table header = Table.CreateTable(streamAccessor, "header", new[]
            {
                new ColumnDefinition
                {
                    Name = "key",
                    DataType = TypeDefinition.NVARCHAR(20)
                },
                new ColumnDefinition
                {
                    Name = "value",
                    DataType = TypeDefinition.NVARCHAR(50)
                }
            });

            header.Records.Add(new()
            {
                { "key", "CreationDate" },
                { "value", DateTime.UtcNow.ToString() }
            });

            return header;
        }

        public Table AddTable(string tableName, IEnumerable<ColumnDefinition> columnDefinition)
        {
            File tableFile = _fileSystem.RootDirectory.CreateFile($"Tables/{tableName}.dbt");

            if (PointerTable.Records.Contains(x => x["table"] == tableName))
            {
                throw new TableAlreadyExistsException();
            }

            Table table = Table.CreateTable(tableFile.Open().ToAccessor(),tableName,columnDefinition);
            PointerTable.Records.Add(new()
            {
                {"table", "tableName"},
                {"filePath", $"Tables/{tableName}.dbt"}
            });
            DataTables.Add(table);
            return table;
        }

        public void DropTable(string tableName)
        {

            DataTables.RemoveAll(x => x.TableHeader.TableName == tableName);
            Record filePathRecord = PointerTable.Records.Get(x => x["table"] == tableName);
            PointerTable.Records.Remove(filePathRecord);
            PointerTable.Records.Flush();
            _fileSystem.Delete(filePathRecord["filePath"]);
        }
        public Table GetTable(string tableName)
        {
            return DataTables.Find(x => x.TableHeader.TableName == tableName);
        }

        public void Dispose()
        {
            _fileSystem?.Dispose();
        }
        
    }

}
