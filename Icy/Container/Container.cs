using Icy.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Icy.Container
{
    public struct ContainerBindingsOptions<T> where T : Container<T>
    {
        public Func<T, Dictionary<string, object>, object> concrete;
        public bool shared;
    }

    public class Container<TContainer> where TContainer : Container<TContainer>
    {
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
        protected Dictionary<Type, ContainerBindingsOptions<TContainer>> _bindings = new Dictionary<Type, ContainerBindingsOptions<TContainer>>();

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
         * The stack of concretions currently being built.
         *
         * @var array
         */
        protected Type[] _buildStack = new Type[0];

        /**
         * The extension closures for services.
         *
         * @var array
         */
        protected Dictionary<Type, Func<object, TContainer, object>[]> _extenders = new Dictionary<Type, Func<object, TContainer, object>[]>();

        /**
         * All of the registered rebound callbacks.
         *
         * @var array
         */
        protected Dictionary<Type, Action<TContainer, object>[]> _reboundCallbacks = new Dictionary<Type, Action<TContainer, object>[]>();
        /**
         * All of the global resolving callbacks.
         *
         * @var array
         */
        protected Action<object, TContainer>[] _globalResolvingCallbacks = new Action<object, TContainer>[0];
        /**
         * All of the global after resolving callbacks.
         *
         * @var array
         */
        protected Action<object, TContainer>[] _globalAfterResolvingCallbacks = new Action<object, TContainer>[0];
        /**
         * All of the resolving callbacks by class type.
         *
         * @var array
         */
        protected Dictionary<Type, Action<object, TContainer>[]> _resolvingCallbacks = new Dictionary<Type, Action<object, TContainer>[]>();
        /**
         * All of the after resolving callbacks by class type.
         *
         * @var array
         */
        protected Dictionary<Type, Action<object, TContainer>[]> _afterResolvingCallbacks = new Dictionary<Type, Action<object, TContainer>[]>();


        /**
 * Determine if the given abstract type has been bound.
 *
 * @param  string  $abstract
 * @return bool
 */
        public bool bound(Type abstracts)
        {
            return this._bindings.ContainsKey(abstracts) || this._instances.ContainsKey(abstracts) || this.isAlias(abstracts);
        }

        /**
         * Determine if the given abstract type has been resolved.
         *
         * @param  string  $abstract
         * @return bool
         */
        public bool resolved(Type abstracts)
        {
            if (this.isAlias(abstracts))
            {
                abstracts = this.getAlias(abstracts);
            }
            return this._resolved.ContainsKey(abstracts) || this._instances.ContainsKey(abstracts);
        }

        /**
         * Determine if a given string is an alias.
         *
         * @param  string  $name
         * @return bool
         */
        public bool isAlias(Type type)
        {
            return this._aliases.ContainsKey(type);
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

        /**
         * Drop all of the stale instances and aliases.
         *
         * @param  string  $abstract
         * @return void
         */
        protected void dropStaleInstances(Type abstracts)
        {
            if (this._instances.ContainsKey(abstracts)) this._instances.Remove(abstracts);
            if (this._aliases.ContainsKey(abstracts)) this._aliases.Remove(abstracts);
        }

        /**
        * Register a binding with the container.
        *
        * @param  string|array  $abstract
        * @param  \Closure|string|null  $concrete
        * @param  bool  $shared
        * @return void
*/


        public void bind<TAbstract>(object concrete = null, bool shared = false)
        {
            this.bind(typeof(TAbstract), concrete, shared);
        }
        public void bind<TAbstract>(Func<TContainer, Dictionary<string, object>, object> concrete = null, bool shared = false)
        {
            this.bind(typeof(TAbstract), concrete, shared);
        }

        public void bind(Type abstracts, object concrete = null, bool shared = false)
        {
            if (concrete == null)
            {
                concrete = abstracts;
            }
            // If the factory is not a Closure, it means it is just a class name which is
            // bound into this container to the abstract type and we will just wrap it
            // up inside its own Closure to give us more convenience when extending.

            this.bind(abstracts, this.getClosure(abstracts, (Type)concrete), shared);
        }
        public void bind(Type abstracts, Func<TContainer, Dictionary<string, object>, object> concrete = null, bool shared = false)
        {

            // If the given types are actually an array, we will assume an alias is being
            // defined and will grab this "real" abstract class name and register this
            // alias with the container so that it can be used as a shortcut for it.
            //if (is_array($abstract)) {
            //    list($abstract, $alias) = $this->extractAlias($abstract);
            //    $this->alias($abstract, $alias);
            //}
            // If no concrete type was given, we will simply set the concrete type to the
            // abstract type. This will allow concrete type to be registered as shared
            // without being forced to state their classes in both of the parameter.
            this.dropStaleInstances(abstracts);

            if (concrete == null)
            {
                concrete = this.getClosure(abstracts, (Type)abstracts);
            }

            this._bindings[abstracts] = new ContainerBindingsOptions<TContainer>() { concrete = concrete, shared = shared };
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
        protected Func<TContainer, Dictionary<string, object>, object> getClosure(Type abstracts, Type concrete)
        {
            return (container, parameters) =>
            {
                if (abstracts == concrete)
                {
                    return this.build(concrete, parameters);
                }
                else
                {
                    return this.make(concrete, parameters);
                }
            };
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
        public object build(object concrete, Dictionary<string, object> parameters = null)
        {
            parameters = parameters ?? new Dictionary<string, object>();
            // If the concrete type is actually a Closure, we will just execute it and
            // hand back the results of the functions, which allows functions to be
            // used as resolvers for more fine-tuned resolution of these objects.
            if (concrete is Func<TContainer, Dictionary<string, object>, object>)
            {
                return ((Func<TContainer, Dictionary<string, object>, object>)concrete)(this as TContainer, parameters);
            }

            Type concrete1 = (Type)concrete;

            if (concrete1.IsAbstract)
            {
                string message;
                if (this._buildStack.Length > 0)
                {
                    message = string.Format("Target {0} is not instantiable while building {1}.", concrete1, ArrayUtil.map(this._buildStack, item => item.ToString()));
                }
                else
                {
                    message = string.Format("Target {0} is not instantiable.", concrete1);
                }
                throw new Exception(message);
            }

            this._buildStack = ArrayUtil.push(this._buildStack, concrete1);

            // pick constructors
            // DIFF: we filter out constructor with lower number of arguments than the given ones
            var constructors = ArrayUtil.filter(concrete1.GetConstructors(), constructor => constructor.GetParameters().Length >= parameters.Count);

            // If there are no constructors, that means there are no dependencies then
            // we can just resolve the instances of the objects right away, without
            // resolving any other types or dependencies out of these containers.
            if (constructors.Length == 0)
            {
                this._buildStack = ArrayUtil.pop(this._buildStack);
                return Activator.CreateInstance(concrete1, parameters);
            }
            else if (constructors.Length > 1)
            {
                // TODO: attempt to find a constructor by matching the giver parameters types with the tail of the parameters
                throw new Exception("Calling unclear constructor.");
            }

            var dependencies = constructors[0].GetParameters();
            // Once we have all the constructor's parameters we can create each of the
            // dependency instances and then use the reflection instances to make a
            // new instance of this class, injecting the created dependencies in.
            var parameters1 = this.keyParametersByArgument(dependencies, parameters);
            var instances = this.getDependencies(dependencies, parameters1);

            ArrayUtil.pop(this._buildStack);

            // TODO: Dependency injection?
            return Activator.CreateInstance(concrete1, instances);
        }



        /**
         * If extra parameters are passed by numeric ID, rekey them by argument name.
         *
         * @param  array  $dependencies
         * @param  array  $parameters
         * @return array
         */
        protected Dictionary<string, object> keyParametersByArgument(ParameterInfo[] dependencies, Dictionary<string, object> parameters)
        {
            Dictionary<string, object> parameters1 = new Dictionary<string, object>();

            foreach (var e in parameters)
            {
                if (StrUtil.isNumeric(e.Key))
                {
                    int i;
                    if (Int32.TryParse(e.Key, out i))
                    {
                        parameters1[dependencies[i].Name] = e.Value;
                        continue;
                    }
                }

                parameters1[e.Key] = e.Value;
            }
            return parameters1;
        }


        /**
         * Resolve all of the dependencies from the ReflectionParameters.
         *
         * @param  array  $parameters
         * @param  array  $primitives
         * @return array
         */
        protected object[] getDependencies(ParameterInfo[] parameters, Dictionary<string, object> primitives = null)
        {
            primitives = primitives ?? new Dictionary<string, object>();
            var dependencies = new object[0];
            foreach (var parameter in parameters)
            {
                var dependency = parameter.ParameterType;
                // If the class is null, it means the dependency is a string or some other
                // primitive type which we can not resolve since it is not a class and
                // we will just bomb out with an error since we have no-where to go.
                if (primitives.ContainsKey(parameter.Name))
                {
                    dependencies = ArrayUtil.push(dependencies, primitives[parameter.Name]);
                }
                else if (!(parameter.ParameterType.IsClass || parameter.ParameterType.IsInterface))
                {
                    dependencies = ArrayUtil.push(dependencies, this.resolveNonClass(parameter));
                }
                else
                {
                    dependencies = ArrayUtil.push(dependencies, this.resolveClass(parameter));
                }
            }
            return dependencies;
        }
        /**
         * Resolve a non-class hinted dependency.
         *
         * @param  \ReflectionParameter  $parameter
         * @return mixed
         *
         * @throws \Illuminate\Contracts\Container\BindingResolutionException
         */
        protected object resolveNonClass(ParameterInfo parameter)
        {
            /*
            TODO: Contextual binding
            if (!is_null($concrete = $this->getContextualConcrete('$'.$parameter->name)))
            {
                if ($concrete instanceof Closure) {
                    return call_user_func($concrete, $this);
                } else {
                    return $concrete;
                }
            }
            */
            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }
            string message = string.Format("Unresolvable dependency resolving {0} in class {1}", parameter.Name, parameter.Member.DeclaringType.FullName);
            throw new Exception(message);
        }
        /**
         * Resolve a class based dependency from the container.
         *
         * @param  \ReflectionParameter  $parameter
         * @return mixed
         *
         * @throws \Illuminate\Contracts\Container\BindingResolutionException
         */
        protected object resolveClass(ParameterInfo parameter)
        {
            try
            {
                return this.make(parameter.ParameterType);
            }
            // If we can not resolve the class instance, we will check to see if the value
            // is optional, and if it is we will return the optional parameter value as
            // the value of the dependency, similarly to how we do this with scalars.
            catch (Exception e)
            {
                if (parameter.IsOptional)
                {
                    return parameter.DefaultValue;
                }
                throw e;
            }
        }

        /**
 * Fire the "rebound" callbacks for the given abstract type.
 *
 * @param  string  $abstract
 * @return void
 */
        protected void rebound(Type abstracts)
        {
            var instance = this.make(abstracts);
            foreach (var callback in this.getReboundCallbacks(abstracts))
            {
                callback(this as TContainer, instance);
            }
        }
        /**
         * Get the rebound callbacks for a given type.
         *
         * @param  string  $abstract
         * @return array
         */
        protected Action<TContainer, object>[] getReboundCallbacks(Type abstracts)
        {
            if (this._reboundCallbacks.ContainsKey(abstracts))
            {
                return this._reboundCallbacks[abstracts];
            }
            return new Action<TContainer, object>[0];
        }

        /**
     * Resolve the given type from the container.
     *
     * @param  string  $abstract
     * @param  array   $parameters
     * @return mixed
     */
        public TAbstract make<TAbstract>(Dictionary<string, object> parameters = null)
        {
            return (TAbstract)this.make(typeof(TAbstract), parameters);
        }

        public object make(Type abstracts, Dictionary<string, object> parameters = null)
        {
            parameters = parameters ?? new Dictionary<string, object>();
            abstracts = this.getAlias(abstracts);
            // If an instance of the type is currently being managed as a singleton we'll
            // just return an existing instance instead of instantiating new instances
            // so the developer can keep using the same objects instance every time.
            if (this._instances.ContainsKey(abstracts))
            {
                return this._instances[abstracts];
            }
            var concrete = this.getConcrete(abstracts);
            // We're ready to instantiate an instance of the concrete type registered for
            // the binding. This will instantiate the types, as well as resolve any of
            // its "nested" dependencies recursively until all have gotten resolved.
            object inst;
            if (this.isBuildable(concrete, abstracts))
            {
                inst = this.build(concrete, parameters);
            }
            else
            {
                inst = this.make((Type)concrete, parameters);
            }
            // If we defined any extenders for this type, we'll need to spin through them
            // and apply them to the object being built. This allows for the extension
            // of services, such as changing configuration or decorating the object.
            foreach (var extender in this.getExtenders(abstracts))
            {
                inst = extender(inst, this as TContainer);
            }
            // If the requested type is registered as a singleton we'll want to cache off
            // the instances in "memory" so we can return it later without creating an
            // entirely new instance of an object on each subsequent request for it.
            if (this.isShared(abstracts))
            {
                this._instances[abstracts] = inst;
            }
            this.fireResolvingCallbacks(abstracts, inst);
            this._resolved[abstracts] = true;
            return inst;
        }


        /**
         * Get the concrete type for a given abstract.
         *
         * @param  string  $abstract
         * @return mixed   $concrete
         */
        protected object getConcrete(Type abstracts)
        {
            //if (! is_null($concrete = $this->getContextualConcrete($abstract))) {
            //    return $concrete;
            //}
            // If we don't have a registered resolver or concrete for the type, we'll just
            // assume each type is a concrete name and will attempt to resolve it as is
            // since the container should be able to resolve concretes automatically.
            if (!this._bindings.ContainsKey(abstracts))
            {
                return abstracts;
            }
            return this._bindings[abstracts].concrete;
        }

        /**
         * Determine if the given concrete is buildable.
         *
         * @param  mixed   $concrete
         * @param  string  $abstract
         * @return bool
         */
        protected bool isBuildable(object concrete, Type abstracts)
        {
            return (concrete is Type && (Type)concrete == abstracts) || concrete is Func<TContainer, Dictionary<string, object>, object>;
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
            if (this._bindings.ContainsKey(abstracts))
            {
                return this._bindings[abstracts].shared;
            }
            return false;
        }


        /**
         * Get the extender callbacks for a given type.
         *
         * @param  string  $abstract
         * @return array
         */
        protected Func<object, TContainer, object>[] getExtenders(Type abstracts)
        {
            if (this._extenders.ContainsKey(abstracts))
            {
                return this._extenders[abstracts];
            }
            return new Func<object, TContainer, object>[0];
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
        protected Action<object, TContainer>[] getCallbacksForType(Type abstracts, object inst, Dictionary<Type, Action<object, TContainer>[]> callbacksPerType)
        {
            Action<object, TContainer>[] results = new Action<object, TContainer>[0];
            foreach (var e in callbacksPerType)
            {
                Type type = e.Key;
                Action<object, TContainer>[] callbacks = e.Value;

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
        protected void fireCallbackArray(object inst, Action<object, TContainer>[] callbacks)
        {
            foreach (var callback in callbacks)
            {
                callback(inst, this as TContainer);
            }
        }

        /**
         * Register a shared binding in the container.
         *
         * @param  string|array  $abstract
         * @param  \Closure|string|null  $concrete
         * @return void
         */
        public void singleton<TAbstract>(Func<TContainer, Dictionary<string, object>, object> concrete = null)
        {
            this.bind(typeof(TAbstract), concrete, true);
        }
        public void singleton(Type abstracts, Type concrete = null)
        {
            this.bind(abstracts, concrete, true);
        }

        /**
 * Register an existing instance as shared in the container.
 *
 * @param  string  $abstract
 * @param  mixed   $instance
 * @return void
 */
        public void instance<TAbstract>(object instance)
        {
            this.instance(typeof(TAbstract), instance);
        }

        public void instance(Type abstracts, object instance)
        {
            // First, we will extract the alias from the abstract if it is an array so we
            // are using the correct name when binding the type. If we get an alias it
            // will be registered with the container so we can resolve it out later.
            // TODO: Register aliases
            //if (is_array($abstract)) {
            //    list($abstract, $alias) = $this->extractAlias($abstract);
            //    $this->alias($abstract, $alias);
            //}
            //unset($this->aliases[$abstract]);
            // We'll check to determine if this type has been bound before, and if it has
            // we will fire the rebound callbacks registered with the container and it
            // can be updated with consuming classes that have gotten resolved here.
            var bound = this.bound(abstracts);
            this._instances[abstracts] = instance;
            if (bound)
            {
                this.rebound(abstracts);
            }
        }


        public object this[Type t]
        {
            /**
             * Get the value at a given offset.
             *
             * @param  string  $key
             * @return mixed
             */
            get { return this.make(t); }

            /**
             * Set the value at a given offset.
             *
             * @param  string  $key
             * @param  mixed   $value
             * @return void
             */
            set
            {
                this.bind(t, value);
            }
        }


    }
}
