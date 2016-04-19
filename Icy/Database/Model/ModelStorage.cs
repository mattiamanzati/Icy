using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Icy.Foundation;

namespace Icy.Database.Model
{
    public class ModelStorage
    {
        /**
         * Indicates whether attributes are snake cased on arrays.
         *
         * @var bool
         */
        public bool _snakeAttributes = true;
        /**
         * The connection resolver instance.
         *
         * @var \Illuminate\Database\ConnectionResolverInterface
         */
        public ConnectionResolverInterface _resolver;
        /**
         * The event dispatcher instance.
         *
         * @var \Illuminate\Contracts\Events\Dispatcher
         */
        public object _dispatcher;
        /**
         * The array of booted models.
         *
         * @var array
         */
        public Dictionary<Type, bool> _booted = new Dictionary<Type, bool>();
        /**
         * The array of global scopes on the model.
         *
         * @var array
         */
        public Dictionary<Type, Dictionary<Type, IScope>> _globalScopes = new Dictionary<Type, Dictionary<Type, IScope>>();
        /**
         * Indicates if all mass assignment is enabled.
         *
         * @var bool
         */
        public bool _unguarded = false;
        /**
         * The cache of the mutated attributes for each class.
         *
         * @var array
         */
        public Dictionary<Type, string[]> _mutatorCache = new Dictionary<Type, string[]>();

        /**
         * A reference to the Application object the instance lives in
         **/
        protected Application app;

        public ModelStorage(Application app)
        {
            this.app = app;
        }
    }
}
