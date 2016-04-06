using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
#if (!ICY_NET20)
using System.ServiceModel.Web;
#endif

namespace Icy.Util
{
    public static class ContextResolver
    {
#if (!ICY_NET20)
        public static Dictionary<Type, ConditionalWeakTable<WebOperationContext, object>> _storage = new Dictionary<Type, ConditionalWeakTable<WebOperationContext, object>>();
#endif
        public static Dictionary<Type, object> _defaults = new Dictionary<Type, object>();

        public static T resolve<T>() where T: class, new()
        {
            object ret = default(T);

#if (!ICY_NET20)
            if (WebOperationContext.Current != null)
            {
                if (!_storage.ContainsKey(typeof(T)))
                {
                    _storage[typeof(T)] = new ConditionalWeakTable<WebOperationContext, object>();
                }

                _storage[typeof(T)].TryGetValue(WebOperationContext.Current, out ret);
                if (ret == null)
                {
                    ret = new T();
                    _storage[typeof(T)].Add(WebOperationContext.Current, ret);
                }

                return ret as T;
            }
#endif
                if (!_defaults.ContainsKey(typeof(T)))
                {
                    _defaults[typeof(T)] = new T();
                }

                return _defaults[typeof(T)] as T;
        }
    }
}
