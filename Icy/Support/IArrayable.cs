using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Support
{
    public interface IArrayable
    {
        T[] toArray<T>();
    }
}
