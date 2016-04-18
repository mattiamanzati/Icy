using System;
using System.Collections.Generic;
using System.Text;
using Icy.Util;
using QueryBuilder = Icy.Database.Query.Builder;

namespace Icy.Database.Model
{
    public class Builder<T> where T : Model<T>, new()
    {
        /**
         * The base query builder instance.
         *
         * @var \Illuminate\Database\Query\Builder
         */
        protected QueryBuilder _query;
        /**
         * The model being queried.
         *
         * @var \Illuminate\Database\Eloquent\Model
         */
        protected Model<T> _model;
        /**
         * The relationships that should be eager loaded.
         *
         * @var array
         */
        protected object[] _eagerLoad = new object[0];
        /**
         * All of the registered builder macros.
         *
         * @var array
         */
        protected object _macros = new object[0];
        /**
         * A replacement for the typical delete function.
         *
         * @var \Closure
         */
        protected object _onDelete;
        /**
         * The methods that should be returned from query builder.
         *
         * @var array
         */
        protected string[] _passthru = new string[]{
        "insert", "insertGetId", "getBindings", "toSql",
        "exists", "count", "min", "max", "avg", "sum", "getConnection",
    };
        /**
         * Applied global scopes.
         *
         * @var array
         */
        protected Dictionary<string, IScope> _scopes = new Dictionary<string, IScope>();
        /**
         * Create a new Eloquent query builder instance.
         *
         * @param  \Illuminate\Database\Query\Builder  $query
         * @return void
         */
        public Builder(QueryBuilder query)
        {
            this._query = query;
        }
    //    /**
    //     * Register a new global scope.
    //     *
    //     * @param  string  $identifier
    //     * @param  \Illuminate\Database\Eloquent\Scope|\Closure  $scope
    //     * @return $this
    //     */
    //    public virtual Builder<T> withGlobalScope(string identifier, Scope<T> scope)
    //    {
    //        this._scopes[identifier] = scope;
    //        scope.extend(this);
    //        return this;
    //    }
    //    /**
    //     * Remove a registered global scope.
    //     *
    //     * @param  \Illuminate\Database\Eloquent\Scope|string  $scope
    //     * @return $this
    //     */

    //    public virtual Builder<T> withoutGlobalScope(object scope)
    //    {
    //        if (scope is string)
    //        {
    //            this._scopes.Remove(scope.ToString());
    //            return this;
    //        }

    //        foreach (var e in this._scopes)
    //        {
    //            if (e.Value.GetType() == (Type)scope)
    //            {
    //                this._scopes.Remove(e.Key);
    //            }
    //        }
    //        return this;
    //    }

    //    /**
    //     * Remove all or passed registered global scopes.
    //     *
    //     * @param  array|null  $scopes
    //     * @return $this
    //     */

    //    public virtual Builder<T> withoutGlobalScopes(object[] scopes = null)
    //    {
    //        if (scopes != null)
    //        {
    //            foreach (var scope in scopes)
    //            {
    //                this.withoutGlobalScope(scope);
    //            }
    //        }
    //        else
    //        {
    //            this._scopes = new Dictionary<string, Scope<T>>();
    //        }
    //        return this;
    //    }
    //    /**
    //     * Find a model by its primary key.
    //     *
    //     * @param  mixed  $id
    //     * @param  array  $columns
    //     * @return \Illuminate\Database\Eloquent\Model|\Illuminate\Database\Eloquent\Collection|null
    //     */
    //    public virtual Model<T> find(object id, object[] columns = null)
    //    {
    //        columns = columns ?? new object[] { "*" };

    //        this._query.where(this._model.getQualifiedKeyName(), "=", id);
    //        return this.first(columns);
    //    }

        
    ///**
    // * Find a model by its primary key.
    // *
    // * @param  array  $ids
    // * @param  array  $columns
    // * @return \Illuminate\Database\Eloquent\Collection
    // */
    //public virtual Collection findMany(object[] ids, object[] columns = null)
    //{
    //    columns = columns ?? new object[]{ "*" };

    //    if (ids.Length == 0) {
    //        return this._model.newCollection();
    //    }

