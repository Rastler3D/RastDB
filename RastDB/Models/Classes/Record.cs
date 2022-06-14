using System.Collections;
using RastDB.Extensions;
using RastDB.Utils;
using static RastDB.Specification.RecordDescription;

namespace RastDB;

public record Record : IEnumerable<String> {
    private readonly StreamAccessor _streamAccessor;
    private readonly ushort _recordOffset;
    private readonly List<FieldDescription> _fieldDescriptions;


   

    internal ushort Offset => _recordOffset;
    public bool IsFree 
    { 
        get=>_streamAccessor.ReadBoolean(_recordOffset + RECORD_FREE_FLAG_OFFSET);
        set=>_streamAccessor.Write(_recordOffset + RECORD_FREE_FLAG_OFFSET,value);
    }
    public ushort NextFree
    {
        get => _streamAccessor.ReadUInt16(_recordOffset + RECORD_NEXT_FREE_OFFSET);
        set => _streamAccessor.Write(_recordOffset + RECORD_NEXT_FREE_OFFSET, value);
    }

    public Record(StreamAccessor streamAccessor, ushort recordOffset, List<FieldDescription> fieldDescriptions)
    {
        _streamAccessor = streamAccessor;
        _recordOffset = recordOffset;
        _fieldDescriptions = fieldDescriptions;
    }

    public string this[string fieldName]
    {
        get
        {
            FieldDescription description = _fieldDescriptions.Find(x => x.FieldName == fieldName);
            return _streamAccessor.ReadDbField(_recordOffset + RECORD_VALUES_OFFSET, description);
        }
        set
        {
            FieldDescription description = _fieldDescriptions.Find(x => x.FieldName == fieldName);
            _streamAccessor.WriteDbField(_recordOffset + RECORD_VALUES_OFFSET, description,value);
        }
    }
    public IEnumerator<string> GetEnumerator()
    {
        foreach (var field in _fieldDescriptions)
        {
            yield return _streamAccessor.ReadDbField(_recordOffset + RECORD_VALUES_OFFSET, field);
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public virtual bool Equals(Record other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(_streamAccessor, other._streamAccessor) && _recordOffset == other._recordOffset && Equals(_fieldDescriptions, other._fieldDescriptions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_streamAccessor, _recordOffset, _fieldDescriptions);
    }

    
}


