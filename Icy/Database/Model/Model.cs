using Icy.Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using Icy.Util;
using System.Globalization;

namespace Icy.Database.Model
{
    public class Model<T> where T : Model<T>
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
        public Model(Application app, Dictionary<string, object> attributes = null)
        {
            attributes = attributes ?? new Dictionary<string, object>();

            this.app = app;
            this.storage = app.make<ModelStorage>();

            this.bootIfNotBooted();
            this.syncOriginal();
            this.fill(attributes);
        }


        /**
         * Check if the model needs to be booted and if so, do it.
         *
         * @return void
         */
        protected virtual void bootIfNotBooted()
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
        protected virtual void boot()
        {
            this.bootTraits();
        }
        /**
         * Boot all of the bootable traits on the model.
         *
         * @return void
         */
        protected virtual void bootTraits()
        {
            foreach (var e in ReflectionUtil.getInterfaces(this.GetType()))
            {
                if (ReflectionUtil.existsMethod(this.GetType(), "boot" + e.Name))
                {
                    ReflectionUtil.callMethod(this, "boot" + e.Name);
                }
            }
        }


        /**
         * Fill the model with an array of attributes.
         *
         * @param  array  $attributes
         * @return $this
         *
         * @throws \Illuminate\Database\Eloquent\MassAssignmentException
         */
        public virtual Model<T> fill(Dictionary<string, object> attributes = null)
        {
            var totallyGuarded = this.totallyGuarded();
            foreach (var e in this.fillableFromArray(attributes))
            {
                string key = this.removeTableFromKey(e.Key);
                // The developers may choose to place some attributes in the "fillable"
                // array, which means only those attributes may be set through mass
                // assignment to the model, and all others will just be ignored.
                if (this.isFillable(key))
                {
                    this.setAttribute(key, e.Value);
                }
                else if (totallyGuarded)
                {
                    throw new MassAssignmentException(key);
                }
            }
            return this;
        }

        /**
         * Sync the original attributes with the current.
         *
         * @return $this
         */
        public virtual Model<T> syncOriginal()
        {
            this._original = DictionaryUtil.copy(this._attributes);
            return this;
        }


        /**
         * Determine if the model is totally guarded.
         *
         * @return bool
         */
        public virtual bool totallyGuarded()
        {
            return this.getFillable().Length == 0 && (this.getGuarded().Length == 0 && this.getGuarded()[0] == "*");
        }

        /**
         * Remove the table name from a given key.
         *
         * @param  string  $key
         * @return string
         */
        protected virtual string removeTableFromKey(string key)
        {
            if (!key.Contains("."))
            {
                return key;
            }
            return key.Substring(key.LastIndexOf("."));
        }

        /**
         * Get the fillable attributes for the model.
         *
         * @return array
         */
        public virtual string[] getFillable()
        {
            return this._fillable;
        }

        /**
         * Set the fillable attributes for the model.
         *
         * @param  array  $fillable
         * @return $this
         */
        public virtual Model<T> fillable(string[] fillable)
        {
            this._fillable = ArrayUtil.copy(fillable);
            return this;
        }
        /**
         * Get the guarded attributes for the model.
         *
         * @return array
         */
        public virtual string[] getGuarded()
        {
            return this._guarded;
        }
        /**
         * Set the guarded attributes for the model.
         *
         * @param  array  $guarded
         * @return $this
         */
        public virtual Model<T> guard(string[] guarded)
        {
            this._guarded = ArrayUtil.copy(guarded);
            return this;
        }


        /**
         * Determine if the given attribute may be mass assigned.
         *
         * @param  string  $key
         * @return bool
         */
        public virtual bool isFillable(string key)
        {
            if (this.storage._unguarded)
            {
                return true;
            }
            // If the key is in the "fillable" array, we can of course assume that it's
            // a fillable attribute. Otherwise, we will check the guarded array when
            // we need to determine if the attribute is black-listed on the model.
            if (ArrayUtil.indexOf(this.getFillable(), key) > -1)
            {
                return true;
            }
            if (this.isGuarded(key))
            {
                return false;
            }
            return this.getFillable().Length == 0 && !key.StartsWith("_");
        }

