using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database.Model
{
    public class MassAssignmentException: Exception
    {
        public MassAssignmentException(string key): base(key) { }
    }
}
