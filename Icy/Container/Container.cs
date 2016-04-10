using System;
using System.Collections.Generic;
using System.Text;
using Icy.Util;

namespace Icy.Container
{

    public struct ContainerBindingsOptions
    {
        public Func<Container, object[], object> concrete;
        public bool shared;
    }

    public class Container : IContainer
    {

        /**
         * The current globally available container (if any).
         *
         * @var static
         */
        protected static IContainer _instance;
        /**
         * An array of the types that have been resolved.
         *
         * @var array
         */
        protected Dictionary<Type, bool> _resolved = new Dictionary<Type, bool>();
        /**
         * The container's bindings.
         *
         * @var array
         */
        protected Dictionary<Type, ContainerBindingsOptions> _bindings = new Dictionary<Type, ContainerBindingsOptions>();
        /**
         * The container's shared instances.
         *
         * @var array
         */
        protected Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        /**
         * The registered type aliases.
         *
         * @var array
         */
        protected Dictionary<Type, Type> _aliases = new Dictionary<Type, Type>();
        /**
         * The extension closures for services.
         *
         * @var array
         */
        protected Dictionary<Type, Func<object, Container, object>[]> _extenders = new Dictionary<Type, Func<object, Container, object>[]>();
        /**
         * All of the registered tags.
         *
         * @var array
         */
        protected Dictionary<Type, Type[]> _tags = new Dictionary<Type, Type[]>();
        /**
         * The stack of concretions currently being built.
         *
         * @var array
         */
        protected Type[] _buildStack = new Type[0];
        /**
         * The contextual binding map.
         *
         * @var array
         */
        // TODO: Support contextual bindings
        //public $contextual = [];
        /**
         * All of the registered rebound callbacks.
         *
         * @var array
         */
        protected Dictionary<Type, Action<Container, object>[]> _reboundCallbacks = new Dictionary<Type, Action<Container, object>[]>();
        /**
         * All of the global resolving callbacks.
         *
         * @var array
         */
        protected Action<object, Container>[] _globalResolvingCallbacks = new Action<object, Container>[0];
        /**
         * All of the global after resolving callbacks.
         *
         * @var array
         */
        protected Action<object, Container>[] _globalAfterResolvingCallbacks = new Action<object, Container>[0];
        /**
         * All of the resolving callbacks by class type.
         *
         * @var array
         */
        protected Dictionary<Type, Action<object, Container>[]> _resolvingCallbacks = new Dictionary<Type, Action<object, Container>[]>();
        /**
         * All of the after resolving callbacks by class type.
         *
         * @var array
         */
        protected Dictionary<Type, Action<object, Container>[]> _afterResolvingCallbacks = new Dictionary<Type, Action<object, Container>[]>();

        /**
 * Define a contextual binding.
 *
 * @param  string  $concrete
 * @return \Illuminate\Contracts\Container\ContextualBindingBuilder
 * TODO
public function when($concrete)
{
    $concrete = $this->normalize($concrete);
    return new ContextualBindingBuilder($this, $concrete);
}
/**
 * Determine if the given abstract type has been bound.
 *
 * @param  string  $abstract
 * @return bool
 */
        public bool bound(Type t)
        {
            return this._bindings.ContainsKey(t) || this._instances.ContainsKey(t) || this.isAlias(t);
        }
        public bool bound<TAbstract>()
        {
            return this.bound(typeof(TAbstract));
        }
        /**
         * Determine if the given abstract type has been resolved.
         *
         * @param  string  $abstract
         * @return bool
         */
        public bool resolved<TAbstract>()
        {
            return this.resolved(typeof(TAbstract));
        }
        public bool resolved(Type t)
        {
            if (this.isAlias(t))
            {
                t = this.getAlias(t);
            }
            return this._resolved.ContainsKey(t) || this._instances.ContainsKey(t);
        }
        /**
         * Determine if a given string is an alias.
         *
         * @param  string  $name
         * @return bool
         */
        public bool isAlias<TAbstract>()
        {
            return this.isAlias(typeof(TAbstract));
        }
        public bool isAlias(Type t)
        {
            return this._aliases.ContainsKey(t);
        }


        /**
         * Get the alias for an abstract if available.
         *
         * @param  string  $abstract
         * @return string
         */
        protected Type getAlias(Type t)
        {
            return this._aliases.ContainsKey(t) ? this._aliases[t] : t;
        }
        protected Type getAlias<TAbstract>()
        {
            return this.getAlias(typeof(TAbstract));
        }

