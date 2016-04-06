using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database.Model
{
    // adf2ffd  on 19 Dec 2015
    public interface Scope<T> where T: Model<T>, new()
    {
        void apply(Builder<T> builder, Model<T> model);
        void extend(Builder<T> builder);
    }
}
