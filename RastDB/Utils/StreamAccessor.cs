using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastDB.Utils
{
    public class StreamAccessor
    {
        public Stream BaseStream => _stream;
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;
        private readonly Stream _stream;


        public StreamAccessor(Stream stream)
        {
            _stream = stream;
            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);
        }


        public long Seek(int offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public int Read(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.Read();
        }

        public int Read(int position,byte[] buffer, int index, int count)
        {
            _reader.BaseStream.Position=position;
			return _reader.Read(buffer, index, count);
        }

        public int Read(int position,char[] buffer, int index, int count)
        {
            _reader.BaseStream.Position=position;
			return _reader.Read(buffer, index, count);
        }

        public int Read(int position,Span<byte> buffer)
        {
            _reader.BaseStream.Position=position;
			return _reader.Read(buffer);
        }

        public int Read(int position,Span<char> buffer)
        {
            _reader.BaseStream.Position=position;
			return _reader.Read(buffer);
        }

        public int Read7BitEncodedInt(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.Read7BitEncodedInt();
        }

        public long Read7BitEncodedInt64(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.Read7BitEncodedInt64();
        }

        public bool ReadBoolean(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadBoolean();
        }

        public byte ReadByte(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadByte();
        }

        public byte[] ReadBytes(int position,int count)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadBytes(count);
        }

        public char ReadChar(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadChar();
        }

        public char[] ReadChars(int position,int count)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadChars(count);
        }

        public decimal ReadDecimal(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadDecimal();
        }

        public double ReadDouble(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadDouble();
        }

        public Half ReadHalf(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadHalf();
        }

        public short ReadInt16(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadInt16();
        }

        public int ReadInt32(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadInt32();
        }

        public long ReadInt64(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadInt64();
        }

        public sbyte ReadSByte(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadSByte();
        }

        public float ReadSingle(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadSingle();
        }

        public string ReadString(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadString();
        }
        public string ReadString(int position,int length)
        {
            _reader.BaseStream.Position = position;
            byte[] arr = _reader.ReadBytes(length);
            return Encoding.UTF8.GetString(arr).Trim('\0');
        }

        public ushort ReadUInt16(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadUInt16();
        }

        public uint ReadUInt32(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadUInt32();
        }

        public ulong ReadUInt64(int position)
        {
            _reader.BaseStream.Position=position;
			return _reader.ReadUInt64();
        }

        public void Write(int position,bool value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,byte value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,byte[] buffer)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(buffer);
        }

        public void Write(int position,byte[] buffer, int index, int count)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(buffer, index, count);
        }

        public void Write(int position,char ch)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(ch);
        }

        public void Write(int position,char[] chars)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(chars);
        }

        public void Write(int position,char[] chars, int index, int count)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(chars, index, count);
        }

        public void Write(int position,decimal value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,double value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,Half value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,short value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,int value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,long value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,ReadOnlySpan<byte> buffer)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(buffer);
        }

        public void Write(int position,ReadOnlySpan<char> chars)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(chars);
        }

        public void Write(int position,sbyte value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,float value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,string value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position, string value, int length)
        {
            _writer.BaseStream.Position = position;
            byte[] arr = Encoding.UTF8.GetBytes(value, 0, length > value.Length ? value.Length : length);
            _writer.Write(arr);
        }

        public void Write(int position,ushort value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,uint value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write(int position,ulong value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write(value);
        }

        public void Write7BitEncodedInt(int position,int value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write7BitEncodedInt(value);
        }

        public void Write7BitEncodedInt64(int position,long value)
        {
          _writer.BaseStream.Position=position;
		  _writer.Write7BitEncodedInt64(value);
        }

        public void Fill(int position, int value)
        {
           Fill(position,value, _stream.Length - position);
        }

        public void Fill(int position, int value, long count)
        {
            byte[] data = new byte[count];
            Array.Fill(data, (byte)value);
            _writer.Write(data);
        }


        public int PeekChar()
        {
            return _reader.PeekChar();
        }

        public void Close()
        {
            _stream.Close();
        }

        public void Dispose()
        {
            _reader.Dispose();
            _writer.Dispose();
        }
        public ValueTask DisposeAsync()
        {
            return _writer.DisposeAsync();
        }

        public void Flush()
        {
            _writer.Flush();
        }



    }
}