        /**
         * Determine if the given key is guarded.
         *
         * @param  string  $key
         * @return bool
         */
        public virtual bool isGuarded(string key)
        {
            return ArrayUtil.indexOf(this.getGuarded(), key) > -1 || (this.getGuarded().Length == 1 && this.getGuarded()[0] == "*");
        }

        /**
         * Set a given attribute on the model.
         *
         * @param  string  $key
         * @param  mixed  $value
         * @return $this
         */
        public virtual Model<T> setAttribute(string key, object value)
        {
            // First we will check for the presence of a mutator for the set operation
            // which simply lets the developers tweak the attribute as it is set on
            // the model, such as "json_encoding" an listing of data for storage.
            if (this.hasSetMutator(key))
            {
                var method = "set" + StrUtil.studly(key) + "Attribute";
                ReflectionUtil.callMethod(this, method, value);
                return this;
            }
            // If an attribute is listed as a "date", we'll convert it from a DateTime
            // instance into a form proper for storage on the database tables using
            // the connection grammar's date format. We will auto set the values.
            else if (value != null && (ArrayUtil.indexOf(this.getDates(), key) > -1 || this.isDateCastable(key)))
            {
                value = this.fromDateTime(value);
            }
            if (this.isJsonCastable(key) && value != null)
            {
                value = this.asJson(value);
            }
            this._attributes[key] = value;
            return this;
        }

        /**
         * Determine if a set mutator exists for an attribute.
         *
         * @param  string  $key
         * @return bool
         */
        public virtual bool hasSetMutator(string key)
        {
            return ReflectionUtil.existsMethod(this.GetType(), "set" + StrUtil.studly(key) + "Attribute");
        }

        /**
         * Get the attributes that should be converted to dates.
         *
         * @return array
         */
        public virtual string[] getDates()
        {
            string[] defaults = new string[] { CREATED_AT, UPDATED_AT };
            return this._timestamps ? ArrayUtil.concat(this._dates, defaults) : this._dates;
        }

        /**
 * Convert a DateTime to a storable string.
 *
 * @param  \DateTime|int  $value
 * @return string
 */
        public virtual string fromDateTime(object value)
        {
            var format = this.getDateFormat();
            var dt = this.asDateTime(value);
            return dt.ToString(format);
        }
        /**
         * Return a timestamp as DateTime object.
         *
         * @param  mixed  $value
         * @return \Carbon\Carbon
         */
        protected virtual DateTime asDateTime(object value)
        {
            // If this value is already a Carbon instance, we shall just return it as is.
            // This prevents us having to re-instantiate a Carbon instance when we know
            // it already is one, which wouldn't be fulfilled by the DateTime check.
            if (value is DateTime) {
                return (DateTime)value;
            }
            // If the value is already a DateTime instance, we will just skip the rest of
            // these checks since they will be a waste of time, and hinder performance
            // when checking the field. We will just return the DateTime right away.
            /*
            if ($value instanceof DateTimeInterface) {
                return new Carbon(
                $value->format('Y-m-d H:i:s.u'), $value->getTimeZone()
                );
            }
            */
            // If this value is an integer, we will assume it is a UNIX timestamp's value
            // and format a Carbon object from this timestamp. This allows flexibility
            // when defining your date fields as they might be UNIX timestamps here.
            if (value is int)
            {
                return (new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).AddSeconds((int)value);
            }
            // If the value is in simply year, month, day format, we will instantiate the
            // Carbon instances from that format. Again, this provides for simple date
            // fields on the database, while still supporting Carbonized conversion.
            /* TODO
            if (preg_match('/^(\d{4})-(\d{1,2})-(\d{1,2})$/', $value))
            {
                return Carbon::createFromFormat('Y-m-d', $value)->startOfDay();
            }*/
            // Finally, we will just assume this date is in the format used by default on
            // the database connection and use that format to create the Carbon object
            // that is returned back out to the developers after we convert it here.
            return DateTime.ParseExact(value.ToString(), this.getDateFormat(), CultureInfo.InvariantCulture);
        }

        /**
         * Get the format for database stored dates.
         *
         * @return string
         */
        protected virtual string getDateFormat()
        {
            return this._dateFormat != null ? this._dateFormat : this.getConnection().getQueryGrammar().getDateFormat();
        }
    }
}
