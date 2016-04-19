using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Icy.Util
{
    public class ReflectionUtil
    {
        public static Type getTraitImplementation(Type t)
        {
            var attribute = System.Attribute.GetCustomAttribute(t, typeof(TraitAttribute)) as TraitAttribute;
            if (attribute == null) return null;
            return attribute.TraitType;
        }

        public static Type[] getTraits(Type t)
        {
            var traits = new Type[0];
            foreach (var type in t.GetInterfaces())
            {
                var type1 = getTraitImplementation(type);
                if (type1 != null)
                {
                    traits = ArrayUtil.push(traits, type1);
                }
            }
            return traits;
        }

        public static MethodInfo getMethod(Type t, string name)
        {
            var m = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (m != null) return m;

            foreach(var trait in getTraits(t))
            {
                m = getMethod(trait, name);
                if (m != null) return m;
            }

            return null;
        }

        public static Type[] getInterfaces(Type t)
        {
            return t.GetInterfaces();
        }

        public static bool existsMethod(Type t, string name)
        {
            if (getMethod(t, name) != null) return true;          

            return false;
        }

        public static object callMethod(object inst, string name, params object[] parameters)
        {
            var t = inst.GetType();
            var m = getMethod(t, name);

            return m.Invoke(inst, parameters);
        }

    }
}
