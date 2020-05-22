using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    /// <summary>
    /// Claim value type.
    /// </summary>
    public enum ClaimValueType : int
    {
        None = 0,
        Value = 1,
        String = 2,
        DateTime = 3,
    }
}