    //    this._query.whereIn(this._model.getQualifiedKeyName(), ids);
    //    return this.get(columns);
    //}
    ///**
    // * Find a model by its primary key or throw an exception.
    // *
    // * @param  mixed  $id
    // * @param  array  $columns
    // * @return \Illuminate\Database\Eloquent\Model|\Illuminate\Database\Eloquent\Collection
    // *
    // * @throws \Illuminate\Database\Eloquent\ModelNotFoundException
    // */
    //public virtual Model<T> findOrFail(object id, object[] columns = null)
    //{
    //    columns = columns ?? new object[]{ "*" };
    //    var result = this.find(id, columns);
    //    if (result != null) {
    //        return result;
    //    }
    //    throw (new ModelNotFoundException()).setModel(this._model.GetType());
    //}
    ///**
    // * Execute the query and get the first result.
    // *
    // * @param  array  $columns
    // * @return \Illuminate\Database\Eloquent\Model|static|null
    // */
    //public virtual Model<T> first(object[] columns = null)
    //{
    //    columns = columns ?? new object[]{ "*" };
    //    return this.take(1).get(columns).first();
    //}

        
    ///**
    // * Execute the query and get the first result or throw an exception.
    // *
    // * @param  array  $columns
    // * @return \Illuminate\Database\Eloquent\Model|static
    // *
    // * @throws \Illuminate\Database\Eloquent\ModelNotFoundException
    // */
    //public virtual Model<T> firstOrFail(object[] columns = null)
    //{
    //    Model<T> model = this.first(columns);

    //    if (model != null) {
    //        return model;
    //    }
    //    throw (new ModelNotFoundException()).setModel(this._model.GetType());
    //}
    ///**
    // * Execute the query as a "select" statement.
    // *
    // * @param  array  $columns
    // * @return \Illuminate\Database\Eloquent\Collection|static[]
    // */
    //public virtual Collection<T> get(object columns = null)
    //{
    //    columns = columns ?? new object[]{ "*" };
    //    var builder = this.applyScopes();
    //    Model<T>[] models = builder.getModels(columns);
    //    // If we actually found models we will also eager load any relationships that
    //    // have been specified as needing to be eager loaded, which will solve the
    //    // n+1 query issue for the developers to avoid running a lot of queries.
    //    if (models.Length > 0) {
    //        models = builder.eagerLoadRelations(models);
    //    }
    //    return builder.getModel().newCollection(models);
    //}
    ///**
    // * Get a single column's value from the first result of a query.
    // *
    // * @param  string  $column
    // * @return mixed
    // */
    //public virtual object value(column)
    //{
    //    Model<T> result = this.first(new object[]{ column });
    //    if (result != null) {
    //        return result.getAttribute(column);
    //    }
    //}
    ///**
    // * Chunk the results of the query.
    // *
    // * @param  int  $count
    // * @param  callable  $callback
    // * @return bool
    // */
    //public virtual bool chunk(int count, Func<Collection<T>, bool> callback)
    //{
    //    int page = 1;
    //    Collection<T> results = this.forPage(page, count).get();
    //    while (results.Length > 0) {
    //        // On each chunk result set, we will pass them to the callback and then let the
    //        // developer take care of everything within the callback, which allows us to
    //        // keep the memory low for spinning through large result sets for working.
    //        if (callback(results) == false) {
    //            return false;
    //        }
    //        page++;
    //        results = this.forPage(page, count).get();
    //    }
    //    return true;
    //}
    ///**
    // * Execute a callback over each item while chunking.
    // *
    // * @param  callable  $callback
    // * @param  int  $count
    // * @return bool
    // */
    //public virtual bool each(Func<Model<T>, int, bool> callback, int count = 1000)
    //{
    //    if ((this._query._orders == null || this._query._orders.Length == 0) && (this._query._unionOrders == null || this._query._unionOrders.Length == 0)) {
    //        this._query.orderBy(this._model.getQualifiedKeyName(), "asc");
    //    }
    //    return this.chunk(count, (results) => {
    //        for (var i = 0; i < results.Length; i++) {
    //            if (callback(results[i], i) == false) {
    //                return false;
    //            }
    //        }
    //        return true;
    //    });
    //}
    ///**
    // * Get an array with the values of a given column.
    // *
    // * @param  string  $column
    // * @param  string|null  $key
    // * @return \Illuminate\Support\Collection
    // */
    //public virtual Collection<T> pluck(string column, string key = null)
    //{
    //    $results = $this->toBase()->pluck($column, $key);
    //    // If the model has a mutator for the requested column, we will spin through
    //    // the results and mutate the values so that the mutated version of these
    //    // columns are returned as you would expect from these Eloquent models.
    //    if ($this->model->hasGetMutator($column)) {
    //        foreach ($results as $key => &$value) {
    //            $fill = [$column => $value];
    //            $value = $this->model->newFromBuilder($fill)->$column;
    //        }
    //    }
    //    return collect($results);
    //}

    }
}
