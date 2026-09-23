using System;

namespace DotNetRingBufferConcept;

/// <summary>
/// Fifth evolution: Thread-safe Single-Producer Single-Consumer (SPSC) Lock-Free Ring Buffer.
/// Uses volatile memory barriers to guarantee acquire-release semantics without locks.
/// </summary>

public sealed class SpscRingBuffer<T>
{
    private readonly T[] _buffer;
    private readonly int _capacityMask;

    // Pointers accessed concurrently by separate threads
    private ulong _writeHead; // Written by Producer, read by Consumer
    private ulong _readTail;  // Written by Consumer, read by Producer

    public SpscRingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

        if ((capacity & (capacity - 1)) != 0)
            throw new ArgumentException("Capacity must be a power of two.", nameof(capacity));

        _buffer = new T[capacity];
        _capacityMask = capacity - 1;
        _writeHead = 0;
        _readTail = 0;
    }

    public int Capacity => _capacityMask + 1;

    // Volatile read guarantees reading fresh value from main memory/cache hierarchy
    public int Count
    {
        get
        {
            ulong head = Volatile.Read(ref _writeHead);
            ulong tail = Volatile.Read(ref _readTail);
            return (int)(head - tail);
        }
    }

    public bool IsEmpty => Volatile.Read(ref _writeHead) == Volatile.Read(ref _readTail);

    public bool IsFull => Count == Capacity;

    /// <summary>
    /// Called strictly by the PRODUCER thread.
    /// </summary>
    public bool TryWrite(T item)
    {
        ulong currentHead = _writeHead; // Local read (Producer owns _writeHead)
        ulong currentTail = Volatile.Read(ref _readTail); // Acquire semantics (Read tail updated by Consumer)

        if ((int)(currentHead - currentTail) == Capacity)
            return false; // Buffer Full

        int index = (int)(currentHead & (ulong)_capacityMask);
        _buffer[index] = item;

        // Release semantics: Guarantees array write is visible BEFORE head pointer update
        Volatile.Write(ref _writeHead, currentHead + 1);
        return true;
    }

    /// <summary>
    /// Called strictly by the CONSUMER thread.
    /// </summary>
    public bool TryRead(out T result)
    {
        ulong currentTail = _readTail; // Local read (Consumer owns _readTail)
        ulong currentHead = Volatile.Read(ref _writeHead); // Acquire semantics (Read head updated by Producer)

        if (currentHead == currentTail)
        {
            result = default!;
            return false; // Buffer Empty
        }

        int index = (int)(currentTail & (ulong)_capacityMask);
        result = _buffer[index];

        _buffer[index] = default!; // Clear reference

        // Release semantics: Guarantees item read is completed BEFORE tail pointer update
        Volatile.Write(ref _readTail, currentTail + 1);
        return true;
    }
}