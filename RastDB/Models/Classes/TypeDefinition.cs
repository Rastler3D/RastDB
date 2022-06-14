using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastDB.Models.Classes
{
    public class TypeDefinition
    {
        internal DataType Type { get; set; }
        internal List<byte> Arguments = new(10);
        internal int Length { get; set; }
        public static TypeDefinition INT()
        {
            return new TypeDefinition()
            {
                Type = DataType.INT,
                Length = sizeof(int)
            };
        }
        public static TypeDefinition NVARCHAR(byte n)
        {
            return new TypeDefinition()
            {
                Type = DataType.NVARCHAR,
                Length = sizeof(byte) * n,
                Arguments = { n }

            };
        }
        public static TypeDefinition DECIMAL(byte precision, byte scale)
        {
            return new TypeDefinition()
            {
                Type = DataType.DECIMAL,
                Length = sizeof(byte) * precision + 1,
                Arguments = { precision,scale }

            };
        }
    }
}
