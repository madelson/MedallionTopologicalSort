using System;
using System.Collections.Generic;
using System.Text;

#if NET45
namespace System
{
    internal struct ValueTuple<T1>
    {
        public T1 Item1;

        public ValueTuple(T1 item1) { this.Item1 = item1; }
    }

    internal struct ValueTuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public ValueTuple(T1 item1, T2 item2) 
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Struct)]
    internal sealed class TupleElementNamesAttribute : Attribute
    {
        public TupleElementNamesAttribute(string[] transformNames) 
        {
            this.TransformNames = transformNames;
        }

        public IList<string> TransformNames { get; }
    }
}
#endif