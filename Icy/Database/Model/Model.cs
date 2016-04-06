using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database.Model
{
    public class Model<T> where T: Model<T>
    {
    }
}
