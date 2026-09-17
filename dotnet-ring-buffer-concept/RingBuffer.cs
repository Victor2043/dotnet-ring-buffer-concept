public ref struct RingBuffer<T>
{
    private readonly Span<T> _buffer;
    private readonly int _capacityMask; // Bitwise mask: capacity - 1

    private ulong _writeHead;
    private ulong _readTail;

    public RingBuffer(Span<T> buffer)
    {
        if (buffer.IsEmpty)
            throw new ArgumentException("Buffer memory span cannot be empty.", nameof(buffer));

        // Capacity MUST be a power of two
        if ((buffer.Length & (buffer.Length - 1)) != 0)
            throw new ArgumentException("Capacity must be a power of two (e.g., 2, 4, 8, 16, 32).", nameof(buffer));

        _buffer = buffer;
        _capacityMask = buffer.Length - 1; // Precomputed mask (e.g., 8 - 1 = 7 -> 0000 0111)
        _writeHead = 0;
        _readTail = 0;
    }

    public readonly int Capacity => _capacityMask + 1;

    public readonly int Count => (int)(_writeHead - _readTail);

    public readonly bool IsEmpty => _writeHead == _readTail;

    public readonly bool IsFull => Count == Capacity;

    public bool TryWrite(T item)
    {
        if (IsFull)
            return false;

        // Bitwise AND (&) instead of Modulo (%)
        int index = (int)(_writeHead & (ulong)_capacityMask);
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

        // Bitwise AND (&) instead of Modulo (%)
        int index = (int)(_readTail & (ulong)_capacityMask);
        result = _buffer[index];

        _buffer[index] = default!; // Clear reference
        _readTail++;
        return true;
    }
}