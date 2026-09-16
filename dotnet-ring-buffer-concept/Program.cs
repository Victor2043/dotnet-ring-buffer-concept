using System;

namespace DotNetRingBufferConcept;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Ring Buffer Concept - Commit 01: Basic Prototype ===");

        // Buffer Size 4
        var ringBuffer = new RingBuffer<int>(capacity: 4);

        Console.WriteLine($"Total Capacity: {ringBuffer.Capacity}");
        Console.WriteLine($"Empty on startup? {ringBuffer.IsEmpty}\n");

        // 1. Fill the buffer up to its maximum usable capacity (3 items)
        Console.WriteLine("--- Writing 3 items ---");
        ringBuffer.TryWrite(10);
        ringBuffer.TryWrite(20);
        ringBuffer.TryWrite(30);

        Console.WriteLine($"Item count: {ringBuffer.Count}");
        Console.WriteLine($"Buffer Full? {ringBuffer.IsFull}\n");

        // Attempt to write to a full buffer
        bool wrote = ringBuffer.TryWrite(40);
        Console.WriteLine($"Successfully wrote the 4th item (with state reservation)? {wrote}\n");

        // 2. Consume items to free up space
        Console.WriteLine("--- Reading 2 items ---");
        if (ringBuffer.TryRead(out int item1))
            Console.WriteLine($"Read item: {item1}");

        if (ringBuffer.TryRead(out int item2))
            Console.WriteLine($"Read item: {item2}");

        Console.WriteLine($"Remaining item count: {ringBuffer.Count}");
        Console.WriteLine($"Buffer Empty? {ringBuffer.IsEmpty}\n");

        // 3. Demonstrate Wrap-Around (circular advancement)
        Console.WriteLine("--- Writing 2 more items (Forcing Wrap-Around) ---");
        ringBuffer.TryWrite(40);
        ringBuffer.TryWrite(50);

        Console.WriteLine($"Item count: {ringBuffer.Count}");
        Console.WriteLine($"Buffer Full? {ringBuffer.IsFull}\n");

        // 4. Consume all remaining items
        Console.WriteLine("--- Reading all remaining items ---");
        while (ringBuffer.TryRead(out int item))
        {
            Console.WriteLine($"Read item: {item}");
        }

        Console.WriteLine($"\nBuffer Empty at the end? {ringBuffer.IsEmpty}");
    }
}


public sealed class RingBuffer<T>
{
    private readonly T[] _buffer;
    private readonly int _capacity;
    private int _head; // Write pointer
    private int _tail; // Read pointer

    public RingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

        // Reserve +1 internal slot to distinguish the "Full" state from the "Empty" state
        _capacity = capacity + 1;
        _buffer = new T[_capacity];
        _head = 0;
        _tail = 0;
    }

    public int Capacity => _capacity - 1;

    public bool IsEmpty => _head == _tail;

    public bool IsFull => (_head + 1) % _capacity == _tail;

    public int Count
    {
        get
        {
            if (_head >= _tail)
                return _head - _tail;

            return _capacity - (_tail - _head);
        }
    }

    /// <summary>
    /// Attempts to write an element to the buffer.
    /// Returns false if the buffer is full.
    /// </summary>
    public bool TryWrite(T item)
    {
        if (IsFull)
            return false;

        _buffer[_head] = item;
        _head = (_head + 1) % _capacity; // Circular advancement using modulo
        return true;
    }

    /// <summary>
    /// Attempts to read and remove an element from the buffer.
    /// Returns false if the buffer is empty.
    /// </summary>
    public bool TryRead(out T result)
    {
        if (IsEmpty)
        {
            result = default!;
            return false;
        }

        result = _buffer[_tail];
        _buffer[_tail] = default!; // Clear the reference to prevent memory leaks for reference types
        _tail = (_tail + 1) % _capacity; // Circular advancement using modulo
        return true;
    }
}