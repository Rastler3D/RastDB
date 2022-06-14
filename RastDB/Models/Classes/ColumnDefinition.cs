using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastDB.Models.Classes
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public TypeDefinition DataType { get; set; }
        public (int Seed, int Increment) Identity { get; set; } = (-1, -1);
    }
}
