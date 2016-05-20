using System;


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
