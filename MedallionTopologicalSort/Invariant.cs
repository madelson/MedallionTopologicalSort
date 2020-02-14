using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Medallion.Collections
{
    internal static class Invariant
    {
        [Conditional("DEBUG")]
        public static void Require(bool condition, string? message = null)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Invariant violated" + (message != null ? ": " + message : string.Empty));
            }
        }
    }
}
