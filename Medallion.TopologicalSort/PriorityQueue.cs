using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections
{
    /// <summary>
    /// Minimal PQ implementation used for stable sorting and 
    /// <see cref="IOrderedEnumerable{TElement}.CreateOrderedEnumerable{TKey}(Func{TElement, TKey}, IComparer{TKey}, bool)"/>.
    /// 
    /// Based on MedallionPriorityQueue implementation
    /// </summary>
    internal class PriorityQueue : IQueue
    {
        private readonly Func<int, int, bool> _less;
        private readonly int _maxCapacity;
        private int[] _heap = new int[10];

        public PriorityQueue(Func<int, int, bool> less, int maxCapacity)
        {
            this._less = less;
            this._maxCapacity = maxCapacity;
        }

        public int Count { get; private set; }

        public void Enqueue(int item)
        {
            if (this._heap.Length == this.Count)
            {
                this.Expand();
            }

            var lastIndex = this.Count++;
            this.Swim(lastIndex, item);
        }

        public int Dequeue()
        {
            Invariant.Require(this.Count > 0, "dequeue from empty pq");

            var result = this._heap[0];
            if (--this.Count > 0)
            {
                var last = this._heap[this.Count];
                this.Sink(0, last);
            }

            return result;
        }

        private void Expand()
        {
            var capacity = this._heap.Length;
            var remainingCapacity = this._maxCapacity - capacity;
            Invariant.Require(remainingCapacity > 0, "pq over capacity");
            var newCapacity = remainingCapacity <= capacity ? capacity + remainingCapacity : 2 * capacity;
            Array.Resize(ref this._heap, newCapacity);
        }

        /// <summary>
        /// Performs the heap "swim" operation starting at <paramref name="index"/>. <paramref name="item"/>
        /// will be placed in its determined final position
        /// </summary>
        private void Swim(int index, int item)
        {
            var i = index;
            while (i > 0)
            {
                // find the parent index/item
                var parentIndex = (i - 1) >> 1;
                var parentItem = this._heap[parentIndex];

                // if the parent is <= item, we're done
                if (!this._less(item, parentItem))
                {
                    break;
                }

                // shift the parent down and traverse our pointer up
                this._heap[i] = parentItem;
                i = parentIndex;
            }

            // finally, leave item at the final position
            this._heap[i] = item;
        }

        /// <summary>
        /// Performs the heap "sink" operation starting at <paramref name="index"/>. <paramref name="item"/>
        /// will be placed in its determined final position
        /// </summary>
        private void Sink(int index, int item)
        {
            var half = this.Count >> 1;
            var i = index;
            while (i < half)
            {
                // pick the lesser of the left and right children
                var childIndex = (i << 1) + 1;
                var childItem = this._heap[childIndex];
                var rightChildIndex = childIndex + 1;
                if (rightChildIndex < this.Count && this._less(this._heap[rightChildIndex], childItem))
                {
                    childItem = this._heap[childIndex = rightChildIndex];
                }

                // if item is <= either child, stop
                if (!this._less(childItem, item))
                {
                    break;
                }

                // move smaller child up and move our pointer down the heap
                this._heap[i] = childItem;
                i = childIndex;
            }

            // finally, put the initial item in the final position
            this._heap[i] = item;
        }
    }
}