        /**
         * Register a binding with the container.
         *
         * @param  string|array  $abstract
         * @param  \Closure|string|null  $concrete
         * @param  bool  $shared
         * @return void
         */

        public void bind<TAbstract, TConcrete>(Func<Container, object[], object> concrete = null, bool shared = false) where TConcrete : TAbstract
        {
            this.bind(typeof(TAbstract), concrete == null ? this.getClosure(typeof(TAbstract), typeof(TConcrete)) : concrete, shared);
        }

        public void bind(Type abstracts, Func<Container, object[], object> concrete = null, bool shared = false)
        {
            // If no concrete type was given, we will simply set the concrete type to the
            // abstract type. This will allow concrete type to be registered as shared
            // without being forced to state their classes in both of the parameter.
            this.dropStaleInstances(abstracts);
            // If the factory is not a Closure, it means it is just a class name which is
            // bound into this container to the abstract type and we will just wrap it
            // up inside its own Closure to give us more convenience when extending.
            //if (!(concrete is Func<Container, object[], object>)) {
            //    concrete = this.getClosure(abstracts, concrete);
            //}
            this._bindings[abstracts] = new ContainerBindingsOptions() { concrete = concrete, shared = shared };
            // If the abstract type was already resolved in this container we'll fire the
            // rebound listener so that any objects which have already gotten resolved
            // can have their copy of the object updated via the listener callbacks.
            if (this.resolved(abstracts))
            {
                this.rebound(abstracts);
            }
        }


        /**
         * Get the Closure to be used when building a type.
         *
         * @param  string  $abstract
         * @param  string  $concrete
         * @return \Closure
         */

        protected Func<Container, object[], object> getClosure(Type abstracts, Type concrete)
        {
            return (c, parameters) =>
            {
                if (abstracts == concrete)
                {
                    return this.build(concrete, parameters);
                }
                else {
                    return this.make(concrete, parameters);
                }
            };
        }

        /**
     * Register a binding if it hasn't already been registered.
     *
     * @param  string  $abstract
     * @param  \Closure|string|null  $concrete
     * @param  bool  $shared
     * @return void
     */
        public void bindIf<TAbstract, TConcrete>(Func<Container, object[], object> concrete = null, bool shared = false) where TConcrete : TAbstract
        {
            this.bindIf(typeof(TAbstract), concrete, shared);
        }

        public void bindIf(Type abstracts, Func<Container, object[], object> concrete = null, bool shared = false)
        {
            if (!this.bound(abstracts))
            {
                this.bind(abstracts, concrete, shared);
            }
        }
        /**
         * Register a shared binding in the container.
         *
         * @param  string|array  $abstract
         * @param  \Closure|string|null  $concrete
         * @return void
         */
        public void singleton<TAbstract, TConcrete>(Func<Container, object[], object> concrete = null) where TConcrete : TAbstract
        {
            this.bind(typeof(TAbstract), concrete, true);
        }


        /**
         * "Extend" an abstract type in the container.
         *
         * @param  string    $abstract
         * @param  \Closure  $closure
         * @return void
         *
         * @throws \InvalidArgumentException
         */
        public void extend<TAbstract>(Func<object, Container, object> closure)
        {
            Type t = typeof(TAbstract);
            if (this._instances.ContainsKey(t))
            {
                this._instances[t] = closure(this._instances[t], this);
                this.rebound(t);
            }
            else {
                if (!this._extenders.ContainsKey(t))
                {
                    this._extenders[t] = new Func<object, Container, object>[0];
                }
                this._extenders[t] = ArrayUtil.push(this._extenders[t], closure);
            }
        }


        /**
         * Register an existing instance as shared in the container.
         *
         * @param  string  $abstract
         * @param  mixed   $instance
         * @return void
         */
        public void instance<TAbstract>(TAbstract instance)
        {
            this.instance(typeof(TAbstract), instance);
        }

        public void instance(Type t, object instance)
        {
            // First, we will extract the alias from the abstract if it is an array so we
            // are using the correct name when binding the type. If we get an alias it
            // will be registered with the container so we can resolve it out later.
            if (this._aliases.ContainsKey(t)) this._aliases.Remove(t);
            // We'll check to determine if this type has been bound before, and if it has
            // we will fire the rebound callbacks registered with the container and it
            // can be updated with consuming classes that have gotten resolved here.
            bool bound = this.bound(t);
            this._instances[t] = instance;
            if (bound)
            {
                this.rebound(t);
            }
        }


