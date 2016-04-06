using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Support
{
    public class Collection<T>: System.Collections.ObjectModel.Collection<T>
    {
        public Collection(List<T> items): base(items)
        {
        }

        public static Collection<T> make(List<T> items){
            return new Collection<T>(items);
        }
    }
}
