using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database.Schema.Grammars
{
    public class Grammar
    {
        public virtual string compileTableExists() { throw new NotImplementedException(); }
        public virtual string compileColumnExists() { throw new NotImplementedException(); }
        public virtual string compileColumnType() { throw new NotImplementedException(); }
    }
}
