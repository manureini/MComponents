using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MGrid
{
    public interface IMGridFormatterFactoryProvider
    {
        public IMGridObjectFormatter<T> GetFormatter<T>();
    }
}
