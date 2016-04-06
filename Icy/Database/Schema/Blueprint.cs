using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database.Schema
{
    public class Blueprint
    {
        /**
     * The table the blueprint describes.
     *
     * @var string
     */
    protected string _table;
    /**
     * The columns that should be added to the table.
     *
     * @var array
     */
    protected object[] _columns = new object[0];
    /**
     * The commands that should be run for the table.
     *
     * @var array
     */
    protected object[] commands = new object[0];
    /**
     * The storage engine that should be used for the table.
     *
     * @var string
     */
    public string _engine;
    /**
     * The default character set that should be used for the table.
     */
    public string _charset;
    /**
     * The collation that should be used for the table.
     */
    public string _collation;
    /**
     * Whether to make the table temporary.
     *
     * @var bool
     */
    public bool _temporary = false;

        public Blueprint(string table, Action<Blueprint> callback = null)
        {
            this._table = table;

            if (callback != null)
            {
                callback(this);
            }
        }

        internal void rename(string to)
        {
            throw new NotImplementedException();
        }

        internal void dropIfExists()
        {
            throw new NotImplementedException();
        }

        internal void drop()
        {
            throw new NotImplementedException();
        }

        internal void create()
        {
            throw new NotImplementedException();
        }

        internal void build(Connection connection, Grammars.Grammar grammar)
        {
            throw new NotImplementedException();
        }
    }
}
