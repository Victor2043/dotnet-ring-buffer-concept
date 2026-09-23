namespace DotNetRingBufferConcept;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Ring Buffer Concept  ===");

        // Buffer Size 4
        // The caller owns the memory backing array (or memory slice)

        // Span<int> memoryBacking = stackalloc int[4]; // Allocated on Stack, 0 Heap pressure
        // var ringBuffer = new RingBuffer<int>(memoryBacking);

        var ringBuffer = new SpscRingBuffer<int>(4);

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
