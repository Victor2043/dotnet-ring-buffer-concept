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

    // Pointers represent absolute operation counters (Monotonic)
    private ulong _writeHead; // Total items written
    private ulong _readTail;  // Total items read

    public RingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

        // No internal +1 reservation needed anymore
        _capacity = capacity;
        _buffer = new T[_capacity];
        _writeHead = 0;
        _readTail = 0;
    }

    public int Capacity => _capacity;

    // Pending items count for reading
    public int Count => (int)(_writeHead - _readTail);

    public bool IsEmpty => _writeHead == _readTail;

    public bool IsFull => Count == _capacity;

    public bool TryWrite(T item)
    {
        if (IsFull)
            return false;

        // Physical index derived on-the-fly
        int index = (int)(_writeHead % (ulong)_capacity);
        _buffer[index] = item;

        _writeHead++; // Monotonic increment
        return true;
    }

    public bool TryRead(out T result)
    {
        if (IsEmpty)
        {
            result = default!;
            return false;
        }

        // Physical index derived on-the-fly
        int index = (int)(_readTail % (ulong)_capacity);
        result = _buffer[index];

        _buffer[index] = default!; // Clear reference
        _readTail++;               // Monotonic increment
        return true;
    }
}