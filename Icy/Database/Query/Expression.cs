using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Database.Query
{
    // 99c428b  on 1 Jun
    public class Expression
    {
        /**
         * The value of the expression.
         *
         * @var mixed
         */
        protected object _value;


        /**
         * Create a new raw query expression.
         *
         * @param  mixed  $value
         * @return void
         */
        public Expression(object value)
        {
            this._value = value;
        }


        /**
         * Get the value of the expression.
         *
         * @return mixed
         */
        public object getValue()
        {
            return this._value;
        }


        /**
         * Get the value of the expression.
         *
         * @return string
         */
        public override string ToString()
        {
            return this.getValue().ToString();
        }
    }
}
