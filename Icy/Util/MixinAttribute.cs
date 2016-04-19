using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icy.Util
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class TraitAttribute : Attribute
    {
        public Type TraitType { get; set; }

        public TraitAttribute(Type type)
        {
            TraitType = type;
        }
    }
}
