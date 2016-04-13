using Icy.Support;
using Icy.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Foundation
{

    public class Application : Icy.Container.Container
    {
        /**
         * The Laravel framework version.
         *
         * @var string
         */
        const int VERSION = 502029;

        /**
         * All of the registered service providers.
         *
         * @var array
         */
        protected ServiceProvider[] _serviceProviders = new ServiceProvider[0];
        /**
         * The names of the loaded service providers.
         *
         * @var array
         */
        protected Dictionary<Type, bool> _loadedProviders = new Dictionary<Type, bool>();
        /**
         * The deferred services and their providers.
         *
         * @var array
         */
        protected Type[] _deferredServices = new Type[0];


        /**
         * Indicates if the application has "booted".
         *
         * @var bool
         */
        protected bool _booted = false;

        public Dictionary<Type, object> _config = new Dictionary<Type, object>();

        /*
         * Gets or sets the config object for that type (laravel uses a string, I use a class to get it typed)
         **/
        public T config<T>() where T : class, new()
        {
            var t = typeof(T);
            if (!this._config.ContainsKey(t)) this._config[t] = new T();
            return (T)this._config[t];
        }

        public void config<T>(T value)
        {
            this._config[typeof(T)] = value;
        }

        /*
         * Creates the app config from a XML file
         **/
        public static Application createFromConfig()
        {
            return new Application();
        }



        /**
         * Register a service provider with the application.
         *
         * @param  \Illuminate\Support\ServiceProvider|string  $provider
         * @param  array  $options
         * @param  bool   $force
         * @return \Illuminate\Support\ServiceProvider
         */
        public ServiceProvider register(ServiceProvider provider, Dictionary<Type, object> options = null, bool force = false)
        {
            options = options ?? new Dictionary<Type, object>();

            ServiceProvider registered = this.getProvider(provider);
            if (registered != null && !force)
            {
                return registered;
            }

            provider.register();

            // Once we have registered the service we will iterate through the options
            // and set each of them on the application so they will be available on
            // the actual loading of the service objects and for developer usage.
            foreach (var e in options)
            {
                this[e.Key] = e.Value;
            }
            this.markAsRegistered(provider);
            // If the application has already booted, we will call this boot method on
            // the provider class so it has an opportunity to do its boot logic and
            // will be ready for any usage by the developer's application logics.
            if (this._booted)
            {
                this.bootProvider(provider);
            }
            return provider;
        }
        /**
         * Get the registered service provider instance if it exists.
         *
         * @param  \Illuminate\Support\ServiceProvider|string  $provider
         * @return \Illuminate\Support\ServiceProvider|null
         */
        public ServiceProvider getProvider(ServiceProvider provider)
        {
            var providers = ArrayUtil.filter(this._serviceProviders, (p) => p.GetType().IsAssignableFrom(provider.GetType()));
            return providers.Length > 0 ? providers[0] : null;
        }

        /**
         * Mark the given provider as registered.
         *
         * @param  \Illuminate\Support\ServiceProvider  $provider
         * @return void
         */
        protected void markAsRegistered(ServiceProvider provider)
        {
            //$this['events']->fire($class = get_class($provider), [$provider]);
            this._serviceProviders = ArrayUtil.push(this._serviceProviders, provider);
            this._loadedProviders[provider.GetType()] = true;

        }

        /**
         * Boot the given service provider.
         *
         * @param  \Illuminate\Support\ServiceProvider  $provider
         * @return mixed
         */
        protected void bootProvider(ServiceProvider provider)
        {
            provider.boot();
        }
    }
}
