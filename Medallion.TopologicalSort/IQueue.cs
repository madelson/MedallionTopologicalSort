using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal interface IQueue
    {
        int Count { get; }
        void Enqueue(int value);
        int Dequeue();
    }

    internal sealed class FifoQueue : Queue<int>, IQueue { }
}
