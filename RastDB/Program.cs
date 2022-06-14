using System.Data.SqlTypes;
using RastDB;
using RastDB.Extensions;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using RastDB.Models.Classes;

Database database = InitilizeDatabase();
PrintTable(database.DataTables[0]);




void PrintTable(Table table)
{
    foreach (var field in table.FieldDescriptions)
    {
        Console.Write("{0,-25}", field.FieldName);
    }
    foreach (var record in table.Records)
    {
        Console.WriteLine("\n\n");
        foreach (var value in record)
        {
            Console.Write("{0,-25}", value);
        }
    }
}


Database InitilizeDatabase()
{
    Database database = Database.CreateDatabase("database.dbf","MyDataBase");
    Table productTable = database.AddTable("Product", new[]
    {
        new ColumnDefinition
        {
            Name = "Id",
            DataType = TypeDefinition.INT(),
            Identity = (0,1)
        },
        new ColumnDefinition
        {
            Name = "Name",
            DataType = TypeDefinition.NVARCHAR(30)
        },
        new ColumnDefinition
        {
            Name = "Price",
            DataType = TypeDefinition.DECIMAL(10,2)
        }
    });
    productTable.Records.Add(new()
    {
        { "Name", "Chicken" },
        { "Price", "100.5000" }
    });
    productTable.Records.Add(new()
    {
        { "Name", "Phone" },
        { "Price", "1312.32100" }
    });
    productTable.Records.Add(new()
    {
        { "Name", "Computer" },
        { "Price", "51210313131231909.5000" }
    });

    return database;
}
