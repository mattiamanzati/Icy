using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Foundation
{

    public class Application: Icy.Container.Container
    {
        public Dictionary<Type, object> _config = new Dictionary<Type, object>();

        /*
         * Gets or sets the config object for that type (laravel uses a string, I use a class to get it typed)
         **/
        public T config<T>() where T: class, new()
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
        public static Application createFromConfig(){
            return new Application();
        }
    }
}