        /**
         * Alias a type to a different name.
         *
         * @param  string  $abstract
         * @param  string  $alias
         * @return void
         */
        public void alias<TAbstract, TAlias>()
        {
            this.alias(typeof(TAbstract), typeof(TAlias));
        }
        public void alias(Type abstracts, Type alias)
        {
            this._aliases[alias] = abstracts;
        }
        /**
         * Extract the type and alias from a given definition.
         *
         * @param  array  $definition
         * @return array
         *
         * TODO: Deprecated
        protected Type[] extractAlias(array $definition)
        {
            return [key($definition), current($definition)];
        }
        /**
         * Bind a new callback to an abstract's rebind event.
         *
         * @param  string    $abstract
         * @param  \Closure  $callback
         * @return mixed
         */
        public object rebinding(Type t, Action<Container, object> callback)
        {
            if (!this._reboundCallbacks.ContainsKey(t)) this._reboundCallbacks[t] = new Action<Container, object>[0];
            this._reboundCallbacks[t] = ArrayUtil.push(this._reboundCallbacks[t], callback);
            if (this.bound(t))
            {
                return this.make(t);
            }
            return null;
        }
        /**
         * Refresh an instance on the given target and method.
         *
         * @param  string  $abstract
         * @param  mixed   $target
         * @param  string  $method
         * @return mixed
         */
        public object refresh(Type abstracts, object target, string method)
        {
            return this.rebinding(abstracts, (app, instance) =>
            {
                //$target->{$method}($instance);
                throw new NotImplementedException();
            });
        }

        /**
 * Fire the "rebound" callbacks for the given abstract type.
 *
 * @param  string  $abstract
 * @return void
 */
        protected void rebound(Type abstracts)
        {
            object instance = this.make(abstracts);
            foreach (var callback in this.getReboundCallbacks(abstracts))
            {
                callback(this, instance);
            }
        }
        /**
         * Get the rebound callbacks for a given type.
         *
         * @param  string  $abstract
         * @return array
         */
        protected Action<Container, object>[] getReboundCallbacks(Type abstracts)
        {
            if (this._reboundCallbacks.ContainsKey(abstracts))
            {
                return this._reboundCallbacks[abstracts];
            }
            return new Action<Container, object>[0];
        }

        /**
     * Resolve the given type from the container.
     *
     * @param  string  $abstract
     * @param  array   $parameters
     * @return mixed
     */
        public TAbstract make<TAbstract>(object[] parameters = null)
        {
            return (TAbstract)this.make(typeof(TAbstract), parameters);
        }

        public object make(object t, object[] parameters = null)
        {
            parameters = parameters ?? new object[0];
            object inst = null;
            Type type = null;
            if (t is Type) type = (Type)t;

            // If an instance of the type is currently being managed as a singleton we'll
            // just return an existing instance instead of instantiating new instances
            // so the developer can keep using the same objects instance every time.
            if (this._instances.ContainsKey(type))
            {
                return this._instances[type];
            }
            object concrete = this.getConcrete(t);
            // We're ready to instantiate an instance of the concrete type registered for
            // the binding. This will instantiate the types, as well as resolve any of
            // its "nested" dependencies recursively until all have gotten resolved.
            if (this.isBuildable(concrete, t))
            {
                inst = this.build(concrete, parameters);
            }
            else {
                inst = this.make(concrete, parameters);
            }
            // If we defined any extenders for this type, we'll need to spin through them
            // and apply them to the object being built. This allows for the extension
            // of services, such as changing configuration or decorating the object.
            foreach (var extender in this.getExtenders(type))
            {
                inst = extender(inst, this);
            }
            // If the requested type is registered as a singleton we'll want to cache off
            // the instances in "memory" so we can return it later without creating an
            // entirely new instance of an object on each subsequent request for it.
            if (this.isShared(type))
            {
                this._instances[type] = inst;
            }
            this.fireResolvingCallbacks(type, inst);
            this._resolved[type] = true;
            return inst;
        }


