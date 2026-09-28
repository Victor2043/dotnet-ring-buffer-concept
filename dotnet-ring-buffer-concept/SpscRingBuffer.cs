using System;
using System.Runtime.InteropServices;

namespace DotNetRingBufferConcept;

/// <summary>
/// Sixth evolution: Cache-line padded SPSC Lock-Free Ring Buffer.
/// Eliminates False Sharing between producer and consumer threads.
/// </summary>
public sealed class SpscRingBuffer<T>
{
    private readonly T[] _buffer;
    private readonly int _capacityMask;

    // Cache line 1: Modified exclusively by Producer
    private PaddedHead _head;

    // Cache line 2: Modified exclusively by Consumer
    private PaddedTail _tail;

    public SpscRingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

        if ((capacity & (capacity - 1)) != 0)
            throw new ArgumentException("Capacity must be a power of two.", nameof(capacity));

        _buffer = new T[capacity];
        _capacityMask = capacity - 1;
        _head = default;
        _tail = default;
    }

    public int Capacity => _capacityMask + 1;

    public int Count
    {
        get
        {
            ulong head = Volatile.Read(ref _head.Value);
            ulong tail = Volatile.Read(ref _tail.Value);
            return (int)(head - tail);
        }
    }

    public bool IsEmpty => Volatile.Read(ref _head.Value) == Volatile.Read(ref _tail.Value);

    public bool IsFull => Count == Capacity;

    public bool TryWrite(T item)
    {
        ulong currentHead = _head.Value; // Producer local read
        ulong currentTail = Volatile.Read(ref _tail.Value); // Consumer tail acquire

        if ((int)(currentHead - currentTail) == Capacity)
            return false;

        int index = (int)(currentHead & (ulong)_capacityMask);
        _buffer[index] = item;

        // Release write strictly on Producer's cache line
        Volatile.Write(ref _head.Value, currentHead + 1);
        return true;
    }

    public bool TryRead(out T result)
    {
        ulong currentTail = _tail.Value; // Consumer local read
        ulong currentHead = Volatile.Read(ref _head.Value); // Producer head acquire

        if (currentHead == currentTail)
        {
            result = default!;
            return false;
        }

        int index = (int)(currentTail & (ulong)_capacityMask);
        result = _buffer[index];

        _buffer[index] = default!;

        // Release write strictly on Consumer's cache line
        Volatile.Write(ref _tail.Value, currentTail + 1);
        return true;
    }

    // Explicit layout guaranteeing 64-byte alignment and padding per struct
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    private struct PaddedHead
    {
        [FieldOffset(0)]
        public ulong Value;
    }

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    private struct PaddedTail
    {
        [FieldOffset(0)]
        public ulong Value;
    }
}