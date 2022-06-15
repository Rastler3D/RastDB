namespace RastFileSystem.IO;

public class RfsStream : Stream
{
    private readonly File _file;

    private long _position;

    public RfsStream(File file)
    {
        _file = file;
    }

    public long RemainsToEnd => Length - Position;


    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => _file.Size;

    public override long Position
    {
        get => _position;
        set
        {
            if (value >= 0 && value <= Length)
            {
                _position = value;
            }
            else
            {
                SetLength(value);
                _position = value;
            }
        }
    }

    public override void Flush()
    {
        _file.MemorySequence.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int readedBytes;
        for (readedBytes = 0; readedBytes < count; readedBytes++)
        {
            if (RemainsToEnd == 0) break;
            buffer[offset + readedBytes] = _file.MemorySequence[Position++];
        }

        return readedBytes;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        Position = origin switch
        {
            SeekOrigin.Begin => 0,
            SeekOrigin.Current => Position,
            SeekOrigin.End => Length - 1,
            _ => throw new NotImplementedException()
        } + offset;

        return Position;
    }

    public override void SetLength(long value)
    {
        _file.Size = value;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (RemainsToEnd < count) SetLength(Length + (count - RemainsToEnd));
        for (var i = 0; i < count; i++) _file.MemorySequence[Position++] = buffer[offset + i];
    }
}