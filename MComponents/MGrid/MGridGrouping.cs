using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MComponents.MGrid
{
    public class MGridGrouping<T> : IGrouping<object, T>
    {
        public object Key { get; set; }

        protected T[] Values { get; set; }

        public MGridGrouping(object pKey, T[] pValues)
        {
            Key = pKey;
            Values = pValues;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
    }
}
