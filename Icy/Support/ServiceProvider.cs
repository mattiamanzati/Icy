using Icy.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icy.Support
{
    public abstract class ServiceProvider
    {
        /**
         * The application instance.
         *
         * @var \Illuminate\Contracts\Foundation\Application
         */
        protected Application app;

        /**
         * Create a new service provider instance.
         *
         * @param  \Illuminate\Contracts\Foundation\Application  $app
         * @return void
         */
        public ServiceProvider(Application app)
        {
            this.app = app;
        }
        /**
         * Register the service provider.
         *
         * @return void
         */
        abstract public void register();
        abstract public void boot();
    }
}
