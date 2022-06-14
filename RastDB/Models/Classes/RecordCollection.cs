using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RastDB.Exceptions;
using RastDB.Utils;
using static RastDB.Specification.FieldDescription;
using static RastDB.Specification.RecordDescription;

namespace RastDB.Models.Classes
{
    public class RecordCollection : IEnumerable<Record>
    {
        private readonly StreamAccessor _streamAccessor;
        private readonly TableHeader _tableHeader;
        private readonly List<FieldDescription> _fieldDescriptions;
        private List<Record> _records = new();

        public RecordCollection(StreamAccessor streamAccessor,TableHeader tableHeader, List<FieldDescription> fieldDescriptions)
        {
            _streamAccessor = streamAccessor;
            _tableHeader = tableHeader;
            _fieldDescriptions = fieldDescriptions;
            InitializeRecords();
        }

        private void InitializeRecords()
        {
            _records = new List<Record>();
            for (ushort i = _tableHeader.RecordsOffset; i < _tableHeader.FreeSpace; i += _tableHeader.RecordSize)
            {
                _records.Add(new Record(_streamAccessor,i,_fieldDescriptions));
            }
        }

        public IEnumerator<Record> GetEnumerator()
        {
            return _records.Where(x => !x.IsFree).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();

        }

        public void AddAll(List<Dictionary<string, string>> values)
        {
            foreach (var value in values)
            {
                Add(value);
            }
        }
        public void Add(Dictionary<string,string> values)
        {
            var freeSpace = _tableHeader.FirstFree;
            Record record = new Record(_streamAccessor,freeSpace,_fieldDescriptions);
            
            foreach (var description in _fieldDescriptions)
            {
                if (description.AutoIncrementValue == NO_AUTOINCREMENT)
                {
                    if (!values.TryGetValue(description.FieldName, out var value))
                    {
                        throw new ValueNotPresentedException();
                    };
                    record[description.FieldName] = value;
                    values.Remove(description.FieldName);
                }
                else
                {
                    record[description.FieldName] = description.NextAutoIncrement.ToString();
                    description.NextAutoIncrement += description.AutoIncrementValue;
                }
            }

            if (values.Count > 0)
            {
                throw new TooManyArgumentsException();
            }

            if (_tableHeader.FirstFree == _tableHeader.FreeSpace)
            {
                _tableHeader.FreeSpace += _tableHeader.RecordSize;
                _tableHeader.FirstFree = _tableHeader.FreeSpace;
                record.NextFree = NO_NEXT_FREE;
            }
            else
            {
                if (record.NextFree == NO_NEXT_FREE)
                {
                    throw new UnexpectedSituationException();
                }
                _tableHeader.FirstFree = record.NextFree;
            }

            record.IsFree = false;
            _records.Add(record);
            _tableHeader.RecordsCount++;
            _streamAccessor.Flush();
        }

        public void Flush()
        {
            _streamAccessor.Flush();
        }
        public Record Get(Predicate<Record> predicate)
        {
            Record record = _records.Find(predicate);
            if (record?.IsFree is false)
            {
                return record;
            }
            return null;
        }

        public void Clear()
        {
            _records.ForEach(x=>x.IsFree=true);
            _streamAccessor.Flush();
        }

        public bool Contains(Record item)
        {
            if (item?.IsFree is false)
            {
               return _records.Contains(item);
            }
            return false; 
        }
        public bool Contains(Predicate<Record> predicate)
        {
            Record record = _records.Find(predicate);
            if (record?.IsFree is false)
            {
                return true;
            }
            return false;
        }

        public bool Remove(Record item)
        {
            if (_records.Contains(item))
            {
                item.IsFree = true;
                item.NextFree = _tableHeader.FirstFree;
                _tableHeader.FirstFree = item.Offset;
                _tableHeader.RecordsCount--;
                _streamAccessor.Flush();
                return true;
            }
            return false;
        }
        public bool Remove(Predicate<Record> predicate)
        {
            return Remove(_records.Find(predicate));
        }

        public int Count => _tableHeader.RecordsCount;
    }
}
