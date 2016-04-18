using Icy.Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using Icy.Util;

namespace Icy.Database.Model
{
    public class Model<T> where T : Model<T>, new()
    {
        /**
 * The connection name for the model.
 *
 * @var string
 */
        protected string _connection;
        /**
         * The table associated with the model.
         *
         * @var string
         */
        protected string _table;
        /**
         * The primary key for the model.
         *
         * @var string
         */
        protected string _primaryKey = "id";
        /**
         * The number of models to return for pagination.
         *
         * @var int
         */
        protected int _perPage = 15;
        /**
         * Indicates if the IDs are auto-incrementing.
         *
         * @var bool
         */
        public bool _incrementing = true;
        /**
         * Indicates if the model should be timestamped.
         *
         * @var bool
         */
        public bool _timestamps = true;
        /**
         * The model's attributes.
         *
         * @var array
         */
        protected Dictionary<string, object> _attributes = new Dictionary<string, object>();
        /**
         * The model attribute's original state.
         *
         * @var array
         */
        protected Dictionary<string, object> _original = new Dictionary<string, object>();
        /**
         * The loaded relationships for the model.
         *
         * @var array
         */
        protected Dictionary<string, bool> _relations = new Dictionary<string, bool>();
        /**
         * The attributes that should be hidden for arrays.
         *
         * @var array
         */
        protected string[] hidden = new string[0];
        /**
         * The attributes that should be visible in arrays.
         *
         * @var array
         */
        protected string[] visible = new string[0];
        /**
         * The accessors to append to the model's array form.
         *
         * @var array
         */
        protected string[] _appends = new string[0];
        /**
         * The attributes that are mass assignable.
         *
         * @var array
         */
        protected string[] _fillable = new string[0];
        /**
         * The attributes that aren't mass assignable.
         *
         * @var array
         */
        protected string[] _guarded = new string[] { "*" };
        /**
         * The attributes that should be mutated to dates.
         *
         * @var array
         */
        protected string[] _dates = new string[0];
        /**
         * The storage format of the model's date columns.
         *
         * @var string
         */
        protected string _dateFormat;
        /**
         * The attributes that should be cast to native types.
         *
         * @var array
         */
        protected Dictionary<string, Type> _casts = new Dictionary<string, Type>();
        /**
         * The relationships that should be touched on save.
         *
         * @var array
         */
        protected string[] _touches = new string[0];
        /**
         * User exposed observable events.
         *
         * @var array
         */
        protected string[] observables = new string[0];
        /**
         * The relations to eager load on every query.
         *
         * @var array
         */
        protected string[] with = new string[0];
        /**
         * The class name to be used in polymorphic relations.
         *
         * @var string
         */
        protected string _morphClass;
        /**
         * Indicates if the model exists.
         *
         * @var bool
         */
        public bool _exists = false;
        /**
         * Indicates if the model was inserted during the current request lifecycle.
         *
         * @var bool
         */
        // TODO: Is this really needed?
        //public $wasRecentlyCreated = false;

        /**
         * The many to many relationship methods.
         *
         * @var array
         */
        public static string[] manyMethods = new string[] { "belongsToMany", "morphToMany", "morphedByMany" };
        /**
         * The name of the "created at" column.
         *
         * @var string
         */
        const string CREATED_AT = "created_at";
        /**
         * The name of the "updated at" column.
         *
         * @var string
         */
        const string UPDATED_AT = "updated_at";

        protected Application app;
        protected ModelStorage storage;

        /**
 * Create a new Eloquent model instance.
 *
 * @param  array  $attributes
 * @return void
 */
        public Model(Application app = null, Dictionary<string, object> attributes = null)
        {
            attributes = attributes ?? new Dictionary<string, object>();

            this.app = app;
            this.storage = app.make<ModelStorage>();

            this.bootIfNotBooted();
            this.syncOriginal();
            //this.fill(attributes);
        }


        /**
         * Check if the model needs to be booted and if so, do it.
         *
         * @return void
         */
        protected void bootIfNotBooted()
        {
            if (!this.storage._booted.ContainsKey(this.GetType()))
            {
                this.storage._booted[this.GetType()] = true;
                // $this->fireModelEvent('booting', false);
                this.boot();
                // $this->fireModelEvent('booted', false);
            }
        }
        /**
         * The "booting" method of the model.
         *
         * @return void
         */
        protected void boot()
        {
            this.bootTraits();
        }
        /**
         * Boot all of the bootable traits on the model.
         *
         * @return void
         */
        protected void bootTraits()
        {
            foreach (var e in this.GetType().GetInterfaces())
            {
                var bootMethod = this.GetType().GetMethod("boot" + e.Name, System.Reflection.BindingFlags.NonPublic);
                if (bootMethod != null)
                {
                    bootMethod.Invoke(this, new object[] { });
                }
            }
        }

        /**
         * Sync the original attributes with the current.
         *
         * @return $this
         */
        public Model<T> syncOriginal()
        {
            this._original = DictionaryUtil.copy(this._attributes);
            return this;
        }
    }
}
