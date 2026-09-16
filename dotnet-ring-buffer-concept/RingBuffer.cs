namespace dotnet_ring_buffer_concept
{
    public ref struct RingBuffer<T>
    {
        private readonly Span<T> _buffer;
        private readonly int _capacity;

        private ulong _writeHead;
        private ulong _readTail;

        public RingBuffer(Span<T> buffer)
        {
            if (buffer.IsEmpty)
                throw new ArgumentException("Buffer memory span cannot be empty.", nameof(buffer));

            _buffer = buffer;
            _capacity = buffer.Length;
            _writeHead = 0;
            _readTail = 0;
        }

        public readonly int Capacity => _capacity;

        public readonly int Count => (int)(_writeHead - _readTail);

        public readonly bool IsEmpty => _writeHead == _readTail;

        public readonly bool IsFull => Count == _capacity;

        public bool TryWrite(T item)
        {
            if (IsFull)
                return false;

            int index = (int)(_writeHead % (ulong)_capacity);
            _buffer[index] = item;

            _writeHead++;
            return true;
        }

        public bool TryRead(out T result)
        {
            if (IsEmpty)
            {
                result = default!;
                return false;
            }

            int index = (int)(_readTail % (ulong)_capacity);
            result = _buffer[index];

            _buffer[index] = default!; // Clear reference
            _readTail++;
            return true;
        }
    }
}