        /**
         * Get the concrete type for a given abstract.
         *
         * @param  string  $abstract
         * @return mixed   $concrete
         */
        protected object getConcrete(object abstracts)
        {
            //if (! is_null($concrete = $this->getContextualConcrete($abstract))) {
            //    return $concrete;
            //}
            // If we don't have a registered resolver or concrete for the type, we'll just
            // assume each type is a concrete name and will attempt to resolve it as is
            // since the container should be able to resolve concretes automatically.
            if (abstracts is Type && this._bindings.ContainsKey((Type)abstracts))
            {
                return this._bindings[(Type)abstracts].concrete;
            }

            return abstracts;
        }



        /**
         * Determine if the given concrete is buildable.
         *
         * @param  mixed   $concrete
         * @param  string  $abstract
         * @return bool
         */
        protected bool isBuildable(object concrete, object abstracts)
        {
            return concrete == abstracts || concrete is Func<Container, object[], object>;
        }

        /**
 * Instantiate a concrete instance of the given type.
 *
 * @param  string  $concrete
 * @param  array   $parameters
 * @return mixed
 *
 * @throws \Illuminate\Contracts\Container\BindingResolutionException
 */
        public object build(object concrete, object[] parameters = null)
        {
            parameters = parameters ?? new object[0];
            // If the concrete type is actually a Closure, we will just execute it and
            // hand back the results of the functions, which allows functions to be
            // used as resolvers for more fine-tuned resolution of these objects.
            if (concrete is Func<Container, object[], object>)
            {
                return ((Func<Container, object[], object>)concrete)(this, parameters);
            }

            Type type = (Type)concrete;

            if (type.IsAbstract)
            {
                throw new Exception(string.Format("Target {0} is not instantiable."));
            }

            // TODO: Dependency injection?
            return Activator.CreateInstance(type, parameters);
        }


        /**
         * Determine if a given type is shared.
         *
         * @param  string  $abstract
         * @return bool
         */
        public bool isShared(Type abstracts)
        {
            if (this._instances.ContainsKey(abstracts))
            {
                return true;
            }
            if (!this._bindings.ContainsKey(abstracts))
            {
                return false;
            }
            return this._bindings[abstracts].shared == true;
        }


        /**
         * Get the extender callbacks for a given type.
         *
         * @param  string  $abstract
         * @return array
         */
        protected Func<object, Container, object>[] getExtenders(Type abstracts)
        {
            if (this._extenders.ContainsKey(abstracts))
            {
                return this._extenders[abstracts];
            }
            return new Func<object, Container, object>[0];
        }


        /**
         * Drop all of the stale instances and aliases.
         *
         * @param  string  $abstract
         * @return void
         */
        protected void dropStaleInstances(Type abstracts)
        {
            this._instances.Remove(abstracts);
            this._aliases.Remove(abstracts);
        }


        /**
         * Fire all of the resolving callbacks.
         *
         * @param  string  $abstract
         * @param  mixed   $object
         * @return void
         */
        protected void fireResolvingCallbacks(Type abstracts, object inst)
        {
            this.fireCallbackArray(inst, this._globalResolvingCallbacks);

            this.fireCallbackArray(
                inst, this.getCallbacksForType(
                    abstracts, inst, this._resolvingCallbacks
                )
            );
            this.fireCallbackArray(inst, this._globalAfterResolvingCallbacks);
            this.fireCallbackArray(
                inst, this.getCallbacksForType(
                    abstracts, inst, this._afterResolvingCallbacks
                )
            );
        }
        /**
         * Get all callbacks for a given type.
         *
         * @param  string  $abstract
         * @param  object  $object
         * @param  array   $callbacksPerType
         *
         * @return array
         */
        protected Action<object, Container>[] getCallbacksForType(Type abstracts, object inst, Dictionary<Type, Action<object, Container>[]> callbacksPerType)
        {
            Action<object, Container>[] results = new Action<object, Container>[0];
            foreach (var e in callbacksPerType)
            {
                Type type = e.Key;
                Action<object, Container>[] callbacks = e.Value;

                if (type == abstracts || type.IsAssignableFrom(inst.GetType()))
                {
                    results = ArrayUtil.concat(results, callbacks);
                }
            }
            return results;
        }
        /**
         * Fire an array of callbacks with an object.
         *
         * @param  mixed  $object
         * @param  array  $callbacks
         * @return void
         */
        protected void fireCallbackArray(object inst, Action<object, Container>[] callbacks)
        {
            foreach (var callback in callbacks)
            {
                callback(inst, this);
            }
        }

    }
}
