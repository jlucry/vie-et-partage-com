using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    /// <summary>
    /// Post state.
    /// </summary>
    public enum State : int
    {
        NotValided = 0,
        Valided = 1,
        Trashed = 2
    }
}
