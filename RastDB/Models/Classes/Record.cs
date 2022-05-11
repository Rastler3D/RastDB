namespace RastDB;

public record Record
{
    private char[][] _values;
    public bool IsFree { get; set; }
    public int NextFree { get; set; }
    public string[] Value 
    { 
        get=>;
        set;
    }
}


