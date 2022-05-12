using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RastFileSystem
{
    public class MemorySequence : IEnumerable<byte>
    {
        public bool IsGrowable => Growing.GetInvocationList().Any();
        public event EventHandler<MemorySequence> Growing;

        private List<MemoryMappedViewAccessor> _memoryBlocks;

        public long Length { get; set; } = 0;

        public MemorySequence(params MemoryMappedViewAccessor[] memoryBlocks)
        {
            _memoryBlocks = memoryBlocks.ToList();
            Length = _memoryBlocks.Sum(x=>x.Capacity);
        }

        public byte this[long index]
        {
            set => SetValue(index, value);

            get => GetValue(index);
        }

        private byte GetValue(long index, int block = 0)
        {
            if (block >= _memoryBlocks.Count)
            {
                throw new IndexOutOfRangeException();
            }

            long nextIndex = index - _memoryBlocks[block].Capacity;

            if (nextIndex < 0)
            {
                return _memoryBlocks[block].ReadByte(index);
            }

            return GetValue(nextIndex, block++);
        }

        private void SetValue(long index, byte value, int block = 0)
        {
            if (block >= _memoryBlocks.Count)
            {
                throw new IndexOutOfRangeException();
            }

            long nextIndex = index - _memoryBlocks[block].Capacity;

            if (nextIndex < 0)
            {
                _memoryBlocks[block].Write(index, value);
            }

            SetValue(nextIndex, value, block++);
        }

        public IntPtr GetPointer(long index)
        {
            return GetPointer(index, 0);
        }

        private IntPtr GetPointer(long index,int block)
        {
            if (block >= _memoryBlocks.Count)
            {
                throw new IndexOutOfRangeException();
            }

            long nextIndex = index - _memoryBlocks[block].Capacity;

            if (nextIndex < 0)
            {
                unsafe
                {
                    byte* pointer = (byte*)IntPtr.Zero;
                    _memoryBlocks[block].SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
                    pointer += index;
                    return (IntPtr)pointer;
                }
            }

            return GetPointer(nextIndex, block++);
        }
        public void Add(MemoryMappedViewAccessor memoryBlock)
        {
            _memoryBlocks.Add(memoryBlock);
        }
        public void Grow()
        {
            Growing(this);
        }
        public void Trim(int targetCount)
        {
            foreach (var memoryBlock in _memoryBlocks.Skip(targetCount))
            {
                memoryBlock.Dispose();
            }
            _memoryBlocks = _memoryBlocks.Take(targetCount).ToList();
        }
        public IEnumerable<IntPtr> GetPointerEnumerator<T>()
        {
            int typeSize = Unsafe.SizeOf<T>();
            for (int i = 0; i < Length; i+=typeSize)
            {
                yield return GetPointer(i);
            }

            yield break;
        }
        public IEnumerator<byte> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return this[i];
            }

            yield break;
        }
        public void Flush()
        {
            _memoryBlocks.ForEach(x => x.Flush());
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
